using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Setup
{
    public enum SearchMode { Equals, StartsWith, Contains, EndsWith }

    public class SetupUtils
    {
        /// <summary>
        /// Returns the first asset that matches the file path and name passed. Will try
        /// full path first, then will try just the file name.
        /// </summary>
        /// <param name="fileNameOrPath">File name as standalone or fully pathed</param>
        /// <returns>Object or null if it was not found</returns>
        public static UnityEngine.Object GetAsset(string fileNameOrPath, Type assetType)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(fileNameOrPath))
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(fileNameOrPath, assetType);
                if (obj != null)
                {
                    return obj;
                }
                else
                {
                    string path = GetAssetPath(Path.GetFileName(fileNameOrPath));
                    if (!string.IsNullOrEmpty(path))
                    {
                        return AssetDatabase.LoadAssetAtPath(path, assetType);
                    }
                }
            }
#endif
            return null;
        }

        public static string GetSettingsDirectory()
        {
            return GetAssetPath("Setup1" + Path.AltDirectorySeparatorChar + "Settings", false, SearchMode.EndsWith);
        }

        /// <summary>
        /// Return GaiaSettings or null;
        /// </summary>
        /// <returns>Gaia settings or null if not found</returns>
        public static SetupSettings GetSetupSettings()
        {
            return GetAsset("Setup Settings.asset", typeof(SetupSettings)) as SetupSettings;
        }

        /// <summary>
        /// Gets a List of types for all classes that inherit from a certain source class type.
        /// </summary>
        /// <typeparam name="T">The type you want to get the inherited classes for</typeparam>
        /// <returns></returns>
        public static List<Type> GetInheritedTypes<T>() where T : class
        {
            List<Type> returnTypes = new List<Type>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.Name.Contains("GWC"))) //myType.IsSubclassOf(typeof(T))))
            {
                Debug.Log(type.Name);
                //returnTypes.Add(type);
            }
            return returnTypes;
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string name, bool isFile = true, SearchMode searchMode = SearchMode.Equals)
        {
#if UNITY_EDITOR
            string fName = Path.GetFileNameWithoutExtension(name);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);

                string compareString = isFile ? Path.GetFileName(path) : path; 

                switch (searchMode)
                {
                    case SearchMode.Equals:
                        if (compareString == name)
                        {
                            return path;
                        }
                        break;
                    case SearchMode.StartsWith:
                        if (compareString.StartsWith(name))
                        {
                            return path;
                        }
                        break;
                    case SearchMode.Contains:
                        if (compareString.Contains(name))
                        {
                            return path;
                        }
                        break;
                    case SearchMode.EndsWith:
                        if (compareString.EndsWith(name))
                        {
                            return path;
                        }
                        break;
                }
            }
