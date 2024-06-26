#ifndef TERRIAN_TWO_SAMPLE_INCLUDED
#define TERRIAN_TWO_SAMPLE_INCLUDED

//Return random value from float2
float2 hash(float2 p)
{
    return frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), p)) * 43758.5453);
}

//Returns terrain uv in world space
void getWorldPosTerrainUV_float(float3 worldPos, float2 terrainPosition, float2 terrainSize, out float2 uv)
{
    float2 pos = worldPos.xz - terrainPosition;
    uv = pos / terrainSize;
}

//Returns terrain uv in world space
float2 worldPosTerrainUV(float2 worldPos, float2 terrainPosition, float2 terrainSize)
{
    float2 pos = worldPos - terrainPosition;
    return pos / terrainSize;
}

//Samples the terrain world normal texture
float3 worldSpaceTerrainNormal(float2 uv)
{
    return SAMPLE_TEXTURE2D(_WorldNormalMap, SamplerState_Linear_Clamp, uv).xyz * 2 - 1;
}

//Samples the terrain world normal texture
float3 worldSpaceTerrainNormalLOD(float2 uv, float lod)
{
    return SAMPLE_TEXTURE2D_LOD(_WorldNormalMap, SamplerState_Linear_Clamp, uv, lod).xyz * 2 - 1;
}

//Blends the tangent space normals into the terrain world normal
float3 worldSpaceNormalBlend(float3 terrainNormal, float3 surfaceNormal)
{
    float3 terrainTangent = cross(terrainNormal, float3(0, 0, 1));
    float3 terrainBitangent = cross(terrainTangent, terrainNormal);
    return surfaceNormal.x * terrainTangent + surfaceNormal.y * terrainBitangent + surfaceNormal.z * terrainNormal;
}


#ifdef _ALPHATEST_ON
TEXTURE2D(_TerrainHolesTexture);
SAMPLER(sampler_TerrainHolesTexture);
#endif

#ifdef _ALPHATEST_ON
void clipHoles(float2 uv, out float alpha)
{
    float hole = SAMPLE_TEXTURE2D(_TerrainHolesTexture, sampler_TerrainHolesTexture, uv).r;
    alpha = (hole == 0.0f ? -1 : 1);
}
#endif

void GTSTerrainHoles_float(float3 worldPos, out float alpha)
{
#ifdef _ALPHATEST_ON
    float2 terrainUV = worldPosTerrainUV(worldPos.xz, _TerrainPosSize.xy, _TerrainPosSize.zw);
    clipHoles(terrainUV, alpha);
#else
    alpha = 1;
#endif
}


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

//Remaps a float2 value
float2 RemapFloat2(float2 value, float2 from1, float2 to1, float2 from2, float2 to2)
{
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

//Remaps a float4 value
float4 RemapFloat4(float4 value, float4 from1, float4 to1, float4 from2, float4 to2)
{
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

// Approximate version from: https://hal.inria.fr/hal-01824773/document
void TriangleGrid(float2 uv,
    out float w1, out float w2, out float w3,
    out float2 vertex1, out float2 vertex2, out float2 vertex3)
{
    uv = uv * 3.464;

    const float2x2 gridToSkewedGrid = float2x2(1.0, 0.0, -0.57735027, 1.15470054);
    float2 skewedCoord = mul(gridToSkewedGrid, uv);

    int2 baseId = int2(floor(skewedCoord));
    float3 temp = float3(frac(skewedCoord), 0);
    temp.z = 1.0 - temp.x - temp.y;
    if (temp.z > 0.0)
    {
        w1 = temp.z;
        w2 = temp.y;
        w3 = temp.x;
        vertex1 = baseId;
        vertex2 = baseId + int2(0, 1);
        vertex3 = baseId + int2(1, 0);
    }
    else
    {
        w1 = -temp.z;
        w2 = 1.0 - temp.y;
        w3 = 1.0 - temp.x;
        vertex1 = baseId + int2(1, 1);
        vertex2 = baseId + int2(1, 0);
        vertex3 = baseId + int2(0, 1);
    }
}

//Samples the packed albedo and normal maps, based on whether it's triplanar or not
void SampleLayerTexture(float3 worldPos, float3 worldNormal, int index,
    float2 uv, float4 derivData, float4 triPlanarData,
    float4 remapData, float4 heightData, float4 colorData, float4 layerDataA,
    out float4 sampledAlbedoPacked, out float4 sampledNormalPacked)
{

    UNITY_BRANCH
    if (triPlanarData.z)
    {
        //Approximate version from: https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSwizzle.shader
        float3 triplanarBlend = pow(abs(worldNormal), float3(4,15,4));
        triplanarBlend /= max(dot(triplanarBlend, float3(1, 1, 1)), 0.0001);

        float2 triPlanarSize = triPlanarData.xy;

        //Sample world normal axis planes
        float2 uvX = worldPos.zy / triPlanarSize;
        float2 uvY = worldPos.xz / triPlanarSize;
        float2 uvZ = worldPos.xy / triPlanarSize;

        //Slight offset to prevent mirroring
        uvY += 0.33;
        uvZ += 0.67;

        //Flip normals if the world normal is negative
        float3 axisSign = worldNormal < 0 ? -1 : 1;

        //flip tangents based on the axis
        uvX.x *= axisSign.x;
        uvY.x *= axisSign.y;
        uvZ.x *= -axisSign.z;

        //Sample the albedo color
        float4 albedoX = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uvX, index, derivData.xy, derivData.zw);
        float4 albedoY = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uvY, index, derivData.xy, derivData.zw);
        float4 albedoZ = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uvZ, index, derivData.xy, derivData.zw);
        sampledAlbedoPacked = albedoX * triplanarBlend.x + albedoY * triplanarBlend.y + albedoZ * triplanarBlend.z;
        sampledAlbedoPacked.rgb *= colorData.rgb;
        sampledAlbedoPacked.a = pow(abs(sampledAlbedoPacked.a), heightData.x) * heightData.y + heightData.z;

        //Sample the normal packed
        float4 normalX = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uvX, index, derivData.xy, derivData.zw);
        float4 normalY = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uvY, index, derivData.xy, derivData.zw);
        float4 normalZ = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uvZ, index, derivData.xy, derivData.zw);

        sampledNormalPacked = normalX * triplanarBlend.x + normalY * triplanarBlend.y + normalZ * triplanarBlend.z;
        sampledNormalPacked.xy = sampledNormalPacked.xy * 2 - 1;
        sampledNormalPacked.xy *= layerDataA.z;
        sampledNormalPacked.zw = RemapFloat2(sampledNormalPacked.zw, float2(0, 0), float2(1, 1), remapData.xy, remapData.zw);

    }
    else
    {
        UNITY_BRANCH
        if ((int)layerDataA.y)
        {
            float w1, w2, w3;
            float2 vertex1, vertex2, vertex3;
            TriangleGrid(uv, w1, w2, w3, vertex1, vertex2, vertex3);

            float2 uv1 = uv + hash(vertex1);
            float2 uv2 = uv + hash(vertex2);
            float2 uv3 = uv + hash(vertex3);

            float4 albedo1 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uv1, index, derivData.xy, derivData.zw);
            float4 albedo2 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uv2, index, derivData.xy, derivData.zw);
            float4 albedo3 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uv3, index, derivData.xy, derivData.zw);

            sampledAlbedoPacked = (w1 * albedo1) + (w2 * albedo2) + (w3 * albedo3);
            sampledAlbedoPacked.rgb *= colorData.rgb;
            sampledAlbedoPacked.a = pow(abs(sampledAlbedoPacked.a), heightData.x) * heightData.y + heightData.z;

            float4 normal1 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uv1, index, derivData.xy, derivData.zw);
            float4 normal2 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uv2, index, derivData.xy, derivData.zw);
            float4 normal3 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uv3, index, derivData.xy, derivData.zw);

            sampledNormalPacked = (w1 * normal1) + (w2 * normal2) + (w3 * normal3);
            sampledNormalPacked.xy = sampledNormalPacked.xy * 2 - 1;
            sampledNormalPacked.xy *= layerDataA.z;
            sampledNormalPacked.zw = RemapFloat2(sampledNormalPacked.zw, float2(0, 0), float2(1, 1), remapData.xy, remapData.zw);
        }
        else
        {
            //Get Albedo
            sampledAlbedoPacked = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uv, index, derivData.xy, derivData.zw);
            sampledAlbedoPacked.rgb *= colorData.rgb;
            sampledAlbedoPacked.a = pow(abs(sampledAlbedoPacked.a), heightData.x) * heightData.y + heightData.z;

            //Get Normal
            sampledNormalPacked = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uv, index, derivData.xy, derivData.zw);
            sampledNormalPacked.xy = sampledNormalPacked.xy * 2 - 1;
            sampledNormalPacked.xy *= layerDataA.z;
            sampledNormalPacked.zw = RemapFloat2(sampledNormalPacked.zw, float2(0, 0), float2(1, 1), remapData.xy, remapData.zw);
        }
    }
    
}

