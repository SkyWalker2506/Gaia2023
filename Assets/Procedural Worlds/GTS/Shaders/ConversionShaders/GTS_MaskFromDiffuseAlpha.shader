Shader "Hidden/GTS_MaskFromDiffuseAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DiffuseTex("DiffuseTex", 2D) = "white" {}
        _AO("AO", Float) = 1
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

            sampler2D _MainTex;
            sampler2D _DiffuseTex;
            float _AO;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 finalMask = float4(0,0,0,0);

                float diffuseAlpha = tex2D(_DiffuseTex, i.uv).a;

                return float4(0,_AO,0, diffuseAlpha);
            }
            ENDCG
        }
    }
}
