using UnityEngine;
namespace ProceduralWorlds.GTS
{
    public static class GTSShaderID
    {
        // Keywords
        public static readonly int Tessellation;
        public static readonly string TessellationOn;
        public static readonly int DetailNormals;
        public static readonly string DetailNormalsOn;
        public static readonly int Snow;
        public static readonly string SnowOn;
        public static readonly int Rain;
        public static readonly string RainOn;
        public static readonly int Geo;
        public static readonly string GeoOn;
        public static readonly int Variation;
        public static readonly string VariationOn;
        public static readonly int HeightBlend;
        public static readonly string HeightBlendOn;
        public static readonly int MobileVR;
        public static readonly string MobileVROn;
        public static readonly int Colormap;
        public static readonly string ColormapOn;
        public static readonly int Vegetationmap;
        public static readonly string VegetationmapOn;
        public static readonly int WorldAlignedUVs;
        public static readonly int ObjectSpaceDataA;

        // Main Settings
        public static readonly int TextureArrayAlbedo;
        public static readonly int TextureArrayNormal;
        public static readonly int SplatmapIndex;
        public static readonly int SplatmapIndexLowRes;
        public static readonly int Resolution;
        public static readonly int WorldNormalMap;
        public static readonly int TerrainPosSize;
        public static readonly int BlendFactor;

        // Detail Settings
        public static readonly int DetailNormalMap;
        public static readonly int DetailNearFarData;

        // Geo Settings
        public static readonly int GeoNearData;
        public static readonly int GeoFarData;
        public static readonly int GeoMap;
        public static readonly int GeoNormal;

        // Variation Settings
        public static readonly int MacroVariationMap;
        public static readonly int MacroVariationData;

        // Tessellation Settings
        public static readonly int TessellationMultiplier;
        public static readonly int TessellationFactorMinDistance;
        public static readonly int TessellationFactorMaxDistance;

        // Snow Settings
        public static readonly int SnowDataA;
        public static readonly int SnowDataB;
        public static readonly int SnowAlbedoMap;
        public static readonly int SnowNormalMap;
        public static readonly int SnowMaskMap;
        public static readonly int SnowStochastic;
        public static readonly int SnowHeightData;
        public static readonly int SnowDisplacementData;
        public static readonly int SnowColor;
        public static readonly int SnowMaskRemapMin;
        public static readonly int SnowMaskRemapMax;
        public static readonly int TerrainHeightmapRecipSize;
        public static readonly int GlobalBlendData;
        public static readonly int GlobalSnowIntensity;
        public static readonly int GlobalCoverLayer1FadeStart;
        public static readonly int GlobalCoverLayer1FadeDist;

        //Rain Settings
        public static readonly int RainDataA;
        public static readonly int RainDataB;
        public static readonly int RainMap;
        
        // Control Textures
        public static readonly int[] Controls;

        // Per Layer Settings
        public static readonly int[] LayerHeightData;
        public static readonly int[] MaskMapRemapMin;
        public static readonly int[] MaskMapRemapMax;
        public static readonly int[] MaskMapRemap;
        public static readonly int[] Color;
        public static readonly int[] TriPlanarData;
        public static readonly int[] DisplacementData;
        public static readonly int[] LayerST;
        public static readonly int[] LayerDataA;

        // Per Layer Array Settings
        public static readonly int HeightDataArray;
        public static readonly int UVDataArray;
        public static readonly int MaskMapRemapMinArray;
        public static readonly int MaskMapRemapMaxArray;
        public static readonly int MaskMapRemapArray;
        public static readonly int ColorArray;
        public static readonly int TriPlanarDataArray;
        public static readonly int DisplacementDataArray;
        public static readonly int LayerSTArray;
        public static readonly int LayerDataAArray;

        public static readonly int ColorMapTexture;
        public static readonly int ColorMapNormalTexture;
        public static readonly int ColorMapNearFarData;
        public static readonly int ColorMapDataA;

        public static readonly int VegetationMapTexture;
        public static readonly int VegetationMapNormalTexture;
        public static readonly int VegetationMapNearFarData;
        public static readonly int VegetationMapDataA;

        //Misc Settings
        public static readonly int HeightScale;
        public static readonly int HeightMap;

