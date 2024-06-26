#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Gaia
{
    public class GWS_VerifySavingAssets : GWSetting
    {

        private void OnEnable()
        {
            m_RPBuiltIn = true;
            m_RPHDRP = true;
            m_RPURP = true;
            m_name = "Verify Saving Assets";
            m_infoTextOK = "'Verify Saving Assets' is currently OFF - this is the recommended setting for Gaia.";
            m_infoTextIssue = "The 'Verify Saving Assets' feature is turned on in the Unity Editor. This setting will make working with Gaia difficult as you will get constant popup dialogs asking you to confirm changes. Pleae turn this setting off and track changes via your source control system instead.";
            Status = GWSettingStatus.Warning;
            m_link = "https://docs.unity3d.com/Manual/Preferences.html";
            m_linkDisplayText = "Unity Preferences (see 'Verify Saving Assets')";
            Initialize();
        }

        public override bool PerformCheck()
        {
#if UNITY_EDITOR
            if (EditorPrefs.HasKey("VerifySavingAssets"))
            {
                if (EditorPrefs.GetBool("VerifySavingAssets"))
                {
                    Status = GWSettingStatus.Warning;
                    return true;
                }
            }
            Status = GWSettingStatus.OK;
#endif
            return false;
        }

        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            EditorPrefs.SetBool("VerifySavingAssets", false);
            Status = GWSettingStatus.OK;
#endif
            return true;
        }
    }
}
