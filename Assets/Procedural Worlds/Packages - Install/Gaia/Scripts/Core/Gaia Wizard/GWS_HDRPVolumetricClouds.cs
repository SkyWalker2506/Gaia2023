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
    public class GWS_HDRPVolumetricClouds : GWS_HDRPBoolCheck
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Volumetric Clouds";
            m_boolTargetValue = true;
            m_boolValueDoesNotMatchMessage = "Volumetric Clouds are disabled in the HD Render pipeline asset - you will not be able to use a sky with volumetric clouds without enabling this setting.";
            m_infoTextOK = "Volumetric Clouds are enabled in the HD Render pipeline asset.";
            m_infoTextIssue = m_boolValueDoesNotMatchMessage;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Override-Volumetric-Clouds.html";
            m_linkDisplayText = "Volumetric Clouds in the HDRP Manual";
            m_canRestore = true;
            Initialize();
        }

        public override bool GetBoolValue()
        {
#if HDPipeline 
            return GetHDRPAsset().currentPlatformRenderPipelineSettings.supportVolumetricClouds;
            
#else
            return true;
#endif
        }

        public override bool FixBool()
        {
#if HDPipeline
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.supportVolumetricClouds = m_boolTargetValue;
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
            rps.supportVolumetricClouds = !m_boolTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

    }
}
