using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    public static class GTSMenuItems
    {
        [MenuItem("Window/Procedural Worlds/GTS/GTS Manager...", priority = 0)]
        public static void OpenGTSManager()
        {
            GTSManagerEditorWindow win = EditorWindow.GetWindow<GTSManagerEditorWindow>();
            win.titleContent = new GUIContent("GTS Manager");
            win.minSize = new Vector2(300f, 300f);
            win.Show();
        }
        [MenuItem("Window/Procedural Worlds/GTS/GTS Maintenance...", priority = 1)]
        public static void PerformGTSMaintenance()
        {
            GTSEditorUtility.PerformMaintenance();
        }
    }
}