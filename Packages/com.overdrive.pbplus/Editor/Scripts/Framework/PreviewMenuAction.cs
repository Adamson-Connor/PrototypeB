using UnityEngine;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using System.Reflection;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Base class for menu actions that provide live preview functionality.
    /// Actions inherit from this to get automatic preview-then-confirm behavior.
    /// Automatically reads metadata from ProBuilderPlusAction attribute.
    /// </summary>
    public abstract class PreviewMenuAction : MenuAction
    {
        private ProBuilderPlusActionAttribute _cachedAttribute;

        /// <summary>
        /// Sets the cached attribute. Called by ActionAutoDiscovery to avoid double reflection.
        /// </summary>
        internal void SetCachedAttribute(ProBuilderPlusActionAttribute attribute)
        {
            _cachedAttribute = attribute;
        }

        /// <summary>
        /// Gets the ProBuilderPlusAction attribute for this action
        /// </summary>
        protected ProBuilderPlusActionAttribute ActionAttribute
        {
            get
            {
                if (_cachedAttribute == null)
                {
                    _cachedAttribute = GetType().GetCustomAttribute<ProBuilderPlusActionAttribute>();
                }
                return _cachedAttribute;
            }
        }

        /// <summary>
        /// Automatically reads icon path from ProBuilderPlusAction attribute
        /// </summary>
        public override string iconPath => ActionAttribute?.IconPath ?? "";

        /// <summary>
        /// Automatically loads icon using framework's LoadIcon method
        /// </summary>
        public override Texture2D icon =>
            string.IsNullOrEmpty(iconPath) ? null : Overdrive.ProBuilderPlus.ProBuilderPlusCore.LoadIcon(iconPath);

        /// <summary>
        /// Automatically reads tooltip from ProBuilderPlusAction attribute
        /// </summary>
        public override TooltipContent tooltip =>
            new TooltipContent(ActionAttribute.DisplayName, ActionAttribute.Tooltip);

        /// <summary>
        /// Automatically reads menu title from ProBuilderPlusAction attribute
        /// </summary>
        public override string menuTitle => ActionAttribute?.DisplayName ?? GetType().Name;

        /// <summary>
        /// Instructions text to display in the overlay. Automatically reads from ProBuilderPlusAction attribute.
        /// </summary>
        public string Instructions => ActionAttribute?.Instructions ?? "Configure settings and click Apply to confirm.";

        /// <summary>
        /// Called when the preview is first started. Set up your preview state here.
        /// </summary>
        internal abstract void StartPreview();

        /// <summary>
        /// Called when preview parameters change or selection updates. Recalculate and redraw preview.
        /// </summary>
        internal abstract void UpdatePreview();

        /// <summary>
        /// Called when user confirms the action. Apply the actual changes to the mesh here.
        /// </summary>
        /// <returns>ActionResult indicating success/failure</returns>
        internal abstract ActionResult ApplyChanges();

        /// <summary>
        /// Called when preview ends (confirm or cancel). Clean up preview state here.
        /// </summary>
        internal abstract void CleanupPreview();

        /// <summary>
        /// Called when selection changes during preview. Override to handle selection changes differently.
        /// Default behavior is to update preview for new selection.
        /// </summary>
        internal virtual void OnSelectionChangedDuringPreview()
        {
            // Default: Update preview for new selection
            RefreshPreviewForNewSelection();
        }

        /// <summary>
        /// Updates the preview to work with the current selection.
        /// </summary>
        internal virtual void RefreshPreviewForNewSelection()
        {
            StartPreview(); // Restart with new selection
        }

        /// <summary>
        /// Framework implementation - don't override this. Override the abstract methods instead.
        /// </summary>
        protected sealed override ActionResult PerformActionImplementation()
        {
            return PreviewActionFramework.HandleAction(this);
        }

        /// <summary>
        /// Override this to provide settings UI. Don't add Confirm/Cancel buttons - the framework handles those.
        /// </summary>
        public abstract override VisualElement CreateSettingsContent();

        /// <summary>
        /// Disable ProBuilder's default overlay system since we use our custom framework
        /// </summary>
        protected override MenuActionState optionsMenuState => MenuActionState.Hidden;
    }
}
