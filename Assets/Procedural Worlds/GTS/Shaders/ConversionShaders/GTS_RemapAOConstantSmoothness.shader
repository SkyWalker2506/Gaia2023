Shader "Hidden/GTS_RemapAOConstantSmoothness"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskMap("MaskMap", 2D) = "white" {}
        _AORemap("AORemap", Vector) = (1,1,1,1)
        _SmoothnessRemap("SmoothnessRemap", Vector) = (1,1,1,1)
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
            sampler2D _MaskMap;
            float2 _AORemap;
            float2 _SmoothnessRemap;

            float Remap(float value, float from1, float to1, float from2, float to2) 
            {
                return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            }

            fixed4 frag (v2f i) : SV_Target
            {


                fixed4 maskMap = tex2D(_MaskMap, i.uv);


                float remappedAO = Remap(maskMap.g, 0 , 1, _AORemap.x, _AORemap.y);
                float remappedSmoothness = Remap(1, 0, 1, _SmoothnessRemap.x, _SmoothnessRemap.y);

                return float4(0,remappedAO,0, remappedSmoothness);

            }
            ENDCG
        }
    }
}
