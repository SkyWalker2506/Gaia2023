using UnityEngine;

namespace ProceduralWorlds.GTS
{
    [CreateAssetMenu(fileName = "GTS Runtime", menuName = "Procedural Worlds/GTS/Runtime Data", order = 1)]
    public class GTSRuntime : ScriptableObject
    {
        public Vector4[] HeightDataArray;
        public Vector4[] UVDataArray;
        public Vector4[] MaskMapRemapMinArray;
        public Vector4[] MaskMapRemapMaxArray;
        public Vector4[] MaskMapRemapArray;
        public Vector4[] ColorArray;
        public Vector4[] TriPlanarDataArray;
        public Vector4[] DisplacementDataArray;
        public Vector4[] LayerDataAArray;


        public Vector4 SnowDataA;
        public Vector4 SnowDataB;
        public Vector4 SnowDisplacementData;
        public Vector4 SnowHeightData;
        public Vector4 SnowColor;
        public Vector4 SnowMaskRemapMin;
        public Vector4 SnowMaskRemapMax;

        // Setting global textures is fine.
        public Texture SnowAlbedoMap;
        public Texture SnowNormalMap;
        public Texture SnowMaskMap;

        // Set Legacy Snow Values
        public float GlobalSnowIntensity;
        public float GlobalCoverLayer1FadeStart;
        public float GlobalCoverLayer1FadeDist;

        public Vector4 RainDataA;
        public Vector4 RainDataB;
        public Texture RainDataTexture;
    }
}