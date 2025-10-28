using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Core framework that manages preview actions - handles lifecycle, overlay management, and event coordination.
    /// </summary>
    public static class PreviewActionFramework
    {
        private static PreviewActionOverlay s_CurrentOverlay;
        private static PreviewMenuAction s_CurrentAction;
        private static bool s_IsActive;
        private static bool s_SelectionUpdateDisabled;

        static PreviewActionFramework()
        {

            // Subscribe to selection change events for automatic preview updates
            Selection.selectionChanged += OnSelectionChanged;
            ProBuilderEditor.selectionUpdated += OnProBuilderSelectionUpdated;

            // Subscribe to events that should auto-cancel (user must explicitly confirm)
            ToolManager.activeToolChanged += OnToolChanged;
            ToolManager.activeContextChanged += OnContextChanged;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            // Subscribe to SceneView events for ESC key handling
            SceneView.duringSceneGui += OnSceneGUI;

        }

        /// <summary>
        /// Main entry point - called by PreviewMenuAction.PerformActionImplementation()
        /// </summary>
        public static ActionResult HandleAction(PreviewMenuAction action)
        {
            try
            {
                // If we have a different action active, end it first
                if (s_CurrentAction != null && s_CurrentAction != action)
                {
                    EndCurrentPreview(false); // Don't apply the previous action
                }

                if (!s_IsActive)
                {
                    // Starting new preview
                    StartPreview(action);
                }
                else if (s_CurrentAction == action)
                {
                    // Updating existing preview (parameter changed)
                    s_CurrentAction.UpdatePreview();
                }

                return new ActionResult(ActionResult.Status.Success, $"Preview active: {action.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Preview framework error: {ex.Message}");
                EndCurrentPreview(false);
                return new ActionResult(ActionResult.Status.Failure, $"Preview error: {ex.Message}");
            }
        }

        private static void StartPreview(PreviewMenuAction action)
        {

            s_CurrentAction = action;
            s_IsActive = true;
            s_SelectionUpdateDisabled = false;

            // Create and show overlay
            s_CurrentOverlay = new PreviewActionOverlay();
            SceneView.AddOverlayToActiveView(s_CurrentOverlay);
            s_CurrentOverlay.displayed = true;


            // Start the action's preview
            s_CurrentAction.StartPreview();
        }

        /// <summary>
        /// Called when user clicks Confirm button
        /// </summary>
        public static void ConfirmAction()
        {
            if (s_CurrentAction != null)
            {
                try
                {
                    var result = s_CurrentAction.ApplyChanges();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error applying changes: {ex.Message}");
                }
            }
            EndCurrentPreview(false); // Don't apply again
        }

        /// <summary>
        /// Called when user clicks Cancel button
        /// </summary>
        public static void CancelAction()
        {
            EndCurrentPreview(false); // Just cleanup, no application
        }

        /// <summary>
        /// Ends the current preview session
        /// </summary>
        /// <param name="apply">Whether to apply changes before ending (default: false - cancel)</param>
        public static void EndCurrentPreview(bool apply = false)
        {
            if (!s_IsActive) return;

            try
            {
                if (apply && s_CurrentAction != null)
                {
                    s_CurrentAction.ApplyChanges();
                }

                // Cleanup action
                s_CurrentAction?.CleanupPreview();

                // Cleanup overlay
                if (s_CurrentOverlay != null)
                {
                    SceneView.RemoveOverlayFromActiveView(s_CurrentOverlay);
                    s_CurrentOverlay.displayed = false;
                    s_CurrentOverlay = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during preview cleanup: {ex.Message}");
            }
            finally
            {
                s_CurrentAction = null;
                s_IsActive = false;
                s_SelectionUpdateDisabled = false;
            }
        }

        /// <summary>
        /// Gets the settings UI from the current action
        /// </summary>
        public static VisualElement GetCurrentActionSettings()
        {
            return s_CurrentAction?.CreateSettingsContent() ?? new Label("No active preview");
        }

        /// <summary>
        /// Gets the display name for the current action
        /// </summary>
        public static string GetCurrentActionName()
        {
            return s_CurrentAction?.menuTitle ?? "Preview Action";
        }

        /// <summary>
        /// Gets the instructions text for the current action
        /// </summary>
        public static string GetCurrentActionInstructions()
        {
            return s_CurrentAction?.Instructions ?? "Configure settings and click Apply to confirm.";
        }

        /// <summary>
        /// Requests a preview update (called when settings change)
        /// </summary>
        public static void RequestPreviewUpdate()
        {
            if (s_IsActive && s_CurrentAction != null)
            {
                s_CurrentAction.UpdatePreview();
            }
        }

        private static void OnSelectionChanged()
        {
            if (s_IsActive && !s_SelectionUpdateDisabled)
            {
                // Check if selection is now empty - auto-cancel if so
                if (Selection.transforms.Length == 0)
                {
                    EndCurrentPreview(false);
                    return;
                }

                s_SelectionUpdateDisabled = true;
                EditorApplication.delayCall += () =>
                {
                    if (s_IsActive && s_CurrentAction != null)
                    {
                        s_CurrentAction.OnSelectionChangedDuringPreview();
                    }
                    s_SelectionUpdateDisabled = false;
                };
            }
        }

        private static void OnProBuilderSelectionUpdated(System.Collections.Generic.IEnumerable<ProBuilderMesh> selection)
        {
            if (s_IsActive && !s_SelectionUpdateDisabled)
            {
                // Check if ProBuilder selection is now empty - auto-cancel if so
                if (!selection.Any())
                {
                    EndCurrentPreview(false);
                    return;
                }

                // Check if element selection is empty based on current select mode - auto-cancel if so
                bool hasElementSelection = false;
                var selectMode = ProBuilderEditor.selectMode;

                foreach (var mesh in selection)
                {
                    if (mesh == null) continue;

                    switch (selectMode)
                    {
                        case SelectMode.Vertex:
                            if (mesh.selectedVertices != null && mesh.selectedVertices.Count > 0)
                                hasElementSelection = true;
                            break;
                        case SelectMode.Edge:
                            if (mesh.selectedEdges != null && mesh.selectedEdges.Count > 0)
                                hasElementSelection = true;
                            break;
                        case SelectMode.Face:
                            var selectedFaces = mesh.GetSelectedFaces();
                            if (selectedFaces != null && selectedFaces.Length > 0)
                                hasElementSelection = true;
                            break;
                        case ProBuilderPlusSelectMode.Object:
                        case SelectMode.TextureVertex:
                        case SelectMode.TextureFace:
                            hasElementSelection = true; // These modes don't need element selection
                            break;
                    }

                    if (hasElementSelection) break;
                }

                if (!hasElementSelection)
                {
                    EndCurrentPreview(false);
                    return;
                }

                s_SelectionUpdateDisabled = true;
                EditorApplication.delayCall += () =>
                {
                    if (s_IsActive && s_CurrentAction != null)
                    {
                        s_CurrentAction.OnSelectionChangedDuringPreview();
                    }
                    s_SelectionUpdateDisabled = false;
                };
            }
        }

        /// <summary>
        /// Check if a specific action is currently active
        /// </summary>
        public static bool IsActionActive(PreviewMenuAction action)
        {
            return s_IsActive && s_CurrentAction == action;
        }

        /// <summary>
        /// Check if any preview action is currently active
        /// </summary>
        public static bool IsAnyActionActive => s_IsActive;

        private static void OnToolChanged()
        {
            // Auto-cancel when tool changes (user must explicitly confirm)
            if (s_IsActive)
            {
                EndCurrentPreview(false);
            }
        }

        private static void OnContextChanged()
        {
            // Auto-cancel when context changes (user must explicitly confirm)
            if (s_IsActive)
            {
                EndCurrentPreview(false);
            }
        }

        private static void OnSelectModeChanged(SelectMode mode)
        {
            // Auto-cancel when ProBuilder select mode changes (user must explicitly confirm)
            if (s_IsActive)
            {
                EndCurrentPreview(false);
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            Event current = Event.current;

            // Only handle input when we have an active preview
            if (!s_IsActive) return;

            // Handle ESC key to cancel preview
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                EndCurrentPreview(false);
                current.Use(); // Consume the event so it doesn't propagate
            }
        }
    }
}