void SampleLayerDisplacement(float3 worldPos, float3 worldNormal, int indexLOD,
    float2 uvLOD, float4 triPlanarData,
    float4 displacementData, float4 heightData, float4 layerDataA,
    out float2 displacementTessellation, out float heightLOD)
{
    displacementTessellation = float2(0, 0);
    heightLOD = 0;


    UNITY_BRANCH
        if (triPlanarData.z)
        {
            //Approximate version from: https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSwizzle.shader
            float3 triplanarBlend = pow(abs(worldNormal), float3(4,15,4));
            triplanarBlend /= max(dot(triplanarBlend, float3(1, 1, 1)), 0.0001);

            float2 triPlanarSize = triPlanarData.xy;

            //Sample world normal axis planes
            float2 uvX = worldPos.zy / triPlanarSize;
            float2 uvY = worldPos.xz / triPlanarSize;
            float2 uvZ = worldPos.xy / triPlanarSize;

            //Slight offset to prevent mirroring
            uvY += 0.33;
            uvZ += 0.67;

            //Flip normals if the world normal is negative
            float3 axisSign = worldNormal < 0 ? -1 : 1;

            //flip tangents based on the axis
            uvX.x *= axisSign.x;
            uvY.x *= axisSign.y;
            uvZ.x *= -axisSign.z;

            //Get Displacement
            float displacementX = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoArray, sampler_AlbedoArray, uvX, indexLOD, 0).a;
            float displacementY = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoArray, sampler_AlbedoArray, uvY, indexLOD, 0).a;
            float displacementZ = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoArray, sampler_AlbedoArray, uvZ, indexLOD, 0).a;
            float displacement = displacementX * triplanarBlend.x + displacementY * triplanarBlend.y + displacementZ * triplanarBlend.z;
            displacement = pow(abs(displacement), displacementData.x) * displacementData.y + displacementData.z;
            heightLOD = pow(abs(displacement), heightData.x) * heightData.y + heightData.z;
            displacementTessellation = float2(displacement, displacementData.a);
        }
        else
        {
            UNITY_BRANCH
            if ((int)layerDataA.y)
            {
                float w1, w2, w3;
                float2 vertex1, vertex2, vertex3;
                TriangleGrid(uvLOD, w1, w2, w3, vertex1, vertex2, vertex3);

                float2 uv1 = uvLOD + hash(vertex1);
                float2 uv2 = uvLOD + hash(vertex2);
                float2 uv3 = uvLOD + hash(vertex3);

                float displacement1 = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoArray, sampler_AlbedoArray, uv1, indexLOD, 0).a;
                float displacement2 = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoArray, sampler_AlbedoArray, uv2, indexLOD, 0).a;
                float displacement3 = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoArray, sampler_AlbedoArray, uv3, indexLOD, 0).a;

                float displacement = (w1 * displacement1) + (w2 * displacement2) + (w3 * displacement3);
                displacement = pow(abs(displacement), displacementData.x) * displacementData.y + displacementData.z;
                heightLOD = pow(abs(displacement), heightData.x) * heightData.y + heightData.z;
                displacementTessellation = float2(displacement, displacementData.a);
            }
            else
            {

                //Get Displacement
                float displacement = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoArray, sampler_AlbedoArray, uvLOD, indexLOD, 0).a;
                heightLOD = pow(abs(displacement), heightData.x) * heightData.y + heightData.z;
                displacement = pow(abs(displacement), displacementData.x) * displacementData.y + displacementData.z;
                displacementTessellation = float2(displacement, displacementData.a);
            }
        }
    
}


