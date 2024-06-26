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
    public class GWS_HDRPSSRTransparency : GWS_HDRPBoolCheck
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Screen Space Reflections Transparency";
            m_boolTargetValue = true;
            m_boolValueDoesNotMatchMessage = "Screen Space Reflections are disabled in the HD Render pipeline asset - without this being enabled, transparent objects are not being taken into account for SSR rendering.";
            m_infoTextOK = "Transparency is enabled for SSR in the HD Render pipeline asset.";
            m_infoTextIssue = m_boolValueDoesNotMatchMessage;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Override-Screen-Space-Reflection.html";
            m_linkDisplayText = "SSR in the HDRP Manual";
            m_canRestore = true;
            Initialize();
        }

        public override bool GetBoolValue()
        {
#if HDPipeline && UNITY_EDITOR
            return GetHDRPAsset().currentPlatformRenderPipelineSettings.supportSSRTransparent;
#else
            return true;
#endif
        }

        public override bool FixBool()
        {
#if HDPipeline
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.supportSSRTransparent = m_boolTargetValue;
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
            rps.supportSSRTransparent = !m_boolTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

    }
}
