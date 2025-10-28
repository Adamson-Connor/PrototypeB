using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using Overdrive.ProBuilderPlus;
using System.Linq;

namespace Overdrive.ProBuilderPlus
{
    [Overlay(typeof(SceneView), "PB+", defaultDisplay = false, defaultDockZone = DockZone.LeftColumn)]
    [Icon("Packages/com.overdrive.shared/Editor/Resources/icons/ProBuilderPlus_Icon.png")]
    public class ProBuilderPlusOverlay : Overlay, ICreateHorizontalToolbar, ICreateVerticalToolbar
{
    private VisualElement _root;
    private VisualElement _editorsContainer;
    private Label _actionsLabel;
    private VisualElement _actionsContainer;
    private VisualElement _mainElement;
    private ToggleButtonGroup _editModeButtons;
    private Button _objectModeButton;
    private Button _vertModeButton;
    private Button _edgeModeButton;
    private Button _faceModeButton;

    // Cache toolbar content for dynamic updates
    private OverlayToolbar _cachedHorizontalToolbar;
    private OverlayToolbar _cachedVerticalToolbar;

    // Icon mode state
    private bool _iconMode = false;

    // CTRL key state tracking to prevent repeat spam
    private bool _ctrlKeyDown = false;

    public override VisualElement CreatePanelContent()
    {
        _root = new VisualElement();
        
        // Add right-click context menu to the root element
        _root.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));

        // Try to load the normal UI, if it fails show placeholder
        if (!TryInitializeUI())
        {
            CreatePlaceholderUI();
        }

