using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEditor.EditorTools;
using System.Collections.Generic;
using System.Linq;

namespace Overdrive.ProBuilderPlus
{
    public enum UVMode
{
    Auto,
    Manual
}


public static class UVModeStorage
{
    private static Dictionary<(ProBuilderMesh mesh, int faceIndex), UVMode> _faceUVModes = new Dictionary<(ProBuilderMesh, int), UVMode>();

    public static UVMode GetUVMode(ProBuilderMesh mesh, int faceIndex)
    {
        var key = (mesh, faceIndex);
        return _faceUVModes.TryGetValue(key, out var mode) ? mode : UVMode.Auto;
    }

    public static void SetUVMode(ProBuilderMesh mesh, int faceIndex, UVMode mode)
    {
        var key = (mesh, faceIndex);
        _faceUVModes[key] = mode;
    }

    public static void ClearUVMode(ProBuilderMesh mesh, int faceIndex)
    {
        var key = (mesh, faceIndex);
        _faceUVModes.Remove(key);
    }
}

[Overlay(typeof(SceneView), "PBi", defaultDockZone = DockZone.RightColumn)]
class ProBuilderInfoOverlay : Overlay
{
    private VisualElement _root;
    private VisualElement _elementSelectedContainer;
    private VisualElement _noElementSelectedContainer;
    private VisualElement _objectModeContainer;
    private UnityEditor.UIElements.ColorField _vertexColorField;
    private UnityEditor.UIElements.ObjectField _materialField;
    private IntegerField _smoothingGroupField;
    private EnumField _uvModeField;
    private EnumField _uvFillModeField;
    private EnumField _uvAnchorField;
    private IntegerField _uvGroupField;
    private VisualElement _uvAutoItemsContainer;
    private VisualElement _uvManualItemsContainer;
    private FloatField _uvRotationFloatField;
    private Vector2Field _uvScaleField;
    private Vector2Field _uvOffsetField;
    private UnityEngine.UIElements.Button _groupButton;
    private UnityEngine.UIElements.Button _ungroupButton;

    private List<ProBuilderMesh> _selectedMeshes = new List<ProBuilderMesh>();
    private string _currentElementType = "Object";
    private int _currentSelectionCount = 0;
    private HashSet<int> _lastSelectedFaceIndices = new HashSet<int>();
    private HashSet<int> _lastSelectedEdgeIndices = new HashSet<int>();
    private HashSet<int> _lastSelectedVertexIndices = new HashSet<int>();
    
    // Flag to prevent recursive updates when setting values programmatically
    private bool _isUpdatingValues = false;
    private int _updateValueDepth = 0;
    private bool _pendingUpdate = false;

    // Track which fields are showing mixed values
    private bool _vertexColorShowingMixed = false;
    private bool _materialShowingMixed = false;
    private bool _smoothingGroupShowingMixed = false;
    private bool _uvModeShowingMixed = false;
    private bool _uvFillModeShowingMixed = false;
    private bool _uvAnchorShowingMixed = false;
    private bool _uvGroupShowingMixed = false;
    private bool _uvRotationShowingMixed = false;
    private bool _uvScaleShowingMixed = false;
    private bool _uvOffsetShowingMixed = false;

    public ProBuilderInfoOverlay()
    {
        displayName = "PBi";
        minSize = new Vector2(80, 0);
        maxSize = new Vector2(300, float.MaxValue);
        defaultSize = new Vector2(225, 0);
    }


    public override VisualElement CreatePanelContent()
    {
        // Load UXML from Resources
        var visualTreeAsset = Resources.Load<VisualTreeAsset>("UXML/ProBuilderPlus_Inspector");
        if (visualTreeAsset == null)
        {
            var errorRoot = new VisualElement();
            errorRoot.Add(new Label("Could not load UXML file"));
            return errorRoot;
        }
        
        _root = visualTreeAsset.Instantiate();
        
        // Query for main containers
        _elementSelectedContainer = _root.Q<VisualElement>("ElementSelected");
        _noElementSelectedContainer = _root.Q<VisualElement>("NoElementSelected");
        _objectModeContainer = _root.Q<VisualElement>("ObjectMode");
        
        // Query for UI elements within ElementSelected container
        _vertexColorField = _elementSelectedContainer?.Q<UnityEditor.UIElements.ColorField>("VertexColor");
        _materialField = _elementSelectedContainer?.Q<UnityEditor.UIElements.ObjectField>("Material");
        _smoothingGroupField = _elementSelectedContainer?.Q<IntegerField>("SmoothingGroup");
        _uvModeField = _elementSelectedContainer?.Q<EnumField>("UV-AutoManualMode");
        _uvAutoItemsContainer = _elementSelectedContainer?.Q<VisualElement>("UV-AutoItems");
        _uvManualItemsContainer = _elementSelectedContainer?.Q<VisualElement>("UV-ManualItems");
        _uvFillModeField = _uvAutoItemsContainer?.Q<EnumField>("UV-FillMode");
        _uvAnchorField = _uvAutoItemsContainer?.Q<EnumField>("UV-Anchor");
        _uvGroupField = _uvAutoItemsContainer?.Q<IntegerField>("UV-Group");
        _uvRotationFloatField = _uvManualItemsContainer?.Q<FloatField>("UV-Rotation");
        _uvScaleField = _uvManualItemsContainer?.Q<Vector2Field>("UV-Scale");
        _uvOffsetField = _uvManualItemsContainer?.Q<Vector2Field>("UV-Offset");

        // Query for Group and Ungroup buttons
        _groupButton = _uvAutoItemsContainer?.Q<UnityEngine.UIElements.Button>("GroupSelected");
        _ungroupButton = _uvAutoItemsContainer?.Q<UnityEngine.UIElements.Button>("UngroupSelected");

        // Set up field properties
        if (_materialField != null)
        {
            _materialField.objectType = typeof(Material);
        }

        if (_uvModeField != null)
        {
            _uvModeField.Init(UVMode.Auto);
        }

        if (_uvFillModeField != null)
        {
            _uvFillModeField.Init(AutoUnwrapSettings.Fill.Fit);
        }

        if (_uvAnchorField != null)
        {
            _uvAnchorField.Init(AutoUnwrapSettings.Anchor.MiddleCenter);
        }

        // Set up event handlers for value changes
        SetupValueChangeHandlers();

        // Subscribe to ProBuilder events
        ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
        ProBuilderEditor.selectionUpdated += OnProBuilderSelectionUpdated;
        Selection.selectionChanged += OnSelectionChanged;
        ToolManager.activeContextChanged += OnActiveContextChanged;

        // Initialize ProBuilder status before updating display
        UpdateProBuilderStatus();
        
        // Update initially
        UpdateDisplay();

        return _root;
    }