float3 UnpackNormalmapRGAG(float4 packednormal)
{
    packednormal.x *= packednormal.w;
    float3 normal;
    normal.xy = packednormal.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

float3 UnpackNormalTangent(float4 packednormal)
{
#if defined(UNITY_NO_DXT5nm)
    return packednormal.xyz * 2 - 1;
#else
  return UnpackNormalmapRGAG(packednormal);
#endif
}



//--Global Weather Data
//Snow
#ifdef _SNOW_ON
TEXTURE2D(_PW_SnowAlbedoMap);
TEXTURE2D(_PW_SnowNormalMap);
TEXTURE2D(_PW_SnowMaskMap);
float4 _PW_SnowDataA;
float4 _PW_SnowDataB;
float4 _PW_SnowColor;
float4 _PW_SnowMaskRemapMin;
float4 _PW_SnowMaskRemapMax;
float4 _PW_SnowHeightData;
float4 _PW_SnowDisplacementData;
#endif

//Rain
#ifdef _RAIN_ON
float4 _PW_RainDataA;
float4 _PW_RainDataB;
TEXTURE2D(_PW_RainMap);
#endif

#ifdef _RAIN_ON
float generateRain(float pointSample, float speed, float offset)
{
    float time = frac((_Time.y * speed) + offset);
    float rainGen = sin(lerp(0, -20, time) * pointSample);
    float timeMask = sin(time * 3.1415);

    return saturate(rainGen * timeMask);
}

float sampleRain(float2 uv, float4 pointData)
{
    float rainA = generateRain(pointData.r, _PW_RainDataB.r, 0);
    float rainB = generateRain(pointData.g, _PW_RainDataB.r, 0.5);
    float rainC = generateRain(pointData.b, _PW_RainDataB.r, 0.75);
    float rainD = generateRain(pointData.a, _PW_RainDataB.r, 0.25);

    return saturate(rainA + rainB + rainC + rainD);
}

float3 getRainNormals(in float3 worldPosition)
{
    float2 rainCenterUVs = worldPosition.xz / _PW_RainDataB.a;
    float4 centerRainMap = SAMPLE_TEXTURE2D(_PW_RainMap, SamplerState_Linear_Repeat, rainCenterUVs);

    float2 rainXDerivUVs = (worldPosition.xz / _PW_RainDataB.a) + float2(0.001, 0);
    float4 xDerivRainMap = SAMPLE_TEXTURE2D(_PW_RainMap, SamplerState_Linear_Repeat, rainXDerivUVs);

    float2 rainYDerivUVs = (worldPosition.xz / _PW_RainDataB.a) + float2(0, 0.001);
    float4 yDerivRainMap = SAMPLE_TEXTURE2D(_PW_RainMap, SamplerState_Linear_Repeat, rainYDerivUVs);

    //Sample center rain
    float centerRain = sampleRain(rainCenterUVs, centerRainMap);
    float xDerivRain = sampleRain(rainXDerivUVs, xDerivRainMap);
    float yDerivRain = sampleRain(rainYDerivUVs, yDerivRainMap);

    float xDeriv = centerRain - xDerivRain;
    float yDeriv = centerRain - yDerivRain;

    float3 xVector = float3(xDeriv, 1, 0);
    float3 yVector = float3(0, 1, yDeriv);

    return cross(xVector, yVector);
}
#endif

//Sand

#define MAXARRAYELEMENTS 8
#if !defined(_CREATE_ARRAYS_ON)
uniform float4 _HeightData[MAXARRAYELEMENTS];
uniform float4 _UVData[MAXARRAYELEMENTS];
uniform float4 _MaskMapRemapData[MAXARRAYELEMENTS];
uniform float4 _ColorData[MAXARRAYELEMENTS];
uniform float4 _TriPlanarData[MAXARRAYELEMENTS];
uniform float4 _DisplacementData[MAXARRAYELEMENTS];
uniform float4 _LayerDataA[MAXARRAYELEMENTS];
#endif

float snowHeightBlend(float heightA, float heightB, float mask, float blendFactor)
{
    float t = saturate(mask);
    t -= 0.2;
    heightA = heightA * (1 - t);
    heightB = heightB * t;

    float heightStart = max(heightA, heightB) - blendFactor;
    float level1 = max(heightA - heightStart, 0);
    float level2 = max(heightB - heightStart, 0);

    return saturate((0 * level1) + (1 * level2)) / (level1 + level2);
}

void GTSVertexData_float(float3 worldPos, out float globalBlendDistance, out float nearCameraDistance)
{
    //Calculate camera  distance
    float3 CameraToWorldDirection = _WorldSpaceCameraPos.xyz - worldPos;
    float squareMag = dot(CameraToWorldDirection, CameraToWorldDirection);
    nearCameraDistance = 1 - saturate(squareMag * 0.000001);

    globalBlendDistance = saturate(pow(saturate(squareMag * _GlobalBlendData.x), _GlobalBlendData.y));
}

void BlendOverlay(float3 Base, float3 Blend, float Opacity, out float3 Out)
{
    float3 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
    float3 result2 = 2.0 * Base * Blend;
    float3 zeroOrOne = step(Base, 0.5);
    Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
    Out = lerp(Base, Out, Opacity);
}

void GTSLayers_float(float2 terrainUV, float3 worldPos, float3 worldNormal, float2 splatTexelSize,
    float globalBlendDistance, float nearCameraDistance,
    out float3 blendedAlbedo, out float3 blendedNormal, out float4 blendedMask)
{
#ifdef UNITY_INSTANCING_ENABLED
    //Calculate camera  distance
    float3 CameraToWorldDirection = _WorldSpaceCameraPos.xyz - worldPos;
    float squareMag = dot(CameraToWorldDirection, CameraToWorldDirection);
    nearCameraDistance = 1 - saturate(squareMag * 0.000001);
    globalBlendDistance = saturate(pow(saturate(squareMag * _GlobalBlendData.x), _GlobalBlendData.y));
#endif

    float3 worldToObjPos = mul(UNITY_MATRIX_I_M, float4(worldPos, 1.0)).xyz;

    //Create Terrain World UVs
#ifdef UNITY_INSTANCING_ENABLED
    terrainUV = worldPosTerrainUV(worldPos.xz, _TerrainPosSize.xy, _TerrainPosSize.zw);
#else
    terrainUV = _WorldAlignedUVs == 1 ? worldPosTerrainUV(worldPos.xz, _TerrainPosSize.xy, _TerrainPosSize.zw) : terrainUV;
#endif

    float2 sampleCoords = (terrainUV / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;

    //Sample Terrain splatmaps, used for weighting
    float2 controlUV;
    controlUV = (terrainUV * (splatTexelSize - 1) + 0.5) * (1 / splatTexelSize);
    float4 splat = SAMPLE_TEXTURE2D(_Control0, SamplerState_Linear_Clamp, controlUV);
#ifndef _MOBILE_VR_ON
    float4 splat1 = SAMPLE_TEXTURE2D(_Control1, SamplerState_Linear_Clamp, controlUV);
#endif

    //Sample terrain normal map
    float3 worldTerrainNormal = SAMPLE_TEXTURE2D(_WorldNormalMap, SamplerState_Linear_Clamp, sampleCoords).rgb * 2 - 1;

#ifdef _DETAIL_NORMALS_ON

    float3 detailWorldPos = _ObjectSpaceDataA.x == 0 ? worldPos : worldToObjPos;

    //Get Detail UVs
    float2 detailNearUV = detailWorldPos.xz / _DetailNearFarData.x;
    float2 detailFarUV = detailWorldPos.xz / _DetailNearFarData.z;

    //Sample Textures
    float4 detailNearNormalData = SAMPLE_TEXTURE2D(_DetailNormal, SamplerState_Linear_Repeat, detailNearUV);
    detailNearNormalData.xy = (detailNearNormalData.xy * 2 - 1) * _DetailNearFarData.y;
    float4 detailFarNormalData = SAMPLE_TEXTURE2D(_DetailNormal, SamplerState_Linear_Repeat, detailFarUV);
    detailFarNormalData.xy = (detailFarNormalData.xy * 2 - 1) * _DetailNearFarData.w;

    //Blend Near and Far Normals
    float4 blendedDetailNormals = lerp(detailNearNormalData, detailFarNormalData, globalBlendDistance);
    float detailNormalGreyscale = saturate(dot(blendedDetailNormals.xy, blendedDetailNormals.xy));
    float blendedDetailLayerStrength = 1;

#endif

#ifndef _MOBILE_VR_ON
    //Declare weight splat array
    float weightSplats[] = { splat.r, splat.g, splat.b, splat.a, splat1.r, splat1.g, splat1.b, splat1.a };

    //Sample top 4 splats
    float4 splatmapIndexed = SAMPLE_TEXTURE2D_LOD(_SplatmapIndex, SamplerState_Point_Repeat, controlUV, 0) * 256;
#endif

    float2 dxuv = ddx(terrainUV);
    float2 dyuv = ddy(terrainUV);
    float4 uvData;
    float4 deriv;
    float4 finalNormalPackedBlend = float4(0, 0, 0, 0);

#ifndef _MOBILE_VR_ON
    //Declare re-useable variables
    int currentIndex;
    float2 layerUV;
    float4 currentRemapData;
    float4 combinedAlbedoPacked;
#endif

    //Geo Layers
#ifdef _GEOLOGICAL_ON
    float blendedGeoLayerStrength = 1;
#endif

#ifdef _MOBILE_VR_ON
    int index = 0;
    float2 uvA = layerUVs(terrainUV, _LayerST0);
    deriv = float4(dxuv * _LayerST0.x, dyuv * _LayerST0.y);
    float4 albedoA = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uvA, index, deriv.xy, deriv.zw);
    float4 normalA = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uvA, index, deriv.xy, deriv.zw);
    albedoA.rgb *= _ColorData[index];
    normalA.xy = normalA.xy * 2 - 1;
    normalA.xy *= _LayerDataA[index].z;
    normalA.zw = RemapFloat2(normalA.zw, float2(0, 0), float2(1, 1), _MaskMapRemapData[index].xy, _MaskMapRemapData[index].zw);
    float weightA = splat.r;

    index = 1;
    float2 uvB = layerUVs(terrainUV, _LayerST1);
    deriv = float4(dxuv * _LayerST1.x, dyuv * _LayerST1.y);
    float4 albedoB = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uvB, index, deriv.xy, deriv.zw);
    float4 normalB = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uvB, index, deriv.xy, deriv.zw);
    albedoB.rgb *= _ColorData[index];
    normalB.xy = normalB.xy * 2 - 1;
    normalB.xy *= _LayerDataA[index].z;
    normalB.zw = RemapFloat2(normalB.zw, float2(0, 0), float2(1, 1), _MaskMapRemapData[index].xy, _MaskMapRemapData[index].zw);
    float weightB = splat.g;

    index = 2;
    float2 uvC = layerUVs(terrainUV, _LayerST2);
    deriv = float4(dxuv * _LayerST2.x, dyuv * _LayerST2.y);
    float4 albedoC = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uvC, index, deriv.xy, deriv.zw);
    float4 normalC = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uvC, index, deriv.xy, deriv.zw);
    albedoC.rgb *= _ColorData[index];
    normalC.xy = normalC.xy * 2 - 1;
    normalC.xy *= _LayerDataA[index].z;
    normalC.zw = RemapFloat2(normalC.zw, float2(0, 0), float2(1, 1), _MaskMapRemapData[index].xy, _MaskMapRemapData[index].zw);
    float weightC = splat.b;

    index = 3;
    float2 uvD = layerUVs(terrainUV, _LayerST3);
    deriv = float4(dxuv * _LayerST3.x, dyuv * _LayerST3.y);
    float4 albedoD = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoArray, sampler_AlbedoArray, uvD, index, deriv.xy, deriv.zw);
    float4 normalD = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalArray, sampler_NormalArray, uvD, index, deriv.xy, deriv.zw);
    albedoD.rgb *= _ColorData[index];
    normalD.xy = normalD.xy * 2 - 1;
    normalD.xy *= _LayerDataA[index].z;
    normalD.zw = RemapFloat2(normalD.zw, float2(0, 0), float2(1, 1), _MaskMapRemapData[index].xy, _MaskMapRemapData[index].zw);
    float weightD = splat.a;

    blendedAlbedo = (albedoA * weightA + albedoB * weightB + albedoC * weightC + albedoD * weightD).rgb;
    finalNormalPackedBlend = normalA * weightA + normalB * weightB + normalC * weightC + normalD * weightD;
    blendedMask = float4(0, finalNormalPackedBlend.b, 0, finalNormalPackedBlend.a);

    
