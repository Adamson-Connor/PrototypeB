using UnityEngine;
using UnityEditor.ProBuilder;

namespace Overdrive.ProBuilderPlus
{
    /// <summary>
    /// Interface for actions that want to appear in ProBuilderPlus toolbars.
    /// Implementing this interface allows actions to provide their own metadata.
    /// </summary>
    public interface IProBuilderPlusAction
    {
        /// <summary>
        /// Unique identifier for this action (used for UI management)
        /// </summary>
        string ActionId { get; }
        
        /// <summary>
        /// Display name shown in the UI
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Tooltip text for the action button
        /// </summary>
        string TooltipText { get; }
        
        /// <summary>
        /// Path to icon resource (relative to Assets folder)
        /// </summary>
        string IconPath { get; }
        
        /// <summary>
        /// ProBuilder menu command to execute (if any). Null for custom actions.
        /// </summary>
        string MenuCommand { get; }
        
        /// <summary>
        /// Check if this action should be enabled in the current context
        /// </summary>
        bool IsActionEnabled { get; }
        
        /// <summary>
        /// Execute the action (for custom actions that don't use menu commands)
        /// </summary>
        void ExecuteAction();
    }
}
