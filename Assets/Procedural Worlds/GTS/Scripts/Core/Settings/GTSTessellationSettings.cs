using System;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSTessellationSettings : GTSProfileSettings
    {
        public float multiplier = 1f;
        public float minDistance = 5f;
        public float maxDistance = 25f;
        public override void Reset()
        {
            base.Reset();
            multiplier = 1f;
            minDistance = 5f;
            maxDistance = 25f;
        }
    }
}