    private void SetupValueChangeHandlers()
    {
        // Vertex color change handler
        if (_vertexColorField != null)
        {
            _vertexColorField.RegisterValueChangedCallback(OnVertexColorChanged);
        }

        // Material change handler (faces only)
        if (_materialField != null)
        {
            _materialField.RegisterValueChangedCallback(OnMaterialChanged);
        }

        // Smoothing group change handler (faces only)
        if (_smoothingGroupField != null)
        {
            _smoothingGroupField.RegisterValueChangedCallback(OnSmoothingGroupChanged);
        }

        // UV mode change handler (faces only)
        if (_uvModeField != null)
        {
            _uvModeField.RegisterValueChangedCallback(OnUVModeChanged);
        }

        // UV fill mode change handler (faces only)
        if (_uvFillModeField != null)
        {
            _uvFillModeField.RegisterValueChangedCallback(OnUVFillModeChanged);
        }

        // UV group change handler (faces only)
        if (_uvGroupField != null)
        {
            _uvGroupField.RegisterValueChangedCallback(OnUVGroupChanged);
        }

        // UV settings change handlers (faces only)
        if (_uvAnchorField != null)
        {
            _uvAnchorField.RegisterValueChangedCallback(OnUVAnchorChanged);
        }

        if (_uvRotationFloatField != null)
        {
            _uvRotationFloatField.RegisterValueChangedCallback(OnUVRotationChanged);
            _uvRotationFloatField.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => ClearMixedStateOnFocus(_uvRotationFloatField, ref _uvRotationShowingMixed));
        }

