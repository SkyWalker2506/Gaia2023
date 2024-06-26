Shader "Hidden/GTS_TerrainWorldNormalCurvature"
{
    Properties
    {
        _WorldNormal ("World Normal", 2D) = "white" {}
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

            Texture2D _WorldNormal;

            SamplerState my_linear_clamp_sampler;

            fixed4 frag(v2f i) : SV_Target
            {
                //float3 worldNormal = tex2D(_WorldNormal, i.uv).rgb * 2 - 1;

                float3 worldNormal = _WorldNormal.Sample(my_linear_clamp_sampler, i.uv).rgb * 2 - 1;
                

                //Create Curvature
                float deriv = 0.01;
                float2 ddxTerrainUV = i.uv + float2(deriv, 0);
                float2 ddyTerrainUV = i.uv + float2(0, deriv);

                float3 ddxTerrain = _WorldNormal.Sample(my_linear_clamp_sampler, ddxTerrainUV).rgb * 2 - 1;
                float3 ddyTerrain = _WorldNormal.Sample(my_linear_clamp_sampler, ddyTerrainUV).rgb * 2 - 1;

                //float3 ddxTerrain = tex2D(_WorldNormal, ddxTerrainUV).rgb * 2 - 1;
                //float3 ddyTerrain = tex2D(_WorldNormal, ddyTerrainUV).rgb * 2 - 1;
                ddxTerrain = worldNormal - ddxTerrain;
                ddyTerrain = worldNormal - ddyTerrain;

                float fwidthNormal = abs(ddxTerrain) + abs(ddyTerrain);
                float curvature = saturate(length(fwidthNormal) * 10);

                //return float4(tex2D(_WorldNormal, i.uv).rgb, curvature);
                return float4(_WorldNormal.Sample(my_linear_clamp_sampler, i.uv).rgb, curvature);

            }
            ENDCG
        }
    }
}
