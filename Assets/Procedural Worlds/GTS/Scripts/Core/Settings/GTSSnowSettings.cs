using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSSnowSettings : GTSProfileSettings
    {
        public Texture2D albedoTexture;
        public Texture2D normalTexture;
        public Texture2D maskTexture;
        public float power = 1f;
        public float minHeight = 100f;
        public float blendRange = 20f;
        public float slopeBlend = 20f;
        public float age = 0f;
        public float scale = 1f;
        public float normalStrength = 3f;
        public Vector4 maskRemapMin = new Vector4(0, 0, 0, 0);
        public Vector4 maskRemapMax = new Vector4(1, 1, 3, 1);
        public float heightContrast = 1f;
        public float heightBrightness = 1f;
        public float heightIncrease = 0f;
        public float displacementContrast = 1f;
        public float displacementBrightness = 0.4f;
        public float displacementIncrease = 0.1f;
        public float tessellationAmount = 25f;
        public Color color = Color.white;
        public bool objectSpace = false;
        public override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            albedoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(isLowPoly ? GTSConstants.LowPolySnowAlbedoTexturePath : GTSConstants.SnowAlbedoTexturePath);
            normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(isLowPoly ? GTSConstants.LowPolySnowNormalTexturePath : GTSConstants.SnowNormalTexturePath);
            maskTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(isLowPoly ? GTSConstants.LowPolySnowMaskTexturePath : GTSConstants.SnowMaskTexturePath);
#endif
            minHeight = 100f;
            blendRange = 20f;
            slopeBlend = 20f;
            age = 0f;
            scale = 1f;
            normalStrength = 3f;
            maskRemapMin = new Vector4(0, 0, 0, 0);
            maskRemapMax = new Vector4(1, 1, 3, 1);
            displacementContrast = 1f;
            displacementBrightness = 0.4f;
            displacementIncrease = 0.1f;
            tessellationAmount = 25f;
            color = Color.white;
            objectSpace = false;
        }
    }
}