using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ProceduralWorlds.GTS
{
    [InitializeOnLoad]
    public static class PWInitializeOnLoad
    {
        static PWInitializeOnLoad()
        {
            AssetDatabase.importPackageCompleted -= DetectMaintenence;
            AssetDatabase.importPackageCompleted += DetectMaintenence;
            GTSEditorEvents.Subscribe();
            ProcessDefaultSettings();
            ProcessScriptDefine();
        }

        private static void DetectMaintenence(string packageName)
        {
            if (Directory.Exists(GTSEditorUtility.GetAssetPath("Dev Utilities")))
                return;
            if (Application.isPlaying)
                return;
            string maintenanceFilePath = GTSConstants.GTSMaintenanceTokenPath;
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(maintenanceFilePath);
            if (asset != null)
            {
                GTSEditorUtility.PerformMaintenance();
                // Delete Maintainence File
                AssetDatabase.DeleteAsset(maintenanceFilePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void ProcessDefaultSettings()
        {
            GTSDefaults defaults = GTSDefaults.Instance;
            if (defaults == null)
                return;
            GTSDebug.Enabled = defaults.debugEnabled;
        }

        private static void ProcessScriptDefine()
        {
            // Make sure we inject GTS_PRESENT
            bool updateScripting = false;

#if UNITY_2023_1_OR_NEWER
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
#else
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
            if (symbols.Contains("GTS_PRESENT"))
            {
                updateScripting = true;
                symbols = symbols.Replace("GTS_PRESENT", "GTS_PRESENT");
            }

            if (!symbols.Contains("GTS_PRESENT"))
            {
                updateScripting = true;
                symbols += ";" + "GTS_PRESENT";
            }

            if (updateScripting)
            {
#if UNITY_2023_1_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbols);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
#endif
            }
        }
    }
}