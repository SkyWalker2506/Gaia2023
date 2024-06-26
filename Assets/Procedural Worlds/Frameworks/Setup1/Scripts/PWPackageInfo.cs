using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if GAIA_2023
using Gaia;
using System;
using System.Linq;
#endif
#if UNITY_EDITOR
using UnityEditor.Compilation;
using UnityEditor;
#endif


public enum PWPackageStatus { Unknown, NotInstalled, Installed, NotUnityPackage}

public enum PWPackageCategory {Core, Assets, Runtime }


[System.Serializable]
public enum PWPackageAction {
    [InspectorName("-")]
    Nothing = 0,  
    Install = 1,
    Uninstall = 2,
    [InspectorName("Delete Source Package")]
    DeleteSourcePackage = 3,
    [InspectorName("Update And Delete Source")]
    UpdateAndDelete = 4
    }

public enum PWPackageActionInstalled
{
    [InspectorName("-")]
    Nothing = 0,
    Uninstall = 2,
    [InspectorName("Delete Source Package")]
    DeleteSourcePackage = 3
}

public enum PWPackageActionInstalledUpdateAvailable
{
    [InspectorName("-")]
    Nothing = 0,
    [InspectorName("Update")]
    Install = 1,
    Uninstall = 2,
    [InspectorName("Delete Source Package")]
    DeleteSourcePackage = 3,
    [InspectorName("Update And Delete Source")]
    UpdateAndDelete = 4
}

public enum PWPackageActionUnInstalled
{
    [InspectorName("-")]
    Nothing = 0,
    Install = 1,
    [InspectorName("Delete Source Package")]
    DeleteSourcePackage = 3,
    [InspectorName("Install And Delete Source")]
    InstallAndDelete = 4
}

public enum PWPackageActionInstalledNoPackage
{
    [InspectorName("-")]
    Nothing = 0,
    Uninstall = 2,
}

/// <summary>
/// Base class for a "Gaia Wizard" setting. A "Gaia Wizard" settings can be a certain project specific setting or aspect that
/// we want to check against in the Gaia Manager and, if possible, offer to fix automatically if the setting is currently configured
/// in a unfavorable way.
/// Example: Project is not set up to use linear color space 
/// 
/// </summary>

