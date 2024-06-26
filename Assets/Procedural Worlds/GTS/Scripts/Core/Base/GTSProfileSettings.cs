using System;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public abstract class GTSProfileSettings
    {
        public bool enabled = false;
        public bool isExpanded = false;
        public virtual void Reset()
        {
            enabled = false;
            isExpanded = false;
        }
#if UNITY_EDITOR
        [NonSerialized] public bool isLowPoly = false;
#endif
    }
}