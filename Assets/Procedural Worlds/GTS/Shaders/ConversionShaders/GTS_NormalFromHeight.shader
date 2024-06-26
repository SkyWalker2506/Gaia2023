Shader "Hidden/GTS_NormalFromHeight"
{
    Properties
    {
        _HeightMap ("Texture", 2D) = "white" {}
        _Epsilon ("Epsilon", Float) = 0.01
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

            sampler2D _HeightMap;
            float _Epsilon;

            fixed4 frag(v2f i) : SV_Target
            {

                //float originalHeight = tex2D(_HeightMap, i.uv).r;
                //float2 ddxUV = float2(_Epsilon, 0);
                //float2 ddyUV = float2(0, _Epsilon);
                //
                //float ddxHeight = tex2D(_HeightMap, i.uv + ddxUV).r;
                //float ddyHeight = tex2D(_HeightMap, i.uv + ddyUV).r;
                //
                //ddxHeight = originalHeight - ddxHeight;
                //ddyHeight = originalHeight - ddyHeight;
                //
                //float4 finalNormal = float4(ddxHeight, ddyHeight, 0 ,0);
                //finalNormal.z = sqrt(1.0 - saturate(dot(finalNormal.rg, finalNormal.rg)));

                //finalNormal.xy = finalNormal.xy * 0.5 + 0.5;

                //float4 finalNormal = float4(ddxHeight, ddyHeight, 1, 0);

                //return finalNormal;

                float2 uv = i.uv;

                //uv = uv * (1 + (0.5));

                uv *= .998;

                float ep = 0.01;

                float2 du = float2(ep, 0);
                float u1 = tex2D(_HeightMap, uv - du);
                float u2 = tex2D(_HeightMap, uv + du);
                float3 tu = float3(1, u2 - u1, 0);

                float2 dv = float2(0, ep);
                float v1 = tex2D(_HeightMap, uv - dv);
                float v2 = tex2D(_HeightMap, uv + dv);
                float3 tv = float3(0, v2 - v1, 1);

                float3 norm = cross(tv, tu);
                norm = normalize(norm);

                norm = float3(norm.x, norm.z, 1);

                norm.xy *= 50;

                return float4(norm.rgb, 1);

                }
            ENDCG
        }
    }
}
