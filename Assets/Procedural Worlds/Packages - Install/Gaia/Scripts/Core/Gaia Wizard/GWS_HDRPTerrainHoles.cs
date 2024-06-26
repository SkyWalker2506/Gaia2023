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
    public class GWS_HDRPTerrainHoles : GWS_HDRPBoolCheck
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "Terrain Holes";
            m_infoTextOK = "Terrain Holes are enabled in the HD Render pipeline asset.";
            m_boolValueDoesNotMatchMessage = "Terrain Holes are disabled in the HD Render pipeline asset - you will not be able to paint holes on your terrains until it is enabled.";
            m_infoTextIssue = m_boolValueDoesNotMatchMessage;
            m_boolTargetValue = true;
            m_link = "https://docs.unity3d.com/2019.3/Documentation/Manual/terrain-PaintHoles.html";
            m_linkDisplayText = "The Paint Holes feature in the Unity Manual";
            m_canRestore = true;
            Initialize();
        }

        public override bool GetBoolValue()
        {
#if HDPipeline 
            return GetHDRPAsset().currentPlatformRenderPipelineSettings.supportTerrainHole;
#else
            return true;
#endif
        }

        public override bool FixBool()
        {
#if HDPipeline
            HDRenderPipelineAsset hdrpa = GetHDRPAsset();
            RenderPipelineSettings rps = hdrpa.currentPlatformRenderPipelineSettings;
            rps.supportTerrainHole = m_boolTargetValue;
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
            rps.supportTerrainHole = !m_boolTargetValue;
            hdrpa.currentPlatformRenderPipelineSettings = rps;
            return true;
#else
            return false;
#endif
        }
    }
}
