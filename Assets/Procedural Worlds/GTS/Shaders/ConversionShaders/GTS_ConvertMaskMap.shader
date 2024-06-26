Shader "Hidden/GTS_ConvertMaskMap"
{
    Properties
    {
        _Albedo ("Albedo", 2D) = "white" {}
        _HasAlbedo ("HasAlbedo", Int) = 1
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

            sampler2D _Albedo;
            int _HasAlbedo;

            fixed4 frag(v2f i) : SV_Target
            {

                float4 mask = float4(0,1,0,0.1);
                
                if (_HasAlbedo)
                {
                    fixed4 albedo = tex2D(_Albedo, i.uv);
                    mask.a = albedo.a;
                }

                return mask;
            }
            ENDCG
        }
    }
}
