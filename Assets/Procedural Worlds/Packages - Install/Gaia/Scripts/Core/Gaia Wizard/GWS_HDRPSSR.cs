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
    public class GWS_HDRPSSR : GWS_HDRPBoolCheck
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Screen Space Reflections";
            m_boolTargetValue = true;
            m_boolValueDoesNotMatchMessage = "Screen Space Reflections are disabled in the HD Render pipeline asset - enabling this can further enhance the rendering of your scene, especially when reflective surfaces are involved.";
            m_infoTextOK = "Screen Space Relections are enabled in the HD Render pipeline asset.";
            m_infoTextIssue = m_boolValueDoesNotMatchMessage;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Override-Screen-Space-Reflection.html";
            m_linkDisplayText = "SSR in the HDRP Manual";
            m_canRestore = true;
            Initialize();
        }

        public override bool GetBoolValue()
        {
#if HDPipeline && UNITY_EDITOR
            return GetHDRPAsset().currentPlatformRenderPipelineSettings.supportSSR;
#else
            return true;
#endif
        }

        public override bool FixBool()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.supportSSR = m_boolTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            EditorUtility.SetDirty(hdrpa);
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
            rps.supportSSR = !m_boolTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

    }
}
