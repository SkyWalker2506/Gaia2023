#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Gaia
{
    public class GWS_InputSystem : GWSetting
    {

        //static AddRequest Request;

        private void OnEnable()
        {
            m_RPBuiltIn = true;
            m_RPHDRP = true;
            m_RPURP = true;
            m_canAutoFix = false;
            m_name = "Input System";
            m_infoTextOK = "The Unity Input System package is installed in this project.";
            m_infoTextIssue = "The Unity Input System package is not installed in this project. You will not be able to use Gaia's Flycam controller, or use keyboard shortcuts for tools like the Gaia screenshotter, etc." +
            "If you intend to use these tools, then please install the Input System Package from the Unity Package Manager. Click the 'Fix' button to open the Package Manager window.";
            m_link = "https://docs.unity3d.com/Packages/com.unity.inputsystem@1.6/manual/index.html";
            m_linkDisplayText = "Unity Input System Manual";
            Initialize();
        }

        public override bool PerformCheck()
        {
#if GAIA_INPUT_SYSTEM
            Status = GWSettingStatus.OK;
            return false;
#else
            Status = GWSettingStatus.Warning;
            return true;
#endif
        }

        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
#endif

            //Commented out for now - there are UX issues when automatically adding the package from script,
            //especially if the package is not downloaded yet

            //if (autoFix || EditorUtility.DisplayDialog("Install Input System Package?",
            //"Do you want to install the Unity Input System package into your project now?",
            //"Continue", "Cancel"))
            //{
            //    EditorUtility.DisplayProgressBar("Starting Package Install", "Requesting the installation of the Input System package...", 0.15f);
            //    Request = Client.Add("com.unity.inputsystem");
            //    EditorApplication.update += Progress;
            //    return true;
            //}

            return false;
        }

        //void Progress()
        //{
        //    if (Request.IsCompleted)
        //    {
        //        EditorUtility.ClearProgressBar();
        //        if (Request.Status == StatusCode.Success)
        //            Debug.Log("Installed: " + Request.Result.packageId);
        //        else if (Request.Status >= StatusCode.Failure)
        //            Debug.Log(Request.Error.message);

        //        EditorApplication.update -= Progress;
        //        PerformCheck();
        //    }
        //}

    }
}
