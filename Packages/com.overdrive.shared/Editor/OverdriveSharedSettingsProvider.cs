using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

    public class OverdriveSharedSettingsProvider:SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateOverdriveSharedSettingsProvider()
        {
            return new OverdriveSharedSettingsProvider("Project/Overdrive",SettingsScope.Project)
            {
                keywords = new HashSet<string>(new[] { "Overdrive","Scene","Discard","Child","Order","Blocks","Todo","Clear","EasyEdit" })
            };
        }

        private OverdriveSharedSettingsProvider(string path,SettingsScope scope) : base(path,scope) { }

        public override void OnActivate(string searchContext,VisualElement rootElement)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/OverdriveShared_ProjectSettings");

            var container = visualTree.CloneTree();

            var pinsBtn = container.Q<Button>("Btn-Web-Pins");
            if(pinsBtn != null)
            {
                pinsBtn.clicked += () =>
                {
                    Application.OpenURL("https://www.overdrivetoolset.com/pins");
                };
            }
            else
            {
                Debug.LogError("Pins button not found in OverdriveSharedSettingsProvider UI.");
            }

            var blocksBtn = container.Q<Button>("Btn-Web-SceneBlocks");
            if(blocksBtn != null)
            {
                blocksBtn.clicked += () =>
                {
                    Application.OpenURL("https://www.overdrivetoolset.com/sceneblocks");
                };
            }
            else
            {
                Debug.LogError("Blocks button not found in OverdriveSharedSettingsProvider UI.");
            }

            var multiverseBtn = container.Q<Button>("Btn-Web-Multiverse");
            if(multiverseBtn != null)
            {
                multiverseBtn.clicked += () =>
                {
                    Application.OpenURL("https://www.overdrivetoolset.com/multiverse");
                };
            }
            else
            {
                Debug.LogError("Multiverse button not found in OverdriveSharedSettingsProvider UI.");
            }

            var collectionsBtn = container.Q<Button>("Btn-Web-Collections");
            if(collectionsBtn != null)
            {
                collectionsBtn.clicked += () =>
                {
                    Application.OpenURL("https://www.overdrivetoolset.com/collections");
                };
            }
            else
            {
                Debug.LogError("Collections button not found in OverdriveSharedSettingsProvider UI.");
            }

            var todoBtn = container.Q<Button>("Btn-Web-Todo");
            if(todoBtn != null)
            {
                todoBtn.clicked += () =>
                {
                    Application.OpenURL("https://www.overdrivetoolset.com/todo");
                };
            }
            else
            {
                Debug.LogError("Todo button not found in OverdriveSharedSettingsProvider UI.");
            }

        //ProBuilder Plus
        var proBuilderBtn = container.Q<Button>("Btn-Web-ProBuilderPlus");
        if(proBuilderBtn != null)
        {
            proBuilderBtn.clicked += () =>
            {
                Application.OpenURL("https://www.overdrivetoolset.com/probuilder-plus");
            };
        }
        else
        {
            Debug.LogError("ProBuilder Plus button not found in OverdriveSharedSettingsProvider UI.");
        }

            var easyEditBtn = container.Q<Button>("Btn-Web-EasyEdit");
            if(easyEditBtn != null)
            {
                easyEditBtn.clicked += () =>
                {
                    Application.OpenURL("https://www.overdrivetoolset.com/easyedit");
                };
            }
            else
            {
                Debug.LogError("EasyEdit button not found in OverdriveSharedSettingsProvider UI.");
            }

            rootElement.Add(container);
        }
    }
