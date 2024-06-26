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
    public class GWS_HDRPMaxPlanarReflections : GWS_HDRPIntCheck
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Max Planar Reflections";
            m_infoTextOK = "The probe cache size is set to 64 planar reflections or larger in the HDRP asset. This prevents error messages from appearing early when working with planar reflections in the scene.";
            m_intValueDoesNotMatchMessage = "The probe cache size is set smaller than 64 planar reflections in the HDRP Asset. This can lead to error messages from appearing early when working with planar reflections in the scene.";
            m_infoTextIssue = m_intValueDoesNotMatchMessage;
            m_intTargetValue = 64;
            m_checkType = GWSIntCheckType.LargerThanEquals;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/HDRP-Asset.html#reflections";
            m_linkDisplayText = "Reflection Settings in the HDRP Manual";
            m_canRestore = true;
            Initialize();
        }

        public override int GetIntValue()
        {
#if HDPipeline && UNITY_EDITOR 
            //The atlas size enum maps to the int value in pixels
            return GetHDRPAsset().currentPlatformRenderPipelineSettings.lightLoopSettings.maxPlanarReflectionOnScreen;
#else
            return 0;
#endif
        }

        public override bool FixInt()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            EditorUtility.SetDirty(this);
            rps.lightLoopSettings.maxPlanarReflectionOnScreen = m_intTargetValue;
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
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.lightLoopSettings.maxPlanarReflectionOnScreen = m_originalValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }
    }
}
