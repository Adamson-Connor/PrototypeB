using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Overdrive.ProBuilderPlus;

namespace Overdrive.ProBuilderPlus
{
    internal static class UserPreferencesProvider
    {
        private static Button moreInfoButton;
        private static Toggle collapseToFirstToggle;
        private static FloatField weldDistanceField;
        private static FloatField extrudeDistanceField;
        private static Toggle extrudeAsGroupToggle;
        private static FloatField loopPositionField;
        private static EnumField loopDirectionField;
        private static EnumField loopModeField;
        private static IntegerField bridgeRotationOffsetField;
        private static Toggle bridgeReverseOrderToggle;
        private static Toggle bridgeUseFullBordersToggle;
        private static FloatField connectionPositionField;
        private static EnumField connectionDirectionField;
        private static EnumField connectionModeField;
        private static FloatField extrudeFacesDistanceField;
        private static EnumField extrudeFacesMethodField;
        private static EnumField extrudeFacesSpaceField;
        private static EnumField extrudeFacesAxisField;
        private static FloatField insetFacesDistanceField;
        private static Toggle conformNormalsOtherDirectionToggle;

        [SettingsProvider]
        public static SettingsProvider CreateProBuilderPlusUserPreferencesProvider()
        {
            SettingsProvider provider = new SettingsProvider("Preferences/Overdrive/ProBuilder Plus", SettingsScope.User)
            {
                label = "ProBuilder Plus",
                activateHandler = (searchContext, rootElement) =>
                {
                    VisualTreeAsset settings = Resources.Load<VisualTreeAsset>("UXML/ProBuilderPlus_UserPreferences");

                    if (settings != null)
                    {
                        TemplateContainer settingsContainer = settings.Instantiate();

                        // Setup UI elements and bind to preferences
                        SetupUI(settingsContainer);

                        rootElement.Add(settingsContainer);
                    }
                    else
                    {
                        Debug.LogError("UserPreferencesProvider: Could not load ProBuilderPlus_UserPreferences.uxml from Resources");
                        var errorLabel = new Label("ProBuilderPlus_UserPreferences.uxml not found");
                        errorLabel.style.color = Color.red;
                        rootElement.Add(errorLabel);
                    }
                },
                deactivateHandler = OnDeactivate,
                keywords = new HashSet<string>(new[] { "ProBuilder", "ProBuilder Plus", "vertex", "collapse", "weld", "preferences" })
            };

            return provider;
        }

        private static void SetupUI(TemplateContainer container)
        {
            // Setup Collapse to First toggle
            collapseToFirstToggle = container.Q<Toggle>("CollapseToFirst");
            if (collapseToFirstToggle != null)
            {
                collapseToFirstToggle.SetValueWithoutNotify(UserPreferences.CollapseToFirst);
                collapseToFirstToggle.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.CollapseToFirst = evt.newValue;
                });
            }

