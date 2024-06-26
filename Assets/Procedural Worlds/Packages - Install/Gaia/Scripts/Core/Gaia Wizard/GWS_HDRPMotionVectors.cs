using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor.Rendering;
using UnityEditor;
#endif
namespace Gaia
{
    public class GWS_HDRPMotionVectors : GWSetting
    {
        private void OnEnable()
        {
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_canAutoFix = false;
            m_canRestore = false;
            m_name = "Terrain Detail Motion Vector / Motion Blur Bug";
            m_infoTextOK = "This project runs HDRP in 2023.1.13 or higher and it should not be affected by this bug.";
            m_infoTextIssue = "This project runs HDRP in Unity 2023.1.0 to 2023.1.12. There is an Unity bug in that range that leads to terrain details being rendered blurry. As a workaround, you can deactivate Motion Blur e.g. via Edit > Project Settings > Graphics > HDRP Global Settings > Volume Profiles > Motion Blur. Please visit the link below for more information about this issue and screenshots for the workaround.";
            Status = GWSettingStatus.Warning;
            m_link = "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/advanced/hdrp-terrain-details-blurry-motion-vector-bug-r170/";
            m_linkDisplayText = "Bug Description & Solutions on Canopy";
            Initialize();
        }

        public override bool PerformCheck()
        {
            string minorVersion = Application.unityVersion.Substring(Application.unityVersion.LastIndexOf('.') + 1, 2);
#if UNITY_2023_1
            try
            {
                int minorVersionInt = Int32.Parse(minorVersion);
                if (minorVersionInt > 12)
                {
                    //All good, bug should be fixed
                    Status = GWSettingStatus.OK;
                    return false;
                }
                else
                {
                    //Affected version, this should have the bug
                    Status = GWSettingStatus.Warning;
                    return true;
                }
            }
            catch (FormatException)
            {
                //Do not display a warning if version cannot be parsed
                Status = GWSettingStatus.OK;
                return false;
            }
#else
            //all good, this bug only shows in the 2023.1 version
            Status = GWSettingStatus.OK;
            return false;
#endif
        }

        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            string message = "This Unity bug cannot be fixed automatically, as there are many smaller nuances to this issue, and a proper fix requires to upgrade to Unity 2023.1.13. What you can do as a workaround: \r\n\r\n"
                + "Deactivate Motion Blur e.g. in the HDRP Global settings. Go to Edit > Project Settings > Graphics > HDRP Global Settings > Volume Profiles > Motion Blur, and deactivate the checkbox for 'Motion Blur' there. \r\n\r\n"
                + "To make this warning disappear from the project settings, please press the 'Ignore' button\r\n\r\n"
                + "We do have a detailed bug description for this issue on Canopy, including screenshots and more information. Do you want to open this website now?\r\n\r\n";
            if (EditorUtility.DisplayDialog("Fixing the Motion Vector Bug", message, "Open Canopy", "Cancel"))
            {
                Application.OpenURL(m_link);
            }
#endif
            return false;
        }
    }
}
