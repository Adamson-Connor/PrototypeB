using System;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Type of ProBuilderPlus action for UI placement
    /// </summary>
    public enum ProBuilderPlusActionType
    {
        /// <summary>
        /// Regular geometry/selection actions that appear in mode-specific toolbars
        /// </summary>
        Action,
        
        /// <summary>
        /// Editor utility actions that belong in the EditorPanel
        /// </summary>
        EditorPanel
    }
    
    /// <summary>
    /// Extended selection modes for ProBuilderPlus actions
    /// </summary>
    public static class ProBuilderPlusSelectMode
    {
        /// <summary>
        /// Object-level actions (no element selection)
        /// </summary>
        public const SelectMode Object = (SelectMode)0x1000;
    }
    
    /// <summary>
    /// Attribute for ProBuilderPlus actions that provides metadata for automatic discovery.
    /// Actions can use this instead of manual registration in ActionInfoProvider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProBuilderPlusActionAttribute : Attribute
    {
        /// <summary>
        /// Unique identifier for this action
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Display name shown in the UI
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Tooltip text for the action button
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Instructions text displayed in the preview overlay
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Path to icon resource (relative to Assets folder)
        /// </summary>
        public string IconPath { get; set; }
        
        /// <summary>
        /// ProBuilder menu command to execute (if any). Leave null for custom actions.
        /// </summary>
        public string MenuCommand { get; set; }
        
        /// <summary>
        /// Selection modes this action is valid for
        /// </summary>
        public SelectMode ValidModes { get; set; } = SelectMode.Face | SelectMode.Edge | SelectMode.Vertex;
        
        /// <summary>
        /// Order for displaying in toolbar (lower numbers appear first)
        /// </summary>
        public int Order { get; set; } = 100;

        /// <summary>
        /// Type of action for UI placement (defaults to Action)
        /// </summary>
        public ProBuilderPlusActionType ActionType { get; set; } = ProBuilderPlusActionType.Action;

        /// <summary>
        /// Whether this action supports instant mode (CTRL+click execution with default settings)
        /// </summary>
        public bool SupportsInstantMode { get; set; } = true;

        public ProBuilderPlusActionAttribute(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}