#else
    //Single Layer Sample
    float4 lowResAlbedo = SAMPLE_TEXTURE2D(_SplatmapIndexLowRes, SamplerState_Linear_Clamp, terrainUV);

    //Only sample top 4 if within certain distance.
    UNITY_BRANCH
        if (nearCameraDistance > 0)
        {
            float4 albedoA = float4(0, 0, 0, 0);
            float4 normalPackedBlendA = float4(0, 0, 1, 0);
            float4 albedoB = float4(0, 0, 0, 0);
            float4 normalPackedBlendB = float4(0, 0, 1, 0);
            float4 albedoC = float4(0, 0, 0, 0);
            float4 normalPackedBlendC = float4(0, 0, 1, 0);
            float4 albedoD = float4(0, 0, 0, 0);
            float4 normalPackedBlendD = float4(0, 0, 1, 0);
            float4 splatControl = float4(0, 0, 0, 0);

            //Layer A
            currentIndex = round(splatmapIndexed.x);
#ifdef _DETAIL_NORMALS_ON
            float4 detailStrengthControl = float4(0, 0, 0, 0);
#endif
#ifdef _GEOLOGICAL_ON
            float4 geoStrengthControl = float4(0, 0, 0, 0);
#endif
            UNITY_BRANCH
            if (weightSplats[currentIndex] > 0)
            {
                float4 layerDataA_A = _LayerDataA[currentIndex];
                uvData = _UVData[currentIndex];
                layerUV = layerUVs(terrainUV, uvData);
                deriv = float4(dxuv * uvData.x, dyuv * uvData.y);
                //Sample Current Layer
                SampleLayerTexture(worldPos, worldNormal, currentIndex,
                    layerUV, deriv, _TriPlanarData[currentIndex],
                    _MaskMapRemapData[currentIndex], _HeightData[currentIndex], _ColorData[currentIndex], layerDataA_A,
                    albedoA, normalPackedBlendA);
                //Set new splat weights Geo
#ifdef _GEOLOGICAL_ON
                geoStrengthControl.x = layerDataA_A.x;
#endif
#ifdef _DETAIL_NORMALS_ON
                detailStrengthControl.x = layerDataA_A.w;
#endif
            }
            
            //Layer A
            //Set new splat weights
            splatControl.x = (weightSplats[currentIndex]);


            //Layer B
            currentIndex = round(splatmapIndexed.y);
            UNITY_BRANCH
            if (weightSplats[currentIndex] > 0)
            {
                float4 layerDataA_B = _LayerDataA[currentIndex];
                uvData = _UVData[currentIndex];
                layerUV = layerUVs(terrainUV, uvData);
                deriv = float4(dxuv * uvData.x, dyuv * uvData.y);
                float heightLODB;
                //Sample Current Layer
                SampleLayerTexture(worldPos, worldNormal, currentIndex,
                    layerUV, deriv, _TriPlanarData[currentIndex],
                    _MaskMapRemapData[currentIndex], _HeightData[currentIndex], _ColorData[currentIndex], layerDataA_B,
                    albedoB, normalPackedBlendB);
#ifdef _GEOLOGICAL_ON
                geoStrengthControl.y = layerDataA_B.x;
#endif
#ifdef _DETAIL_NORMALS_ON
                detailStrengthControl.y = layerDataA_B.w;
#endif
            }

            //Set new splat weights
            splatControl.y = (weightSplats[currentIndex]);

            //Set new splat weights Geo

            //Layer C
            currentIndex = round(splatmapIndexed.z);
            UNITY_BRANCH
            if (weightSplats[currentIndex] > 0)
            {
                float4 layerDataA_C = _LayerDataA[currentIndex];
                uvData = _UVData[currentIndex];
                layerUV = layerUVs(terrainUV, uvData);
                deriv = float4(dxuv * uvData.x, dyuv * uvData.y);
                //Sample Current Layer
                SampleLayerTexture(worldPos, worldNormal, currentIndex,
                    layerUV, deriv, _TriPlanarData[currentIndex],
                    _MaskMapRemapData[currentIndex], _HeightData[currentIndex], _ColorData[currentIndex], layerDataA_C,
                    albedoC, normalPackedBlendC);
                //Set new splat weights Geo
#ifdef _GEOLOGICAL_ON
                geoStrengthControl.z = layerDataA_C.x;
#endif
#ifdef _DETAIL_NORMALS_ON
                detailStrengthControl.z = layerDataA_C.w;
#endif
            }

            //Set new splat weights
            splatControl.z = (weightSplats[currentIndex]);



            //Layer D
            currentIndex = round(splatmapIndexed.w);
            UNITY_BRANCH
            if (weightSplats[currentIndex] > 0)
            {
                float4 layerDataA_D = _LayerDataA[currentIndex];
                uvData = _UVData[currentIndex];
                layerUV = layerUVs(terrainUV, uvData);
                deriv = float4(dxuv * uvData.x, dyuv * uvData.y);
                //Sample Current Layer
                SampleLayerTexture(worldPos, worldNormal, currentIndex,
                    layerUV, deriv, _TriPlanarData[currentIndex],
                    _MaskMapRemapData[currentIndex], _HeightData[currentIndex], _ColorData[currentIndex], layerDataA_D,
                    albedoD, normalPackedBlendD);

                //Set new splat weights Geo
#ifdef _GEOLOGICAL_ON
                geoStrengthControl.w = layerDataA_D.x;
#endif
#ifdef _DETAIL_NORMALS_ON
                detailStrengthControl.w = layerDataA_D.w;
#endif
            }


            //Set new splat weights
            splatControl.w = (weightSplats[currentIndex]);



#ifdef _HEIGHT_BLEND_ON
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
#endif
            //Blend Albedo and Normals based on splats
            combinedAlbedoPacked = albedoA * splatControl.x + albedoB * splatControl.y + albedoC * splatControl.z + albedoD * splatControl.w;
            blendedAlbedo = combinedAlbedoPacked.rgb;
            finalNormalPackedBlend = normalPackedBlendA * splatControl.x + normalPackedBlendB * splatControl.y + normalPackedBlendC * splatControl.z + normalPackedBlendD * splatControl.a;
            //Set final blended mask
            blendedMask = float4(0, finalNormalPackedBlend.z, 0, finalNormalPackedBlend.a);

            float blendDist = saturate(nearCameraDistance * 2);

            blendedAlbedo = lerp(lowResAlbedo.rgb, blendedAlbedo, blendDist);
            finalNormalPackedBlend = lerp(float4(0, 0, 0, 0), finalNormalPackedBlend, blendDist);
            blendedMask = lerp(float4(0, 1, 0, lowResAlbedo.a), blendedMask, blendDist);

            //Blend Geo Layer Strengths
#ifdef _GEOLOGICAL_ON
            blendedGeoLayerStrength = geoStrengthControl.x * splatControl.x + geoStrengthControl.y * splatControl.y + geoStrengthControl.z * splatControl.z + geoStrengthControl.w * splatControl.w;
            blendedGeoLayerStrength = lerp(0, blendedGeoLayerStrength, blendDist);
#endif
            //Blend Detail Layer Strengths
#ifdef _DETAIL_NORMALS_ON
            blendedDetailLayerStrength = detailStrengthControl.x * splatControl.x + detailStrengthControl.y * splatControl.y + detailStrengthControl.z * splatControl.z + detailStrengthControl.w * splatControl.w;
            blendedDetailLayerStrength = lerp(1, blendedDetailLayerStrength, blendDist);
#endif
        }
        else
        {
            blendedAlbedo = lowResAlbedo.rgb;
            finalNormalPackedBlend = float4(0, 0, 0, 0);
            blendedMask = float4(0, 1, 0, lowResAlbedo.a);
#ifdef _GEOLOGICAL_ON
            blendedGeoLayerStrength = 0;
#endif
        }
