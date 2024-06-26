using System.IO;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.GTS
{
    public static class GTSEditorUtility
    {
        public static GTSProfile CreateNewProfile()
        {
            string fileName = "New GTS Profile";
            string profilesFolder = GTSConstants.GetUserDataFolder("Profiles");
            string fullPath = $"{profilesFolder}/{fileName}.asset";
            // Get Unique Asset Path
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
            GTSProfile gtsProfile = ScriptableObject.CreateInstance<GTSProfile>();
            gtsProfile.name = fileName;
            // AssetDatabase.CreateAsset(gtsProfile, fullPath);
            ProjectWindowUtil.CreateAsset(gtsProfile, fullPath);
            return gtsProfile;
        }

        public static T[] GetScriptableObjects<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string fileName)
        {
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            foreach (string asset in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(asset);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }

            return "";
        }

        public static void ImportPipelinePackages(bool forceImport = false)
        {
            GTSDefaults defaults = GTSDefaults.Instance;
            if (defaults == null)
                return;
            GTSPipeline currentPipeline = GTSUtility.GetCurrentPipeline();

            #region Remove All Shader Folders

            string shaderFolder = AssetDatabase.GetAssetPath(defaults.shaderFolder);
            if (!string.IsNullOrEmpty(shaderFolder))
            {
                // Attempt to Remove HDRP Folder
                string hdrpPath = $"{shaderFolder}/HDRP";
                string hdrpFolderGUID = AssetDatabase.AssetPathToGUID(hdrpPath);
                if (!string.IsNullOrEmpty(hdrpFolderGUID))
                    AssetDatabase.DeleteAsset(hdrpPath);
                // Attempt to Remove URP Folder
                string urpPath = $"{shaderFolder}/URP";
                string urpFolderGUID = AssetDatabase.AssetPathToGUID(urpPath);
                if (!string.IsNullOrEmpty(urpFolderGUID))
                    AssetDatabase.DeleteAsset(urpPath);
            }

            #endregion

            #region Import Package

            if (!defaults.packagesImported || forceImport)
            {
                string packagePath = null;
                switch (currentPipeline)
                {
                    case GTSPipeline.URP:
                        packagePath = AssetDatabase.GetAssetPath(defaults.urpPackage);
                        break;
                    case GTSPipeline.HDRP:
#if UNITY_2023_1_OR_NEWER
                        packagePath = AssetDatabase.GetAssetPath(defaults.hdrp2023_1_Package);
#elif UNITY_2022_3_OR_NEWER
                        packagePath = AssetDatabase.GetAssetPath(defaults.hdrp2022_3_Package);
#elif UNITY_2022_2_OR_NEWER
                        packagePath = AssetDatabase.GetAssetPath(defaults.hdrp2022Package);
#else
                        packagePath = AssetDatabase.GetAssetPath(defaults.hdrpPackage);
#endif
                        break;
                }

                if (!string.IsNullOrEmpty(packagePath))
                {
                    // Import the appropriate package
                    AssetDatabase.ImportPackage(packagePath, false);
                }
            }

            #endregion
        }
        
        public static void ReimportConversionShaders()
        {
            GTSDefaults defaults = GTSDefaults.Instance;
            if (defaults == null)
                return;
            string shadersFolderPath = AssetDatabase.GetAssetPath(defaults.shaderFolder);
            if (!string.IsNullOrEmpty(shadersFolderPath))
            {
                string conversionShaderFolderPath = $"{shadersFolderPath}/ConversionShaders";
                // Re-import conversion shaders folder and it's contents (ImportRecursive)
                AssetDatabase.ImportAsset(conversionShaderFolderPath, ImportAssetOptions.ImportRecursive);
            }
        }

        public static void PerformMaintenance()
        {
            ImportPipelinePackages(true);
            ReimportConversionShaders();
        }
    }
}