#endif
            return "";
        }

        public static bool CheckIfPathExists(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Expects a path starting at the asset folder, and will return a full file system path starting at the Drive Letter
        /// </summary>
        /// <param name="inputPath">Unity path starting at the asset folder</param>
        /// <returns></returns>
        public static string GetFullFileSystemPath(string inputPath)
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + inputPath;
        }

        public static string GetInstallRootPath()
        {
            //Default Directory, will be returned if not in Editor
            string rootDir = "Assets/Procedural Worlds/Packages - Install/";
#if UNITY_EDITOR
            string[] assets = AssetDatabase.FindAssets("Packages_Install_Readme", null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (Path.GetFileName(path) == "Packages_Install_Readme.txt")
                {
                    rootDir = path.Replace("/Packages_Install_Readme.txt", "");
                }
            }
#endif
            return rootDir;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PWPackageConfigSettings GetOrCreatePackageConfigSettings()
        {
#if UNITY_EDITOR
            PWPackageConfigSettings returnedObject = GetAsset("Package Config Settings.asset", typeof(PWPackageConfigSettings)) as PWPackageConfigSettings;
            if (returnedObject == null)
            {
                returnedObject = ScriptableObject.CreateInstance<PWPackageConfigSettings>();
                
                string path = SetupUtils.GetUserSettingsDirectory() + "/Package Config Settings.asset";
                AssetDatabase.CreateAsset(returnedObject, path);
                AssetDatabase.ImportAsset(path);
                returnedObject = (PWPackageConfigSettings)AssetDatabase.LoadAssetAtPath(path, typeof(PWPackageConfigSettings));
            }
            return returnedObject;
#else
            return null;
#endif

        }

        private static string GetUserSettingsDirectory()
        {

            //Using default user directory for now, for the lack of a better place
            string path = "Assets/Gaia User Data/Settings";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// Get config by name
        /// </summary>
        /// <param name="name">Name of the config</param>
        /// <param name="silent">Fail silently</param>
        /// <returns>Config or null</returns>
        public static AppConfig GetAppConfig(string name, bool silent = false, bool nameIsPath = false)
        {
            string path = null;

            if (nameIsPath)
            {
                path = name;
            }
            else
            {
                path = GetAssetPath(name + ".pwcfg");
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }
            }

            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                object firstObj = formatter.Deserialize(stream);
                string cfgVersion = "1";

                if (firstObj.GetType() == typeof(string))
                {
                    cfgVersion = (string)firstObj;
                    firstObj = null;
                }
                else if (firstObj.GetType() != typeof(double))
                {
                    Debug.LogError("Unknown cfg file 0: " + firstObj.GetType());
                    return null;
                }

                switch (cfgVersion)
                {
                    case "1":
                        return LoadV1(cfgVersion, firstObj, formatter, stream);
                    default:
                        throw new ApplicationException("Unknown config version " + cfgVersion);
                }
            }
        }

        private static AppConfig LoadV1(string cfgVersion, object firstObj, BinaryFormatter formatter, Stream stream)
        {
            double lastUpdated = 0;

            // The last time the config was upated
            if (firstObj == null)
            {
                lastUpdated = (double)formatter.Deserialize(stream);
            }
            else
            {
                lastUpdated = (double)firstObj;
            }

            // Min Unity version
            var minUnity = (string)formatter.Deserialize(stream);

            // Name
            var pName = (string)formatter.Deserialize(stream);

            // Logo
            var width = (int)formatter.Deserialize(stream);
            var height = (int)formatter.Deserialize(stream);
            var format = (TextureFormat)formatter.Deserialize(stream);
            var mipmap = (bool)formatter.Deserialize(stream);
            var bytes = (byte[])formatter.Deserialize(stream);
            Texture2D logo = null;
            if (width < 1 || height < 1 || bytes.Length < 1)
            {
            }
            else
            {
                logo = new Texture2D(width, height, format, mipmap);
                logo.LoadRawTextureData(bytes);
                logo.Apply();
            }

            // Namespace, Folders
            var nameSpace = (string)formatter.Deserialize(stream);
            var folder = (string)formatter.Deserialize(stream);
            var scriptsFolder = (string)formatter.Deserialize(stream);
            var editorScriptsFolder = (string)formatter.Deserialize(stream);
            var docsFolder = (string)formatter.Deserialize(stream);

            // Versioning
            var majorVersion = (string)formatter.Deserialize(stream);
            var minorVersion = (string)formatter.Deserialize(stream);
            var patchVersion = (string)formatter.Deserialize(stream);

            // Languages
            var availableLanguages = (SystemLanguage[])formatter.Deserialize(stream);

            // Links
            var tutorialsLink = (string)formatter.Deserialize(stream);
            var discordLink = (string)formatter.Deserialize(stream);
            var supportLink = (string)formatter.Deserialize(stream);
            var asLink = (string)formatter.Deserialize(stream);

            // Remote settings
            var newsURL = (string)formatter.Deserialize(stream);

            // Other settings
            var hasWelcome = (bool)formatter.Deserialize(stream);

            return new AppConfig(
                cfgVersion,
                lastUpdated,

                // Min Unity version
                minUnity,

                // Name
                pName,

                // Logo
                logo,

                // Namespace, Folders
                nameSpace,
                folder,
                scriptsFolder,
                editorScriptsFolder,
                docsFolder,

                // Versioning
                majorVersion,
                minorVersion,
                patchVersion,

                // Languages
                availableLanguages,

                // Links
                tutorialsLink,
                discordLink,
                supportLink,
                asLink,

                // Remote settings
                newsURL,

                // Other settings
                hasWelcome
            );
        }

        /// <summary>
        /// Gets the file size of a directory
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns></returns>
        public static long DirSize(DirectoryInfo dirInfo)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = dirInfo.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = dirInfo.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

    }
}