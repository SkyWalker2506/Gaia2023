using UnityEngine;
namespace ProceduralWorlds.GTS
{
    public class GTSIndexSettings : GTSProfileSettings
    {
        public Material weightSplatIndexMat;
        public float blurDistance = 0.001f;
        public float blurSteps = 64.0f;
        public override void Reset()
        {
            base.Reset();
            blurDistance = 0.001f;
            blurSteps = 64.0f;
        }
    }
}