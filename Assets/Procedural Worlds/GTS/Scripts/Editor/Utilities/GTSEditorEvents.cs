using Object = UnityEngine.Object;
namespace ProceduralWorlds.GTS
{
    public static class GTSEditorEvents
    {
        private static void EditorDestroy(Object @object)
        {
            Object.DestroyImmediate(@object);
        }
        public static void Subscribe()
        {
            GTSEvents.Destroy = EditorDestroy;
        }
    }
}