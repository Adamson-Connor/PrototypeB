using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEditor.Overlays;
using UnityEditor.EditorTools;

namespace Overdrive.ProBuilderPlus
{
    [CustomEditor(typeof(ProBuilderMesh))]
    public class ProBuilderMeshEditor : UnityEditor.Editor
{
    ProBuilderInfoOverlay m_Overlay;
    bool m_OverlayAdded = false;

    void OnEnable()  // Called when ProBuilder object is selected
    {
        if (m_Overlay == null)
        {
            m_Overlay = new ProBuilderInfoOverlay();
        }

        // Subscribe to context changes to show/hide overlay based on edit mode
        ToolManager.activeContextChanged += OnActiveContextChanged;

        // Check if we should show the overlay initially
        UpdateOverlayVisibility();
    }

    void OnDisable() // Called when ProBuilder object is deselected
    {
        ToolManager.activeContextChanged -= OnActiveContextChanged;

        if (m_Overlay != null && m_OverlayAdded)
        {
            SceneView.RemoveOverlayFromActiveView(m_Overlay);
            m_OverlayAdded = false;
        }
    }

    private void OnActiveContextChanged()
    {
        UpdateOverlayVisibility();
    }

    private void UpdateOverlayVisibility()
    {
        if (m_Overlay == null) return;

        // Only show overlay when NOT in object mode (GameObjectToolContext)
        bool shouldShowOverlay = ToolManager.activeContextType != typeof(GameObjectToolContext);

        if (shouldShowOverlay && !m_OverlayAdded)
        {
            SceneView.AddOverlayToActiveView(m_Overlay);
            m_Overlay.displayed = true; // Make overlay visible by default
            m_OverlayAdded = true;
        }
        else if (!shouldShowOverlay && m_OverlayAdded)
        {
            SceneView.RemoveOverlayFromActiveView(m_Overlay);
            m_OverlayAdded = false;
        }
    }
    }
}
