using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class GWS_ColorSpace : GWSetting
    {
        private void OnEnable()
        {
            m_RPBuiltIn = true;
            m_RPHDRP = true;
            m_RPURP = true;
            m_name = "Color Space";
            m_infoTextOK = "The color space selected is the Linear color space. This is best for most projects.";
            m_infoTextIssue = "The color space selected is the Gamma color space. Most projects will use the Linear color space.";
            m_link = "https://docs.unity3d.com/Manual/LinearRendering-LinearOrGammaWorkflow.html";
            m_linkDisplayText = "Unity Manual - Linear or gamma workflow";
            Initialize();
        }

        public override bool PerformCheck()
        {
#if UNITY_EDITOR
            if (PlayerSettings.colorSpace == ColorSpace.Linear)
            {
                Status = GWSettingStatus.OK;
                return false;
            }
            else
            {
                Status = GWSettingStatus.Warning;
                return true;
            }
#else
            return false;
#endif
        }

        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            if (autoFix || EditorUtility.DisplayDialog("Set Color Space to Linear?",
            "Do you want to set the color space to linear in this project? This will take a while to process, depending on the number of art assets in your project.",
            "Continue", "Cancel"))
            {
                PlayerSettings.colorSpace = ColorSpace.Linear;
                EditorGUIUtility.ExitGUI();
                PerformCheck();
                return true;
            }
            else
            {
                return false;
            }
#else
            return false;
#endif
        }


    }
}
