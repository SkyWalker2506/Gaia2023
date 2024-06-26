#ifndef PW_SNOWFUNCTIONS_INCLUDED
#define PW_SNOWFUNCTIONS_INCLUDED

#define HALF_MIN 6.103515625e-5

float _PW_Global_CoverLayer1FadeDist;
float4 _PW_SnowDataA;
float4 _PW_SnowColor;
float _PW_Global_CoverLayer1Progress;

void GetSnow_float(in float3 worldPos, in float3 albedo, out float3 base)
{
    float snowStart = ((worldPos.g - _PW_SnowDataA.z));
    half sgn   = max ( sign ( snowStart ), 0 );
    half fade  = clamp ( length ( snowStart ) / max ( _PW_Global_CoverLayer1FadeDist, HALF_MIN ), 0.0, 1.0 ) * sgn;
    float snowMask = lerp(0, .1, fade) * _PW_Global_CoverLayer1Progress;
    base = lerp(albedo, saturate(albedo + _PW_SnowColor.xyz), snowMask);
}

void GetSnow_half(in float3 worldPos, in float3 albedo, out float3 base)
{
    float snowStart = ((worldPos.g - _PW_SnowDataA.z));
    half sgn   = max ( sign ( snowStart ), 0 );
    half fade  = clamp ( length ( snowStart ) / max ( _PW_Global_CoverLayer1FadeDist, HALF_MIN ), 0.0, 1.0 ) * sgn;
    float snowMask = lerp(0, .1, fade) * _PW_Global_CoverLayer1Progress;
    base = lerp(albedo, saturate(albedo + _PW_SnowColor.xyz), snowMask);
}

#endif
