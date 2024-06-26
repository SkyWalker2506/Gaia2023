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
    public class GWS_HDRPDecalAtlasSize : GWS_HDRPIntCheck
    {
        [SerializeField]
        private int m_originalWidth;
        [SerializeField]
        private int m_originalHeight;

        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Decal Atlas Size";
            m_infoTextOK = "The decal atlas is sized at 4096 x 4096 pixels or larger. This prevents error messages from appearing early when working with decals.";
            m_intValueDoesNotMatchMessage = "The decal atlas is sized smaller than 4096 x 4096 pixels. This can lead to error messages from appearing early when working with decals.";
            m_infoTextIssue = m_intValueDoesNotMatchMessage;
            m_intTargetValue = 4096;
            m_checkType = GWSIntCheckType.LargerThanEquals;
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/HDRP-Asset.html#decals";
            m_linkDisplayText = "Decal Atlas Size Settings";
            m_canRestore = true;
            Initialize();
        }

        public override int GetIntValue()
        {
#if HDPipeline  && UNITY_EDITOR
            return Mathf.Min(GetHDRPAsset().currentPlatformRenderPipelineSettings.decalSettings.atlasWidth, GetHDRPAsset().currentPlatformRenderPipelineSettings.decalSettings.atlasHeight);
#else
            return 0;
#endif
        }

        public override bool FixInt()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            m_originalWidth = rps.decalSettings.atlasWidth;
            m_originalHeight = rps.decalSettings.atlasHeight;
            EditorUtility.SetDirty(this);
            rps.decalSettings.atlasWidth = 4096;
            rps.decalSettings.atlasHeight = 4096;
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
            rps.decalSettings.atlasWidth = m_originalWidth;
            rps.decalSettings.atlasHeight = m_originalHeight;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }

        public override string GetOriginalValueString()
        {
            return $"{m_originalWidth} x {m_originalHeight}";
        }
    }
}
