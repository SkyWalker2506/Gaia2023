using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSDetailSettings : GTSProfileSettings
    {
        public Texture2D normalTexture;
        public float nearTiling = 100f;
        public float nearStrength = 0.6f;
        public float farTiling = 1000f;
        public float farStrength = 0.8f;
        public bool objectSpace = false;
        public override void Reset()
        {
            base.Reset();
            enabled = true;
#if UNITY_EDITOR
            normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(isLowPoly ? GTSConstants.LowPolyDetailNormalTexturePath : GTSConstants.DetailNormal3TexturePath);
#endif
            nearTiling = 100f;
            nearStrength = 0.6f;
            farTiling = 1000f;
            farStrength = 0.8f;
           objectSpace = false;
    }
    }
}