#endif
    

#ifdef _COLORMAP_ON
        //Sample the textures
        float4 colormap = SAMPLE_TEXTURE2D(_ColormapTex, SamplerState_Linear_Repeat, terrainUV);

        //Create alpha
        float colormapAlpha = saturate(colormap.a * _ColormapDataA.x);

        //Create near far intensities
        float colorIntensity = lerp(_ColormapNearFarData.x, _ColormapNearFarData.y, globalBlendDistance);
        colormap.rgb = saturate(colormap.rgb * _ColormapDataA.y);

        //Output final blend
        blendedAlbedo = lerp(blendedAlbedo, colormap.rgb, colormapAlpha * colorIntensity);
#endif

#ifdef _GEOLOGICAL_ON
        float3 geoWorldPos = _ObjectSpaceDataA.y == 0 ? worldPos : worldToObjPos;
        float2 geoNormals = float2(0, 0);
        //Sample near geo data
        float2 nearGeoUVs = float2(0, (geoWorldPos.y / _GeoNearData.y) + _GeoNearData.z);
        float3 nearGeoMap = SAMPLE_TEXTURE2D(_GeoMap, SamplerState_Linear_Repeat, nearGeoUVs).rgb * 2;
        float2 nearGeoNormal = (SAMPLE_TEXTURE2D(_GeoNormal, SamplerState_Linear_Repeat, nearGeoUVs).ag * 2 - 1) * _GeoNearData.w;

        //Sample far geo data
        float2 farGeoUVs = float2(0, (geoWorldPos.y / _GeoFarData.y) + _GeoFarData.z);
        float3 farGeoMap = SAMPLE_TEXTURE2D(_GeoMap, SamplerState_Linear_Repeat, farGeoUVs).rgb * 2;
        float2 farGeoNormal = (SAMPLE_TEXTURE2D(_GeoNormal, SamplerState_Linear_Repeat, farGeoUVs).ag * 2 - 1) * _GeoFarData.w;

        //Blend geo data
        float3 nearGeoBlend = ((nearGeoMap + float3(-0.3, -0.3, -0.3)) * _GeoNearData.x);
        float3 farGeoBlend = ((farGeoMap + float3(-0.3, -0.3, -0.3)) * _GeoFarData.x);
        float3 geoBlend = lerp(nearGeoBlend, farGeoBlend, globalBlendDistance) * blendedGeoLayerStrength;
        blendedAlbedo = saturate(blendedAlbedo + geoBlend);

        geoNormals = lerp(nearGeoNormal, farGeoNormal, globalBlendDistance) * blendedGeoLayerStrength;

