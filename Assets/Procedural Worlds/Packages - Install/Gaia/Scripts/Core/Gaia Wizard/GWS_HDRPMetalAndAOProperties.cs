using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class GWS_HDRPMetalAOProperties : GWS_HDRPBoolCheck
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Decal Metal and AO Properties";
            m_boolTargetValue = false;
            m_boolValueDoesNotMatchMessage = "Metal and AO Properties for Decals are enabled in the HD Render pipeline asset - this can cause issues with blending / rendering of decals.";
            m_infoTextOK = "Metal and AO Properties for Decals are disabled in the HD Render pipeline asset.";
            m_infoTextIssue = m_boolValueDoesNotMatchMessage;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Decal-Shader.html#surface-options";
            m_linkDisplayText = "HDRP Decals - Metal and AO Properties";
            m_canRestore = true;
            Initialize();
        }

        public override bool GetBoolValue()
        {
#if HDPipeline  && UNITY_EDITOR
            return GetHDRPAsset().currentPlatformRenderPipelineSettings.decalSettings.perChannelMask;
#else
            return true;
#endif
        }

        public override bool FixBool()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.decalSettings.perChannelMask = m_boolTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
#if UNITY_EDITOR
            EditorUtility.SetDirty(hdrpa);
#endif
            return true;
#else
            return false;
#endif
        }

        public override bool RestoreBool()
        {
#if HDPipeline
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.decalSettings.perChannelMask = !m_boolTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

    }
}
