Shader "Hidden/GTS_WeightedIndex"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Splat1("Splat1", 2D) = "black" {}
        _Splat2("Splat2", 2D) = "black" {}
        _Splat3("Splat3", 2D) = "black" {}
        _Splat4("Splat4", 2D) = "black" {}
        _NumSplats("NumSplats", Int) = 2
        _BlurDistance("Blur Distance", Float) = 0.01
        _BlurSteps("BlurSteps", Float) = 64
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


            //Blur
            float4 blur(float2 uv, sampler2D tex, float distance, float steps)
            {
                float4 CurColor = float4(0, 0, 0, 0);
                float2 NewUV = uv;
                int incre = 0;
                float StepSize = distance / (int)steps;
                float CurDistance = 0;
                float2 CurOffset = 0;
                float SubOffset = 0;
                float TwoPi = 6.283185;
                float accumdist = 0;
                float RadialSteps = 8;
                float RadialOffset = 0.618;
                float KernelPower = 1;

                if (steps < 1)
                {
                    return tex2D(tex, uv);
                }
                else
                {
                    while (incre < (int)steps)
                    {
                        CurDistance += StepSize;
                        for (int j = 0; j < (int)RadialSteps; j++)
                        {
                            SubOffset += 1;
                            CurOffset.x = cos(TwoPi * (SubOffset / RadialSteps));
                            CurOffset.y = sin(TwoPi * (SubOffset / RadialSteps));
                            NewUV.x = uv.x + CurOffset.x * CurDistance;
                            NewUV.y = uv.y + CurOffset.y * CurDistance;
                            float distpow = pow(CurDistance, KernelPower);

                            CurColor += tex2D(tex, NewUV) * distpow;

                            accumdist += distpow;
                        }
                        SubOffset += RadialOffset;
                        incre++;
                    }
                    CurColor = CurColor;
                    CurColor /= accumdist;

                    return float4(CurColor);
                }

                //return float4(0, 0, 0, 0);

            }



            sampler2D _MainTex;
            sampler2D _Splat1;
            sampler2D _Splat2;
            sampler2D _Splat3;
            sampler2D _Splat4;
            int _NumSplats;
            float _BlurDistance;
            float _BlurSteps;

            fixed4 frag(v2f i) : SV_Target
            {
                //float4 splat1 = tex2D(_Splat1, i.uv);
                //float4 splat2 = tex2D(_Splat2, i.uv);
                //float4 splat3 = tex2D(_Splat3, i.uv);
                //float4 splat4 = tex2D(_Splat4, i.uv);

                float steps = 32;
                float blurDistance = 0.01;

                float4 splat1 = blur(i.uv, _Splat1, _BlurDistance, _BlurSteps);
                float4 splat2 = blur(i.uv, _Splat2, _BlurDistance, _BlurSteps);
                float4 splat3 = blur(i.uv, _Splat3, _BlurDistance, _BlurSteps);
                float4 splat4 = blur(i.uv, _Splat4, _BlurDistance, _BlurSteps);

                if (_NumSplats == 1)
                {
                    float2 weightData[] = {
                        float2(splat1.r, 0),
                        float2(splat1.g, 1),
                        float2(splat1.b, 2),
                        float2(splat1.a, 3)
                    };
                    
                    float2 orderedSplatData[] = weightData;
                    int i, j;
                    float value;
                    float2 temp;

                    for (i = 1; i < 4; i++)
                    {
                        value = orderedSplatData[i].x;
                        temp = orderedSplatData[i];
                        for (j = i - 1; j >= 0 && orderedSplatData[j].x < value; j--)
                        {
                            orderedSplatData[j + 1] = orderedSplatData[j];
                        }
                        orderedSplatData[j + 1] = temp;
                    }

                    return float4(orderedSplatData[0].y / 256, orderedSplatData[1].y / 256, orderedSplatData[2].y / 256, orderedSplatData[3].y / 256);

                }
                else if (_NumSplats == 2)
                {

                    float2 weightData[] = {
                    float2(splat1.r, 0),
                    float2(splat1.g, 1),
                    float2(splat1.b, 2),
                    float2(splat1.a, 3),
                    float2(splat2.r, 4),
                    float2(splat2.g, 5),
                    float2(splat2.b, 6),
                    float2(splat2.a, 7)
                    };

                    float2 orderedSplatData[] = weightData;
                    int i, j;
                    float value;
                    float2 temp;

                    for (i = 1; i < 8; i++)
                    {
                        value = orderedSplatData[i].x;
                        temp = orderedSplatData[i];
                        for (j = i - 1; j >= 0 && orderedSplatData[j].x < value; j--)
                        {
                            orderedSplatData[j + 1] = orderedSplatData[j];
                        }
                        orderedSplatData[j + 1] = temp;
                    }

                    return float4(orderedSplatData[0].y / 256, orderedSplatData[1].y / 256, orderedSplatData[2].y / 256, orderedSplatData[3].y / 256);
                    //return float4(1 / 256, 1 / 256, 1 / 256, 1 / 256);
                }

                else if (_NumSplats == 3)
                {
                    float2 weightData[] = {
                    float2(splat1.r, 0),
                    float2(splat1.g, 1),
                    float2(splat1.b, 2),
                    float2(splat1.a, 3),
                    float2(splat2.r, 4),
                    float2(splat2.g, 5),
                    float2(splat2.b, 6),
                    float2(splat2.a, 7),
                    float2(splat3.r, 8),
                    float2(splat3.g, 9),
                    float2(splat3.b, 10),
                    float2(splat3.a, 11)
                    };

                    float2 orderedSplatData[] = weightData;
                    int i, j;
                    float value;
                    float2 temp;

                    for (i = 1; i < 12; i++)
                    {
                        value = orderedSplatData[i].x;
                        temp = orderedSplatData[i];
                        for (j = i - 1; j >= 0 && orderedSplatData[j].x < value; j--)
                        {
                            orderedSplatData[j + 1] = orderedSplatData[j];
                        }
                        orderedSplatData[j + 1] = temp;
                    }

                    return float4(orderedSplatData[0].y / 256, orderedSplatData[1].y / 256, orderedSplatData[2].y / 256, orderedSplatData[3].y / 256);

                }
                else if (_NumSplats == 4)
                {
                    float2 weightData[] = {
                    float2(splat1.r, 0),
                    float2(splat1.g, 1),
                    float2(splat1.b, 2),
                    float2(splat1.a, 3),
                    float2(splat2.r, 4),
                    float2(splat2.g, 5),
                    float2(splat2.b, 6),
                    float2(splat2.a, 7),
                    float2(splat3.r, 8),
                    float2(splat3.g, 9),
                    float2(splat3.b, 10),
                    float2(splat3.a, 11),
                    float2(splat4.r, 12),
                    float2(splat4.g, 13),
                    float2(splat4.b, 14),
                    float2(splat4.a, 15)
                    };

                    float2 orderedSplatData[] = weightData;
                    int i, j;
                    float value;
                    float2 temp;

                    for (i = 1; i < 16; i++)
                    {
                        value = orderedSplatData[i].x;
                        temp = orderedSplatData[i];
                        for (j = i - 1; j >= 0 && orderedSplatData[j].x < value; j--)
                        {
                            orderedSplatData[j + 1] = orderedSplatData[j];
                        }
                        orderedSplatData[j + 1] = temp;
                    }

                    return float4(orderedSplatData[0].y / 256, orderedSplatData[1].y / 256, orderedSplatData[2].y / 256, orderedSplatData[3].y / 256);

                }

                return float4(0, 0, 0, 0);

            }
            ENDCG
        }
    }
}
