#ifndef TESTTERRAIN
#define TESTTERRAIN

#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
#define ENABLE_TERRAIN_PERPIXEL_NORMAL
#endif

#ifdef UNITY_INSTANCING_ENABLED
TEXTURE2D(_TerrainHeightmapTexture);
TEXTURE2D(_TerrainNormalmapTexture);
SAMPLER(sampler_TerrainNormalmapTexture);
#endif

UNITY_INSTANCING_BUFFER_START(Terrain)
UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData)  // float4(xBase, yBase, skipScale, ~)
UNITY_INSTANCING_BUFFER_END(Terrain)

CBUFFER_START(UnityTerrain)
#ifdef UNITY_INSTANCING_ENABLED
//float4 _TerrainHeightmapRecipSize;
float4 _TerrainHeightmapScale;
#endif
CBUFFER_END


void TerrainInstancing_float(float4 positionOS, float3 normal, out float4 o_position, out float3 o_normal, out float2 o_uv)
{
	o_position = positionOS;
	o_normal = normal;
	o_uv = float2(0,0);
#ifdef UNITY_INSTANCING_ENABLED
	float2 patchVertex = positionOS.xy;
	float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

	float2 sampleCoords = (patchVertex.xy + instanceData.xy) * instanceData.z;
	float height = UnpackHeightmap(_TerrainHeightmapTexture.Load(int3(sampleCoords, 0)));

	o_position.xz = sampleCoords * _TerrainHeightmapScale.xz;
	o_position.y = height * _TerrainHeightmapScale.y;

#ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
	o_normal = float3(0, 1, 0);
#else
	o_normal = _TerrainNormalmapTexture.Load(int3(sampleCoords, 0)).rgb * 2 - 1;
#endif
	o_uv = sampleCoords * _TerrainHeightmapRecipSize.zw;
#endif
}

#ifdef BUILTIN
void TerrainInstancing_builtIn(inout appdata_full v)
{
#ifdef UNITY_INSTANCING_ENABLED
	float2 patchVertex = v.vertex.xy;
	float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

	float2 sampleCoords = (patchVertex.xy + instanceData.xy) * instanceData.z;
	float height = UnpackHeightmap(_TerrainHeightmapTexture.Load(int3(sampleCoords, 0)));

	v.vertex.xz = sampleCoords * _TerrainHeightmapScale.xz;
	v.vertex.y = height * _TerrainHeightmapScale.y;
	v.vertex.w = 1.0;

#ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
	v.normal = float3(0, 1, 0);
#else
	v.normal = _TerrainNormalmapTexture.Load(int3(sampleCoords, 0)).rgb * 2 - 1;
#endif
	v.texcoord.xy = sampleCoords * _TerrainHeightmapRecipSize.zw;
#endif
}
#endif

#endif