        if (_uvScaleField != null)
        {
            _uvScaleField.RegisterValueChangedCallback(OnUVScaleChanged);
            var xField = _uvScaleField.Q<FloatField>("unity-x-input");
            var yField = _uvScaleField.Q<FloatField>("unity-y-input");
            xField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => ClearMixedStateOnFocus(_uvScaleField, ref _uvScaleShowingMixed));
            yField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => ClearMixedStateOnFocus(_uvScaleField, ref _uvScaleShowingMixed));
        }

        if (_uvOffsetField != null)
        {
            _uvOffsetField.RegisterValueChangedCallback(OnUVOffsetChanged);
            var xField = _uvOffsetField.Q<FloatField>("unity-x-input");
            var yField = _uvOffsetField.Q<FloatField>("unity-y-input");
            xField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => ClearMixedStateOnFocus(_uvOffsetField, ref _uvOffsetShowingMixed));
            yField?.Q<TextField>()?.RegisterCallback<FocusInEvent>(evt => ClearMixedStateOnFocus(_uvOffsetField, ref _uvOffsetShowingMixed));
        }

        // Group and Ungroup button handlers
        if (_groupButton != null)
        {
            _groupButton.RegisterCallback<ClickEvent>(OnGroupButtonClicked);
        }

        if (_ungroupButton != null)
        {
            _ungroupButton.RegisterCallback<ClickEvent>(OnUngroupButtonClicked);
        }
    }

    public override void OnWillBeDestroyed()
    {
        ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
        ProBuilderEditor.selectionUpdated -= OnProBuilderSelectionUpdated;
        Selection.selectionChanged -= OnSelectionChanged;
        ToolManager.activeContextChanged -= OnActiveContextChanged;
    }

    private void UpdateDisplay()
    {
        if (_root == null) return;
        
        // Get selected ProBuilder meshes
        _selectedMeshes.Clear();
        foreach (var obj in Selection.gameObjects)
        {
            var mesh = obj.GetComponent<ProBuilderMesh>();
            if (mesh != null)
                _selectedMeshes.Add(mesh);
        }

        // Update visibility and values
        UpdateElementVisibility();
        UpdateElementValues();
    }

    #region Value Change Handlers

    private void OnVertexColorChanged(ChangeEvent<Color> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        ApplyVertexColor(evt.newValue);
    }

    private void OnMaterialChanged(ChangeEvent<UnityEngine.Object> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplyMaterial(evt.newValue as Material);
        }
    }

    private void OnSmoothingGroupChanged(ChangeEvent<int> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplySmoothingGroup(evt.newValue);
        }
    }

    private void OnUVModeChanged(ChangeEvent<System.Enum> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            var uvMode = (UVMode)evt.newValue;
            UpdateUVModeVisibility(uvMode);
            ApplyUVMode(uvMode);
        }
    }

    private void OnUVFillModeChanged(ChangeEvent<System.Enum> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplyUVFillMode((AutoUnwrapSettings.Fill)evt.newValue);
        }
    }

    private void OnUVGroupChanged(ChangeEvent<int> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplyUVGroup(evt.newValue);
        }
    }

    private void OnUVAnchorChanged(ChangeEvent<System.Enum> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplyUVAnchor((AutoUnwrapSettings.Anchor)evt.newValue);
        }
    }

    private void OnUVRotationChanged(ChangeEvent<float> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplyUVRotation(evt.newValue);
        }
    }

    private void OnUVScaleChanged(ChangeEvent<Vector2> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplyUVScale(evt.newValue);
        }
    }

    private void OnUVOffsetChanged(ChangeEvent<Vector2> evt)
    {
        if (_isUpdatingValues || _updateValueDepth > 0) return;
        if (_currentElementType == "Face")
        {
            ApplyUVOffset(evt.newValue);
        }
    }

    private void OnGroupButtonClicked(ClickEvent evt)
    {
        if (_currentElementType != "Face") return;

        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        UnityEditor.Undo.RecordObjects(selectedMeshes, "Group Selected Faces");

        foreach (var mesh in selectedMeshes)
        {
            TextureGroupSelectedFaces(mesh);
        }

        // Refresh the mesh display
        foreach (var mesh in selectedMeshes)
        {
            mesh.ToMesh();
            mesh.Refresh();
        }

        ProBuilderEditor.Refresh();
    }

    private void OnUngroupButtonClicked(ClickEvent evt)
    {
        if (_currentElementType != "Face") return;

        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        UnityEditor.Undo.RecordObjects(selectedMeshes, "Ungroup Selected Faces");

        foreach (var mesh in selectedMeshes)
        {
            UngroupSelectedFaces(mesh);
        }

        // Refresh the mesh display
        foreach (var mesh in selectedMeshes)
        {
            mesh.ToMesh();
            mesh.Refresh();
            mesh.Optimize();
        }

        ProBuilderEditor.Refresh();
    }

    #endregion

    #region Apply Changes Methods

    private void ApplyVertexColor(Color color)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change Vertex Color");

        Color linearColor = PlayerSettings.colorSpace == ColorSpace.Linear ? color.linear : color;

        foreach (var mesh in selectedMeshes)
        {
            // Ensure the mesh has color arrays initialized
            Color[] colors = mesh.GetColors();
            if (colors == null || colors.Length != mesh.vertexCount)
            {
                colors = new Color[mesh.vertexCount];
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = Color.white;
            }

            var currentSelectMode = ProBuilderEditor.selectMode;

            switch (currentSelectMode)
            {
                case SelectMode.Face:
                    var selectedFaces = mesh.GetSelectedFaces();
                    if (selectedFaces != null)
                    {
                        foreach (var face in selectedFaces)
                        {
                            foreach (int vertexIndex in face.indexes)
                            {
                                if (vertexIndex < colors.Length)
                                    colors[vertexIndex] = linearColor;
                            }
                        }
                    }
                    break;
                    
                case SelectMode.Edge:
                    var selectedEdges = mesh.selectedEdges;
                    if (selectedEdges != null)
                    {
                        foreach (var edge in selectedEdges)
                        {
                            // Color both vertices of the edge and their coincident vertices
                            int[] edgeVertices = { edge.a, edge.b };

                            foreach (int vertexIndex in edgeVertices)
                            {
                                if (vertexIndex < colors.Length)
                                    colors[vertexIndex] = linearColor;

                                // Find and color all coincident vertices for this edge vertex
                                var sharedVertices = mesh.sharedVertices;
                                foreach (var sharedVertexGroup in sharedVertices)
                                {
                                    if (sharedVertexGroup.Contains(vertexIndex))
                                    {
                                        // Color all vertices in this shared vertex group
                                        foreach (int sharedVertexIndex in sharedVertexGroup)
                                        {
                                            if (sharedVertexIndex < colors.Length)
                                                colors[sharedVertexIndex] = linearColor;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                    
                case SelectMode.Vertex:
                    var selectedVertices = mesh.selectedVertices;
                    if (selectedVertices != null)
                    {
                        // For each selected vertex, find all coincident vertices and color them too
                        foreach (int vertexIndex in selectedVertices)
                        {
                            if (vertexIndex < colors.Length)
                                colors[vertexIndex] = linearColor;

                            // Find and color all coincident vertices (vertices at the same position)
                            var sharedVertices = mesh.sharedVertices;
                            foreach (var sharedVertexGroup in sharedVertices)
                            {
                                if (sharedVertexGroup.Contains(vertexIndex))
                                {
                                    // Color all vertices in this shared vertex group
                                    foreach (int sharedVertexIndex in sharedVertexGroup)
                                    {
                                        if (sharedVertexIndex < colors.Length)
                                            colors[sharedVertexIndex] = linearColor;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            // Set the colors back to the mesh
            mesh.colors = colors;
            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    private void ApplyMaterial(Material material)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes.Select(m => m.GetComponent<Renderer>()).Where(r => r != null).ToArray(), "Change Face Material");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            var renderer = mesh.GetComponent<Renderer>();
            if (renderer == null) continue;

            foreach (var face in selectedFaces)
            {
                // Set the material index for the face
                if (material != null)
                {
                    // Find or add material to renderer
                    var materials = renderer.sharedMaterials.ToList();
                    int materialIndex = materials.IndexOf(material);
                    
                    if (materialIndex == -1)
                    {
                        materials.Add(material);
                        materialIndex = materials.Count - 1;
                        renderer.sharedMaterials = materials.ToArray();
                    }
                    
                    face.submeshIndex = materialIndex;
                }
            }

            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    private void ApplySmoothingGroup(int smoothingGroup)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change Smoothing Group");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                face.smoothingGroup = smoothingGroup;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    private void ApplyUVMode(UVMode uvMode)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change UV Mode");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            var allFaces = mesh.faces.ToArray();

            foreach (var selectedFace in selectedFaces)
            {
                // Find the index of this face
                int faceIndex = -1;
                for (int i = 0; i < allFaces.Length; i++)
                {
                    if (allFaces[i] == selectedFace)
                    {
                        faceIndex = i;
                        break;
                    }
                }

                if (faceIndex != -1)
                {
                    UVModeStorage.SetUVMode(mesh, faceIndex, uvMode);
                }
            }
        }

        // Update visibility based on the new mode
        UpdateUVModeVisibility(uvMode);
    }

    private void ApplyUVFillMode(AutoUnwrapSettings.Fill fillMode)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change UV Fill Mode");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                var uvSettings = face.uv;
                uvSettings.fill = fillMode;
                face.uv = uvSettings;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    private void ApplyUVGroup(int uvGroup)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change UV Group");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                // Note: UV Group might need to be stored as a custom property
                // since ProBuilder's AutoUnwrapSettings doesn't have a direct equivalent
                // This would need to be implemented based on how UV groups are used
            }

            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    private void ApplyUVAnchor(AutoUnwrapSettings.Anchor anchor)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change UV Anchor");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                var uvSettings = face.uv;
                uvSettings.anchor = anchor;
                face.uv = uvSettings;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    private void ApplyUVRotation(float rotation)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change UV Rotation");

        int totalFacesUpdated = 0;
        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                var uvSettings = face.uv;
                uvSettings.rotation = rotation;
                face.uv = uvSettings;
                totalFacesUpdated++;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }

    }

    private void ApplyUVScale(Vector2 scale)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change UV Scale");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                var uvSettings = face.uv;
                uvSettings.scale = scale;
                face.uv = uvSettings;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    private void ApplyUVOffset(Vector2 offset)
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return;

        Undo.RecordObjects(selectedMeshes, "Change UV Offset");

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                var uvSettings = face.uv;
                uvSettings.offset = offset;
                face.uv = uvSettings;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }
    }

    #endregion

    #region UV Texture Group Methods

    private void TextureGroupSelectedFaces(ProBuilderMesh pb)
    {
        if (pb.selectedFaceCount < 1) return;

        Face[] faces = pb.GetSelectedFaces();
        AutoUnwrapSettings cont_uv = faces[0].uv;
        int texGroup = GetUnusedTextureGroup(pb);

        foreach (Face f in faces)
        {
            f.uv = new AutoUnwrapSettings(cont_uv);
            f.textureGroup = texGroup;
        }
    }

    private int GetUnusedTextureGroup(ProBuilderMesh pb, int startIndex = 1)
    {
        var allFaces = pb.faces;
        var usedGroups = new HashSet<int>(allFaces.Select(f => f.textureGroup));

        int textureGroup = startIndex;
        while (usedGroups.Contains(textureGroup))
        {
            textureGroup++;
        }

        return textureGroup;
    }

    private void UngroupSelectedFaces(ProBuilderMesh pb)
    {
        if (pb.selectedFaceCount < 1) return;

        Face[] faces = pb.GetSelectedFaces();
        AutoUnwrapSettings cuv = faces[0].uv;

        foreach (Face f in faces)
        {
            f.textureGroup = -1;
            f.uv = new AutoUnwrapSettings(cuv);
        }
    }

    #endregion

    private void UpdateUVModeVisibility(UVMode uvMode)
    {
        if (_uvAutoItemsContainer != null)
        {
            // Auto items are enabled only in Auto mode, disabled in Manual or Mixed
            _uvAutoItemsContainer.SetEnabled(uvMode == UVMode.Auto);
        }

        if (_uvManualItemsContainer != null)
        {
            // Manual items are always visible
            _uvManualItemsContainer.style.display = DisplayStyle.Flex;
        }
    }

    private void UpdateElementVisibility()
    {
        bool hasProBuilderSelection = _selectedMeshes.Count > 0;
        bool isInEditMode = ToolManager.activeContextType != typeof(GameObjectToolContext);
        bool hasElementSelection = _currentElementType != "Object" && _currentSelectionCount > 0;
        
        // Hide all containers first
        if (_elementSelectedContainer != null)
            _elementSelectedContainer.style.display = DisplayStyle.None;
        if (_noElementSelectedContainer != null)
            _noElementSelectedContainer.style.display = DisplayStyle.None;
        if (_objectModeContainer != null)
            _objectModeContainer.style.display = DisplayStyle.None;
        
        // Show appropriate container based on mode and selection
        if (!isInEditMode)
        {
            // Object mode - show ObjectMode container
            if (_objectModeContainer != null)
                _objectModeContainer.style.display = DisplayStyle.Flex;
        }
        else if (hasElementSelection)
        {
            // Element mode with selection - show ElementSelected container
            if (_elementSelectedContainer != null)
            {
                _elementSelectedContainer.style.display = DisplayStyle.Flex;
            }
        }
        else
        {
            // Element mode without selection - show NoElementSelected container
            if (_noElementSelectedContainer != null)
                _noElementSelectedContainer.style.display = DisplayStyle.Flex;
        }
    }



    private void UpdateElementValues()
    {
        // Only update values when ElementSelected container is visible
        if (_elementSelectedContainer == null || _elementSelectedContainer.style.display != DisplayStyle.Flex)
            return;

        // Set flags to prevent recursive updates during programmatic value changes
        _updateValueDepth++;
        _isUpdatingValues = true;

        try
        {
            // Show/hide UI elements based on mode
            bool isVertexOrEdgeMode = _currentElementType == "Vertex" || _currentElementType == "Edge";
            bool isFaceMode = _currentElementType == "Face";

            // Vertex color - always visible
            if (_vertexColorField != null)
            {
                _vertexColorField.style.display = DisplayStyle.Flex;
                var (vertexColor, hasMixedVertexColor) = GetCurrentSelectionColorWithMixed();
                SetColorFieldMixed(_vertexColorField, hasMixedVertexColor, vertexColor, ref _vertexColorShowingMixed);
            }

            // Material - only visible in face mode
            if (_materialField != null)
            {
                _materialField.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                if (isFaceMode)
                {
                    var (material, hasMixedMaterial) = GetCurrentFaceMaterialWithMixed();
                    SetObjectFieldMixed(_materialField, hasMixedMaterial, material, ref _materialShowingMixed);
                }
            }

            // Smoothing group - only visible in face mode
            if (_smoothingGroupField != null)
            {
                _smoothingGroupField.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                if (isFaceMode)
                {
                    var (smoothingGroup, hasMixedSmoothingGroup) = GetCurrentFaceSmoothingGroupWithMixed();
                    SetIntegerFieldMixed(_smoothingGroupField, hasMixedSmoothingGroup, smoothingGroup, ref _smoothingGroupShowingMixed);
                }
            }

            // UV mode - only visible in face mode
            if (_uvModeField != null)
            {
                _uvModeField.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
                if (isFaceMode)
                {
                    var (uvMode, hasMixedUVMode) = GetCurrentUVModeWithMixed();
                    SetEnumFieldMixed(_uvModeField, hasMixedUVMode, uvMode, ref _uvModeShowingMixed);
                    UpdateUVModeVisibility(hasMixedUVMode ? UVMode.Auto : uvMode); // Use Auto for visibility when mixed
                }
            }

            // UV containers - only visible in face mode
            if (_uvAutoItemsContainer != null)
            {
                _uvAutoItemsContainer.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_uvManualItemsContainer != null)
            {
                _uvManualItemsContainer.style.display = isFaceMode ? DisplayStyle.Flex : DisplayStyle.None;
            }

            // Update UV settings (only for face selections)
            if (isFaceMode)
            {
                var uvValues = GetUVValuesWithMixedDetection();

                // Update UV fill mode
                if (_uvFillModeField != null)
                {
                    SetEnumFieldMixed(_uvFillModeField, uvValues.hasMixedFill, uvValues.fill, ref _uvFillModeShowingMixed);
                }

                // Update UV group
                if (_uvGroupField != null)
                {
                    SetIntegerFieldMixed(_uvGroupField, uvValues.hasMixedGroup, uvValues.group, ref _uvGroupShowingMixed);
                }

                // Update UV anchor
                if (_uvAnchorField != null)
                {
                    SetEnumFieldMixed(_uvAnchorField, uvValues.hasMixedAnchor, uvValues.anchor, ref _uvAnchorShowingMixed);
                }

                // Update UV rotation
                if (_uvRotationFloatField != null)
                {
                    SetFloatFieldMixed(_uvRotationFloatField, uvValues.hasMixedRotation, uvValues.rotation, ref _uvRotationShowingMixed);
                }

                // Update UV scale
                if (_uvScaleField != null)
                {
                    SetVector2FieldMixed(_uvScaleField, uvValues.hasMixedScale, uvValues.scale, ref _uvScaleShowingMixed);
                }

                // Update UV offset
                if (_uvOffsetField != null)
                {
                    SetVector2FieldMixed(_uvOffsetField, uvValues.hasMixedOffset, uvValues.offset, ref _uvOffsetShowingMixed);
                }
            }
        }
        finally
        {
            // Always reset flags after updating values
            _isUpdatingValues = false;
            _updateValueDepth--;
        }
    }

    private (Color color, bool hasMixed) GetCurrentSelectionColorWithMixed()
    {
        if (_selectedMeshes.Count == 0) return (Color.white, false);

        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return (Color.white, false);

        Color? firstColor = null;
        bool hasMixed = false;
        var currentSelectMode = ProBuilderEditor.selectMode;

        foreach (var mesh in selectedMeshes)
        {
            var vertices = mesh.GetVertices();
            if (vertices == null || vertices.Length == 0) continue;

            switch (currentSelectMode)
            {
                case SelectMode.Face:
                    var selectedFaces = mesh.GetSelectedFaces();
                    if (selectedFaces != null)
                    {
                        foreach (var face in selectedFaces)
                        {
                            if (face.indexes.Count > 0 && face.indexes[0] < vertices.Length)
                            {
                                var color = vertices[face.indexes[0]].color.gamma;
                                if (firstColor == null) firstColor = color;
                                else if (!Mathf.Approximately(Vector4.Distance(firstColor.Value, color), 0)) hasMixed = true;
                            }
                        }
                    }
                    break;
                case SelectMode.Edge:
                    var selectedEdges = mesh.selectedEdges;
                    if (selectedEdges != null)
                    {
                        foreach (var edge in selectedEdges)
                        {
                            if (edge.a < vertices.Length)
                            {
                                var color = vertices[edge.a].color.gamma;
                                if (firstColor == null) firstColor = color;
                                else if (!Mathf.Approximately(Vector4.Distance(firstColor.Value, color), 0)) hasMixed = true;
                            }
                        }
                    }
                    break;
                case SelectMode.Vertex:
                    var selectedVertices = mesh.selectedVertices;
                    if (selectedVertices != null)
                    {
                        foreach (int vertexIndex in selectedVertices)
                        {
                            if (vertexIndex < vertices.Length)
                            {
                                var color = vertices[vertexIndex].color.gamma;
                                if (firstColor == null) firstColor = color;
                                else if (!Mathf.Approximately(Vector4.Distance(firstColor.Value, color), 0)) hasMixed = true;
                            }
                        }
                    }
                    break;
            }
            if (hasMixed) break;
        }

        return (firstColor ?? Color.white, hasMixed);
    }

    private (Material material, bool hasMixed) GetCurrentFaceMaterialWithMixed()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return (null, false);

        Material firstMaterial = null;
        bool hasMixed = false;

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            var renderer = mesh.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterials == null) continue;

            foreach (var face in selectedFaces)
            {
                int materialIndex = face.submeshIndex;
                Material material = null;
                if (materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length)
                {
                    material = renderer.sharedMaterials[materialIndex];
                }

                if (firstMaterial == null) firstMaterial = material;
                else if (firstMaterial != material) hasMixed = true;
            }
            if (hasMixed) break;
        }

        return (firstMaterial, hasMixed);
    }

    private (int smoothingGroup, bool hasMixed) GetCurrentFaceSmoothingGroupWithMixed()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return (0, false);

        int? firstSmoothingGroup = null;
        bool hasMixed = false;

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                if (firstSmoothingGroup == null) firstSmoothingGroup = face.smoothingGroup;
                else if (firstSmoothingGroup != face.smoothingGroup) hasMixed = true;
            }
            if (hasMixed) break;
        }

        return (firstSmoothingGroup ?? 0, hasMixed);
    }

    private Material GetCurrentFaceMaterial()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return null;

        var mesh = selectedMeshes[0];
        var selectedFaces = mesh.GetSelectedFaces();
        if (selectedFaces == null || selectedFaces.Length == 0) return null;

        // Get the material from the first selected face
        var face = selectedFaces[0];
        var renderer = mesh.GetComponent<Renderer>();
        if (renderer == null || renderer.sharedMaterials == null) return null;

        // ProBuilder faces have a material index
        int materialIndex = face.submeshIndex;
        if (materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length)
        {
            return renderer.sharedMaterials[materialIndex];
        }

        return null;
    }

    private int GetCurrentFaceSmoothingGroup()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return 0;

        var mesh = selectedMeshes[0];
        var selectedFaces = mesh.GetSelectedFaces();
        if (selectedFaces == null || selectedFaces.Length == 0) return 0;

        // Get the smoothing group from the first selected face
        return selectedFaces[0].smoothingGroup;
    }

    private AutoUnwrapSettings? GetCurrentUVSettings()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return null;

        var mesh = selectedMeshes[0];
        var selectedFaces = mesh.GetSelectedFaces();
        if (selectedFaces == null || selectedFaces.Length == 0) return null;

        // Return the UV settings from the first selected face
        return selectedFaces[0].uv;
    }

    private (UVMode uvMode, bool hasMixed) GetCurrentUVModeWithMixed()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return (UVMode.Auto, false);

        UVMode? firstMode = null;
        bool hasMixed = false;

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            var allFaces = mesh.faces.ToArray();

            foreach (var selectedFace in selectedFaces)
            {
                // Find the index of this face
                int faceIndex = -1;
                for (int i = 0; i < allFaces.Length; i++)
                {
                    if (allFaces[i] == selectedFace)
                    {
                        faceIndex = i;
                        break;
                    }
                }

                if (faceIndex != -1)
                {
                    var faceMode = UVModeStorage.GetUVMode(mesh, faceIndex);

                    if (firstMode == null)
                    {
                        firstMode = faceMode;
                    }
                    else if (firstMode != faceMode)
                    {
                        hasMixed = true;
                        break;
                    }
                }
            }

            if (hasMixed) break;
        }

        return (firstMode ?? UVMode.Auto, hasMixed);
    }

    private UVMode GetCurrentUVMode()
    {
        var (uvMode, _) = GetCurrentUVModeWithMixed();
        return uvMode;
    }

    private AutoUnwrapSettings.Fill GetCurrentUVFillMode()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return AutoUnwrapSettings.Fill.Fit;

        var mesh = selectedMeshes[0];
        var selectedFaces = mesh.GetSelectedFaces();
        if (selectedFaces == null || selectedFaces.Length == 0) return AutoUnwrapSettings.Fill.Fit;

        // Return the fill mode from the first selected face
        return selectedFaces[0].uv.fill;
    }

    private int GetCurrentUVGroup()
    {
        // Default to group 0 for now
        // This could be stored as a preference or per-face custom property
        return 0;
    }

    // Mixed value detection methods
    private struct UVValues
    {
        // UV values
        public bool hasMixedUVMode;
        public bool hasMixedFill;
        public bool hasMixedAnchor;
        public bool hasMixedRotation;
        public bool hasMixedScale;
        public bool hasMixedOffset;
        public bool hasMixedGroup;

        // Other values
        public bool hasMixedVertexColor;
        public bool hasMixedMaterial;
        public bool hasMixedSmoothingGroup;

        // UV values
        public UVMode uvMode;
        public AutoUnwrapSettings.Fill fill;
        public AutoUnwrapSettings.Anchor anchor;
        public float rotation;
        public Vector2 scale;
        public Vector2 offset;
        public int group;

        // Other values
        public Color vertexColor;
        public Material material;
        public int smoothingGroup;
    }

    private UVValues GetUVValuesWithMixedDetection()
    {
        var result = new UVValues();
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0)
        {
            // Default values
            result.uvMode = UVMode.Auto;
            result.fill = AutoUnwrapSettings.Fill.Fit;
            result.anchor = AutoUnwrapSettings.Anchor.MiddleCenter;
            result.rotation = 0f;
            result.scale = Vector2.one;
            result.offset = Vector2.zero;
            result.group = 0;
            result.vertexColor = Color.white;
            result.material = null;
            result.smoothingGroup = 0;
            return result;
        }

        int totalFacesChecked = 0;

        AutoUnwrapSettings.Fill? firstFill = null;
        AutoUnwrapSettings.Anchor? firstAnchor = null;
        float? firstRotation = null;
        Vector2? firstScale = null;
        Vector2? firstOffset = null;
        int? firstGroup = null;

        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces == null) continue;

            foreach (var face in selectedFaces)
            {
                var uvSettings = face.uv;

                // Check fill
                if (firstFill == null) firstFill = uvSettings.fill;
                else if (firstFill != uvSettings.fill) result.hasMixedFill = true;

                // Check anchor
                if (firstAnchor == null) firstAnchor = uvSettings.anchor;
                else if (firstAnchor != uvSettings.anchor) result.hasMixedAnchor = true;

                // Check rotation
                if (firstRotation == null) firstRotation = uvSettings.rotation;
                else if (Mathf.Abs(firstRotation.Value - uvSettings.rotation) > 0.001f) result.hasMixedRotation = true;

                // Check scale
                if (firstScale == null) firstScale = uvSettings.scale;
                else if (Vector2.Distance(firstScale.Value, uvSettings.scale) > 0.001f) result.hasMixedScale = true;

                // Check offset
                if (firstOffset == null) firstOffset = uvSettings.offset;
                else if (Vector2.Distance(firstOffset.Value, uvSettings.offset) > 0.001f) result.hasMixedOffset = true;

                // Check group (placeholder for now)
                if (firstGroup == null) firstGroup = 0; // Default group
                else if (firstGroup != 0) result.hasMixedGroup = true;

                totalFacesChecked++;
            }
        }

        // Set the first values as defaults
        result.fill = firstFill ?? AutoUnwrapSettings.Fill.Fit;
        result.anchor = firstAnchor ?? AutoUnwrapSettings.Anchor.MiddleCenter;
        result.rotation = firstRotation ?? 0f;
        result.scale = firstScale ?? Vector2.one;
        result.offset = firstOffset ?? Vector2.zero;
        result.group = firstGroup ?? 0;

        return result;
    }

    // Helper methods for mixed value display
    private void SetFloatFieldMixed(FloatField field, bool showMixed, float value, ref bool mixedFlag)
    {
        if (field == null)
        {
            Debug.LogWarning("SetFloatFieldMixed: FloatField is null!");
            return;
        }

        if (showMixed)
        {
            if (!mixedFlag)
            {
                var textElement = field.Q(className: "unity-text-element__selectable");
                if (textElement == null)
                {
                    Debug.LogWarning("SetFloatFieldMixed: Could not find text element in FloatField!");
                    return;
                }

                if (textElement is TextElement te)
                {
                    te.text = "-";
                }
                else
                {
                    Debug.LogWarning($"SetFloatFieldMixed: Element is not TextElement, it's {textElement.GetType().Name}!");
                    return;
                }

                mixedFlag = true;
            }
        }
        else
        {
            if (mixedFlag)
            {
                mixedFlag = false;
                // Restore the text element to show the actual value
                var textElement = field.Q(className: "unity-text-element__selectable");
                if (textElement is TextElement te)
                {
                    te.text = value.ToString();
                }
            }
            field.value = value;
        }
    }

    private void SetVector2FieldMixed(Vector2Field field, bool showMixed, Vector2 value, ref bool mixedFlag)
    {
        if (field == null)
        {
            Debug.LogWarning("SetVector2FieldMixed: Vector2Field is null!");
            return;
        }

        if (showMixed)
        {
            if (!mixedFlag)
            {
                var xInput = field.Q(name: "unity-x-input");
                var yInput = field.Q(name: "unity-y-input");

                if (xInput == null)
                {
                    Debug.LogWarning("SetVector2FieldMixed: Could not find unity-x-input!");
                    return;
                }
                if (yInput == null)
                {
                    Debug.LogWarning("SetVector2FieldMixed: Could not find unity-y-input!");
                    return;
                }

                var xTextElement = xInput.Q(className: "unity-text-element__selectable");
                var yTextElement = yInput.Q(className: "unity-text-element__selectable");

                if (xTextElement == null)
                {
                    Debug.LogWarning("SetVector2FieldMixed: Could not find X text element!");
                    return;
                }
                if (yTextElement == null)
                {
                    Debug.LogWarning("SetVector2FieldMixed: Could not find Y text element!");
                    return;
                }

                if (xTextElement is TextElement xTE && yTextElement is TextElement yTE)
                {
                    xTE.text = "-";
                    yTE.text = "-";
                }
                else
                {
                    Debug.LogWarning($"SetVector2FieldMixed: Elements are not TextElements! X: {xTextElement.GetType().Name}, Y: {yTextElement.GetType().Name}");
                    return;
                }

                mixedFlag = true;
            }
        }
        else
        {
            if (mixedFlag)
            {
                mixedFlag = false;
                // Restore the text elements to show the actual values
                var xInput = field.Q(name: "unity-x-input");
                var yInput = field.Q(name: "unity-y-input");
                if (xInput != null && yInput != null)
                {
                    var xTextElement = xInput.Q(className: "unity-text-element__selectable");
                    var yTextElement = yInput.Q(className: "unity-text-element__selectable");
                    if (xTextElement is TextElement xTE && yTextElement is TextElement yTE)
                    {
                        xTE.text = value.x.ToString();
                        yTE.text = value.y.ToString();
                    }
                }
            }
            field.value = value;
        }
    }

    private void SetColorFieldMixed(UnityEditor.UIElements.ColorField field, bool showMixed, Color value, ref bool mixedFlag)
    {
        if (field == null)
        {
            Debug.LogWarning("SetColorFieldMixed: ColorField is null!");
            return;
        }

        if (showMixed)
        {
            if (!mixedFlag)
            {
                field.label = "(mixed)";
                mixedFlag = true;
            }
        }
        else
        {
            if (mixedFlag)
            {
                mixedFlag = false;
                field.label = "Vertex Color";
            }
            field.value = value;
        }
    }

    private void SetObjectFieldMixed(UnityEditor.UIElements.ObjectField field, bool showMixed, UnityEngine.Object value, ref bool mixedFlag)
    {
        if (field == null)
        {
            Debug.LogWarning("SetObjectFieldMixed: ObjectField is null!");
            return;
        }

        if (showMixed)
        {
            if (!mixedFlag)
            {
                field.label = "(mixed)";
                mixedFlag = true;
            }
        }
        else
        {
            if (mixedFlag)
            {
                mixedFlag = false;
                field.label = "Material";
            }
            field.value = value;
        }
    }

    private void SetIntegerFieldMixed(IntegerField field, bool showMixed, int value, ref bool mixedFlag)
    {
        if (field == null)
        {
            Debug.LogWarning("SetIntegerFieldMixed: IntegerField is null!");
            return;
        }

        if (showMixed)
        {
            if (!mixedFlag)
            {
                var textElement = field.Q(className: "unity-text-element__selectable");
                if (textElement == null)
                {
                    Debug.LogWarning("SetIntegerFieldMixed: Could not find text element in IntegerField!");
                    return;
                }

                if (textElement is TextElement te)
                {
                    te.text = "-";
                }
                else
                {
                    Debug.LogWarning($"SetIntegerFieldMixed: Element is not TextElement, it's {textElement.GetType().Name}!");
                    return;
                }

                mixedFlag = true;
            }
        }
        else
        {
            if (mixedFlag)
            {
                mixedFlag = false;
                // Restore the text element to show the actual value
                var textElement = field.Q(className: "unity-text-element__selectable");
                if (textElement is TextElement te)
                {
                    te.text = value.ToString();
                }
            }
            field.value = value;
        }
    }

    private void SetEnumFieldMixed(EnumField field, bool showMixed, System.Enum value, ref bool mixedFlag)
    {
        if (field == null)
        {
            Debug.LogWarning("SetEnumFieldMixed: EnumField is null!");
            return;
        }

        if (showMixed)
        {
            if (!mixedFlag)
            {
                // Clear the field value so no option appears selected in dropdown
                field.value = null;

                // Override the display text to show "-"
                var textElement = field.Q(className: "unity-enum-field__text");
                if (textElement == null)
                {
                    Debug.LogWarning("SetEnumFieldMixed: Could not find text element in EnumField!");
                    return;
                }

                if (textElement is TextElement te)
                {
                    te.text = "-";
                }
                else
                {
                    Debug.LogWarning($"SetEnumFieldMixed: Element is not TextElement, it's {textElement.GetType().Name}!");
                    return;
                }

                mixedFlag = true;
            }
        }
        else
        {
            if (mixedFlag)
            {
                mixedFlag = false;
            }
            field.value = value;
        }
    }

    private void ClearMixedStateOnFocus(VisualElement field, ref bool mixedFlag)
    {
        if (mixedFlag)
        {
            mixedFlag = false;
            // Field will get updated on next value change
        }
    }

    private (bool isInEditMode, string elementType, int selectionCount) GetProBuilderEditModeInfo()
    {
        if (ToolManager.activeContextType == typeof(GameObjectToolContext))
        {
            return (false, "Object", 0);
        }

        // Get current selection mode
        var selectMode = ProBuilderEditor.selectMode;
        var modeString = selectMode.ToString();

        // Get selection count using MeshSelection
        int selectionCount = GetProBuilderSelectionCount(modeString);
        return (true, modeString, selectionCount);
    }

    private int GetProBuilderSelectionCount(string mode)
    {
        switch (mode)
        {
            case "Face":
                return MeshSelection.selectedFaceCount;
                
            case "Edge":
                return MeshSelection.selectedEdgeCount;
                
            case "Vertex":
                return MeshSelection.selectedVertexCount;
                
            default:
                return 0;
        }
    }

    private void UpdateProBuilderStatus()
    {
        var editModeInfo = GetProBuilderEditModeInfo();

        // Check if we need to update
        bool needsUpdate = false;
        
        // Always update if element type or selection count changed
        if (_currentElementType != editModeInfo.elementType || _currentSelectionCount != editModeInfo.selectionCount)
        {
            needsUpdate = true;
        }
        
        // Check if the actual selected elements changed (even if count is the same)
        if (!needsUpdate)
        {
            needsUpdate = HasSelectionChanged();
        }
        
        if (needsUpdate)
        {
            _currentElementType = editModeInfo.elementType;
            _currentSelectionCount = editModeInfo.selectionCount;
            UpdateSelectedElementsCache();
            
            // Use delayed update to prevent rapid-fire updates during drag selection
            if (!_pendingUpdate)
            {
                _pendingUpdate = true;
                EditorApplication.delayCall += () =>
                {
                    _pendingUpdate = false;
                    if (this != null) // Check if overlay still exists
                    {
                        UpdateDisplay();
                    }
                };
            }
        }
    }

    private bool HasSelectionChanged()
    {
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return false;

        var currentSelectMode = ProBuilderEditor.selectMode;
        
        switch (currentSelectMode)
        {
            case SelectMode.Face:
                var currentFaceIndices = GetCurrentFaceIndices();
                return !currentFaceIndices.SetEquals(_lastSelectedFaceIndices);
                
            case SelectMode.Edge:
                var currentEdgeIndices = GetCurrentEdgeIndices();
                return !currentEdgeIndices.SetEquals(_lastSelectedEdgeIndices);
                
            case SelectMode.Vertex:
                var currentVertexIndices = GetCurrentVertexIndices();
                return !currentVertexIndices.SetEquals(_lastSelectedVertexIndices);
                
            default:
                return false;
        }
    }

    private void UpdateSelectedElementsCache()
    {
        var currentSelectMode = ProBuilderEditor.selectMode;
        
        switch (currentSelectMode)
        {
            case SelectMode.Face:
                _lastSelectedFaceIndices = GetCurrentFaceIndices();
                break;
                
            case SelectMode.Edge:
                _lastSelectedEdgeIndices = GetCurrentEdgeIndices();
                break;
                
            case SelectMode.Vertex:
                _lastSelectedVertexIndices = GetCurrentVertexIndices();
                break;
        }
    }

    private HashSet<int> GetCurrentFaceIndices()
    {
        var indices = new HashSet<int>();
        var selectedMeshes = MeshSelection.top.ToArray();
        
        foreach (var mesh in selectedMeshes)
        {
            var selectedFaces = mesh.GetSelectedFaces();
            if (selectedFaces != null)
            {
                var allFaces = mesh.faces.ToArray();
                for (int i = 0; i < allFaces.Length; i++)
                {
                    foreach (var selectedFace in selectedFaces)
                    {
                        if (allFaces[i] == selectedFace)
                        {
                            indices.Add(i);
                            break;
                        }
                    }
                }
            }
        }
        
        return indices;
    }

    private HashSet<int> GetCurrentEdgeIndices()
    {
        var indices = new HashSet<int>();
        var selectedMeshes = MeshSelection.top.ToArray();
        
        foreach (var mesh in selectedMeshes)
        {
            var selectedEdges = mesh.selectedEdges;
            if (selectedEdges != null)
            {
                foreach (var edge in selectedEdges)
                {
                    // Create a unique identifier for this edge using its vertex indices
                    int edgeId = edge.a * 1000000 + edge.b; // Simple hash for edge
                    indices.Add(edgeId);
                }
            }
        }
        
        return indices;
    }

    private HashSet<int> GetCurrentVertexIndices()
    {
        var indices = new HashSet<int>();
        var selectedMeshes = MeshSelection.top.ToArray();
        
        foreach (var mesh in selectedMeshes)
        {
            var selectedVertices = mesh.selectedVertices;
            if (selectedVertices != null)
            {
                foreach (int vertexIndex in selectedVertices)
                {
                    indices.Add(vertexIndex);
                }
            }
        }
        
        return indices;
    }

    private Color GetCurrentSelectionColor()
    {
        if (_selectedMeshes.Count == 0) return Color.white; // @todo - why is this here, if we are also doing MeshSelection.top just below?

        // Get color from actual ProBuilder selection using the correct APIs
        var selectedMeshes = MeshSelection.top.ToArray();
        if (selectedMeshes.Length == 0) return Color.white;

        var mesh = selectedMeshes[0];
        var vertices = mesh.GetVertices();
        if (vertices == null || vertices.Length == 0) return Color.white;

        Color vertexColor = Color.white;

        // Get current mode from ProBuilder
        var currentSelectMode = ProBuilderEditor.selectMode;

        switch (currentSelectMode)
        {
            case SelectMode.Face:
                var selectedFaces = mesh.GetSelectedFaces();
                if (selectedFaces != null && selectedFaces.Length > 0)
                {
                    var face = selectedFaces[0];
                    if (face.indexes.Count > 0 && face.indexes[0] < vertices.Length)
                        vertexColor = vertices[face.indexes[0]].color;
                }
                break;
                
            case SelectMode.Edge:
                var selectedEdges = mesh.selectedEdges;
                if (selectedEdges != null && selectedEdges.Count > 0)
                {
                    var edge = selectedEdges[0];
                    if (edge.a < vertices.Length)
                        vertexColor = vertices[edge.a].color;
                }
                break;
                
            case SelectMode.Vertex:
                var selectedVertices = mesh.selectedVertices;
                if (selectedVertices != null && selectedVertices.Count > 0)
                {
                    int vertexIndex = selectedVertices[0];
                    if (vertexIndex < vertices.Length)
                        vertexColor = vertices[vertexIndex].color;
                }
                break;
        }

        // Convert from linear to gamma space for display
        return vertexColor.gamma;
    }

    private void OnSelectionChanged()
    {
        UpdateProBuilderStatus();
    }

    private void OnSelectModeChanged(SelectMode mode)
    {
        UpdateProBuilderStatus();
    }

    private void OnProBuilderSelectionUpdated(System.Collections.Generic.IEnumerable<ProBuilderMesh> selection)
    {
        UpdateProBuilderStatus();
    }

    private void OnActiveContextChanged()
    {
        UpdateProBuilderStatus();
    }
    }
}
