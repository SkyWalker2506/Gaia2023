using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSRainSettings : GTSProfileSettings
    {
        public float Power = 1f;
        public float MinHeight = 0f;
        public float MaxHeight = 3000f;
        public float Speed = 1f;
        public float Darkness = 0.2f;
        public float Smoothness = 0.8f;
        public float Scale = 2f;
        public Texture2D rainDataTexture;
        public override void Reset()
        {
            base.Reset();
            
            Power = 1f;
            MinHeight = 0f;
            MaxHeight = 3000f;
            Speed = 1f;
            Darkness = 0.2f;
            Smoothness = 0.8f;
            Scale = 2f;
            
#if UNITY_EDITOR
            rainDataTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GTSConstants.RainTexturePath);
#endif


        }
    }
}