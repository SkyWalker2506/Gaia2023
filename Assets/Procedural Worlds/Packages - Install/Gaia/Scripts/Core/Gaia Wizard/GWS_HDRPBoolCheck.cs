using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
using System;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    /// <summary>
    /// Performs a generic check against a bool value in the HDRP pipeline asset. Inherit from this class, set the m_boolTargetValue and implement GetBoolValue, FixBool, RestoreBool,
    /// to quickly add your own bool check. See e.g. GWS_HDRPTerrainHoles as an example implementation.
    /// </summary>
    public class GWS_HDRPBoolCheck : GWSetting
    {
        [HideInInspector]
        public string m_boolValueDoesNotMatchMessage;
        [HideInInspector]
        public bool m_boolTargetValue;

        public virtual bool GetBoolValue()
        {
            return true;
        }

        public virtual bool FixBool()
        {
            return true;
        }

        public virtual bool RestoreBool()
        {
            return true;
        }


        public override bool PerformCheck()
        {
            return PerformBoolCheck(GetBoolValue(), m_boolTargetValue);

        }

#if HDPipeline
        public HDRenderPipelineAsset GetHDRPAsset()
        {
            //Do we have a render pipeline asset at quality level? If yes, we take this one,
            //as it will override what is in the default settings in the "regular" pipeline asset
            HDRenderPipelineAsset asset = (HDRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel());
            if (asset!=null)
            {
                return asset;
            }
            //otherwise: Get the default asset
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                return (HDRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
            }
            else
            {
                Status = GWSettingStatus.Unknown;
                m_infoTextIssue = "Could not find a HD render pipeline asset in the Graphics Settings.";
                return null;
            }
        }
#endif

        private bool PerformBoolCheck(bool HDRPBoolValue, bool m_boolTargetValue)
        {
            if (HDRPBoolValue == m_boolTargetValue)
            {
                Status = GWSettingStatus.OK;
                return false;
            }
            else
            {
                Status = GWSettingStatus.Warning;
                m_infoTextIssue = m_boolValueDoesNotMatchMessage;
                return true;
            }
        }

        public override bool FixNow(bool autoFix = false)
        {
            if (FixBool())
            {
                PerformCheck();
                return true;

            }
            return false;
        }



        public override bool RestoreOriginalValue()
        {
            if (RestoreBool())
            {
                m_wasFixed = false;
                PerformCheck();
                return true;
            }
            return false;
        }

        public override string GetOriginalValueString()
        {
            //Have to return the opposite of the target value as string
            if (m_boolTargetValue)
            {
                return "Disabled";
            }
            else
            {
                return "Enabled";
            }
        }
    }
}
