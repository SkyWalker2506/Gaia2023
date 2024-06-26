using System;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSHeightSettings : GTSProfileSettings
    {
        public float blendFactor = 0.9f;
        public override void Reset()
        {
            base.Reset();
            blendFactor = 0.9f;
        }
    }
}