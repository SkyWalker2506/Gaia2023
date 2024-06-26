using Gaia;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralWorlds.Gaia
{
    /// <summary>
    /// Scans for Gaia Biomes after assets have been installed
    /// </summary>

    public class ScanForBiomes : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var spawnerPackImported = importedAssets.Any(x => x.Contains("Procedural Worlds/Content Packs"));
            if (spawnerPackImported)
            {
                UserFiles userFiles = GaiaUtils.GetOrCreateUserFiles();
                if (userFiles.m_scanForBiomesAfterAssetImport)
                {
                    GaiaEditorUtils.SearchAndAddBiomePresets();
                }
            }

        }
    }
}