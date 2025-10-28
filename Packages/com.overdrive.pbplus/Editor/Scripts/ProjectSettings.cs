using System;
using UnityEditor;
using UnityEngine;

namespace Overdrive.ProBuilderPlus
{
    [FilePath("Library/ProBuilderPlusData/ProBuilderPlusSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ProjectSettings : ScriptableSingleton<ProjectSettings>
    {
        private void OnEnable()
        {
        }

        public static void Save()
        {
            string filePath = GetFilePath();
            string directory = System.IO.Path.GetDirectoryName(filePath);

            instance.Save(true);
        }
    }
}