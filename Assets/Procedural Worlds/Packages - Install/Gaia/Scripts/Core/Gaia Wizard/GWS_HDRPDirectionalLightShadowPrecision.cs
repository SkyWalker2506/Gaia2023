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
    public class GWS_HDRPDirectionalLightShadowPrecision : GWS_HDRPIntCheck
    {
#if HDPipeline
        [SerializeField]
        private DepthBits m_oldShadowPrecision;
#endif

        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Directional Light Shadow Precision";
            m_infoTextOK = "The directional light shadow precision is set to 32 bit in the HD render pipeline asset.";
            m_intValueDoesNotMatchMessage = "The directional light shadow precision is set to less than 32 bit in the HD render pipeline asset. This reduces quality for shadows thrown from the directional light (sun) in the scene.";
            m_infoTextIssue = m_intValueDoesNotMatchMessage;
            m_intTargetValue = 32;
            m_checkType = GWSIntCheckType.LargerThanEquals;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/HDRP-Asset.html#shadow";
            m_linkDisplayText = "Shadow Settings in the HDRP Manual";
            m_canRestore = true;
            Initialize();
        }

        public override int GetIntValue()
        {
#if HDPipeline && UNITY_EDITOR
            //The atlas size enum maps to the int value in pixels
            return (int)GetHDRPAsset().currentPlatformRenderPipelineSettings.hdShadowInitParams.directionalShadowsDepthBits;
#else
            return 0;
#endif
        }

        public override bool FixInt()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            m_oldShadowPrecision = rps.hdShadowInitParams.directionalShadowsDepthBits;
            EditorUtility.SetDirty(this);
            rps.hdShadowInitParams.directionalShadowsDepthBits = (DepthBits)m_intTargetValue;
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
            rps.hdShadowInitParams.directionalShadowsDepthBits = m_oldShadowPrecision;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

        public override string GetOriginalValueString()
        {
#if HDPipeline
            return m_oldShadowPrecision.ToString();
#else
            return "";
#endif
        }
    }
}