namespace ProceduralWorlds.Setup
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Procedural Worlds/Setup/PW Package Info")]
    public class PWPackageInfo : ScriptableObject
    {
        internal bool m_foldedOut;
        public string m_ID;
        public string m_displayName;
        public PWPackageCategory m_category;
        [HideInInspector]
        public PWPackageStatus m_status = PWPackageStatus.Unknown;
        public float m_installedSizeMB;
        public float m_packageSizeMB;
        public UnityEngine.Object m_unityPackage;
        public string m_installFolder;
        public string m_packageInfo;
        [HideInInspector]
        public bool m_updateAvailable;
        public bool m_containsGaiaBiomes;
        public bool m_containsGaiaLayers;
        public bool m_containsRPMaterials;
        public PWPackageActionUnInstalled m_defaultAction = PWPackageActionUnInstalled.InstallAndDelete;
        public PWPackageAction m_currentAction;
        public int m_installOrder;

        public delegate void PackageInstallStarted();
        public event PackageInstallStarted OnPackageInstallStarted;

        public delegate void PackageInstallCompleted();
        public event PackageInstallCompleted OnPackageInstallCompleted;

        public delegate void PackageUnInstallCompleted();
        public event PackageUnInstallCompleted OnPackageUnInstallCompleted;

        private string m_installPath;
        
        private GUIStyle m_linkStyle;
        private GUIContent m_linkContent;
        private static string m_installRoot;
        private static GUIContent m_iconOKContent;
        private static GUIContent m_iconUpdateContent;
        private static GUIContent m_iconWarningContent;
        private static GUIContent m_iconNoUnityPkgContent;
        private static GUIContent m_iconNotInstalledContent;

        private static SetupSettings m_setupSettings;
        private static SetupSettings SetupSettings
        {
            get
            {
                if (m_setupSettings == null)
                {
                    m_setupSettings = SetupUtils.GetSetupSettings();
                }
                return m_setupSettings;
            }
        }


        public virtual void Initialize()
        {
            if (m_iconOKContent == null)
            {
                m_iconOKContent = new GUIContent(SetupSettings.m_IconOK, "This package is currently installed");
            }

            if (m_iconUpdateContent == null)
            {
                m_iconUpdateContent = new GUIContent(SetupSettings.m_IconUpdateAvailable, "There is an update available for this installed package");
            }

            if (m_iconWarningContent == null)
            {
                m_iconWarningContent = new GUIContent(SetupSettings.m_IconWarning, "Warning - There is a potential issue with this package");
            }

            if (m_iconNoUnityPkgContent == null)
            {
                m_iconNoUnityPkgContent = new GUIContent(SetupSettings.m_IconNoUnityPackage, "No Unity Package - The installer unity package is missing, this content cannot be installed.");
            }

            if (m_iconNotInstalledContent == null)
            {
                m_iconNotInstalledContent = new GUIContent(SetupSettings.m_IconNotInstalled, "Not Installed - This package is currently not installed");
            }

            if (m_installRoot == null)
            {
                m_installRoot = SetupUtils.GetInstallRootPath();
            }

            m_displayName = m_displayName.Trim();
            m_ID = m_ID.Trim();

            //If install folder is not given, assume it installs in a folder equal to the name
            if (m_installFolder == "")
            {
                m_installFolder = m_displayName;
            }

            m_installFolder = m_installFolder.Trim();

            m_installPath = m_installRoot + "/" + m_installFolder;

            StatusUpdate();

        }

        private void StatusUpdate()
        {
#if UNITY_EDITOR
            if (SetupUtils.CheckIfPathExists(m_installPath))
            {
                m_status = PWPackageStatus.Installed;
                m_installedSizeMB = SetupUtils.DirSize(new DirectoryInfo(m_installPath)) / (float)1048576;
                EditorUtility.SetDirty(this);
                return;
            }
            else
            {
                m_status = PWPackageStatus.NotInstalled;
            }

            if (m_unityPackage == null)
            {
                m_status = PWPackageStatus.NotUnityPackage;
            }
            else
            {
                FileInfo fi = new FileInfo(AssetDatabase.GetAssetPath(m_unityPackage));
                m_packageSizeMB = fi.Length / (float)1048576;
                EditorUtility.SetDirty(this);
            }
#endif
        }

        public void Install()
        {
#if UNITY_EDITOR
            if (OnPackageInstallStarted != null)
            {
                OnPackageInstallStarted();
            }
            //subscribing to the package install events, so we can react accordingly after the import is finished
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;


            AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(m_unityPackage), false);
#endif
        }

        private void OnCompilationFinished(object obj)
        {
#if UNITY_EDITOR
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
#endif
            StartPostInstall();
        }

        private void OnImportPackageCompleted(string packageName)
        {
            if (packageName == m_unityPackage.name)
            {
#if UNITY_EDITOR
                CompilationPipeline.compilationFinished -= OnCompilationFinished;
                AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
#endif

                StartPostInstall();
            }
           
        }

        private void StartPostInstall()
        {
            PostInstall();
            StatusUpdate();
           
            if (OnPackageInstallCompleted != null)
            {
                OnPackageInstallCompleted();
            }
        }

        /// <summary>
        /// Checks and actions after the main package installation
        /// </summary>
        private void PostInstall()
        {
            if (m_containsGaiaBiomes)
            {
                GaiaBiomeInstall();
            }
            if (m_containsGaiaLayers)
            {
                FixGaiaLayers();
            }
            if (m_containsRPMaterials)
            {
                UpdateMaterials();
            }
#if UNITY_EDITOR
            //Make sure progress bar is cleared
            EditorUtility.ClearProgressBar();
#endif
        }

        private void UpdateMaterials()
        {
#if GAIA_2023
            GaiaUtils.RefreshPipeline();
            GaiaUtils.ProcessMaterialLibrary();
#endif
        }
      
        private void FixGaiaLayers()
        {
#if GAIA_2023 && UNITY_EDITOR
            string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[1] { m_installPath });
            List<string> allPrefabPaths = new List<string>();

            int currentGUID = 0;
            foreach (string prefabGUID in allPrefabGuids)
            {
                EditorUtility.DisplayProgressBar("Adjusting Prefab Layers...", $"Collecting Prefabs..., {currentGUID} of {allPrefabGuids.Length}", (float)currentGUID / (float)allPrefabGuids.Length);
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
                //It can happen that FindAssets returns directories in the search above,
                //need to make sure the path ends in ".prefab"
                if (prefabPath.EndsWith(".prefab"))
                {
                    allPrefabPaths.Add(prefabPath);
                }
            }

            //Perform layer fix
            GaiaUtils.FixPrefabLayers(allPrefabPaths);
