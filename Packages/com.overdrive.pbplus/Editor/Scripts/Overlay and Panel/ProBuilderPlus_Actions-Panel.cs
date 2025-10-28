using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Overdrive.ProBuilderPlus;

namespace Overdrive.ProBuilderPlus
{
    public class ProBuilderPlusPanel : EditorWindow
{
    private VisualElement _root;
    private VisualElement _editorsContainer;
    private Label _actionsLabel;
    private VisualElement _actionsContainer;

    [MenuItem("Tools/ProBuilder/ProBuilder Plus Panel")]
    public static void ShowWindow()
    {
        var window = GetWindow<ProBuilderPlusPanel>();
        window.titleContent = new GUIContent("ProBuilder Plus");
        window.Show();
    }

    public void CreateGUI()
    {
        _root = rootVisualElement;
        
        // Load the UXML template
        var template = Resources.Load<VisualTreeAsset>("UXML/ProBuilderPlus_Actions-Panel");
        if (template == null)
        {
            throw new System.Exception("ProBuilderPlus_Actions-Panel.uxml not found");
        }
        
        // Instantiate the template
        var panelRoot = template.Instantiate();
        _root.Add(panelRoot);
        
        // Get references to named elements from UXML
        _editorsContainer = _root.Q<VisualElement>("EditorButtons");
        _actionsContainer = _root.Q<VisualElement>("ActionButtons");
        _actionsLabel = _root.Q<Label>("ActionsLabel");
        
        if (_editorsContainer == null || _actionsContainer == null || _actionsLabel == null)
        {
            throw new System.Exception("Required UI elements not found in UXML template");
        }

        // Create buttons using Core methods
        ProBuilderPlusCore.PopulateEditorButtons(_editorsContainer);
        UpdateActions();
    }

    private void UpdateActions()
    {
        if (_actionsContainer == null || _actionsLabel == null) return;
        
        _actionsLabel.text = ProBuilderPlusCore.GetActionsLabelText();
        ProBuilderPlusCore.PopulateActionButtons(_actionsContainer);
    }

    void OnEnable()
    {
        ProBuilderPlusCore.Initialize();
        ProBuilderPlusCore.OnStatusChanged += OnStatusChanged;
    }

    void OnDisable()
    {
        ProBuilderPlusCore.OnStatusChanged -= OnStatusChanged;
    }

    private void OnStatusChanged()
    {
        UpdateActions();
    }
    }
}
