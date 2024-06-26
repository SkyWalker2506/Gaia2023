using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Gaia
{
    /// <summary>
    /// This class refreshes the project settings list in the Gaia Manager in case the window is open.
    /// e.g. when a package was installed or color space was changed etc. to properly update the list of issues.
    /// </summary>
    /// 
    internal class GaiaManagerRefresh : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (EditorWindow.HasOpenInstances<GaiaManagerEditor>())
            {
                var manager = EditorWindow.GetWindow<GaiaManagerEditor>(false, "Gaia Manager");
                //Manager can be null if the dependency package installation is started upon opening the manager window.
                if (manager != null)
                {
                    manager.CheckForSetupIssues();
                }
            }
        }
    }
}