        return _root;
    }
    
    private bool TryInitializeUI()
    {
        // Load the UXML template from Resources
        var template = Resources.Load<VisualTreeAsset>("UXML/ProBuilderPlus_Actions-Overlay");
        
        if (template == null)
        {
            return false; // Assets not ready yet
        }
        
        // Instantiate the template
        var overlayRoot = template.Instantiate();
        _root.Add(overlayRoot);
        
        // Get references to named elements from UXML
        _editorsContainer = _root.Q<VisualElement>("EditorButtons");
        _actionsContainer = _root.Q<VisualElement>("ActionButtons");
        _actionsLabel = _root.Q<Label>("ActionsLabel");
        _mainElement = _root.Q<VisualElement>("Main");
        _editModeButtons = _root.Q<ToggleButtonGroup>("EditModeButtons");
        _objectModeButton = _root.Q<Button>("ObjectMode");
        _vertModeButton = _root.Q<Button>("VertMode");
        _edgeModeButton = _root.Q<Button>("EdgeMode");
        _faceModeButton = _root.Q<Button>("FaceMode");

        if (_editorsContainer == null || _actionsContainer == null || _actionsLabel == null || _mainElement == null)
        {
            return false;
        }

        InitializeUIElements();
        return true;
    }
    
    private void InitializeUIElements()
    {
        // Create buttons using Core methods
        ProBuilderPlusCore.PopulateEditorButtons(_editorsContainer);
        UpdateActions();

        // Wire up edit mode buttons
        if (_objectModeButton != null)
        {
            _objectModeButton.clicked += () => EditorApplication.ExecuteMenuItem("Tools/Overdrive Actions/Enter GameObject Mode");
        }
        if (_vertModeButton != null)
        {
            _vertModeButton.clicked += () => EditorApplication.ExecuteMenuItem("Tools/Overdrive Actions/Edit Vertices");
        }
        if (_edgeModeButton != null)
        {
            _edgeModeButton.clicked += () => EditorApplication.ExecuteMenuItem("Tools/Overdrive Actions/Edit Edges");
        }
        if (_faceModeButton != null)
        {
            _faceModeButton.clicked += () => EditorApplication.ExecuteMenuItem("Tools/Overdrive Actions/Edit Faces");
        }

        // Apply initial icon mode styles
        ApplyIconModeStyles();
        
        // Apply initial UI element visibility
        UpdateEditModeButtons();
        UpdateEditorButtons();
        UpdateActionButtons();

        /* not needed?
        // Add keyboard event handling for CTRL key logging
        _root.RegisterCallback<KeyDownEvent>(OnKeyDown);
        _root.RegisterCallback<KeyUpEvent>(OnKeyUp);

        // Make the root element focusable to receive keyboard events
        _root.focusable = true;
        _root.tabIndex = 0;

        // Focus the overlay when it's created
        _root.schedule.Execute(() => {
            _root.Focus();
        }).ExecuteLater(100);
        */
    }
    
    private void CreatePlaceholderUI()
    {
        // Create main container
        var container = new VisualElement();
        container.style.paddingTop = 10;
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;
        container.style.paddingBottom = 10;
        container.style.minWidth = 200;
        
        // Title label
        var titleLabel = new Label("ProBuilder Plus");
        titleLabel.style.fontSize = 14;
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.marginBottom = 8;
        container.Add(titleLabel);
        
        // QuickStart Helper button
        var quickStartButton = new Button(() => {
            Application.OpenURL("https://www.overdrivetoolset.com/probuilder-plus/");
        });
        quickStartButton.text = "QuickStart Helper";
        quickStartButton.style.marginBottom = 4;
        container.Add(quickStartButton);
        
        // Discord button
        var discordButton = new Button(() => {
            Application.OpenURL("https://discord.gg/JVQecUp7rE");
        });
        discordButton.text = "Discord";
        discordButton.style.marginBottom = 8;
        container.Add(discordButton);
        
        // Ready button
        var readyButton = new Button(() => {
            _root.Clear();
            if (TryInitializeUI())
            {
                // Force a layout refresh to ensure proper sizing
                _root.MarkDirtyRepaint();
                _root.schedule.Execute(() => {
                    _root.MarkDirtyRepaint();
                }).ExecuteLater(1);
            }
            else
            {
                // Re-create placeholder since we cleared it
                CreatePlaceholderUI();
            }
        });
        readyButton.text = "Ready!";
        readyButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f, 1f));
        container.Add(readyButton);
        
        _root.Add(container);
    }

    private void UpdateActions()
    {
        if (_actionsContainer == null || _actionsLabel == null) return;

        _actionsLabel.text = ProBuilderPlusCore.GetActionsLabelText();
        ProBuilderPlusCore.PopulateActionButtons(_actionsContainer);
        
        // Update all UI element visibility
        UpdateEditModeButtons();
        UpdateEditorButtons();
        UpdateActionButtons();
    }
    
    private void UpdateEditorButtons()
    {
        if (_editorsContainer == null) return;
        
        // Check user preference for showing editor buttons
        bool shouldShow = UserPreferences.ShowEditorButtons;
        _editorsContainer.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    private void UpdateActionButtons()
    {
        if (_actionsContainer == null || _actionsLabel == null) return;
        
        // Check user preference for showing action buttons
        bool shouldShow = UserPreferences.ShowActionButtons;
        _actionsContainer.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
        _actionsLabel.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    private void UpdateEditModeButtons()
    {
        if (_editModeButtons == null) return;
        
        // Check user preference for showing edit mode buttons
        bool shouldShow = UserPreferences.ShowEditModeButtons;
        _editModeButtons.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
        
        if (!shouldShow) return; // Don't update selection if hidden
        
        // Enable/disable based on ProBuilder object selection
        bool hasProBuilderObjects = ProBuilderPlusCore.HasProBuilderObjects;
        _editModeButtons.SetEnabled(hasProBuilderObjects);
        
        // Get current edit mode from ProBuilderPlusCore
        string currentMode = ProBuilderPlusCore.CurrentEditMode;
        
        // Map edit mode to button index (based on UXML order: ObjectMode, VertMode, EdgeMode, FaceMode)
        int selectedIndex = currentMode switch
        {
            "Object" => 0,
            "Vertex" => 1, 
            "Edge" => 2,
            "Face" => 3,
            _ => 0 // Default to Object mode
        };
        
        // Create a ToggleButtonGroupState with the selected index
        // Using SetValueWithoutNotify to avoid triggering events that could cause recursion
        var groupState = new ToggleButtonGroupState((ulong)(1 << selectedIndex), 4); // 4 buttons total
        _editModeButtons.SetValueWithoutNotify(groupState);
    }

    public OverlayToolbar CreateHorizontalToolbarContent()
    {
        _cachedHorizontalToolbar = new OverlayToolbar();
        PopulateToolbar(_cachedHorizontalToolbar, true);
        return _cachedHorizontalToolbar;
    }

    public OverlayToolbar CreateVerticalToolbarContent()
    {
        _cachedVerticalToolbar = new OverlayToolbar();
        PopulateToolbar(_cachedVerticalToolbar, true);
        return _cachedVerticalToolbar;
    }
    
    private void PopulateToolbar(OverlayToolbar toolbar, bool iconOnly)
    {
        ProBuilderPlusCore.PopulateToolbar(toolbar, iconOnly);
    }

    public override void OnCreated()
    {
        ProBuilderPlusCore.Initialize();
        ProBuilderPlusCore.Subscribe(this, OnStatusChanged);

        // Subscribe to SceneView events for keyboard handling when scene has focus
        SceneView.duringSceneGui += OnSceneGUI;

        // Ensure overlay doesn't display automatically
        displayed = false;
    }

    public override void OnWillBeDestroyed()
    {
        ProBuilderPlusCore.Unsubscribe(this, OnStatusChanged);

        // Unsubscribe from SceneView events
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnStatusChanged()
    {
        UpdateActions();
        
        // Update cached toolbar content
        if (_cachedHorizontalToolbar != null)
        {
            PopulateToolbar(_cachedHorizontalToolbar, true);
        }
        
        if (_cachedVerticalToolbar != null)
        {
            PopulateToolbar(_cachedVerticalToolbar, true);
        }
    }
    
    private void BuildContextMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Icon Mode", (a) => {
            ToggleIconMode();
        }, (a) => _iconMode ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
        
        evt.menu.AppendSeparator();
        
        evt.menu.AppendAction("Show Edit Mode Buttons", (a) => {
            UserPreferences.ShowEditModeButtons = !UserPreferences.ShowEditModeButtons;
            UpdateEditModeButtons(); // Refresh the display
        }, (a) => UserPreferences.ShowEditModeButtons ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
        
        evt.menu.AppendAction("Show Editor Buttons", (a) => {
            UserPreferences.ShowEditorButtons = !UserPreferences.ShowEditorButtons;
            UpdateEditorButtons(); // Refresh the display
        }, (a) => UserPreferences.ShowEditorButtons ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
        
        evt.menu.AppendAction("Show Action Buttons", (a) => {
            UserPreferences.ShowActionButtons = !UserPreferences.ShowActionButtons;
            UpdateActionButtons(); // Refresh the display
        }, (a) => UserPreferences.ShowActionButtons ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
        
        evt.menu.AppendSeparator();
        
        evt.menu.AppendAction("Help and Info ...", (a) => {
            Application.OpenURL("https://www.overdrivetoolset.com/probuilder-plus");
        });
    }
    
    private void ToggleIconMode()
    {
        _iconMode = !_iconMode;
        ApplyIconModeStyles();
    }
    
    private void ApplyIconModeStyles()
    {
        if (_mainElement == null || _actionsContainer == null) return;
        
        if (_iconMode)
        {
            _mainElement.AddToClassList("slim");
            _actionsContainer.AddToClassList("icon-only-button");
        }
        else
        {
            _mainElement.RemoveFromClassList("slim");
            _actionsContainer.RemoveFromClassList("icon-only-button");
        }
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        // Debug: Log CTRL key events (only on initial press, not repeats)
        if ((evt.keyCode == KeyCode.LeftControl || evt.keyCode == KeyCode.RightControl) && !_ctrlKeyDown)
        {
            _ctrlKeyDown = true;
            Overdrive.ProBuilderPlus.ProBuilderPlusCore.SetCtrlKeyState(true);
            _root?.AddToClassList("instant-mode");
            DisableNonInstantButtons(true);
        }
    }

    private void OnKeyUp(KeyUpEvent evt)
    {
        // Debug: Log CTRL key events
        if ((evt.keyCode == KeyCode.LeftControl || evt.keyCode == KeyCode.RightControl) && _ctrlKeyDown)
        {
            _ctrlKeyDown = false;
            Overdrive.ProBuilderPlus.ProBuilderPlusCore.SetCtrlKeyState(false);
            _root?.RemoveFromClassList("instant-mode");
            DisableNonInstantButtons(false);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // Only handle input when this overlay is displayed
        if (!displayed) return;

        Event current = Event.current;

        // Debug: Log CTRL key events when scene has focus (only on initial press, not repeats)
        if (current.type == EventType.KeyDown && (current.keyCode == KeyCode.LeftControl || current.keyCode == KeyCode.RightControl) && !_ctrlKeyDown)
        {
            _ctrlKeyDown = true;
            Overdrive.ProBuilderPlus.ProBuilderPlusCore.SetCtrlKeyState(true);
            _root?.AddToClassList("instant-mode");
            DisableNonInstantButtons(true);
        }
        if (current.type == EventType.KeyUp && (current.keyCode == KeyCode.LeftControl || current.keyCode == KeyCode.RightControl) && _ctrlKeyDown)
        {
            _ctrlKeyDown = false;
            Overdrive.ProBuilderPlus.ProBuilderPlusCore.SetCtrlKeyState(false);
            _root?.RemoveFromClassList("instant-mode");
            DisableNonInstantButtons(false);
        }
    }

    private void DisableNonInstantButtons(bool disable)
    {
        if (_root == null) return;

        // Find all buttons with "dont-allow-instant" class
        var nonInstantButtons = _root.Query<Button>(className: "dont-allow-instant").ToList();
        foreach (var button in nonInstantButtons)
        {
            if (disable)
            {
                // Disable them when CTRL is held
                button.SetEnabled(false);
            }
            else
            {
                // When CTRL is released, restore proper enabled state
                RestoreButtonEnabledState(button);
            }
        }
    }

    private void RestoreButtonEnabledState(Button button)
    {
        // Find the ActionInfo for this button to check its proper enabled state
        var actions = Overdrive.ProBuilderPlus.ProBuilderPlusCore.GetActions();
        var editorActions = Overdrive.ProBuilderPlus.ProBuilderPlusCore.GetEditorActions();

        // Check both action lists for the button text match
        foreach (var action in actions.Concat(editorActions))
        {
            if (button.text == action.DisplayName)
            {
                button.SetEnabled(action.IsEnabled());
                return;
            }
        }

        // If we can't find the action, default to enabled
        button.SetEnabled(true);
    }
    }
}
