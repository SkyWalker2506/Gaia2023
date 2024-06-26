Shader "PW/GTS_BuiltIn"
{
    Properties
    {
        [Toggle(_HEIGHT_BLEND_ON)]
        _HEIGHT_BLEND("Height Blend", Int) = 0

        [Toggle(_TESSELLATION_ON)]
        _TESSELLATION("Tessellation", Int) = 0

        [Toggle(_SNOW_ON)]
        _SNOW("Snow", Int) = 0
        
        [Toggle(_RAIN_ON)]
        _RAIN("Rain", Int) = 0

        [Toggle(_DETAIL_NORMALS_ON)]
        _DETAIL_NORMALS("Detail Normals", Int) = 0

        [Toggle(_GEOLOGICAL_ON)]
        _GEOLOGICAL("Geological", Int) = 0

        [Toggle(_MACRO_VARIATION_ON)]
        _MACRO_VARIATION_ON("Macro Variation", Int) = 0

        [Toggle(_MOBILE_VR_ON)]
        _MOBILE_VR_ON("Mobile VR", Int) = 0

        //[Toggle(_COLORMAP_ON)]
        //_COLORMAP_ON("Color Map", Int) = 0

        [Toggle]_COLORMAP("_COLORMAP_ON", Float) = 0

        _TerrainPosSize("_TerrainPosSize", Vector) = (0,0,0,0)
        [NoScaleOffset]_Control0("_Control0", 2D) = "black" {}
        [NoScaleOffset]_Control1("_Control1", 2D) = "black" {}
        [NoScaleOffset]_WorldNormalMap("_WorldNormalMap", 2D) = "black" {}
        [NoScaleOffset]_AlbedoArray("_AlbedoArray", 2DArray) = "" {}
        [NoScaleOffset]_NormalArray("_NormalArray", 2DArray) = "" {}
        _BlendFactor("_BlendFactor", Float) = 0.1

        _LayerST0("_LayerST0", Vector) = (1,1,0,0)
        _LayerST1("_LayerST1", Vector) = (1,1,0,0)
        _LayerST2("_LayerST2", Vector) = (1,1,0,0)
        _LayerST3("_LayerST3", Vector) = (1,1,0,0)
        _LayerST4("_LayerST4", Vector) = (1,1,0,0)
        _LayerST5("_LayerST5", Vector) = (1,1,0,0)
        _LayerST6("_LayerST6", Vector) = (1,1,0,0)
        _LayerST7("_LayerST7", Vector) = (1,1,0,0)

        _HeightData0("_HeightData0", Vector) = (1,1,0,0)
        _HeightData1("_HeightData1", Vector) = (1,1,0,0)
        _HeightData2("_HeightData2", Vector) = (1,1,0,0)
        _HeightData3("_HeightData3", Vector) = (1,1,0,0)
        _HeightData4("_HeightData4", Vector) = (1,1,0,0)
        _HeightData5("_HeightData5", Vector) = (1,1,0,0)
        _HeightData6("_HeightData6", Vector) = (1,1,0,0)
        _HeightData7("_HeightData7", Vector) = (1,1,0,0)

        [NoScaleOffset]_SplatmapIndex("_SplatmapIndex", 2D) = "black" {}
        [NoScaleOffset]_SplatmapIndexLowRes("_SplatmapIndexLowRes", 2D) = "black" {}
        _Resolution("_Resolution", Float) = 1024

        _MaskMapRemapData0("_MaskMapRemapData0", Vector) = (1,1,1,1)
        _MaskMapRemapData1("_MaskMapRemapData1", Vector) = (1,1,1,1)
        _MaskMapRemapData2("_MaskMapRemapData2", Vector) = (1,1,1,1)
        _MaskMapRemapData3("_MaskMapRemapData3", Vector) = (1,1,1,1)
        _MaskMapRemapData4("_MaskMapRemapData4", Vector) = (1,1,1,1)
        _MaskMapRemapData5("_MaskMapRemapData5", Vector) = (1,1,1,1)
        _MaskMapRemapData6("_MaskMapRemapData6", Vector) = (1,1,1,1)
        _MaskMapRemapData7("_MaskMapRemapData7", Vector) = (1,1,1,1)

        _Color0("_Color0", Vector) = (1,1,1,1)
        _Color1("_Color1", Vector) = (1,1,1,1)
        _Color2("_Color2", Vector) = (1,1,1,1)
        _Color3("_Color3", Vector) = (1,1,1,1)
        _Color4("_Color4", Vector) = (1,1,1,1)
        _Color5("_Color5", Vector) = (1,1,1,1)
        _Color6("_Color6", Vector) = (1,1,1,1)
        _Color7("_Color7", Vector) = (1,1,1,1)

        [NoScaleOffset]_DetailNormal("_DetailNormal", 2D) = "white" {}
        _DetailNearFarData("_DetailNearFarData", Vector) = (0,0,0,0)

        _DisplacementData0("_DisplacementData0", Vector) = (1,1,1,1)
        _DisplacementData1("_DisplacementData1", Vector) = (1,1,1,1)
        _DisplacementData2("_DisplacementData2", Vector) = (1,1,1,1)
        _DisplacementData3("_DisplacementData3", Vector) = (1,1,1,1)
        _DisplacementData4("_DisplacementData4", Vector) = (1,1,1,1)
        _DisplacementData5("_DisplacementData5", Vector) = (1,1,1,1)
        _DisplacementData6("_DisplacementData6", Vector) = (1,1,1,1)
        _DisplacementData7("_DisplacementData7", Vector) = (1,1,1,1)

        _GeoNearData("_GeoNearData", Vector) = (1,1,1,1)
        _GeoFarData("_GeoFarData", Vector) = (1,1,1,1)
        [NoScaleOffset]_GeoMap("_GeoMap", 2D) = "black" {}
        [NoScaleOffset]_GeoNormal("_GeoNormal", 2D) = "black" {}
        _GeoLayerData0("_GeoLayerData0", Vector) = (1,1,1,1)
        _GeoLayerData1("_GeoLayerData1", Vector) = (1,1,1,1)

        [NoScaleOffset]_MacroVariationMap("_MacroVariationMap", 2D) = "black" {}
        _MacroVariationData("_MacroVariationData", Vector) = (1, 1, 1, 1)

        _TessellationMultiplier("_TessellationMultiplier", Float) = 1

        _LayerDataA0("_LayerDataA0", Vector) = (0, 0, 0, 0)
        _LayerDataA1("_LayerDataA1", Vector) = (0, 0, 0, 0)
        _LayerDataA2("_LayerDataA2", Vector) = (0, 0, 0, 0)
        _LayerDataA3("_LayerDataA3", Vector) = (0, 0, 0, 0)
        _LayerDataA4("_LayerDataA4", Vector) = (0, 0, 0, 0)
        _LayerDataA5("_LayerDataA5", Vector) = (0, 0, 0, 0)
        _LayerDataA6("_LayerDataA6", Vector) = (0, 0, 0, 0)
        _LayerDataA7("_LayerDataA7", Vector) = (0, 0, 0, 0)

        _TerrainHeightmapRecipSize("_TerrainHeightmapRecipSize", Vector) = (0,0,0,0)

        _GlobalBlendData("_GlobalBlendData", Vector) = (0,0,0,0)

        _TriPlanarData0("_TriPlanarData0", Vector) = (0,0,0,0)
        _TriPlanarData1("_TriPlanarData1", Vector) = (0,0,0,0)
        _TriPlanarData2("_TriPlanarData2", Vector) = (0,0,0,0)
        _TriPlanarData3("_TriPlanarData3", Vector) = (0,0,0,0)
        _TriPlanarData4("_TriPlanarData4", Vector) = (0,0,0,0)
        _TriPlanarData5("_TriPlanarData5", Vector) = (0,0,0,0)
        _TriPlanarData6("_TriPlanarData6", Vector) = (0,0,0,0)
        _TriPlanarData7("_TriPlanarData7", Vector) = (0,0,0,0)

        [ToggleUI]_WorldAlignedUVs("_WorldAlignedUVs", Float) = 0
        _ObjectSpaceDataA("_ObjectSpaceDataA", Vector) = (0, 0, 0, 0)

        [NoScaleOffset]_ColormapTex("_ColormapTex", 2D) = "black" {}
        [NoScaleOffset]_ColormapNormalTex("_ColormapNormalTex", 2D) = "white" {}
        _ColormapNearFarData("_ColormapNearFarData", Vector) = (1, 1, 1, 1)
        _ColormapDataA("_ColormapDataA", Vector) = (1, 1, 1, 1)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "TerrainCompatible" = "True"}
        LOD 200

        CGPROGRAM

        #pragma shader_feature _GEOLOGICAL_ON
        #pragma shader_feature _HEIGHT_BLEND_ON
        #pragma shader_feature _TESSELLATION_ON
        #pragma shader_feature _DETAIL_NORMALS_ON
        #pragma shader_feature _SNOW_ON
        #pragma shader_feature _RAIN_ON
        #pragma shader_feature _MACRO_VARIATION_ON
        #pragma shader_feature _MOBILE_VR_ON
        #pragma shader_feature _COLORMAP_ON

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vert
        #pragma multi_compile_instancing
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
                #ifdef UNITY_PASS_SHADOWCASTER
            #undef INTERNAL_DATA
            #undef WorldReflectionVector
            #undef WorldNormalVector
            #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
            #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
            #define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
        #endif


        #include "GTS_BuiltIn_Includes.hlsl"

        #define _ALPHATEST_ON
        #define BUILTIN
        //Sampler States    

        #define SamplerState_Point_Repeat sampler_point_repeat
        SamplerState SamplerState_Point_Repeat;

        #define SamplerState_Point_Clamp sampler_point_clamp
        SamplerState SamplerState_Point_Clamp;

        #define SamplerState_Linear_Clamp sampler_linear_clamp
        SamplerState SamplerState_Linear_Clamp;

        #define SamplerState_Linear_Repeat sampler_linear_repeat
        SamplerState SamplerState_Linear_Repeat;

        SAMPLER(sampler_AlbedoArray);
        SAMPLER(sampler_NormalArray);

        //Textures

        TEXTURE2D(_Control0);
        TEXTURE2D(_Control1);
        TEXTURE2D(_WorldNormalMap);
        TEXTURE2D_ARRAY(_AlbedoArray);
        TEXTURE2D_ARRAY(_NormalArray);

        TEXTURE2D(_SplatmapIndex);
        TEXTURE2D(_SplatmapIndexLowRes);

        TEXTURE2D(_DetailNormal);

        TEXTURE2D(_GeoMap);
        TEXTURE2D(_GeoNormal);

        TEXTURE2D(_MacroVariationMap);

        TEXTURE2D(_ColormapTex);
        TEXTURE2D(_ColormapNormalTex);

        //Vectors
        float4 _TerrainPosSize;
        float _BlendFactor;

        float4 _LayerST0;
        float4 _LayerST1;
        float4 _LayerST2;
        float4 _LayerST3;
        float4 _LayerST4;
        float4 _LayerST5;
        float4 _LayerST6;
        float4 _LayerST7;

        float4 _HeightData0;
        float4 _HeightData1;
        float4 _HeightData2;
        float4 _HeightData3;
        float4 _HeightData4;
        float4 _HeightData5;
        float4 _HeightData6;
        float4 _HeightData7;

        float _Resolution;

        float4 _MaskMapRemapData0;
        float4 _MaskMapRemapData1;
        float4 _MaskMapRemapData2;
        float4 _MaskMapRemapData3;
        float4 _MaskMapRemapData4;
        float4 _MaskMapRemapData5;
        float4 _MaskMapRemapData6;
        float4 _MaskMapRemapData7;

        float4 _Color0;
        float4 _Color1;
        float4 _Color2;
        float4 _Color3;
        float4 _Color4;
        float4 _Color5;
        float4 _Color6;
        float4 _Color7;

        float4 _DetailBlendData;
        float4 _DetailNearFarData;

        float4 _DisplacementData0;
        float4 _DisplacementData1;
        float4 _DisplacementData2;
        float4 _DisplacementData3;
        float4 _DisplacementData4;
        float4 _DisplacementData5;
        float4 _DisplacementData6;
        float4 _DisplacementData7;

        float4 _GeoNearData;
        float4 _GeoFarData;
        float4 _GeoLayerData0;
        float4 _GeoLayerData1;
        float4 _MacroVariationData;

        float4 _LayerDataA0;
        float4 _LayerDataA1;
        float4 _LayerDataA2;
        float4 _LayerDataA3;
        float4 _LayerDataA4;
        float4 _LayerDataA5;
        float4 _LayerDataA6;
        float4 _LayerDataA7;

        float _TessellationMultiplier;

        float4 _TerrainHeightmapRecipSize;

        float4 _GlobalBlendData;

        float4 _TriPlanarData0;
        float4 _TriPlanarData1;
        float4 _TriPlanarData2;
        float4 _TriPlanarData3;
        float4 _TriPlanarData4;
        float4 _TriPlanarData5;
        float4 _TriPlanarData6;
        float4 _TriPlanarData7;

        float _WorldAlignedUVs;
        float4 _ObjectSpaceDataA;

        float4 _ColormapNearFarData;
        float4 _ColormapDataA;

        #include "GTS_TerrainInstancing.hlsl"
        #include "GTS_Functions.hlsl"

        struct Input
        {
            float2 uv;
            float3 worldPos;
            float3 tangent;
            float3 worldNormal;
            float globalBlendDistance;
            float nearCameraDistance;
            float3 normal;
            INTERNAL_DATA
        };

        //Functions
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            TerrainInstancing_builtIn(v);

            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float globalBlendDistance, nearCameraDistance;
            GTSVertexData_float(worldPos, globalBlendDistance, nearCameraDistance);
            o.globalBlendDistance = globalBlendDistance;
            o.nearCameraDistance = nearCameraDistance;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 worldNormal = WorldNormalVector(IN, float3(0, 0, 1));
            float3 worldTangent = WorldNormalVector(IN, float3(1, 0, 0));
            float3 worldBitangent = WorldNormalVector(IN, float3(0, 1, 0));
            float3x3 worldToTangent = float3x3(worldTangent, worldBitangent, worldNormal);

            float3 blendedAlbedo, blendedNormal;
            float4 blendedMask;
            GTSLayers_float(IN.uv, IN.worldPos, worldNormal, _Resolution, IN.globalBlendDistance, IN.nearCameraDistance,
                blendedAlbedo, blendedNormal, blendedMask);

            o.Albedo = blendedAlbedo;
            o.Normal = mul(worldToTangent, blendedNormal);
            o.Metallic = 0;
            o.Smoothness = blendedMask.a;
            o.Occlusion = blendedMask.g;
            o.Alpha = 1;

            float alpha;
            GTSTerrainHoles_float(IN.worldPos, alpha);
            clip(alpha);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