#endif

        //Final Normal Calculation
        blendedNormal.xy = finalNormalPackedBlend.xy;

        #ifdef _DETAIL_NORMALS_ON
            blendedNormal.xy += blendedDetailNormals.xy * blendedDetailLayerStrength;
        #endif
        #ifdef _GEOLOGICAL_ON
            blendedNormal.xy += geoNormals;
        #endif


        //Reconstruct Tangent Z Component from Combined XY Normals
        blendedNormal.z = sqrt(1.0 - saturate(dot(blendedNormal.rg, blendedNormal.rg)));
        blendedNormal = normalize(blendedNormal);

        //Blend Tangent Space Normals on the World Terrain Normal 
        blendedNormal = worldSpaceNormalBlend(worldTerrainNormal, blendedNormal);

        //Add detail into the snow
#ifdef _DETAIL_NORMALS_ON
        blendedAlbedo *= lerp(0.5, 1, 1 - (detailNormalGreyscale * blendedDetailLayerStrength));
#endif

#ifdef _SNOW_ON
        float3 snowWorldPos = _ObjectSpaceDataA.z == 0 ? worldPos : worldToObjPos;

        //Sample snow textures
        float2 snowUV = worldPos.xz / _PW_SnowDataB.r;
        float3 snowAlbedo = SAMPLE_TEXTURE2D(_PW_SnowAlbedoMap, SamplerState_Linear_Repeat, snowUV).rgb;
        float3 snowNormal = UnpackNormalTangent(SAMPLE_TEXTURE2D(_PW_SnowNormalMap, SamplerState_Linear_Repeat, snowUV));
        snowNormal.xy *= _PW_SnowDataB.w;
        float4 snowMask = SAMPLE_TEXTURE2D(_PW_SnowMaskMap, SamplerState_Linear_Repeat, snowUV);
        snowMask = RemapFloat4(snowMask, float4(0, 0, 0, 0), float4(1, 1, 1, 1), _PW_SnowMaskRemapMin, _PW_SnowMaskRemapMax);

        snowMask.b = pow(abs(snowMask.b), _PW_SnowHeightData.x) * _PW_SnowHeightData.y + _PW_SnowHeightData.z;

        //Create snow starting level
        float snowStart = ((snowWorldPos.g - _PW_SnowDataA.z));
        snowStart = smoothstep(-_PW_SnowDataB.y, _PW_SnowDataB.y, snowStart);

        //Create snow slope mask
        float snowSlopeAge = lerp(1, 10, _PW_SnowDataA.a);
        float slopeMask = saturate(pow(abs(worldTerrainNormal.g), _PW_SnowDataB.z * snowSlopeAge));

