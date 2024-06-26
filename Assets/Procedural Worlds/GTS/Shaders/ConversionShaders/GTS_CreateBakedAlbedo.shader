Shader "Hidden/GTS_CreateBakedAlbedo"
{
    Properties
    {

        _GEOLOGICAL_ON("_GEOLOGICAL_ON", Int) = 0
        _HEIGHT_BLEND_ON("_HEIGHT_BLEND_ON", Int) = 0
        
        _TerrainPosSize("_TerrainPosSize", Vector) = (0,0,0,0)
        [NoScaleOffset]_SplatmapIndex("_SplatmapIndex", 2D) = "black" {}
        [NoScaleOffset]_Control0("_Control0", 2D) = "black" {}
        [NoScaleOffset]_Control1("_Control1", 2D) = "black" {}
        [NoScaleOffset]_AlbedoArray("_AlbedoArray", 2DArray) = "" {}
        [NoScaleOffset]_NormalArray("_NormalArray", 2DArray) = "" {}

        [NoScaleOffset]_HeightMap("_HeightMap", 2D) = "black" {}
        _HeightScale("_HeightScale", Float) = 1024

        _BlendFactor("_BlendFactor", Float) = 0.1

        _LayerST0("_LayerST0", Vector) = (1,1,0,0)
        _LayerST1("_LayerST1", Vector) = (1,1,0,0)
        _LayerST2("_LayerST2", Vector) = (1,1,0,0)
        _LayerST3("_LayerST3", Vector) = (1,1,0,0)
        _LayerST4("_LayerST4", Vector) = (1,1,0,0)
        _LayerST5("_LayerST5", Vector) = (1,1,0,0)
        _LayerST6("_LayerST6", Vector) = (1,1,0,0)
        _LayerST7("_LayerST7", Vector) = (1,1,0,0)

        _GeoNearData("_GeoNearData", Vector) = (1,1,1,1)
        _GeoFarData("_GeoFarData", Vector) = (1,1,1,1)
        [NoScaleOffset]_GeoMap("_GeoMap", 2D) = "black" {}
        [NoScaleOffset]_GeoNormal("_GeoNormal", 2D) = "black" {}
        _GeoLayerData0("_GeoLayerData0", Vector) = (1,1,1,1)
        _GeoLayerData1("_GeoLayerData1", Vector) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #include "../GTS_BuiltIn_Includes.hlsl"

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
            TEXTURE2D_ARRAY(_AlbedoArray);
            TEXTURE2D_ARRAY(_NormalArray);

            TEXTURE2D(_GeoMap);

            TEXTURE2D(_SplatmapIndex);
            TEXTURE2D(_HeightMap);
            float _HeightScale;

            float4 _TerrainPosSize;

            int _GEOLOGICAL_ON;
            int _HEIGHT_BLEND_ON;

            float4 _GeoNearData;
            float4 _GeoFarData;
            float4 _GeoLayerData0;
            float4 _GeoLayerData1;

            float _BlendFactor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

#define MAXARRAYELEMENTS 8
            uniform float4 _UVData[MAXARRAYELEMENTS];
            uniform float4 _ColorData[MAXARRAYELEMENTS];
            uniform float4 _LayerDataA[MAXARRAYELEMENTS];
            uniform float4 _HeightData[MAXARRAYELEMENTS];
            uniform float4 _MaskMapRemapData[MAXARRAYELEMENTS];

            //Samples the layer tiling
            float2 layerUVs(float2 uv, float4 layerST)
            {
                return uv * layerST.xy + layerST.zw;
            }

            //Remaps a float value
            float RemapFloat(float value, float from1, float to1, float from2, float to2)
            {
                return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float2 uv = i.uv;

                float4 splat = SAMPLE_TEXTURE2D(_Control0, SamplerState_Linear_Clamp, uv);
                float4 splat1 = SAMPLE_TEXTURE2D(_Control1, SamplerState_Linear_Clamp, uv);

                //Declare weight splat array
                float weightSplats[] = { splat.r, splat.g, splat.b, splat.a, splat1.r, splat1.g, splat1.b, splat1.a };

                //Sample top 4 splats
                float4 splatmapIndexed = SAMPLE_TEXTURE2D_LOD(_SplatmapIndex, SamplerState_Point_Clamp, uv, 0) * 256;

                float4 splatControl = float4(0, 0, 0, 0);
                float4 heightData = float4(0, 0, 0, 0);

                int indexA = round(splatmapIndexed.x);
                float2 layerUV = layerUVs(uv, _UVData[indexA]);
                float4 albedoA = SAMPLE_TEXTURE2D_ARRAY(_AlbedoArray, sampler_AlbedoArray, layerUV, indexA);
                albedoA.rgb *= _ColorData[indexA];
                heightData = _HeightData[indexA];
                albedoA.a = pow(abs(albedoA.a), heightData.x) * heightData.y + heightData.z;
                float smoothnessA = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_NormalArray, layerUV, indexA).a;
                smoothnessA = RemapFloat(smoothnessA, 0, 1, _MaskMapRemapData[indexA].y, _MaskMapRemapData[indexA].w);
                float4 layerA_A = _LayerDataA[indexA];
                splatControl.x = (weightSplats[indexA]);

                int indexB = round(splatmapIndexed.y);
                layerUV = layerUVs(uv, _UVData[indexB]);
                float4 albedoB = SAMPLE_TEXTURE2D_ARRAY(_AlbedoArray, sampler_AlbedoArray, layerUV, indexB);
                albedoB.rgb *= _ColorData[indexB];
                heightData = _HeightData[indexB];
                albedoB.a = pow(abs(albedoB.a), heightData.x) * heightData.y + heightData.z;
                float smoothnessB = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_NormalArray, layerUV, indexB).a;
                smoothnessB = RemapFloat(smoothnessB, 0, 1, _MaskMapRemapData[indexB].y, _MaskMapRemapData[indexB].w);
                float4 layerA_B = _LayerDataA[indexB];
                splatControl.y = (weightSplats[indexB]);

                int indexC = round(splatmapIndexed.z);
                layerUV = layerUVs(uv, _UVData[indexC]);
                float4 albedoC = SAMPLE_TEXTURE2D_ARRAY(_AlbedoArray, sampler_AlbedoArray, layerUV, indexC);
                albedoC.rgb *= _ColorData[indexC];
                heightData = _HeightData[indexC];
                albedoC.a = pow(abs(albedoC.a), heightData.x) * heightData.y + heightData.z;
                float smoothnessC = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_NormalArray, layerUV, indexC).a;
                smoothnessC = RemapFloat(smoothnessC, 0, 1, _MaskMapRemapData[indexC].y, _MaskMapRemapData[indexC].w);
                float4 layerA_C = _LayerDataA[indexC];
                splatControl.z = (weightSplats[indexC]);

                int indexD = round(splatmapIndexed.w);
                layerUV = layerUVs(uv, _UVData[indexD]);
                float4 albedoD = SAMPLE_TEXTURE2D_ARRAY(_AlbedoArray, sampler_AlbedoArray, layerUV, indexD);
                albedoD.rgb *= _ColorData[indexD];
                heightData = _HeightData[indexD];
                albedoD.a = pow(abs(albedoD.a), heightData.x) * heightData.y + heightData.z;
                float smoothnessD = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_NormalArray, layerUV, indexD).a;
                smoothnessD = RemapFloat(smoothnessD, 0, 1, _MaskMapRemapData[indexD].y, _MaskMapRemapData[indexD].w);
                float4 layerA_D = _LayerDataA[indexD];
                splatControl.w = (weightSplats[indexD]);

                if (_HEIGHT_BLEND_ON == 1)
                {
                    //Height blending splat
                    float masks[] = { albedoA.a, albedoB.a, albedoC.a, albedoD.a };
                    float4 splatHeight = float4(masks[0], masks[1], masks[2], masks[3]) * splatControl.rgba;
                    float maxHeight = max(splatHeight.r, max(splatHeight.g, max(splatHeight.b, splatHeight.a)));
                    float heightTransition = _BlendFactor;
                    float transition = max(heightTransition, 1e-5);
                    half4 weightedHeights = splatHeight + transition - maxHeight.xxxx;
                    weightedHeights = max(0, weightedHeights);
                    weightedHeights = (weightedHeights + 1e-6) * splatControl;
                    float sumHeight = max(dot(weightedHeights, float4(1, 1, 1, 1)), 1e-6);
                    splatControl = weightedHeights / sumHeight.xxxx;
                }

                float3 blendedAlbedo = albedoA.rgb * splatControl.x + albedoB.rgb * splatControl.y + albedoC.rgb * splatControl.z + albedoD.rgb * splatControl.w;
                float blendedSmoothness = smoothnessA * splatControl.x + smoothnessB * splatControl.y + smoothnessC * splatControl.z + smoothnessD * splatControl.w;



                if (_GEOLOGICAL_ON == 1)
                {
                    float blendedGeoStrength = layerA_A.x * splatControl.x + layerA_B.x * splatControl.y + layerA_C.x * splatControl.z + layerA_D.x * splatControl.w;

                    float heightmap = UnpackHeightmap(SAMPLE_TEXTURE2D(_HeightMap, SamplerState_Linear_Repeat, uv)) * _HeightScale;
                    //Sample far geo data
                    float2 farGeoUVs = float2(0, (heightmap / _GeoFarData.y) + _GeoFarData.z);
                    float3 farGeoMap = SAMPLE_TEXTURE2D(_GeoMap, SamplerState_Linear_Repeat, farGeoUVs).rgb * 2;
                    float3 farGeoBlend = ((farGeoMap + float3(-0.3, -0.3, -0.3)) * _GeoFarData.x) * blendedGeoStrength;
                    blendedAlbedo = saturate(blendedAlbedo + farGeoBlend);
                }

                return float4(blendedAlbedo, blendedSmoothness);
            }
            ENDCG
        }
    }
}
