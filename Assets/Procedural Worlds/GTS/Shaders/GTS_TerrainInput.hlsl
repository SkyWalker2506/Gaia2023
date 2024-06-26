#ifndef TERRAIN_INPUT
#define TERRAIN_INPUT

/*
#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)
#define SAMPLE_TEXTURE2D_ARRAY(tex,samplerTex,coord,index) tex.Sample(samplerTex,float3(coord, index))
#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplerTex,coord,index,lod) tex.SampleLevel(samplerTex, float3(coord, index), lod)
#define SAMPLE_TEXTURE2D_ARRAY_GRAD(tex,samplerTex,coord, index,ddx,ddy) tex.SampleGrad(samplerTex,float3(coord, index),ddx,ddy)
#define TEXTURE2D(textureName) UNITY_DECLARE_TEX2D_NOSAMPLER(textureName)
#define TEXTURE2D_ARRAY(textureName) UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(textureName)
#define SAMPLER(samplerName) SamplerState samplerName
#else
#if !defined(SAMPLE_TEXTURE2D)
	#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
#endif
#if !defined(SAMPLE_TEXTURE2D_LOD)
	#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
#endif
#if !defined(SAMPLE_TEXTURE2D_GRAD)
	#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex2Dgrad(tex,coord,ddx,ddy)
#endif
#if !defined(SAMPLE_TEXTURE2D_ARRAY)
	#define SAMPLE_TEXTURE2D_ARRAY(tex,samplertex,coord,index) tex2DArray(tex, float3(coord, index))
#endif
#if !defined(SAMPLE_TEXTURE2D_ARRAY_LOD)
	#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplertex,coord,index,lod) tex2DArraylod(tex, float4(coord, index,lod))
#endif
#if !defined(SAMPLE_TEXTURE2D_ARRAY_GRAD)
	#define SAMPLE_TEXTURE2D_ARRAY_GRAD(tex,samplerTex,coord, index,ddx,ddy) tex2DArray(tex,float3(coord, index))
#endif
#if !defined(TEXTURE2D)
	#define TEXTURE2D(textureName) UNITY_DECLARE_TEX2D_NOSAMPLER(textureName)
#endif
#if !defined(TEXTURE2D_ARRAY)
	#define TEXTURE2D_ARRAY(textureName) UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(textureName)
#endif
#if !defined(SAMPLER)
	#define SAMPLER(samplerName) SamplerState samplerName
#endif
#endif*/

//#if !defined(_WorldSpaceCameraPos)
//#define _WorldSpaceCameraPos 
//#endif

SAMPLER(SamplerState_Point_Repeat);
SAMPLER(SamplerState_Point_Clamp);
SAMPLER(SamplerState_Linear_Clamp);
SAMPLER(SamplerState_Linear_Repeat);

SAMPLER(sampler_AlbedoArray);
SAMPLER(sampler_NormalArray);

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

float4 _TerrainPosSize;
float _BlendFactor;

float _Resolution;

float4 _DetailBlendData;
float4 _DetailNearFarData;

float4 _GeoNearData;
float4 _GeoFarData;
float4 _GeoLayerData0;
float4 _GeoLayerData1;
float4 _MacroVariationData;

float _TessellationMultiplier;
float4 _TerrainHeightmapRecipSize;

float4 _GlobalBlendData;

float _WorldAlignedUVs;
float4 _ObjectSpaceDataA;

float4 _ColormapNearFarData;
float4 _ColormapDataA;

float4 _LayerST0;
float4 _LayerST1;
float4 _LayerST2;
float4 _LayerST3;
float4 _LayerST4;
float4 _LayerST5;
float4 _LayerST6;
float4 _LayerST7;

#endif