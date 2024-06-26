using System;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSGlobalSettings : GTSProfileSettings
    {
        public GTSUVTarget uvTarget = GTSUVTarget.WorldAligned;
        public GTSTargetPlatform targetPlatform = GTSTargetPlatform.Desktop;
        public float blendDistance = 0.1f;
        public float blendRange = 1f;
        public override void Reset()
        {
            base.Reset();
            blendDistance = 0.1f;
            blendRange = 1f;
        }
    }
}