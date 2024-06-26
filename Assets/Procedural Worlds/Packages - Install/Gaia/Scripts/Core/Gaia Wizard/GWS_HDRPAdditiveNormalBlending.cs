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
    public class GWS_HDRPAdditiveNormalBlending : GWS_HDRPBoolCheck
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Decal Additive Normal Blending";
            m_boolTargetValue = true;
            m_boolValueDoesNotMatchMessage = "Additive Normal Blending for Decals is disabled in the HD Render pipeline asset - this can cause issues with blending / rendering of decals.";
            m_infoTextOK = "Additive Normal blending for Decals is enabled in the HD Render pipeline asset.";
            m_infoTextIssue = m_boolValueDoesNotMatchMessage;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Decal.html#additive-normal-blending";
            m_linkDisplayText = "HDRP Decals - Additive Normal Blending";
            m_canRestore = true;
            Initialize();
        }

        public override bool GetBoolValue()
        {
#if HDPipeline && UNITY_EDITOR
            return GetHDRPAsset().currentPlatformRenderPipelineSettings.supportSurfaceGradient;
#else
            return true;
#endif
        }

        public override bool FixBool()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.supportSurfaceGradient = true;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            EditorUtility.SetDirty(hdrpa);
            return true;
#else
            return false;
#endif
        }

        public override bool RestoreBool()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.supportSurfaceGradient = false;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

    }
}
