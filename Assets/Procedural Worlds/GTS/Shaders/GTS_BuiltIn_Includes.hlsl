#ifndef GTS_BUILTIN_INCLUDED
#define GTS_BUILTIN_INCLUDED
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
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
		#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex2Dgrad(tex,coord,ddx,ddy)
		#define SAMPLE_TEXTURE2D_ARRAY(tex,samplertex,coord,index) tex2DArray(tex, float3(coord, index))
		#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplertex,coord,index,lod) tex2DArraylod(tex, float4(coord, index,lod))
		#define SAMPLE_TEXTURE2D_ARRAY_GRAD(tex,samplerTex,coord, index,ddx,ddy) tex2DArray(tex,float3(coord, index))
		#define TEXTURE2D(textureName) UNITY_DECLARE_TEX2D_NOSAMPLER(textureName)	
		#define TEXTURE2D_ARRAY(textureName) UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(textureName)
		#define SAMPLER(samplerName) SamplerState samplerName
	#endif
	#define UNITY_MATRIX_I_M   unity_WorldToObject
#endif