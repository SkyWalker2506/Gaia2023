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
    public class GWS_HDRPSkyReflectionSize : GWS_HDRPIntCheck
    {
#if HDPipeline
        [SerializeField]
        private SkyResolution m_oldSkyReflectionSize;
#endif

        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Sky Reflection Size";
            m_infoTextOK = "The Sky Reflection in the global reflection probe is sized below 256 pixels. This represents a good tradeoff between looks and performance for sky reflections.";
            m_intValueDoesNotMatchMessage = "The sky reflection is sized larger than 256 pixels. This will increase the quality of the skybox in reflections, but can reduce performance when no other reflection probe is rendered.";
            m_infoTextIssue = m_intValueDoesNotMatchMessage;
            m_intTargetValue = 256;
            m_checkType = GWSIntCheckType.SmallerThanEquals;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/HDRP-Asset.html#sky";
            m_linkDisplayText = "Sky Settings in the HDRP Manual";
            m_canRestore = true;
            Initialize();
        }

        public override int GetIntValue()
        {
#if HDPipeline && UNITY_EDITOR            
            //The atlas size enum maps to the int value in pixels
            return (int)GetHDRPAsset().currentPlatformRenderPipelineSettings.lightLoopSettings.skyReflectionSize;
#else
            return 0;
#endif
        }

        public override bool FixInt()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            m_oldSkyReflectionSize = rps.lightLoopSettings.skyReflectionSize;
            EditorUtility.SetDirty(this);
            rps.lightLoopSettings.skyReflectionSize = (SkyResolution)m_intTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
#if UNITY_EDITOR
            EditorUtility.SetDirty(hdrpa);
#endif
            return true;
#else
            return false;
#endif
        }

        public override bool RestoreInt()
        {
#if HDPipeline
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.lightLoopSettings.skyReflectionSize = m_oldSkyReflectionSize;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

        public override string GetOriginalValueString()
        {
#if HDPipeline
            return m_oldSkyReflectionSize.ToString();
#else
            return "";
#endif
        }
    }
}
