using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Gaia
{
    public class GWS_PostProcessing : GWSetting
    {

        //static AddRequest Request;

        private void OnEnable()
        {
            m_RPBuiltIn = true;
            m_RPHDRP = false;
            m_RPURP = false;
            m_canAutoFix = false;
            m_name = "Post Processing";
            m_infoTextOK = "Post Processing is installed in this project.";
            m_infoTextIssue = "Post Processing is NOT installed in this project. Post Processing improves rendering quality, " + 
                              "and is needed for under water effects if you are using Gaia Water. Please consider installing Post Processing from the Package "+ 
                              "Manager, or Ignore this if you do not want it in your project.";
            m_link = "https://docs.unity3d.com/Packages/com.unity.postprocessing@3.3/manual/index.html";
            m_linkDisplayText = "Post Processing Stack Overview";
            Initialize();
        }

        public override bool PerformCheck()
        {
#if UNITY_POST_PROCESSING_STACK_V2
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
            //if (autoFix || EditorUtility.DisplayDialog("Install Post Processing Package?",
            //"Do you want to install the post processing package into your project now?",
            //"Continue", "Cancel"))
            //{
            //    EditorUtility.DisplayProgressBar("Starting Package Install", "Requesting the installation of the Post Processing package...", 0);
            //    Request = Client.Add("com.unity.postprocessing");
            //    EditorApplication.update += Progress;
            //    return true;
            //}

            return false;
        }

        // void Progress()
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