            // Setup Weld Distance field
            weldDistanceField = container.Q<FloatField>("WeldDistance");
            if (weldDistanceField != null)
            {
                weldDistanceField.SetValueWithoutNotify(UserPreferences.WeldDistance);
                weldDistanceField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.WeldDistance = Mathf.Max(0.00001f, evt.newValue);
                });
            }

            // Setup Extrude Distance field
            extrudeDistanceField = container.Q<FloatField>("ExtrudeDistance");
            if (extrudeDistanceField != null)
            {
                extrudeDistanceField.SetValueWithoutNotify(UserPreferences.ExtrudeDistance);
                extrudeDistanceField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ExtrudeDistance = evt.newValue;
                });
            }

            // Setup Extrude As Group toggle
            extrudeAsGroupToggle = container.Q<Toggle>("ExtrudeAsGroup");
            if (extrudeAsGroupToggle != null)
            {
                extrudeAsGroupToggle.SetValueWithoutNotify(UserPreferences.ExtrudeAsGroup);
                extrudeAsGroupToggle.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ExtrudeAsGroup = evt.newValue;
                });
            }

            // Setup Loop Position field
            loopPositionField = container.Q<FloatField>("LoopPosition");
            if (loopPositionField != null)
            {
                loopPositionField.SetValueWithoutNotify(UserPreferences.LoopPosition);
                loopPositionField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.LoopPosition = evt.newValue;
                });
            }

            // Setup Loop Direction field
            loopDirectionField = container.Q<EnumField>("LoopDirection");
            if (loopDirectionField != null)
            {
                loopDirectionField.Init((InsertEdgeLoopPreviewAction.ConnectionDirection)UserPreferences.LoopDirection);
                loopDirectionField.SetValueWithoutNotify((InsertEdgeLoopPreviewAction.ConnectionDirection)UserPreferences.LoopDirection);
                loopDirectionField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.LoopDirection = (int)(InsertEdgeLoopPreviewAction.ConnectionDirection)evt.newValue;
                });
            }

            // Setup Loop Mode field
            loopModeField = container.Q<EnumField>("LoopMode");
            if (loopModeField != null)
            {
                loopModeField.Init((InsertEdgeLoopPreviewAction.ConnectionMode)UserPreferences.LoopMode);
                loopModeField.SetValueWithoutNotify((InsertEdgeLoopPreviewAction.ConnectionMode)UserPreferences.LoopMode);
                loopModeField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.LoopMode = (int)(InsertEdgeLoopPreviewAction.ConnectionMode)evt.newValue;
                });
            }

            // Setup Bridge Rotation Offset field
            bridgeRotationOffsetField = container.Q<IntegerField>("BridgeRotationOffset");
            if (bridgeRotationOffsetField != null)
            {
                bridgeRotationOffsetField.SetValueWithoutNotify(UserPreferences.BridgeRotationOffset);
                bridgeRotationOffsetField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.BridgeRotationOffset = evt.newValue;
                });
            }

            // Setup Bridge Reverse Order toggle
            bridgeReverseOrderToggle = container.Q<Toggle>("BridgeReverseOrder");
            if (bridgeReverseOrderToggle != null)
            {
                bridgeReverseOrderToggle.SetValueWithoutNotify(UserPreferences.BridgeReverseOrder);
                bridgeReverseOrderToggle.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.BridgeReverseOrder = evt.newValue;
                });
            }

            // Setup Bridge Use Full Borders toggle
            bridgeUseFullBordersToggle = container.Q<Toggle>("BridgeUseFullBorders");
            if (bridgeUseFullBordersToggle != null)
            {
                bridgeUseFullBordersToggle.SetValueWithoutNotify(UserPreferences.BridgeUseFullBorders);
                bridgeUseFullBordersToggle.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.BridgeUseFullBorders = evt.newValue;
                });
            }

            // Setup Connection Position field
            connectionPositionField = container.Q<FloatField>("ConnectionPosition");
            if (connectionPositionField != null)
            {
                connectionPositionField.SetValueWithoutNotify(UserPreferences.ConnectionPosition);
                connectionPositionField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ConnectionPosition = evt.newValue;
                });
            }

            // Setup Connection Direction field
            connectionDirectionField = container.Q<EnumField>("ConnectionDirection");
            if (connectionDirectionField != null)
            {
                connectionDirectionField.Init((ConnectEdgesPreviewAction.ConnectionDirection)UserPreferences.ConnectionDirection);
                connectionDirectionField.SetValueWithoutNotify((ConnectEdgesPreviewAction.ConnectionDirection)UserPreferences.ConnectionDirection);
                connectionDirectionField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ConnectionDirection = (int)(ConnectEdgesPreviewAction.ConnectionDirection)evt.newValue;
                });
            }

            // Setup Connection Mode field
            connectionModeField = container.Q<EnumField>("ConnectionMode");
            if (connectionModeField != null)
            {
                connectionModeField.Init((ConnectEdgesPreviewAction.ConnectionMode)UserPreferences.ConnectionMode);
                connectionModeField.SetValueWithoutNotify((ConnectEdgesPreviewAction.ConnectionMode)UserPreferences.ConnectionMode);
                connectionModeField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ConnectionMode = (int)(ConnectEdgesPreviewAction.ConnectionMode)evt.newValue;
                });
            }

            // === FACE ACTIONS ===

            // Setup Extrude Faces Method field
            var extrudeFacesMethodField = container.Q<EnumField>("ExtrudeFacesMethod");
            if (extrudeFacesMethodField != null)
            {
                extrudeFacesMethodField.Init((CustomExtrudeMethod)UserPreferences.ExtrudeFacesMethod);
                extrudeFacesMethodField.SetValueWithoutNotify((CustomExtrudeMethod)UserPreferences.ExtrudeFacesMethod);
                extrudeFacesMethodField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ExtrudeFacesMethod = (int)(CustomExtrudeMethod)evt.newValue;
                });
            }

            // Setup Extrude Faces Space field
            var extrudeFacesSpaceField = container.Q<EnumField>("ExtrudeFacesSpace");
            if (extrudeFacesSpaceField != null)
            {
                extrudeFacesSpaceField.Init((ExtrudeSpace)UserPreferences.ExtrudeFacesSpace);
                extrudeFacesSpaceField.SetValueWithoutNotify((ExtrudeSpace)UserPreferences.ExtrudeFacesSpace);
                extrudeFacesSpaceField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ExtrudeFacesSpace = (int)(ExtrudeSpace)evt.newValue;
                });
            }

            // Setup Extrude Faces Axis field
            var extrudeFacesAxisField = container.Q<EnumField>("ExtrudeFacesAxis");
            if (extrudeFacesAxisField != null)
            {
                extrudeFacesAxisField.Init((ExtrudeAxis)UserPreferences.ExtrudeFacesAxis);
                extrudeFacesAxisField.SetValueWithoutNotify((ExtrudeAxis)UserPreferences.ExtrudeFacesAxis);
                extrudeFacesAxisField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ExtrudeFacesAxis = (int)(ExtrudeAxis)evt.newValue;
                });
            }

            // === SHARED ACTIONS ===

            // Setup Remove Extrude Distance field
            var removeExtrudeDistanceField = container.Q<FloatField>("RemoveExtrudeDistance");
            if (removeExtrudeDistanceField != null)
            {
                removeExtrudeDistanceField.SetValueWithoutNotify(UserPreferences.RemoveExtrudeDistance);
                removeExtrudeDistanceField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.RemoveExtrudeDistance = evt.newValue;
                });
            }

            // Setup Fill Entire Path field
            var fillEntirePathField = container.Q<Toggle>("FillEntirePath");
            if (fillEntirePathField != null)
            {
                fillEntirePathField.SetValueWithoutNotify(UserPreferences.FillEntirePath);
                fillEntirePathField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.FillEntirePath = evt.newValue;
                });
            }

            // Setup Bevel Distance field
            var bevelDistanceField = container.Q<FloatField>("BevelDistance");
            if (bevelDistanceField != null)
            {
                bevelDistanceField.SetValueWithoutNotify(UserPreferences.BevelDistance);
                bevelDistanceField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.BevelDistance = evt.newValue;
                });
            }

            // Setup Bevel Perimeter Only field
            var bevelPerimeterOnlyField = container.Q<Toggle>("BevelPerimeterOnly");
            if (bevelPerimeterOnlyField != null)
            {
                bevelPerimeterOnlyField.SetValueWithoutNotify(UserPreferences.BevelPerimeterOnly);
                bevelPerimeterOnlyField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.BevelPerimeterOnly = evt.newValue;
                });
            }

            // Setup Offset Coordinate Space field
            var offsetCoordinateSpaceField = container.Q<EnumField>("OffsetCoordinateSpace");
            if (offsetCoordinateSpaceField != null)
            {
                offsetCoordinateSpaceField.Init((OffsetElementsPreviewActionBase.CoordinateSpace)UserPreferences.OffsetCoordinateSpace);
                offsetCoordinateSpaceField.SetValueWithoutNotify((OffsetElementsPreviewActionBase.CoordinateSpace)UserPreferences.OffsetCoordinateSpace);
                offsetCoordinateSpaceField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.OffsetCoordinateSpace = (int)(OffsetElementsPreviewActionBase.CoordinateSpace)evt.newValue;
                });
            }

            // Setup Offset Vector field
            var offsetVectorField = container.Q<Vector3Field>("OffsetVector");
            if (offsetVectorField != null)
            {
                offsetVectorField.SetValueWithoutNotify(UserPreferences.OffsetVector);
                offsetVectorField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.OffsetVector = evt.newValue;
                });
            }

            // === OBJECT ACTIONS ===

            // Setup Mirror Objects fields
            var mirrorXField = container.Q<Toggle>("MirrorX");
            if (mirrorXField != null)
            {
                mirrorXField.SetValueWithoutNotify((UserPreferences.MirrorSettings & 1) != 0);
                mirrorXField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        UserPreferences.MirrorSettings |= 1;
                    else
                        UserPreferences.MirrorSettings &= ~1;
                });
            }

            var mirrorYField = container.Q<Toggle>("MirrorY");
            if (mirrorYField != null)
            {
                mirrorYField.SetValueWithoutNotify((UserPreferences.MirrorSettings & 2) != 0);
                mirrorYField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        UserPreferences.MirrorSettings |= 2;
                    else
                        UserPreferences.MirrorSettings &= ~2;
                });
            }

            var mirrorZField = container.Q<Toggle>("MirrorZ");
            if (mirrorZField != null)
            {
                mirrorZField.SetValueWithoutNotify((UserPreferences.MirrorSettings & 4) != 0);
                mirrorZField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        UserPreferences.MirrorSettings |= 4;
                    else
                        UserPreferences.MirrorSettings &= ~4;
                });
            }

            var mirrorDuplicateField = container.Q<Toggle>("MirrorDuplicate");
            if (mirrorDuplicateField != null)
            {
                mirrorDuplicateField.SetValueWithoutNotify((UserPreferences.MirrorSettings & 8) != 0);
                mirrorDuplicateField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        UserPreferences.MirrorSettings |= 8;
                    else
                        UserPreferences.MirrorSettings &= ~8;
                });
            }

            // Setup Apply Transform fields
            var applyPositionField = container.Q<Toggle>("ApplyPosition");
            if (applyPositionField != null)
            {
                applyPositionField.SetValueWithoutNotify(UserPreferences.ApplyPosition);
                applyPositionField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ApplyPosition = evt.newValue;
                });
            }

            var applyRotationField = container.Q<Toggle>("ApplyRotation");
            if (applyRotationField != null)
            {
                applyRotationField.SetValueWithoutNotify(UserPreferences.ApplyRotation);
                applyRotationField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ApplyRotation = evt.newValue;
                });
            }

            var applyScaleField = container.Q<Toggle>("ApplyScale");
            if (applyScaleField != null)
            {
                applyScaleField.SetValueWithoutNotify(UserPreferences.ApplyScale);
                applyScaleField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ApplyScale = evt.newValue;
                });
            }

            // Setup Conform Object Normals field
            var conformObjectNormalsOtherDirectionField = container.Q<Toggle>("ConformObjectNormalsOtherDirection");
            if (conformObjectNormalsOtherDirectionField != null)
            {
                conformObjectNormalsOtherDirectionField.SetValueWithoutNotify(UserPreferences.ConformObjectNormalsOtherDirection);
                conformObjectNormalsOtherDirectionField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ConformObjectNormalsOtherDirection = evt.newValue;
                });
            }

            // Setup ProBuilderize fields
            var importQuadsField = container.Q<Toggle>("ImportQuads");
            if (importQuadsField != null)
            {
                importQuadsField.SetValueWithoutNotify(UserPreferences.ImportQuads);
                importQuadsField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ImportQuads = evt.newValue;
                });
            }

            var importSmoothingField = container.Q<Toggle>("ImportSmoothing");
            if (importSmoothingField != null)
            {
                importSmoothingField.SetValueWithoutNotify(UserPreferences.ImportSmoothing);
                importSmoothingField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ImportSmoothing = evt.newValue;
                });
            }

            var smoothingAngleField = container.Q<FloatField>("SmoothingAngle");
            if (smoothingAngleField != null)
            {
                smoothingAngleField.SetValueWithoutNotify(UserPreferences.SmoothingAngle);
                smoothingAngleField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.SmoothingAngle = evt.newValue;
                });
            }

            // Setup UI Settings
            var showEditModeButtonsField = container.Q<Toggle>("ShowEditModeButtons");
            if (showEditModeButtonsField != null)
            {
                showEditModeButtonsField.SetValueWithoutNotify(UserPreferences.ShowEditModeButtons);
                showEditModeButtonsField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ShowEditModeButtons = evt.newValue;
                });
            }
            
            var showEditorButtonsField = container.Q<Toggle>("ShowEditorButtons");
            if (showEditorButtonsField != null)
            {
                showEditorButtonsField.SetValueWithoutNotify(UserPreferences.ShowEditorButtons);
                showEditorButtonsField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ShowEditorButtons = evt.newValue;
                });
            }
            
            var showActionButtonsField = container.Q<Toggle>("ShowActionButtons");
            if (showActionButtonsField != null)
            {
                showActionButtonsField.SetValueWithoutNotify(UserPreferences.ShowActionButtons);
                showActionButtonsField.RegisterValueChangedCallback(evt =>
                {
                    UserPreferences.ShowActionButtons = evt.newValue;
                });
            }
            
            // Setup more info button
            moreInfoButton = container.Q<Button>("Btn_Collections-Web");
            if (moreInfoButton != null)
            {
                moreInfoButton.clicked += () =>
                {
                    Application.OpenURL("https://overdrivetoolset.com/probuilder-plus");
                };
            }
        }

        private static void OnDeactivate()
        {
            moreInfoButton = null;
            collapseToFirstToggle = null;
            weldDistanceField = null;
            extrudeDistanceField = null;
            extrudeAsGroupToggle = null;
            loopPositionField = null;
            loopDirectionField = null;
            loopModeField = null;
            bridgeRotationOffsetField = null;
            bridgeReverseOrderToggle = null;
            bridgeUseFullBordersToggle = null;
            connectionPositionField = null;
            connectionDirectionField = null;
            connectionModeField = null;
        }
    }
}