        public static readonly int WeightIndex_BlurSteps;
        public static readonly int WeightIndex_BlurDistance;
        public static readonly int WeightIndex_NumSplats;
        public static readonly int[] WeightIndex_Splats;

        public static readonly int WorldNormal;

        static GTSShaderID()
        {
            Tessellation = Shader.PropertyToID("_TESSELLATION");
            TessellationOn = "_TESSELLATION_ON";
            DetailNormals = Shader.PropertyToID("_DETAIL_NORMALS");
            DetailNormalsOn = "_DETAIL_NORMALS_ON";
            Snow = Shader.PropertyToID("_SNOW");
            SnowOn = "_SNOW_ON";
            Rain = Shader.PropertyToID("_RAIN");
            RainOn = "_RAIN_ON";
            Geo = Shader.PropertyToID("_GEOLOGICAL");
            GeoOn = "_GEOLOGICAL_ON";
            Variation = Shader.PropertyToID("_MACRO_VARIATION");
            VariationOn = "_MACRO_VARIATION_ON";
            HeightBlend = Shader.PropertyToID("_HEIGHT_BLEND");
            HeightBlendOn = "_HEIGHT_BLEND_ON";
            MobileVR = Shader.PropertyToID("_MOBILE_VR");
            MobileVROn = "_MOBILE_VR_ON";
            Colormap = Shader.PropertyToID("_COLORMAP");
            ColormapOn = "_COLORMAP_ON";
            Vegetationmap = Shader.PropertyToID("_VEGETATIONMAP");
            VegetationmapOn = "_VEGETATIONMAP_ON";
            WorldAlignedUVs = Shader.PropertyToID("_WorldAlignedUVs");
            ObjectSpaceDataA = Shader.PropertyToID("_ObjectSpaceDataA");
            TextureArrayAlbedo = Shader.PropertyToID("_AlbedoArray");
            TextureArrayNormal = Shader.PropertyToID("_NormalArray");
            SplatmapIndex = Shader.PropertyToID("_SplatmapIndex");
            SplatmapIndexLowRes = Shader.PropertyToID("_SplatmapIndexLowRes");
            Resolution = Shader.PropertyToID("_Resolution");
            WorldNormalMap = Shader.PropertyToID("_WorldNormalMap");
            TerrainPosSize = Shader.PropertyToID("_TerrainPosSize");
            BlendFactor = Shader.PropertyToID("_BlendFactor");
            DetailNormalMap = Shader.PropertyToID("_DetailNormal");
            DetailNearFarData = Shader.PropertyToID("_DetailNearFarData");
            GeoNearData = Shader.PropertyToID("_GeoNearData");
            GeoFarData = Shader.PropertyToID("_GeoFarData");
            GeoMap = Shader.PropertyToID("_GeoMap");
            GeoNormal = Shader.PropertyToID("_GeoNormal");
            MacroVariationMap = Shader.PropertyToID("_MacroVariationMap");
            MacroVariationData = Shader.PropertyToID("_MacroVariationData");
            TessellationMultiplier = Shader.PropertyToID("_TessellationMultiplier");
            TessellationFactorMinDistance = Shader.PropertyToID("_TessellationFactorMinDistance");
            TessellationFactorMaxDistance = Shader.PropertyToID("_TessellationFactorMaxDistance");
            SnowDataA = Shader.PropertyToID("_PW_SnowDataA");
            SnowDataB = Shader.PropertyToID("_PW_SnowDataB");
            SnowAlbedoMap = Shader.PropertyToID("_PW_SnowAlbedoMap");
            SnowNormalMap = Shader.PropertyToID("_PW_SnowNormalMap");
            SnowMaskMap = Shader.PropertyToID("_PW_SnowMaskMap");
            SnowColor = Shader.PropertyToID("_PW_SnowColor");
            SnowStochastic = Shader.PropertyToID("_PW_SnowStochastic");
            SnowMaskRemapMin = Shader.PropertyToID("_PW_SnowMaskRemapMin");
            SnowMaskRemapMax = Shader.PropertyToID("_PW_SnowMaskRemapMax");
            SnowDisplacementData = Shader.PropertyToID("_PW_SnowDisplacementData");
            SnowHeightData = Shader.PropertyToID("_PW_SnowHeightData");
            TerrainHeightmapRecipSize = Shader.PropertyToID("_TerrainHeightmapRecipSize");
            GlobalBlendData = Shader.PropertyToID("_GlobalBlendData");
            GlobalSnowIntensity = Shader.PropertyToID("_PW_Global_CoverLayer1Progress");
            GlobalCoverLayer1FadeStart = Shader.PropertyToID("_PW_Global_CoverLayer1FadeStart");
            GlobalCoverLayer1FadeDist = Shader.PropertyToID("_PW_Global_CoverLayer1FadeDist");

            ColorMapTexture = Shader.PropertyToID("_ColormapTex");
            ColorMapNormalTexture = Shader.PropertyToID("_ColormapNormalTex");
            ColorMapNearFarData = Shader.PropertyToID("_ColormapNearFarData");
            ColorMapDataA = Shader.PropertyToID("_ColormapDataA");

            VegetationMapTexture = Shader.PropertyToID("_VegetationmapTex");
            VegetationMapNormalTexture = Shader.PropertyToID("_VegetationmapNormalTex");
            VegetationMapNearFarData = Shader.PropertyToID("_VegetationmapNearFarData");
            VegetationMapDataA = Shader.PropertyToID("_VegetationmapDataA");

            RainDataA = Shader.PropertyToID("_PW_RainDataA");
            RainDataB = Shader.PropertyToID("_PW_RainDataB");
            RainMap = Shader.PropertyToID("_PW_RainMap");
            
            Controls = new int[2];
            for (int i = 0; i < 2; i++)
            {
                Controls[i] = Shader.PropertyToID(string.Format("_Control{0}", i));
            }
            LayerHeightData = new int[8];
            MaskMapRemapMin = new int[8];
            MaskMapRemapMax = new int[8];
            MaskMapRemap = new int[8];
            Color = new int[8];
            TriPlanarData = new int[8];
            DisplacementData = new int[8];
            LayerST = new int[8];
            LayerDataA = new int[8];
            for (int i = 0; i < 8; i++)
            {
                LayerHeightData[i] = Shader.PropertyToID(string.Format("_HeightData{0}", i));
                MaskMapRemapMin[i] = Shader.PropertyToID(string.Format("_MaskMapRemapMin{0}", i));
                MaskMapRemapMax[i] = Shader.PropertyToID(string.Format("_MaskMapRemapMax{0}", i));
                MaskMapRemap[i] = Shader.PropertyToID(string.Format("_MaskMapRemapData{0}", i));
                Color[i] = Shader.PropertyToID(string.Format("_Color{0}", i));
                TriPlanarData[i] = Shader.PropertyToID(string.Format("_TriPlanarData{0}", i));
                DisplacementData[i] = Shader.PropertyToID(string.Format("_DisplacementData{0}", i));
                LayerST[i] = Shader.PropertyToID(string.Format("_LayerST{0}", i));
                LayerDataA[i] = Shader.PropertyToID(string.Format("_LayerDataA{0}", i));
            }

            //Arrays
            HeightDataArray = Shader.PropertyToID("_HeightData");
            UVDataArray = Shader.PropertyToID("_UVData");
            MaskMapRemapMinArray = Shader.PropertyToID("_MaskMapRemapMinData");
            MaskMapRemapMaxArray = Shader.PropertyToID("_MaskMapRemapMaxData");
            MaskMapRemapArray = Shader.PropertyToID("_MaskMapRemapData");
            ColorArray = Shader.PropertyToID("_ColorData");
            TriPlanarDataArray = Shader.PropertyToID("_TriPlanarData");
            DisplacementDataArray = Shader.PropertyToID("_DisplacementData");
            LayerDataAArray = Shader.PropertyToID("_LayerDataA");

            //Misc
            HeightScale = Shader.PropertyToID("_HeightScale");
            HeightMap = Shader.PropertyToID("_HeightMap");

            WeightIndex_BlurSteps = Shader.PropertyToID("_BlurSteps");
            WeightIndex_BlurDistance = Shader.PropertyToID("_BlurDistance");
            WeightIndex_NumSplats = Shader.PropertyToID("_NumSplats");

            WeightIndex_Splats = new int[2];

            for (int i = 0; i < 2; i++)
            {
                WeightIndex_Splats[i] = Shader.PropertyToID(string.Format("_Splat{0}", (i + 1)));
            }

            WorldNormal = Shader.PropertyToID("_WorldNormal");
        }
    }
}