Shader "Hidden/GTS_CombineNormalMask"
{
    Properties
    {
        _Normal ("Normal", 2D) = "white" {}
        _MaskMap ("MaskMap", 2D) = "white" {}
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

            sampler2D _Normal;
            sampler2D _MaskMap;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 normal = tex2D(_Normal, i.uv);
                fixed4 mask = tex2D(_MaskMap, i.uv);

                return float4(normal.a, normal.y, mask.g, mask.a);
            }
            ENDCG
        }
    }
}
