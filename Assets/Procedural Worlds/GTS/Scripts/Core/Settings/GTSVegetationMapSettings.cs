using System;
using UnityEngine;

namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSVegetationMapSettings : GTSProfileSettings
    {
        public bool useFloraColormap = false;
        public Texture colorMapTex;

        public Texture colorMapNormalTex;

        public float alphaIntensity = 2f;
        public float colorIntensity = 0.5f;

        public float nearIntensity = 0.5f;
        public float farIntensity = 1f;

        public float nearNormalIntensity = 0.5f;
        public float farNormalIntensity = 0.6f;

        public override void Reset()
        {
            base.Reset();
            alphaIntensity = 2f;
            colorIntensity = 0.5f;

            nearIntensity = 0.5f;
            farIntensity = 1f;

            nearNormalIntensity = 0.5f;
            farNormalIntensity = 0.6f;
        }
    }
}