#ifdef _DETAIL_NORMALS_ON
        snowAlbedo *= lerp(0.2, 1, 1 - detailNormalGreyscale);
#endif

        //Create Final Snow Mask
        float finalSnowMask = snowStart * saturate(slopeMask * 4) * (1-_PW_SnowDataA.a);
        finalSnowMask *= _PW_SnowDataA.g;

        //Lerp Snow
        blendedAlbedo = lerp(blendedAlbedo, snowAlbedo * _PW_SnowColor.rgb, finalSnowMask);
        blendedMask = lerp(blendedMask, snowMask, finalSnowMask);

        //Snow Normal
        snowNormal = worldSpaceNormalBlend(worldTerrainNormal, snowNormal);
        blendedNormal = lerp(blendedNormal, snowNormal, finalSnowMask);

#endif
    
#ifdef _RAIN_ON
    //Rain
    float topMask = saturate(pow(worldTerrainNormal.g, 4));
    float blendRange = 30;
    float rainStart = ((worldPos.g - _PW_RainDataA.z));
    rainStart = smoothstep(-blendRange, blendRange, rainStart);
    float rainEnd = ((worldPos.g - _PW_RainDataA.w));
    rainEnd = smoothstep(-blendRange, blendRange, rainEnd);
    rainStart = saturate(rainStart);
    rainEnd = saturate(rainEnd);
    float rainRange = rainStart * (1 - rainEnd);
    float rainMask = _PW_RainDataA.y * rainRange;
    
    blendedAlbedo = lerp(blendedAlbedo, blendedAlbedo * _PW_RainDataB.y, rainMask);

    float3 rainNormals = getRainNormals(worldPos);
    float rainNormalStrength = lerp(0, 0.6, rainMask);
    rainNormals = float3(rainNormals.rg * rainNormalStrength, lerp(1, rainNormals.b, saturate(rainNormalStrength)));
    float3 rainNormalBlend = worldSpaceNormalBlend(blendedNormal, rainNormals);
    blendedNormal = lerp(blendedNormal, rainNormalBlend, rainMask  * topMask);
    float rainSmoothness = lerp(.4, _PW_RainDataB.z, topMask);
    blendedMask.a = lerp(blendedMask.a, rainSmoothness, rainMask);
#endif
    
#ifdef _MACRO_VARIATION_ON
        float3 variationWorldPos = _ObjectSpaceDataA.w == 0 ? worldPos : worldToObjPos;
        //Sample variation offsets
        float2 variationUVA = variationWorldPos.xz * _MacroVariationData.r;
        float2 variationUVB = variationWorldPos.xz * _MacroVariationData.g;
        float2 variationUVC = variationWorldPos.xz * _MacroVariationData.b;

        //Sample variation texture at different uv scales
        float4 variationA = SAMPLE_TEXTURE2D(_MacroVariationMap, SamplerState_Linear_Repeat, variationUVA);
        float4 variationB = SAMPLE_TEXTURE2D(_MacroVariationMap, SamplerState_Linear_Repeat, variationUVB);
        float4 variationC = SAMPLE_TEXTURE2D(_MacroVariationMap, SamplerState_Linear_Repeat, variationUVC);

        //Blend variation
        float variation = saturate((variationA.r + 0.5) * (variationB.r + 0.5) * (variationC.r + 0.5));

        //Add variation
        blendedAlbedo = blendedAlbedo * lerp(_MacroVariationData.w, 1, variation);
#endif 

}

