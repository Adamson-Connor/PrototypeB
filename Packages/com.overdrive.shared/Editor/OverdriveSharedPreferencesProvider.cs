using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// OverdriveSharedPreferencesProvider provides a custom settings provider for the Overdrive toolset user preferences.
/// It provides an overview of all Overdrive tools and links to more information for each.
/// </summary>

internal static class OverdriveSharedPreferencesProvider
{
    private static Button pinsButton;
    private static Button sceneBlocksButton;
    private static Button multiverseButton;
    private static Button collectionsButton;
    private static Button todoButton;
    private static Button proBuilderButton;
    private static Button easyEditButton;

    [SettingsProvider]
    public static SettingsProvider CreateOverdriveSharedPreferencesProvider()
    {
        SettingsProvider provider = new SettingsProvider("Preferences/Overdrive", SettingsScope.User)
        {
            label = "Overdrive",
            activateHandler = (searchContext, rootElement) =>
            {
                VisualTreeAsset settings = Resources.Load<VisualTreeAsset>("UI/OverdriveShared_UserPreferences");

                if (settings != null)
                {
                    TemplateContainer settingsContainer = settings.Instantiate();

                    // Setup Pins button
                    pinsButton = settingsContainer.Q<Button>("Btn-Web-Pins");
                    if (pinsButton != null)
                    {
                        pinsButton.clicked += () =>
                        {
                            Application.OpenURL("https://www.overdrivetoolset.com/pins");
                        };
                    }
                    else
                    {
                        Debug.LogError("OverdriveSharedPreferencesProvider: Pins button not found in UI.");
                    }

                    // Setup Scene Blocks button
                    sceneBlocksButton = settingsContainer.Q<Button>("Btn-Web-SceneBlocks");
                    if (sceneBlocksButton != null)
                    {
                        sceneBlocksButton.clicked += () =>
                        {
                            Application.OpenURL("https://www.overdrivetoolset.com/sceneblocks");
                        };
                    }
                    else
                    {
                        Debug.LogError("OverdriveSharedPreferencesProvider: Scene Blocks button not found in UI.");
                    }

                    // Setup Multiverse button
                    multiverseButton = settingsContainer.Q<Button>("Btn-Web-Multiverse");
                    if (multiverseButton != null)
                    {
                        multiverseButton.clicked += () =>
                        {
                            Application.OpenURL("https://www.overdrivetoolset.com/multiverse");
                        };
                    }
                    else
                    {
                        Debug.LogError("OverdriveSharedPreferencesProvider: Multiverse button not found in UI.");
                    }

                    // Setup Collections button
                    collectionsButton = settingsContainer.Q<Button>("Btn-Web-Collections");
                    if (collectionsButton != null)
                    {
                        collectionsButton.clicked += () =>
                        {
                            Application.OpenURL("https://www.overdrivetoolset.com/collections");
                        };
                    }
                    else
                    {
                        Debug.LogError("OverdriveSharedPreferencesProvider: Collections button not found in UI.");
                    }

                    // Setup Todo button
                    todoButton = settingsContainer.Q<Button>("Btn-Web-Todo");
                    if (todoButton != null)
                    {
                        todoButton.clicked += () =>
                        {
                            Application.OpenURL("https://www.overdrivetoolset.com/todo");
                        };
                    }
                    else
                    {
                        Debug.LogError("OverdriveSharedPreferencesProvider: Todo button not found in UI.");
                    }

                    // Setup ProBuilder Plus button
                    proBuilderButton = settingsContainer.Q<Button>("Btn-Web-ProBuilderPlus");
                    if (proBuilderButton != null)
                    {
                        proBuilderButton.clicked += () =>
                        {
                            Application.OpenURL("https://www.overdrivetoolset.com/probuilder-plus");
                        };
                    }
                    else
                    {
                        Debug.LogError("OverdriveSharedPreferencesProvider: ProBuilder Plus button not found in UI.");
                    }

                    // Setup EasyEdit button
                    easyEditButton = settingsContainer.Q<Button>("Btn-Web-EasyEdit");
                    if (easyEditButton != null)
                    {
                        easyEditButton.clicked += () =>
                        {
                            Application.OpenURL("https://www.overdrivetoolset.com/easyedit");
                        };
                    }
                    else
                    {
                        Debug.LogError("OverdriveSharedPreferencesProvider: EasyEdit button not found in UI.");
                    }

                    rootElement.Add(settingsContainer);
                }
                else
                {
                    Debug.LogError("OverdriveSharedPreferencesProvider: Could not load OverdriveShared_UserPreferences.uxml");
                    var errorLabel = new Label("OverdriveShared_UserPreferences.uxml not found");
                    errorLabel.style.color = Color.red;
                    rootElement.Add(errorLabel);
                }
            },
            deactivateHandler = OnDeactivate,
            keywords = new HashSet<string>(new[] { "Overdrive", "Pins", "Scene Blocks", "Multiverse", "Collections", "Todo", "EasyEdit", "toolset" })
        };

        return provider;
    }

    private static void OnDeactivate()
    {
        pinsButton = null;
        sceneBlocksButton = null;
        multiverseButton = null;
        collectionsButton = null;
        todoButton = null;
        easyEditButton = null;
    }
}