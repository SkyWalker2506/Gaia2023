using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSGeoSettings : GTSProfileSettings
    {
        public Texture2D albedoTexture;
        public Texture2D normalTexture;
        public float nearStrength = 0.2f;
        public float nearNormalStrength = 0.5f;
        public float nearScale = 50f;
        public float nearOffset = 0f;
        public float farStrength = 0.2f;
        public float farNormalStrength = 0.5f;
        public float farScale = 200f;
        public float farOffset = 0;
        public bool objectSpace = false;
        public override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            albedoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(isLowPoly ? GTSConstants.LowPolyGeoAlbedoTexturePath : GTSConstants.GeoAlbedoTexturePath);
            normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(isLowPoly ? GTSConstants.LowPolyGeoNormalTexturePath : GTSConstants.GeoNormalTexturePath);
#endif
            nearStrength = 0.2f;
            nearNormalStrength = 0.5f;
            nearScale = 50f;
            nearOffset = 0f;
            farStrength = 0.2f;
            farNormalStrength = 0.5f;
            farScale = 200f;
            farOffset = 0;
            objectSpace = false;
    }
    }
}