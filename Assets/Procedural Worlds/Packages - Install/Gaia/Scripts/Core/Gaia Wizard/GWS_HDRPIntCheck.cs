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
    public enum GWSIntCheckType {SmallerThan, Equals, LargerThan, SmallerThanEquals, LargerThanEquals};


    /// <summary>
    /// Performs a generic check against an int value in the HDRP pipeline asset. Inherit from this class, set the m_inTargetValue, m_checkType and implement GetIntValue, FixInt, RestoreInt,
    /// to quickly add your own int check. See e.g. GWS_HDRPDecalAtlasSize as an example implementation.
    /// </summary>
    public class GWS_HDRPIntCheck : GWSetting
    {
        [HideInInspector]
        public string m_intValueDoesNotMatchMessage;
        [HideInInspector]
        public int m_intTargetValue;
        public int m_originalValue;
        [HideInInspector]
        public GWSIntCheckType m_checkType;
 
        public virtual int GetIntValue()
        {
            return 0;
        }

        public virtual bool FixInt()
        {
            return true;
        }

        public virtual bool RestoreInt()
        {
            return true;
        }


        public override bool PerformCheck()
        {
            return PerformIntCheck(GetIntValue());

        }

#if HDPipeline
        public HDRenderPipelineAsset GetHDRPAsset()
        {
            //Do we have a render pipeline asset at quality level? If yes, we take this one,
            //as it will override what is in the default settings in the "regular" pipeline asset
            HDRenderPipelineAsset asset = (HDRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel());
            if (asset != null)
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

        private bool PerformIntCheck(int HDRPIntValue)
        {
            switch (m_checkType)
            {
                case GWSIntCheckType.SmallerThan:
                    if (HDRPIntValue < m_intTargetValue)
                    {
                        Status = GWSettingStatus.OK;
                        return false;
                    }
                    break;
                case GWSIntCheckType.Equals:
                    if (HDRPIntValue == m_intTargetValue)
                    {
                        Status = GWSettingStatus.OK;
                        return false;
                    }
                    break;
                case GWSIntCheckType.LargerThan:
                    if (HDRPIntValue > m_intTargetValue)
                    {
                        Status = GWSettingStatus.OK;
                        return false;
                    }
                    break;
                case GWSIntCheckType.SmallerThanEquals:
                    if (HDRPIntValue <= m_intTargetValue)
                    {
                        Status = GWSettingStatus.OK;
                        return false;
                    }
                    break;
                case GWSIntCheckType.LargerThanEquals:
                    if (HDRPIntValue >= m_intTargetValue)
                    {
                        Status = GWSettingStatus.OK;
                        return false;
                    }
                    break;
            }

            //If we are here, the check failed
            Status = GWSettingStatus.Warning;
            m_infoTextIssue = m_intValueDoesNotMatchMessage;
            return true;
        }

        public override bool FixNow(bool autoFix = false)
        {
            m_originalValue = GetIntValue();
            if (FixInt())
            {
                PerformCheck();
                m_foldedOut = false;
                return true;

            }
            return false;
        }

        public override bool RestoreOriginalValue()
        {
            if (RestoreInt())
            {
                m_wasFixed = false;
                PerformCheck();
                return true;
            }
            return false;
        }

        public override string GetOriginalValueString()
        {
            return m_originalValue.ToString();
        }
    }
}
