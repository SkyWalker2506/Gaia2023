Shader "Hidden/GTS_RemapSmoothness"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Remap("Remap", Vector) = (0,1, 0, 0)
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
            float2 _Remap;

            float Remap(float value, float from1, float to1, float from2, float to2) 
            {
                return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // remap smoothness
                col.a = Remap(col.a, 0 , 1, _Remap.x, _Remap.y);


                return col;
            }
            ENDCG
        }
    }
}
