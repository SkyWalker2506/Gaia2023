using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Setup
{
    public class SetupAutostart : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            //Check if the "Maintenance Token" exists, if yes, this is a new package install and we need to show the setup window for install / update
            if (!System.IO.Directory.Exists(SetupUtils.GetAssetPath("Dev Utilities")) && File.Exists(SetupUtils.GetSettingsDirectory() + "/" + SetupConstants.maintenanceTokenFilename))
            {
                SetupEditorWindow.ShowSetup();
                RemoveMaintenanceToken();
            }
        }

        private static void RemoveMaintenanceToken()
        {
            if (!System.IO.Directory.Exists(SetupUtils.GetAssetPath("Dev Utilities")))
            {
                FileUtil.DeleteFileOrDirectory(SetupUtils.GetSettingsDirectory() + "\\" + SetupConstants.maintenanceTokenFilename);
                FileUtil.DeleteFileOrDirectory(SetupUtils.GetSettingsDirectory() + "\\" + SetupConstants.maintenanceTokenFilename + ".meta");
                AssetDatabase.Refresh();
            }
        }
    }
}