#endif
        }

        /// <summary>
        /// Checks and actions after the deinstallation
        /// </summary>
        private void PostUnInstall()
        {
            GaiaBiomeCleanup();
        }

        private void GaiaBiomeCleanup()
        {
#if GAIA_2023
            UserFiles userFiles = GaiaUtils.GetOrCreateUserFiles();
            if (userFiles != null)
            {
               userFiles.m_gaiaManagerBiomePresets.RemoveAll(x => x == null);
            }
#endif
        }

        private void GaiaBiomeInstall()
        {
#if GAIA_2023 && UNITY_EDITOR
            UserFiles userFiles = GaiaUtils.GetOrCreateUserFiles();
            if (userFiles != null)
            {
                string searchPath = SetupUtils.GetInstallRootPath() + "/" + m_installFolder;
                AssetDatabase.Refresh();
                //foreach (var path in AssetDatabase.GetAllAssetPaths())
                //{
                //    if (path.EndsWith(m_installFolder))
                //    {
                //        searchPath = ;
                //    }
                //}

                if (searchPath != "")
                {
                    string[] allSpawnerPresetGUIDs = AssetDatabase.FindAssets("t:BiomePreset", new string[1] { searchPath });

                    for (int i = 0; i < allSpawnerPresetGUIDs.Length; i++)
                    {
                        if (allSpawnerPresetGUIDs[i] != null)
                        {
                            BiomePreset bp = (BiomePreset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allSpawnerPresetGUIDs[i]), typeof(BiomePreset));
                            if (bp != null && !userFiles.m_gaiaManagerBiomePresets.Contains(bp))
                            {
                                userFiles.m_gaiaManagerBiomePresets.Add(bp);
                            }
                        }
                    }
                    EditorUtility.SetDirty(userFiles);
                    AssetDatabase.SaveAssets();
                }
            }
#endif
        }

        public void Uninstall()
        {
#if UNITY_EDITOR
            if (SetupUtils.CheckIfPathExists(m_installPath))
            {
                AssetDatabase.StartAssetEditing();
                Directory.Delete(m_installPath, true);
                File.Delete(SetupUtils.GetFullFileSystemPath(m_installPath + ".meta"));
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                PostUnInstall();
                StatusUpdate();
            }
           
            if (OnPackageUnInstallCompleted != null)
            {
                OnPackageUnInstallCompleted();
            }
#endif
        }


        public void DeleteSourcePackage()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m_unityPackage));
            StatusUpdate();
