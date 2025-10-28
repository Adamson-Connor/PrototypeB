using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Overdrive.Framework
{
    /// <summary>
    /// Framework for displaying UITK popups at mouse position in SceneView.
    /// Automatically handles positioning, focus management, and cleanup.
    /// </summary>
    public class OverdrivePopup
    {
        private VisualElement popupElement;
        private SceneView targetSceneView;
        private bool isActive;
        private System.Action onClosed;
        private bool useManualClose;
        private VisualElement contentToFocus;

        /// <summary>
        /// Shows a popup with the provided UITK content at the current mouse position in the active SceneView.
        /// </summary>
        /// <param name="content">The VisualElement to display in the popup</param>
        /// <param name="mousePosition">Optional mouse position override. If null, uses Event.current.mousePosition</param>
        /// <param name="onClosed">Optional callback when the popup is closed (by any means)</param>
        /// <param name="manualClose">If true, disables auto-close. Default is false.</param>
        /// <param name="focusTarget">Optional element to focus after positioning. If null, no auto-focus occurs.</param>
        /// <returns>The OverdrivePopup instance for further customization</returns>
        public static OverdrivePopup Show(VisualElement content, Vector2? mousePosition = null, System.Action onClosed = null, bool manualClose = false, VisualElement focusTarget = null)
        {
            var popup = new OverdrivePopup();
            popup.onClosed = onClosed;
            popup.useManualClose = manualClose;
            popup.contentToFocus = focusTarget;

            popup.targetSceneView = SceneView.lastActiveSceneView;
            if (popup.targetSceneView == null)
            {
                Debug.LogWarning("[OverdrivePopup] No active SceneView found");
                return null;
            }

            // Load container template
            var containerTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.overdrive.shared/Editor/Resources/UI/OverdrivePopup_Container.uxml");

            if (containerTemplate == null)
            {
                Debug.LogError("[OverdrivePopup] Failed to load OverdrivePopup_Container.uxml");
                return null;
            }

            // Instantiate container (this properly applies stylesheets from UXML)
            var container = containerTemplate.Instantiate();
            container.name = "overdrive-popup-container";
            var popupRoot = container.Q("popup-root");

            container.style.position = Position.Absolute;
            container.style.visibility = Visibility.Hidden; // Hide until positioned

            // Add the user's content to the popup-root element
            popupRoot.Add(content);

            // go up to "unity-overlay-canvas"
            var parentElement = popup.targetSceneView.rootVisualElement;
            while (parentElement.parent != null && !parentElement.name.StartsWith("unity-overlay-canvas"))
            {
                parentElement = parentElement.parent;
            }

            // Add container to the parent element
            parentElement.Add(container);

            // Update popupElement reference to the root for event handling
            popup.popupElement = container;
            popup.isActive = true;

            // Position at mouse with bounds checking (deferred until geometry is resolved)
            Vector2 desiredPosition = mousePosition ?? (Event.current != null ? Event.current.mousePosition : Vector2.zero);
            // move down 40 (for toolbar height) and right 10 (to enter the popup a bit)
            desiredPosition -= new Vector2(3, 22);
            EditorApplication.delayCall += () =>
            {
                if (!popup.isActive || container == null) return;

                popup.PositionWithinContainer(container, parentElement, desiredPosition);

                // Focus after positioning (if focusTarget provided)
                if (popup.contentToFocus != null)
                {
                    popup.contentToFocus.focusable = true;
                    // Second delay ensures layout and visibility are fully applied before focus
                    container.schedule.Execute(() =>
                    {
                        if (popup.isActive && popup.contentToFocus != null)
                        {
                            popup.contentToFocus.Focus();
                        }
                    });
                }
            };

            // Setup focus and close handlers (unless manual mode)
            if (!popup.useManualClose)
            {
                popup.SetupCloseHandlers();
            }

            return popup;
        }

        private void SetupCloseHandlers()
        {
            if (popupElement == null) return;

            // Close on focus lost
            popupElement.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (!popupElement.Contains(evt.relatedTarget as VisualElement))
                {
                    Close();
                }
            });

            // Close on click outside
            targetSceneView.rootVisualElement.RegisterCallback<MouseDownEvent>(OnRootMouseDown, TrickleDown.TrickleDown);
        }

        private void OnRootMouseDown(MouseDownEvent evt)
        {
            if (popupElement == null || !isActive) return;

            // Check if click was outside popup
            if (!popupElement.worldBound.Contains(evt.mousePosition))
            {
                Close();
            }
        }

        private void PositionWithinContainer(VisualElement popup, VisualElement container, Vector2 desiredPosition)
        {
            // Get actual sizes
            var popupRect = popup.layout;
            var containerRect = container.layout;

            // Calculate position with offset to stay within bounds
            float x = desiredPosition.x;
            float y = desiredPosition.y;

            // Adjust right edge
            if (x + popupRect.width > containerRect.width)
            {
                x = containerRect.width - popupRect.width;
            }

            // Adjust bottom edge
            if (y + popupRect.height > containerRect.height)
            {
                y = containerRect.height - popupRect.height;
            }

            // Ensure minimum position (don't go negative)
            x = Mathf.Max(0, x);
            y = Mathf.Max(0, y);

            popup.style.left = x;
            popup.style.top = y;
            popup.style.visibility = Visibility.Visible; // Show after positioning
        }

        /// <summary>
        /// Closes and removes the popup from the SceneView.
        /// </summary>
        public void Close()
        {
            if (!isActive || popupElement == null)
            {
                return;
            }

            isActive = false;

            // Unregister handlers
            if (targetSceneView != null)
            {
                targetSceneView.rootVisualElement.UnregisterCallback<MouseDownEvent>(OnRootMouseDown, TrickleDown.TrickleDown);
            }

            // Remove from SceneView
            if (popupElement.parent != null)
            {
                popupElement.parent.Remove(popupElement);
            }

            popupElement = null;
            targetSceneView = null;

            // Notify callback
            onClosed?.Invoke();
        }

        /// <summary>
        /// Provides access to the popup's root element for advanced customization.
        /// </summary>
        public VisualElement RootElement => popupElement;
    }
}
