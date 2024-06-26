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
    public class GWS_HDRPCookies2DAtlasSize : GWS_HDRPIntCheck
    {
#if HDPipeline
        [SerializeField]
        private CookieAtlasResolution m_oldCookieResolution;
#endif

        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Cookie Atlas Size";
            m_infoTextOK = "The cookie atlas is sized at 2048 pixels or larger. This prevents error messages from appearing early when working with lighting based on light cookies.";
            m_intValueDoesNotMatchMessage = "The cookie atlas is sized smaller than 2048 pixels. This can lead to error messages from appearing early when working with lighting based on light cookies.";
            m_infoTextIssue = m_intValueDoesNotMatchMessage;
            m_intTargetValue = 2048;
            m_checkType = GWSIntCheckType.LargerThanEquals;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/HDRP-Asset.html#cookies";
            m_linkDisplayText = "Light Cookie Settings in the HDRP Manual";
            m_canRestore = true;
            Initialize();
        }

        public override int GetIntValue()
        {
#if HDPipeline && UNITY_EDITOR
            //The atlas size enum maps to the int value in pixels
            return (int)GetHDRPAsset().currentPlatformRenderPipelineSettings.lightLoopSettings.cookieAtlasSize;
#else
            return 0;
#endif
        }

        public override bool FixInt()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            m_oldCookieResolution = rps.lightLoopSettings.cookieAtlasSize;
            EditorUtility.SetDirty(this);
            rps.lightLoopSettings.cookieAtlasSize = (CookieAtlasResolution)m_intTargetValue;
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
            rps.lightLoopSettings.cookieAtlasSize = m_oldCookieResolution;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

        public override string GetOriginalValueString()
        {
#if HDPipeline
            return m_oldCookieResolution.ToString();
#else
            return "";
#endif
        }
    }
}
