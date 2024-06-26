using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    [CreateAssetMenu(fileName = "GTS Manager", menuName = "Procedural Worlds/GTS/Manager", order = -1)]
    public class GTSManager : ScriptableObject
    {
        public GTSProfile profile;
        public void Reset()
        {
            profile = null;
        }
    }
}