void GTSLayersDisplacement_float(float2 terrainUV, float3 worldPos, float3 worldNormal, float2 splatTexelSize,
    out float blendedDisplacement, out float blendedTessellation)
{
    
    blendedDisplacement = 0;
    blendedTessellation = 1;
    
#ifdef _TESSELLATION_ON
    //Create Terrain World UVs
    terrainUV = worldPosTerrainUV(worldPos.xz, _TerrainPosSize.xy, _TerrainPosSize.zw);

    //Calculate camera distance
    float3 CameraToWorldDirection = _WorldSpaceCameraPos.xyz - worldPos;
    float squareMag = dot(CameraToWorldDirection, CameraToWorldDirection);
    float nearCameraDistance = 1 - saturate(squareMag * 0.00001);

    float2 sampleCoords = (terrainUV / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
    float3 worldTerrainNormal = SAMPLE_TEXTURE2D_LOD(_WorldNormalMap, SamplerState_Linear_Clamp, terrainUV, 0).rgb * 2 - 1;

    //Sample Terrain splatmaps, used for weighting
    float2 controlUV;
    controlUV = (terrainUV * (splatTexelSize - 1) + 0.5) * (1 / splatTexelSize);
    float4 splatLOD = SAMPLE_TEXTURE2D_LOD(_Control0, SamplerState_Linear_Clamp, controlUV, 0);
    float4 splatLOD1 = SAMPLE_TEXTURE2D_LOD(_Control1, SamplerState_Linear_Clamp, controlUV, 0);

    //Per Layer Data Arrays, so our index map can index into them
    float weightSplatsLOD[] = { splatLOD.r, splatLOD.g, splatLOD.b, splatLOD.a, splatLOD1.r, splatLOD1.g, splatLOD1.b, splatLOD1.a };

    //Sample top 4 splats
    float4 splatmapIndexedLOD = SAMPLE_TEXTURE2D_LOD(_SplatmapIndex, SamplerState_Point_Repeat, controlUV, 0) * 256;

    //Declare re-useable variables
    int currentIndexLOD;
    float2 layerUVLOD;

            UNITY_BRANCH
            if (nearCameraDistance > 0)
            {
                float4 splatControlLOD = float4(0, 0, 0, 0);
                float2 displacementTessellationA = float2(0,0);
                float heightLODA;
                float2 displacementTessellationB = float2(0,0);
                float heightLODB;
                float2 displacementTessellationC = float2(0,0);
                float heightLODC;
                float2 displacementTessellationD = float2(0,0);
                float heightLODD;

                //Layer A
                currentIndexLOD = round(splatmapIndexedLOD.x);
                UNITY_BRANCH
                if (weightSplatsLOD[currentIndexLOD] > 0)
                {
                    layerUVLOD = layerUVs(terrainUV, _UVData[currentIndexLOD]);


                    SampleLayerDisplacement(worldPos, worldNormal, currentIndexLOD,
                        layerUVLOD, _TriPlanarData[currentIndexLOD],
                        _DisplacementData[currentIndexLOD], _HeightData[currentIndexLOD], _LayerDataA[currentIndexLOD],
                        displacementTessellationA, heightLODA);
                    splatControlLOD.x = (weightSplatsLOD[currentIndexLOD]);
                }
                

                //Layer B
                currentIndexLOD = round(splatmapIndexedLOD.y);
                UNITY_BRANCH
                if (weightSplatsLOD[currentIndexLOD] > 0)
                {
                    layerUVLOD = layerUVs(terrainUV, _UVData[currentIndexLOD]);



                    SampleLayerDisplacement(worldPos, worldNormal, currentIndexLOD,
                        layerUVLOD, _TriPlanarData[currentIndexLOD],
                        _DisplacementData[currentIndexLOD], _HeightData[currentIndexLOD], _LayerDataA[currentIndexLOD],
                        displacementTessellationB, heightLODB);
                    splatControlLOD.y = (weightSplatsLOD[currentIndexLOD]);
                }
                

                //Layer C
                currentIndexLOD = round(splatmapIndexedLOD.z);
                UNITY_BRANCH
                if (weightSplatsLOD[currentIndexLOD] > 0)
                {
                    layerUVLOD = layerUVs(terrainUV, _UVData[currentIndexLOD]);


                    SampleLayerDisplacement(worldPos, worldNormal, currentIndexLOD,
                        layerUVLOD, _TriPlanarData[currentIndexLOD],
                        _DisplacementData[currentIndexLOD], _HeightData[currentIndexLOD], _LayerDataA[currentIndexLOD],
                        displacementTessellationC, heightLODC);

                    splatControlLOD.z = (weightSplatsLOD[currentIndexLOD]);
                }
                

                //Layer D
                currentIndexLOD = round(splatmapIndexedLOD.w);
                if (weightSplatsLOD[currentIndexLOD] > 0)
                {
                    layerUVLOD = layerUVs(terrainUV, _UVData[currentIndexLOD]);


                    SampleLayerDisplacement(worldPos, worldNormal, currentIndexLOD,
                        layerUVLOD, _TriPlanarData[currentIndexLOD],
                        _DisplacementData[currentIndexLOD], _HeightData[currentIndexLOD], _LayerDataA[currentIndexLOD],
                        displacementTessellationD, heightLODD);

                    splatControlLOD.w = (weightSplatsLOD[currentIndexLOD]);
                }



                float2 displacementTessellation = float2(0, 0);
                displacementTessellation = displacementTessellationA * splatControlLOD.x + displacementTessellationB * splatControlLOD.y + displacementTessellationC * splatControlLOD.z + displacementTessellationD * splatControlLOD.w;
                blendedDisplacement = displacementTessellation.x;
                blendedTessellation = displacementTessellation.y;
            }
            else
            {
                blendedDisplacement = 0;
                blendedTessellation = 0;
            }
        
    
#ifdef _SNOW_ON
    //Sample snow textures
    float2 snowUV = worldPos.xz / _PW_SnowDataB.r;

    float snowHeight = SAMPLE_TEXTURE2D_LOD(_PW_SnowMaskMap, SamplerState_Linear_Repeat, snowUV, 0).b;
    snowHeight = pow(abs(snowHeight), _PW_SnowHeightData.x) * _PW_SnowHeightData.y + _PW_SnowHeightData.z;
    float snowDisplacement = pow(abs(snowHeight), _PW_SnowDisplacementData.x) * _PW_SnowDisplacementData.y + _PW_SnowDisplacementData.z;
    float snowTessellation = _PW_SnowDisplacementData.w;

    //Create snow starting level
    float snowStartLOD = ((worldPos.g - _PW_SnowDataA.z));
    snowStartLOD = smoothstep(-_PW_SnowDataB.y, _PW_SnowDataB.y, snowStartLOD);

    //Create snow slope mask
    float slopeMaskLOD = saturate(pow(abs(worldTerrainNormal.g), _PW_SnowDataB.z));

    //Create Final Snow Mask
    float finalSnowMaskLOD = snowStartLOD * saturate(slopeMaskLOD * 4) * (1-_PW_SnowDataA.a);
    finalSnowMaskLOD *= _PW_SnowDataA.g;

    blendedDisplacement = lerp(blendedDisplacement, snowDisplacement * _PW_SnowDataA.g, finalSnowMaskLOD);
    blendedTessellation = lerp(blendedTessellation, snowTessellation * _PW_SnowDataA.g, finalSnowMaskLOD);

#endif
    blendedTessellation *= _TessellationMultiplier;
#endif
}

#endif