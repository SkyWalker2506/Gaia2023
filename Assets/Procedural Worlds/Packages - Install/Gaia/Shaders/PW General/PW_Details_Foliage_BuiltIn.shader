Shader "PWS/Details/PW_Details_Foliage_BuiltIn"
{
    Properties
    {
        [Header(PBR)]
        _MipBias ("Mip Bias", Range(-8,8)) = 0
        [HDR]_BaseColor ("Base Color", Color) = (1,1,1,1)
        [NoScaleOffset]_BaseMap ("Base Map (RGB)", 2D) = "white" {}
        [NoScaleOffset]_NormalMap ("Normal Map", 2D) = "white" {}
        _NormalStrength("Normal Strength", Range(0.01, 3)) = 1
        [NoScaleOffset]_MaskMap ("PBR Mask Map", 2D) = "white" {}
        _MaskMapMod("Mask Map modifiers", Vector) = (1,1,1,1)
        _Thickness("Thickness", Range(0, 1)) = 0
        _Billboard("Billboard", Float) = 0
        _Ambient("Ambient", Range(0, 1)) = 0.5
        [Space]
        
        //[Header(Subsurface)]
        //_PW_SSSPower( "Power", Range (0.01, 8 ) ) = 1
		//_PW_SSSDistortion( "Distortion", Range (0.001, 1 ) ) = 0.5
        //_PW_SSSTint ("Tint", Color) = (1,1,1,1)

        [Header(Alpha)]
        _Cutoff ("Alpha Cut Off", Range(0,1)) = 0.05
        [Space]
        
        //wind
        [Header(Wind Options)]
        [Toggle(_PW_SF_WIND_ON)]        
        _PW_SF_WIND("Enable Wind", Int) = 0
        _PW_WindTreeFlex(" Wind Detail Flex", Vector) = (0.8,1.15,0.1,0)
        _PW_WindTreeFrequency (" Wind Detail Frequency", Vector) = (0.25,0.5,1.3,0)
        _PW_WindTreeWidthHeight (" Wind Detail Height", Vector) = (1,1,0.66,0.66)
        
        //_FadeDistance("Fade Distance", Float) = 75
        //_FadeRange("Fade Range", Float) = 20
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  "Queue" = "AlphaTest"}
        LOD 200
        Cull off
        
        CGPROGRAM
        #pragma shader_feature_local _PW_SF_WIND_ON
        
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf FloraStandard fullforwardshadows vertex:vert addshadow 
        //#pragma surface surf Gaia fullforwardshadows vertex:vert keepalpha addshadow 
        #pragma surface surf Gaia fullforwardshadows vertex:vert keepalpha addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5


        #include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "../../../Gaia/Shaders/PW General/PW_GeneralVars.cginc"
		#include "../../../Gaia/Shaders/PW General/PW_GeneralFuncs.cginc"
        #include "../../../Gaia/Shaders/PW General/PW_GeneralWind.cginc"
        #include "../../../Gaia/Shaders/PW General/PW_DitherIncludes.hlsl"

        
        //../../../Flora/Content Resources/Shaders/PW_FloraIncludes.hlsl

        //#include "PW_FloraIncludes.hlsl"
        //#include "PW_FloraWInd.cginc"
        
        
        #pragma multi_compile_instancing
        //#pragma instancing_options procedural:setup

        struct Input
        {
            float2 uv_BaseMap;
            float2 uv_MainTex;
            float facing: VFACE;
            float3 viewDir;
            float4 screenPos;
            float4 screenPosition;
            float3 Normal;
            float3 worldPos;
            float3 worldNormal;
            float ditherFade;
            INTERNAL_DATA
        };

        sampler2D _BaseMap, _NormalMap, _MaskMap;
        half4 _BaseColor, _ColorA, _ColorB;
        //half _Cutoff;
        float4 _MaskMapMod;
        float _Thickness,_MipBias;
        float _NormalStrength;
        float _Billboard;
        float _Ambient;

        //float _FadeDistance;
        //float _FadeRange;
        
        //wind
        //float2 _PW_WindTreeWidthHeight;
        //float4 _PW_WindTreeFlex, _PW_WindTreeFrequency;


        #include "UnityShaderVariables.cginc"
        #include "UnityStandardConfig.cginc"
        #include "UnityLightingCommon.cginc"
        #include "UnityGBuffer.cginc"
        #include "UnityGlobalIllumination.cginc"

        //-------------------------------------------------------------------------------------

        //Unity based Standard PBR Lighting with SSS and thickness added for FLORA Foliage
        //Reduced Specular
        //Engergy conservation

        // Note: BRDF entry points use smoothness and oneMinusReflectivity for optimization
        // purposes, mostly for DX9 SM2.0 level. Most of the math is being done on these (1-x) values, and that saves
        // a few precious ALU slots.


        // Main Physically Based BRDF
        // Derived from Disney work and based on Torrance-Sparrow micro-facet model
        //
        //   BRDF = kD / pi + kS * (D * V * F) / 4
        //   I = BRDF * NdotL
        //
        // * NDF (depending on UNITY_BRDF_GGX):
        //  a) Normalized BlinnPhong
        //  b) GGX
        // * Smith for Visiblity term
        // * Schlick approximation for Fresnel
        /*half4 BRDF1_FLORA_PBS ( half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
            float3 normal, float3 viewDir,
            UnityLight light, UnityIndirect gi, half thickness = 1)
        {
            float perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
            float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

        // NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
        // In this case normal should be modified to become valid (i.e facing camera) and not cause weird artifacts.
        // but this operation adds few ALU and users may not want it. Alternative is to simply take the abs of NdotV (less correct but works too).
        // Following define allow to control this. Set it to 0 if ALU is critical on your platform.
        // This correction is interesting for GGX with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
        // Edit: Disable this code by default for now as it is not compatible with two sided lighting used in SpeedTree.
        #define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0

        #if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
            // The amount we shift the normal toward the view vector is defined by the dot product.
            half shiftAmount = dot(normal, viewDir);
            normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
            // A re-normalization should be applied here but as the shift is small we don't do it to save ALU.
            //normal = normalize(normal);

            float nv = saturate(dot(normal, viewDir)); // TODO: this saturate should no be necessary here
        #else
            half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact
        #endif

            float nl = dot(normal, light.dir);
            float nh = saturate(dot(normal, halfDir));

            half lv = saturate(dot(light.dir, viewDir));
            half lh = saturate(dot(light.dir, halfDir));

            //Translucencey backface transport
            half inverseThickness = inverseThickness = 1-thickness;
            inverseThickness = inverseThickness * inverseThickness * inverseThickness;

            float scatterSize = lerp(0.2,0.4,thickness);
            half energyConservationCurve = ((1 - scatterSize) * (1 - scatterSize));
           
            half3 backfaceHalfDir = normalize(light.dir + (normal * thickness * thickness));
            half vh = dot(viewDir, -backfaceHalfDir);
            vh = vh * 0.5f + 0.5f;
            half backfaceTransportTerm = pow(vh,4 * inverseThickness + 1 ) * inverseThickness * (1-energyConservationCurve);

            half nle = saturate((nl * (1 - scatterSize) + scatterSize));
            nl = saturate(nl);

            // spec color
            specColor = lerp(specColor,half3(0.02f,0.02f,0.02f),inverseThickness);
            
            // Diffuse term
            half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nle * energyConservationCurve;

            // Specular term
            // HACK: theoretically we should divide diffuseTerm by Pi and not multiply specularTerm!
            // BUT 1) that will make shader look significantly darker than Legacy ones
            // and 2) on engine side "Non-important" lights have to be divided by Pi too in cases when they are injected into ambient SH
            float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
        #if UNITY_BRDF_GGX
            // GGX with roughtness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughtness remapping.
            roughness = max(roughness, 0.002);
            float V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
            float D = GGXTerm (nh, roughness);
        #else
            // Legacy
            half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
            half D = NDFBlinnPhongNormalizedTerm (nh, PerceptualRoughnessToSpecPower(perceptualRoughness));
        #endif

            float specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later

        #   ifdef UNITY_COLORSPACE_GAMMA
                specularTerm = sqrt(max(1e-4h, specularTerm));
        #   endif

            // specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
            specularTerm = max(0, specularTerm * nl);
        #if defined(_SPECULARHIGHLIGHTS_OFF)
            specularTerm = 0.0;
        #endif

            // surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
            half surfaceReduction;
        #   ifdef UNITY_COLORSPACE_GAMMA
                surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
        #   else
                surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
        #   endif

            // To provide true Lambert lighting, we need to be able to kill specular completely.
            specularTerm *= any(specColor) ? 1.0 : 0.0;

            half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
            half3 color =   diffColor * (gi.diffuse + light.color * (diffuseTerm + backfaceTransportTerm))
                            + specularTerm * light.color * FresnelTerm (specColor, lh)
                            + surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, nv);

            return half4(color, 1);
        }
        */
        // Based on Minimalist CookTorrance BRDF
        // Implementation is slightly different from original derivation: http://www.thetenthplanet.de/archives/255
        //
        // * NDF (depending on UNITY_BRDF_GGX):
        //  a) BlinnPhong
        //  b) [Modified] GGX
        // * Modified Kelemen and Szirmay-​Kalos for Visibility term
        // * Fresnel approximated with 1/LdotH
        /*half4 BRDF2_FLORA_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
            float3 normal, float3 viewDir,
            UnityLight light, UnityIndirect gi, half thickness = 1)
        {
            float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

            half nl = saturate(dot(normal, light.dir));
            float nh = saturate(dot(normal, halfDir));
            half nv = saturate(dot(normal, viewDir));
            float lh = saturate(dot(light.dir, halfDir));

            // Specular term
            half perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
            half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

        #if UNITY_BRDF_GGX

            // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
            // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
            // https://community.arm.com/events/1155
            float a = roughness;
            float a2 = a*a;

            float d = nh * nh * (a2 - 1.f) + 1.00001f;
        #ifdef UNITY_COLORSPACE_GAMMA
            // Tighter approximation for Gamma only rendering mode!
            // DVF = sqrt(DVF);
            // DVF = (a * sqrt(.25)) / (max(sqrt(0.1), lh)*sqrt(roughness + .5) * d);
            float specularTerm = a / (max(0.32f, lh) * (1.5f + roughness) * d);
        #else
            float specularTerm = a2 / (max(0.1f, lh*lh) * (roughness + 0.5f) * (d * d) * 4);
        #endif

            // on mobiles (where half actually means something) denominator have risk of overflow
            // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
            // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
        #if defined (SHADER_API_MOBILE)
            specularTerm = specularTerm - 1e-4f;
        #endif

        #else

            // Legacy
            half specularPower = PerceptualRoughnessToSpecPower(perceptualRoughness);
            // Modified with approximate Visibility function that takes roughness into account
            // Original ((n+1)*N.H^n) / (8*Pi * L.H^3) didn't take into account roughness
            // and produced extremely bright specular at grazing angles

            half invV = lh * lh * smoothness + perceptualRoughness * perceptualRoughness; // approx ModifiedKelemenVisibilityTerm(lh, perceptualRoughness);
            half invF = lh;

            half specularTerm = ((specularPower + 1) * pow (nh, specularPower)) / (8 * invV * invF + 1e-4h);

        #ifdef UNITY_COLORSPACE_GAMMA
            specularTerm = sqrt(max(1e-4f, specularTerm));
        #endif

        #endif

        #if defined (SHADER_API_MOBILE)
            specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
        #endif
        #if defined(_SPECULARHIGHLIGHTS_OFF)
            specularTerm = 0.0;
        #endif

            // surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)

            // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
            // 1-x^3*(0.6-0.08*x)   approximation for 1/(x^4+1)
        #ifdef UNITY_COLORSPACE_GAMMA
            half surfaceReduction = 0.28;
        #else
            half surfaceReduction = (0.6-0.08*perceptualRoughness);
        #endif

            surfaceReduction = 1.0 - roughness*perceptualRoughness*surfaceReduction;

            half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
            half3 color =   (diffColor + specularTerm * specColor) * light.color * nl
                            + gi.diffuse * diffColor
                            + surfaceReduction * gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);

            return half4(color, 1);
        }
        */

        // Old school, not microfacet based Modified Normalized Blinn-Phong BRDF
        // Implementation uses Lookup texture for performance
        //
        // * Normalized BlinnPhong in RDF form
        // * Implicit Visibility term
        // * No Fresnel term
        //
        // TODO: specular is too weak in Linear rendering mode
        /*half4 BRDF3_FLORA_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
            float3 normal, float3 viewDir,
            UnityLight light, UnityIndirect gi, half thickness = 1)
        {
            float3 reflDir = reflect (viewDir, normal);

            half nl = saturate(dot(normal, light.dir));
            half nv = saturate(dot(normal, viewDir));

            // Vectorize Pow4 to save instructions
            half2 rlPow4AndFresnelTerm = Pow4 (float2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
            half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
            half fresnelTerm = rlPow4AndFresnelTerm.y;

            half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));

            half3 color = BRDF3_Direct(diffColor, specColor, rlPow4, smoothness);
            color *= light.color * nl;
            color += BRDF3_Indirect(diffColor, specColor, gi, grazingTerm, fresnelTerm);

            return half4(color, 1);
        }

        #if !defined (UNITY_BRDF_PBS) // allow to explicitly override BRDF in custom shader
        // still add safe net for low shader models, otherwise we might end up with shaders failing to compile
            #if SHADER_TARGET < 30 || defined(SHADER_TARGET_SURFACE_ANALYSIS) // only need "something" for surface shader analysis pass; pick the cheap one
                #define UNITY_BRDF_PBS BRDF3_FLORA_PBS
            #elif defined(UNITY_PBS_USE_BRDF3)
                #define UNITY_BRDF_PBS BRDF3_FLORA_PBS
            #elif defined(UNITY_PBS_USE_BRDF2)
                #define UNITY_BRDF_PBS BRDF2_FLORA_PBS
            #elif defined(UNITY_PBS_USE_BRDF1)
                #define UNITY_BRDF_PBS BRDF1_FLORA_PBS
            #else
                #error something broke in auto-choosing BRDF
            #endif
        #endif
        */
         //-------------------------------------------------------------------------------------

        //#include "UnityPBSLighting.cginc"

        //struct SurfaceFloraOutputStandard
        /*{
            fixed3 Albedo;      // base (diffuse or specular) color
            float3 Normal;      // tangent space normal, if written
            half3 Emission;
            half Metallic;      // 0=non-metal, 1=metal
            // Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
            // Everywhere in the code you meet smoothness it is perceptual smoothness
            half Smoothness;    // 0=rough, 1=smooth
            half Occlusion;     // occlusion (default 1)
            fixed Alpha;        // alpha for
            half Thickness;
        };*/

        /*inline half4 LightingFloraStandard (SurfaceFloraOutputStandard s, float3 viewDir, UnityGI gi)
        {
            s.Normal = normalize(s.Normal);

            half oneMinusReflectivity;
            half3 specColor;
            s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic,  specColor,  oneMinusReflectivity);

            // shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
            // this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
            half outputAlpha;
            s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, outputAlpha);

            half4 c = UNITY_BRDF_PBS (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect, s.Thickness);
            c.a = outputAlpha;
            return c;
        }*/
        
        /*inline void LightingFloraStandard_GI (
        SurfaceFloraOutputStandard s,
        UnityGIInput data,
        inout UnityGI gi)
        {
        #if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
            gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
        #else
            Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
            gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
        #endif
        }

        */

        //-------------------------------------------------------------------------------------
		half4 LightingGaia ( SurfaceOutputGaia g, half3 viewDir, UnityGI gi)
		{
			half4 addLight = 0;

            SurfaceOutputStandard s;
            s.Albedo 		= g.Albedo;
            s.Normal 		= g.Normal;
            s.Emission 		= g.Emission;
            s.Metallic 		= g.Metallic;
            s.Smoothness 	= g.Smoothness;
            s.Occlusion 	= g.Occlusion;
            s.Alpha 		= g.Alpha;


			AddLighting_half ( g.Albedo, 
						 g.e.worldNormal,
						 gi.light.dir, 
						 gi.light.color, 
						 viewDir,
						 g.e.coverRGBA, 
						 g.e.thickness,
						 float3(0,0,0),
						 0.0f,
						 0.0f,
						 1.0f,
     				     addLight
						 );



			return LightingStandard ( s, viewDir, gi ) + addLight;
        }
        
    	//-------------------------------------------------------------------------------------
        
        inline void LightingGaia_GI ( SurfaceOutputGaia s, UnityGIInput data, inout UnityGI gi )
        {
            UNITY_GI ( gi, s, data );
        }
        

        // Vertex "shader"
        void vert (inout appdata_full v) {
           
           WindCalculations_float(v.vertex.xyz,_PW_WindTreeWidthHeight.xy,_PW_WindTreeFlex,_PW_WindTreeFrequency,unity_ObjectToWorld, unity_WorldToObject, v.vertex.xyz);
		}

        float3 NormalStrength(float3 In, float Strength)
        {
            return float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }

        float4 _PW_SnowDataA;
        float4 _PW_SnowColor;
        //float _PW_Global_CoverLayer1FadeDist;
        
        // Fragment "shader" 
        void surf (Input IN, inout SurfaceOutputGaia o)
        {
            // Albedo & Alpha test
            //float4 uvMipBias = float4(IN.uv_BaseMap.xy,0,max(0,_MipBias));
            float4 uvMipBias = float4(IN.uv_BaseMap.xy,0,_MipBias);
            fixed4 base = tex2Dbias(_BaseMap, uvMipBias);
		    
            float dist = distance(float3(unity_ObjectToWorld._m03,unity_ObjectToWorld._m13,unity_ObjectToWorld._m23), _WorldSpaceCameraPos);
		    dist = (1 - saturate((dist - _PW_DetailRenderDistance.x) / _PW_DetailRenderDistance.y)) * 2;
		    
            //float dither;
		    //Gaia_Dither_float(dist, IN.screenPos, dither);
		    
            base.a = base.a * dist;
		    
            clip(base.a - _Cutoff);

            // PBR masks
            base *= _BaseColor * float4(0.5,0.5,0.5,1);

            //Snow Data
            float snowStart = ((IN.worldPos.g - _PW_SnowDataA.z));
            half sgn   = max ( sign ( snowStart ), 0 );
	        half fade  = clamp ( length ( snowStart ) / max ( _PW_Global_CoverLayer1FadeDist, HALF_MIN ), 0.0, 1.0 ) * sgn;
            float snowMask = lerp(0, .1, fade) * _PW_Global_CoverLayer1Progress;
            base = lerp(base, saturate(base + _PW_SnowColor), snowMask);
            
            
            float4 mask = saturate(tex2Dbias (_MaskMap, uvMipBias) * _MaskMapMod);

            // Output surface struct
            o.Albedo = base.rgb;
            o.Metallic = mask.r;
            o.Occlusion = mask.g;
            o.Smoothness = mask.a * 0.25;
            o.Alpha = base.a;

            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
		    o.Normal = NormalStrength(o.Normal, _NormalStrength);
            o.Normal = normalize(o.Normal);
            o.Normal = abs(o.Normal);
            //o.e.worldNormal = WorldNormalVector ( IN, o.Normal );
            o.e.thickness = 1.0 - mask.b;
            o.e.vertexColor = float4(0,0,0,0);
            
            // Dither Fade
            //float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
            //float fade;
            //float dist = length(float3(unity_ObjectToWorld._m03,unity_ObjectToWorld._m13,unity_ObjectToWorld._m23) - _WorldSpaceCameraPos);
            //DistanceFade_float(dist,fade);

            //float alpha = base.a - _Cutoff;

            //vpos /= 4; // the dither mask texture is 4x4 
            //clip( fade * fade - 0.001 - alpha * alpha ); //Dither from Alpha

            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
