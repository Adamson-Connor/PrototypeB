using UnityEditor;
using UnityEngine;
using UnityEditor.EditorTools;

namespace Overdrive.ProBuilderPlus
{
    public static class ProBuilderPlusCustomActions
    {
        [MenuItem("Tools/Overdrive Actions/Enter GameObject Mode")]
        public static void EnterGameObjectMode()
        {
            Debug.Log("[PBPlus] Enter GameObject Mode - simulating shortcut: Tools/Enter GameObject Mode");
            Overdrive.ShortcutSimulator.SimulateShortcut("Tools/Enter GameObject Mode");
        }

        [MenuItem("Tools/Overdrive Actions/Edit Vertices")]
        public static void EditVertices()
        {
            // If in GameObject context, need to switch to ProBuilder Edit Mode first
            if (UnityEditor.EditorTools.ToolManager.activeContextType == typeof(UnityEditor.EditorTools.GameObjectToolContext))
            {
                Debug.Log("[PBPlus] Not in ProBuilder Edit Mode, switching first...");
                Overdrive.ShortcutSimulator.SimulateShortcut("Tools/Cycle Tool Modes");
            }
            Debug.Log("[PBPlus] Setting ProBuilder select mode to Vertex");
            UnityEditor.ProBuilder.ProBuilderEditor.selectMode = UnityEngine.ProBuilder.SelectMode.Vertex;
        }

        [MenuItem("Tools/Overdrive Actions/Edit Edges")]
        public static void EditEdges()
        {
            // If in GameObject context, need to switch to ProBuilder Edit Mode first
            if (UnityEditor.EditorTools.ToolManager.activeContextType == typeof(UnityEditor.EditorTools.GameObjectToolContext))
            {
                Debug.Log("[PBPlus] Not in ProBuilder Edit Mode, switching first...");
                Overdrive.ShortcutSimulator.SimulateShortcut("Tools/Cycle Tool Modes");
            }
            Debug.Log("[PBPlus] Setting ProBuilder select mode to Edge");
            UnityEditor.ProBuilder.ProBuilderEditor.selectMode = UnityEngine.ProBuilder.SelectMode.Edge;
        }

        [MenuItem("Tools/Overdrive Actions/Edit Faces")]
        public static void EditFaces()
        {
            // If in GameObject context, need to switch to ProBuilder Edit Mode first
            if (UnityEditor.EditorTools.ToolManager.activeContextType == typeof(UnityEditor.EditorTools.GameObjectToolContext))
            {
                Debug.Log("[PBPlus] Not in ProBuilder Edit Mode, switching first...");
                Overdrive.ShortcutSimulator.SimulateShortcut("Tools/Cycle Tool Modes");
            }
            Debug.Log("[PBPlus] Setting ProBuilder select mode to Face");
            UnityEditor.ProBuilder.ProBuilderEditor.selectMode = UnityEngine.ProBuilder.SelectMode.Face;
        }
    }
}