#endif
        }


        /// <summary>
        /// Draws the UI for this entry in the Setup tab
        /// </summary>
        public void DrawUI()
        {
#if UNITY_EDITOR
            bool originalGUIState = GUI.enabled;

            if (m_linkStyle == null)
            {
                m_linkStyle = new GUIStyle(GUI.skin.label);
                m_linkStyle.fontStyle = FontStyle.Normal;
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = Color.white;
                m_linkStyle.stretchWidth = false;
            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);

            if (m_status == PWPackageStatus.Installed)
            {
                if (!m_updateAvailable)
                {
                    GUILayout.Label(m_iconOKContent);
                }
                else
                {
                    GUILayout.Label(m_iconUpdateContent);
                }
            }
            else
            if (m_status == PWPackageStatus.NotUnityPackage)
            {
                GUILayout.Label(m_iconNoUnityPkgContent);
            }
            else
            if (m_status == PWPackageStatus.Unknown)
            {
                GUILayout.Label(m_iconWarningContent);
            }
            if (m_status == PWPackageStatus.NotInstalled)
            {
                GUILayout.Label(m_iconNotInstalledContent);
            }

            GUILayout.Space(5);

            m_foldedOut = EditorGUILayout.Foldout(m_foldedOut, m_displayName, true);
            GUILayout.Space(180);

            float sourceSize = m_packageSizeMB;

            if (m_status == PWPackageStatus.NotUnityPackage || m_unityPackage == null)
            {
                sourceSize = 0.0f;
                GUI.enabled = false;
            }
            GUILayout.Label($"Source: {sourceSize:#,0.00} MB", GUILayout.Width(150));
            GUI.enabled = originalGUIState;
            GUILayout.Space(20);

            float installSize = m_installedSizeMB;

            if (m_status != PWPackageStatus.Installed)
            {
                GUI.enabled = false;
                installSize = 0.0f;
            }
            GUILayout.Label($"Install: {installSize:#,0.00} MB");
            GUI.enabled = originalGUIState;
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();

            switch (m_status)
            {
                case PWPackageStatus.Unknown:
                    m_currentAction = (PWPackageAction)EditorGUILayout.EnumPopup(m_currentAction);
                    break;
                case PWPackageStatus.NotInstalled:
                    m_currentAction = (PWPackageAction)EditorGUILayout.EnumPopup((PWPackageActionUnInstalled)m_currentAction);
                    break;
                case PWPackageStatus.Installed:
                   if (m_unityPackage != null)
                    {
                        if (m_updateAvailable)
                        {
                            m_currentAction = (PWPackageAction)EditorGUILayout.EnumPopup((PWPackageActionInstalledUpdateAvailable)m_currentAction);
                        }
                        else
                        {
                            m_currentAction = (PWPackageAction)EditorGUILayout.EnumPopup((PWPackageActionInstalled)m_currentAction);
                        }
                    }
                    else
                    {
                        m_currentAction = (PWPackageAction)EditorGUILayout.EnumPopup((PWPackageActionInstalledNoPackage)m_currentAction);
                    }
                    break;
                case PWPackageStatus.NotUnityPackage:
                    GUI.enabled = false;
                    m_currentAction = (PWPackageAction)EditorGUILayout.EnumPopup(m_currentAction);
                    GUI.enabled = originalGUIState;
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
            

            //if (m_status == PWPackageStatus.NotInstalled || m_status == PWPackageStatus.Unknown)
            //{
            //    if (GUILayout.Button("Install"))
            //    {
            //        Install();
            //    }
            //}
            //else
            //if (m_status == PWPackageStatus.Installed)
            //{
            //    if (GUILayout.Button("Uninstall"))
            //    {
            //        Uninstall();
            //    }
            //}

            //if (m_unityPackage == null)
            //{
            //    GUI.enabled = false;   
            //}
            //if (GUILayout.Button("Delete Install Pkg."))
            //{
            //    DeleteInstallPackage();
            //}
            //GUI.enabled = originalGUIState;

            EditorGUILayout.EndHorizontal();

            if (m_foldedOut)
            {
                EditorGUI.indentLevel += 3;
                if (m_packageInfo != "")
                {
                    EditorGUILayout.HelpBox(m_packageInfo, MessageType.None);
                }
                EditorGUI.indentLevel -= 3;
            }
#endif
        }
#if UNITY_EDITOR
        bool ClickableHeaderCustomStyle(GUIContent content, GUIStyle style, GUILayoutOption[] options = null)
        {
            var position = GUILayoutUtility.GetRect(content, style, options);
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = style.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = oldColor;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, content, style);
        }
#endif

    }
}
