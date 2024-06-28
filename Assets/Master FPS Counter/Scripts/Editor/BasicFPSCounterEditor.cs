using UnityEditor;

namespace Akila.MasterFPSCounter
{
    [CustomEditor(typeof(BasicFPSCounter)), CanEditMultipleObjects]
    public class BasicFPSCounterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The basic FPS counter uses the simplest form of FPS counters with a single component; if you want a more advanced FPS counter, use the info container instead.", MessageType.Info);
        }
    }
}