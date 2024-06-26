using System;
using UnityEngine;

namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSColorMapSettings : GTSProfileSettings
    {
        public Texture colorMapTex;
        public Texture colorMapNormalTex;

        public float alphaIntensity = 1f;
        public float colorIntensity = 1f;

        public float nearIntensity = 1f;
        public float farIntensity = 1f;

        public float normalScale = 40f;

        public float nearNormalIntensity = 1f;
        public float farNormalIntensity = 1f;

        public override void Reset()
        {
            base.Reset();
            alphaIntensity = 1f;
            colorIntensity = 1f;

            nearIntensity = 1f;
            farIntensity = 1f;

            normalScale = 40f;

            nearNormalIntensity = 1f;
            farNormalIntensity = 1f;
        }
    }
}