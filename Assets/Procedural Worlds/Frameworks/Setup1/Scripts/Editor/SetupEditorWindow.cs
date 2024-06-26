using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralWorlds.Setup
{
    /// <summary>
    /// Setup installation window that allows the user to install Gaia
    /// </summary>
    [InitializeOnLoad]
    public class SetupEditorWindow : EditorWindow
    {

        private List<PWPackageInfo> m_packageInfos = new List<PWPackageInfo>();
        private List<PWPackageInfo> m_actionPackages = new List<PWPackageInfo>();
        private PWPackageConfigSettings m_configSettings;

        private IEnumerator m_updateCoroutine;
        private bool m_waitingForPackage;
        private bool m_installRunning;
        private float m_totalInstalledSize, m_totalSourceSize;
        private PWCommon5.AppConfig m_appConfig;
        private int m_majorVersion;
        private int m_minorVersion;
        private int m_patchVersion;
        private bool m_oldGaia;
        private string m_oldGaiaPath;
        private GUIStyle m_helpBoxStyle;
        private GUIStyle m_linkStyle;
        private GUIContent m_oldGaialinkContent;
        private string m_oldGaiaLink = "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/30_installation__getting_started/migrating-from-gaia-2021-to-gaia-2023-r160/";
        private GUIContent m_PDFGuideContent;
        private GUIContent m_setupDocoContent;
        private string m_setupDocoLink = "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/30_installation__getting_started/setup-window-r167/";
        private GUIContent m_gaiaTutorialContent;
        private string m_gaiaTutorialLink = "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/gaia-gaia-pro-tutorials-overview-r155/";
        private string m_oldGaiaSearchString = "/Scripts/Gaia.pwcfg";
        private Vector2 m_scrollPosition;
        private bool m_linearFoldedOut;
        private bool m_switchToLinear = true;
        private GUIContent m_linearLabel;
        private GUIContent m_linearLinkContent;
        private string m_linearLink = "https://docs.unity3d.com/Manual/LinearRendering-LinearOrGammaWorkflow.html";
        public static bool m_waitingForLinear;

        #region Window Menu Entry
        /// <summary>
        /// Show Gaia Manager editor window
        /// </summary>
        [MenuItem("Window/Procedural Worlds/Setup...", false, 40)]
        public static void ShowSetup()
        {
            try
            {
                var manager = EditorWindow.GetWindow<ProceduralWorlds.Setup.SetupEditorWindow>(false, "Setup");
                //Manager can be null if the dependency package installation is started upon opening the manager window.
                if (manager != null)
                {
                    Vector2 initialSize = new Vector2(850f, 700f);
                    manager.position = new Rect(new Vector2(Screen.currentResolution.width / 2f - initialSize.x / 2f, Screen.currentResolution.height / 2f - initialSize.y / 2f), initialSize);
                    manager.Show();
                }
            }
            catch (Exception ex)
            {
                //not catching anything specific here, but the maintenance and shader installation tasks can trigger a null reference on that "GetWindow" above
                //get rid off the warning for unused "ex"
                if (ex.Message == "")
                { }
            };
        }
        #endregion

        private void OnEnable()
        {

            if (SetPipelineDefines())
            {
                return;
            }

            if (m_packageInfos == null)
            {
                m_packageInfos = new List<PWPackageInfo>();
            }
            if (!m_installRunning)
            {
                UpdatePackages();
                RefreshTotalSizes();
                //Look for older Gaia versions
                m_oldGaia = CheckForOlderGaiaVersions();
            }
            else
            {
                foreach (PWPackageInfo pwPI in m_packageInfos)
                {
                    pwPI.Initialize();
                }
            }
        }

        private bool SetPipelineDefines()
        {
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                return SetScriptingDefinesBuiltIn();
            }
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                return SetScriptingDefinesHDRP();
            }
            else
            {
                return SetScriptingDefinesURP();
            }
        }

        private bool SetScriptingDefinesURP()
        {
            bool isChanged = false;
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            if (!currBuildSettings.Contains("UPPipeline"))
            {
                if (string.IsNullOrEmpty(currBuildSettings))
                {
                    currBuildSettings = "UPPipeline";
                }
                else
                {
                    currBuildSettings += ";UPPipeline";
                }
                isChanged = true;
            }
            if (currBuildSettings.Contains("HDPipeline"))
            {
                currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                isChanged = true;
            }
            if (isChanged)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), currBuildSettings);
            }

            return isChanged;
        }

        private bool SetScriptingDefinesHDRP()
        {
            bool isChanged = false;
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            if (!currBuildSettings.Contains("HDPipeline"))
            {
                if (string.IsNullOrEmpty(currBuildSettings))
                {
                    currBuildSettings = "HDPipeline";
                }
                else
                {
                    currBuildSettings += ";HDPipeline";
                }
                isChanged = true;
            }
            if (currBuildSettings.Contains("UPPipeline"))
            {
                currBuildSettings = currBuildSettings.Replace("UPPipeline;", "");
                currBuildSettings = currBuildSettings.Replace("UPPipeline", "");
                isChanged = true;
            }
            if (isChanged)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), currBuildSettings);
            }

            return isChanged;
        }

        private bool SetScriptingDefinesBuiltIn()
        {
            bool isChanged = false;
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            if (currBuildSettings.Contains("UPPipeline"))
            {
                currBuildSettings = currBuildSettings.Replace("UPPipeline;", "");
                currBuildSettings = currBuildSettings.Replace("UPPipeline", "");
                isChanged = true;
            }
            if (currBuildSettings.Contains("HDPipeline"))
            {
                currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                isChanged = true;
            }
            if (isChanged)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), currBuildSettings);
            }

            return isChanged;
        }

        private void RefreshTotalSizes()
        {
            m_totalInstalledSize = 0;
            m_totalSourceSize = 0;
            for (int i = 0; i < m_packageInfos.Count; i++)
            {
                PWPackageInfo pWPackageInfo = m_packageInfos[i];
                if (pWPackageInfo.m_status == PWPackageStatus.Installed)
                {
                    m_totalInstalledSize += pWPackageInfo.m_installedSizeMB;
                }
                if (pWPackageInfo.m_unityPackage != null)
                {
                    m_totalSourceSize += pWPackageInfo.m_packageSizeMB;
                }
            }
        }


        #region GUI
        void OnGUI()
        {
            bool originalGUIState = GUI.enabled;
            if (m_helpBoxStyle == null || (m_helpBoxStyle.normal.textColor.r + m_helpBoxStyle.normal.textColor.g + m_helpBoxStyle.normal.textColor.b == 0))
            {
                m_helpBoxStyle = new GUIStyle(GUI.skin.box);
                m_helpBoxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_helpBoxStyle.margin = new RectOffset(0, 0, 0, 0);
                m_helpBoxStyle.padding = new RectOffset(5, 5, 5, 5);
                m_helpBoxStyle.alignment = TextAnchor.UpperLeft;
                m_helpBoxStyle.stretchWidth = true;
                m_helpBoxStyle.richText = true;
                m_helpBoxStyle.wordWrap = true;
            }

            if (m_linkStyle == null || (m_linkStyle.normal.textColor.r + m_linkStyle.normal.textColor.g + m_linkStyle.normal.textColor.b == 0))
            {
                m_linkStyle = new GUIStyle(GUI.skin.label);
                m_linkStyle.fontStyle = FontStyle.Normal;
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = Color.white;
                m_linkStyle.stretchWidth = false;
            }

#if !UNITY_2022_3_OR_NEWER
            EditorGUILayout.HelpBox($"Gaia 2023 requires Unity Version 2022.3 or later. Your current Unity version is: {Application.unityVersion}. Please install and use Gaia 2023 in a higher Unity version. If you need Gaia in earlier Unity versions, please consider using the older Gaia 2021 instead.", MessageType.Error);
            GUI.enabled = false;
#endif

            if (m_oldGaialinkContent == null || m_oldGaialinkContent.text == "")
            {
                m_oldGaialinkContent = new GUIContent("Migrating from Gaia 2021 to Gaia 2023", m_oldGaiaLink);
            }

            if (m_PDFGuideContent == null || m_PDFGuideContent.text == "")
            {
                m_PDFGuideContent = new GUIContent("Quick Start Guide PDF", "Opens a Quick Start PDF Document that covers the basic usage of Gaia.");
            }

            if (m_setupDocoContent == null || m_setupDocoContent.text == "")
            {
                m_setupDocoContent = new GUIContent("Setup Window Documentation", m_setupDocoLink);
            }

            if (m_gaiaTutorialContent == null || m_gaiaTutorialContent.text == "")
            {
                m_gaiaTutorialContent = new GUIContent("Gaia Tutorial Overview Page", m_gaiaTutorialLink);
            }

            if (m_linearLabel == null || m_linearLabel.text == "")
            {
                m_linearLabel = new GUIContent("Switch Color Space to Linear", "It is recommended to use linear color space in most projects. Activating this option will switch to linear color space BEFORE the install which will save you time as all following assets are imported using the correct color space already.");
            }

            if (m_linearLinkContent == null || m_linearLinkContent.text == "")
            {
                m_linearLinkContent = new GUIContent("Unity Manual - Linear or gamma workflow", m_linearLink);
            }

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField("<b>Gaia Setup</b>", m_helpBoxStyle);
                        EditorGUILayout.Space(5);
                        if (m_oldGaia)
                        {
                            EditorGUILayout.HelpBox($"We found an older version of Gaia in the following directory: '{m_oldGaiaPath.Replace(m_oldGaiaSearchString, "")}'. Gaia 2023 is NOT compatible with earlier versions of Gaia. Please remove the older Gaia installation completely before proceeding. " +
                            $"To learn about migrating from an earlier Gaia version follow the link below.", MessageType.Error);

                            EditorGUILayout.Space(10);

                            if (ClickableHeaderCustomStyle(m_oldGaialinkContent, m_linkStyle))
                            {
                                Application.OpenURL(m_oldGaiaLink);
                            }

                            EditorGUILayout.Space(10);

                            if (GUILayout.Button("Select Old Gaia Folder"))
                            {
                                UnityEngine.Object oldGaiaPath = AssetDatabase.LoadAssetAtPath(m_oldGaiaPath.Replace(m_oldGaiaSearchString, ""), typeof(UnityEngine.Object));
                                if (oldGaiaPath != null)
                                {
                                    EditorGUIUtility.PingObject(oldGaiaPath);
                                }
                            }

                            if (GUILayout.Button("Check Again"))
                            {
                                m_oldGaia = CheckForOlderGaiaVersions();
                            }

                            GUI.enabled = false;
                        }

                        if (m_installRunning && m_updateCoroutine == null)
                        {
                            m_updateCoroutine = PerformActions();
                            StartEditorUpdates();
                        }
                        GUILayout.Label("<b>Welcome to Gaia!</b>", m_helpBoxStyle);
                        GUILayout.Label("This installer allows you to customize your Gaia installation to suit the way you prefer to work.", m_helpBoxStyle);
                        GUILayout.Label("<b>1. CORE</b>: The Gaia world creation engine. This component is mandatory.", m_helpBoxStyle);
                        GUILayout.Label("<b>2. BIOMES</b>: Assets that Gaia will spawn into your world. Use these to fill your scene with content, or altenatively use them as learning to create your own. Biomes take some time to set up, but once this is done, you can easily re-use them across projects and scenes.", m_helpBoxStyle);
                        GUILayout.Label("<b>3. RUNTIME</b>: Assets and helpers that add value to your scene at runtime.", m_helpBoxStyle);
                        GUILayout.Label("Choose installation tasks by selecting the drop down's next to each component. Please be sure to delete the sources packages when you are done to reduce the space Gaia consumes in your project. For more information, please refer to the Setup Window Documentation linked below.", m_helpBoxStyle);
                        GUILayout.Label("To access Gaia after it has been installed use the <b>Window > Procedural Worlds > Gaia > Show Gaia Manager...</b> menu.", m_helpBoxStyle);
                        EditorGUILayout.Space(5);
                        if (ClickableHeaderCustomStyle(new GUIContent("After installing Gaia, please click here to view the Quick Start Guide and create your first world!"), m_linkStyle))
                        {
                            UnityEngine.Object guide = (UnityEngine.Object)SetupUtils.GetAsset("Gaia Quick Start.pdf", typeof(UnityEngine.Object));
                            if (guide != null)
                            {
                                Application.OpenURL(SetupUtils.GetFullFileSystemPath(AssetDatabase.GetAssetPath(guide)));
                            }
                        }
                        EditorGUILayout.Space(10);

                        EditorGUILayout.BeginVertical(m_helpBoxStyle);
                        {
                            Array categories = Enum.GetValues(typeof(PWPackageCategory));

                            foreach (PWPackageCategory category in categories)
                            {
                                string labelText = "";
                                switch (category)
                                {
                                    case PWPackageCategory.Core:
                                        labelText = "Gaia CORE (Mandatory): Creates worlds.";
                                        break;
                                    case PWPackageCategory.Assets:
                                        labelText = "Gaia BIOMES (Optional): Content to populate your world.";
                                        break;
                                    case PWPackageCategory.Runtime:
                                        labelText = "Gaia RUNTIME (Optional): Handy assets to enhance your scene at runtime.";
                                        break;
                                }

                                EditorGUILayout.LabelField(labelText);

                                //Inject the option to switch to linear color space
                                if (category == PWPackageCategory.Core)
                                {
                                    if (PlayerSettings.colorSpace != ColorSpace.Linear)
                                    {
                                        DrawLinearOption();
                                    }
                                    else
                                    {
                                        m_switchToLinear = false;
                                    }
                                }


                                PWPackageInfo[] categoryPackages = m_packageInfos.Where(x => x.m_category == category).ToArray();

                                GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
                                int originalFontSize = myStyle.fontSize;
                                myStyle.fontSize = 12;

                                for (int i = 0; i < categoryPackages.Length; i++)
                                {
                                    PWPackageInfo pWPackageInfo = categoryPackages[i];
                                    pWPackageInfo.DrawUI();
                                }

                                myStyle.fontSize = originalFontSize;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Space(10);

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(282);
                            GUILayout.Label($"Sources: {m_totalSourceSize:#,0.00} MB", GUILayout.Width(163));
                            GUILayout.Label($"Installed: {m_totalInstalledSize:#,0.00} MB", GUILayout.Width(175));
                            GUILayout.Label($"Total Disk Usage: {m_totalInstalledSize + m_totalSourceSize:#,0.00} MB");
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(5);

                        if (!m_packageInfos.Exists(x => x.m_currentAction != PWPackageAction.Nothing))
                        {
                            GUI.enabled = false;
                        }
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(45);

                            if (GUILayout.Button("Start Selected Actions"))
                            {
                                //Display warning in case of deletion of source packages
                                if (m_packageInfos.Exists(x => x.m_currentAction == PWPackageAction.DeleteSourcePackage || x.m_currentAction == PWPackageAction.UpdateAndDelete))
                                {
                                    if (!EditorUtility.DisplayDialog("Delete Source Package?", "Your selection includes deletion of installer sources, are you sure about this? Deleting the install packages will save space in your project. You can get them back if needed by re-importing Gaia.", "Continue", "Cancel"))
                                    {
                                        GUIUtility.ExitGUI();
                                        return;
                                    }
                                }

                                if (m_packageInfos.Exists(x => x.m_currentAction == PWPackageAction.Install || x.m_currentAction == PWPackageAction.UpdateAndDelete))
                                {
                                    if (EditorUtility.DisplayDialog("Open Quickstart Guide?", "The installation process can take some time. Would you like to view the Quick Start Guide while the installation is running?", "Yes", "No"))
                                    {
                                        UnityEngine.Object guide = (UnityEngine.Object)SetupUtils.GetAsset("Gaia Quick Start.pdf", typeof(UnityEngine.Object));
                                        if (guide != null)
                                        {
                                            Application.OpenURL(SetupUtils.GetFullFileSystemPath(AssetDatabase.GetAssetPath(guide)));
                                            //EditorGUIUtility.PingObject(guide);
                                        }
                                    }
                                }
                                //We copy all the packages we want to work on into a separate list - in this way we can sort the packages independently of what is displayed on the UI during the installation
                                m_actionPackages = m_packageInfos.FindAll(x => x.m_currentAction != PWPackageAction.Nothing);

                                m_updateCoroutine = PerformActions();
                                StartEditorUpdates();
                            }
                            GUILayout.Space(45);
                        }
                        EditorGUILayout.EndHorizontal();
                        GUI.enabled = originalGUIState;
                        EditorGUILayout.Space(5);
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(45);
                            if (!m_packageInfos.Exists(x => x.m_ID == "Gaia" && x.m_status == PWPackageStatus.Installed))
                            {
                                GUI.enabled = false;
                            }
                            if (GUILayout.Button("Open Gaia Manager"))
                            {
                                if (!m_installRunning)
                                {
                                    EditorApplication.ExecuteMenuItem("Window/Procedural Worlds/Gaia/Show Gaia Manager...");
                                    this.Close();
                                }
                            }
                            GUI.enabled = originalGUIState;

                            GUILayout.FlexibleSpace();

                            EditorGUILayout.BeginVertical();
                            if (ClickableHeaderCustomStyle(m_PDFGuideContent, m_linkStyle))
                            {
                                UnityEngine.Object guide = (UnityEngine.Object)SetupUtils.GetAsset("Gaia Quick Start.pdf", typeof(UnityEngine.Object));
                                if (guide != null)
                                {
                                    Application.OpenURL(SetupUtils.GetFullFileSystemPath(AssetDatabase.GetAssetPath(guide)));
                                    //EditorGUIUtility.PingObject(guide);
                                }
                            }
                            if (ClickableHeaderCustomStyle(m_gaiaTutorialContent, m_linkStyle))
                            {
                                Application.OpenURL(m_gaiaTutorialLink);
                            }
                            if (ClickableHeaderCustomStyle(m_setupDocoContent, m_linkStyle))
                            {
                                Application.OpenURL(m_setupDocoLink);
                            }
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(45);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUI.enabled = originalGUIState;
        }

        private void DrawLinearOption()
        {
            EditorGUILayout.BeginHorizontal();
            {

                GUILayout.Space(49);
                m_linearFoldedOut = EditorGUILayout.Foldout(m_linearFoldedOut, m_linearLabel, true);
                GUILayout.Space(180);
                m_switchToLinear = EditorGUILayout.ToggleLeft("", m_switchToLinear);

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            if (m_linearFoldedOut)
            {
                EditorGUILayout.BeginHorizontal();
                {

                    GUILayout.Space(49);
                    string message = "You can choose between linear and gamma color space in Unity. For most projects, linear color space is recommended. If you activate " +
                        "the checkbox to switch to linear before the installation, all assets will be imported in linear color space right away. This takes less time than " +
                        "switching to linear after the installation.";
                    GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
                    myStyle.fontSize = 12;
                    EditorGUILayout.HelpBox(message, MessageType.None);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {

                    GUILayout.Space(49);
                    if (ClickableHeaderCustomStyle(m_linearLinkContent, m_linkStyle))
                    {
                        Application.OpenURL(m_linearLink);
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
        }

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


        private bool CheckForOlderGaiaVersions()
        {
            //Look for the old Gaia config file inside an 2021 Gaia installation
            m_oldGaiaPath = SetupUtils.GetAssetPath(m_oldGaiaSearchString, false, SearchMode.EndsWith);
            if (m_oldGaiaPath == "")
            {
                //No path, no Gaia
                return false;
            }
            else
            {
                //If this path was found, an older Gaia version exists
                return true;
            }
        }

        #endregion

        #region Setup Coroutine
        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        //Stop editor updates
        public void StopEditorUpdates()
        {
            EditorApplication.update -= EditorUpdate;
        }

        /// <summary>
        /// This is executed only in the editor - using it to simulate co-routine execution and update execution
        /// </summary>
        void EditorUpdate()
        {
            if (m_updateCoroutine == null)
            {
                StopEditorUpdates();
            }
            else
            {
                m_updateCoroutine.MoveNext();
            }
        }

        IEnumerator PerformActions()
        {
            //SortPackagesByInstallOrder();
            m_installRunning = true;

            //First we need to check for the option for linear and switch to linear color space before anything else
            if (m_switchToLinear)
            {
                if (PlayerSettings.colorSpace != ColorSpace.Linear)
                {
                    m_waitingForLinear = true;
                    PlayerSettings.colorSpace = ColorSpace.Linear;
                    while (m_waitingForLinear)
                    {
                        yield return null;
                    }
                }
            }

            SortActionPackagesByInstallOrder();

            for (int i = 0; i < m_actionPackages.Count; i++)
            {
                PWPackageInfo pWPackageInfo = m_actionPackages[i];
                switch (pWPackageInfo.m_currentAction)
                {
                    case PWPackageAction.Nothing:
                        //my favorite case
                        break;
                    case PWPackageAction.Install:
                        m_waitingForPackage = true;
                        pWPackageInfo.OnPackageInstallCompleted -= PackageCompleted;
                        pWPackageInfo.OnPackageInstallCompleted += PackageCompleted;
                        pWPackageInfo.Install();
                        while (m_waitingForPackage)
                        {
                            yield return null;
                        }
                        pWPackageInfo.OnPackageInstallCompleted -= PackageCompleted;
                        pWPackageInfo.m_updateAvailable = false;
                        LogLastAction(pWPackageInfo, PWPackageAction.Install);
                        break;
                    case PWPackageAction.Uninstall:
                        m_waitingForPackage = true;
                        pWPackageInfo.OnPackageUnInstallCompleted -= PackageCompleted;
                        pWPackageInfo.OnPackageUnInstallCompleted += PackageCompleted;
                        pWPackageInfo.Uninstall();
                        while (m_waitingForPackage)
                        {
                            yield return null;
                        }
                        pWPackageInfo.OnPackageUnInstallCompleted -= PackageCompleted;
                        pWPackageInfo.m_updateAvailable = false;
                        pWPackageInfo.m_currentAction = PWPackageAction.Nothing;
                        //we do not log a "last action" here, as simply uninstalling the package is not
                        //relevant in case an update comes with a higher version
                        break;
                    case PWPackageAction.DeleteSourcePackage:
                        pWPackageInfo.DeleteSourcePackage();
                        LogLastAction(pWPackageInfo, PWPackageAction.DeleteSourcePackage);
                        pWPackageInfo.m_updateAvailable = false;
                        break;
                    case PWPackageAction.UpdateAndDelete:
                        m_waitingForPackage = true;
                        pWPackageInfo.OnPackageInstallCompleted -= PackageCompleted;
                        pWPackageInfo.OnPackageInstallCompleted += PackageCompleted;
                        pWPackageInfo.Install();
                        while (m_waitingForPackage)
                        {
                            yield return null;
                        }
                        pWPackageInfo.OnPackageInstallCompleted -= PackageCompleted;
                        pWPackageInfo.DeleteSourcePackage();
                        pWPackageInfo.m_updateAvailable = false;
                        LogLastAction(pWPackageInfo, PWPackageAction.UpdateAndDelete);
                        break;
                }
                EditorUtility.SetDirty(pWPackageInfo);
            }

            m_installRunning = false;
            m_updateCoroutine = null;
            StopEditorUpdates();
            SortPackagesByName();
            if (m_configSettings != null)
            {
                EditorUtility.SetDirty(m_configSettings);
            }
            RefreshTotalSizes();
            yield return null;
        }

        private void LogLastAction(PWPackageInfo pWPackageInfo, PWPackageAction lastAction)
        {
            m_configSettings = SetupUtils.GetOrCreatePackageConfigSettings();
            PWPackageConfig configEntry = m_configSettings.m_pWPackageConfigs.Find(x => x.m_packageID == pWPackageInfo.m_ID);
            if (configEntry == null)
            {
                configEntry = new PWPackageConfig();
                m_configSettings.m_pWPackageConfigs.Add(configEntry);
            }
            configEntry.m_packageID = pWPackageInfo.m_ID;
            configEntry.m_lastAction = lastAction;
            configEntry.m_majorVersion = m_majorVersion;
            configEntry.m_minorVersion = m_minorVersion;
            configEntry.m_patchVersion = m_patchVersion;
            if (lastAction == PWPackageAction.Install || lastAction == PWPackageAction.UpdateAndDelete)
            { 
                configEntry.m_installTimeStamp = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds();
            }
            pWPackageInfo.m_currentAction = PWPackageAction.Nothing;
        }

        private void PackageCompleted()
        {
            m_waitingForPackage = false;
            //make sure we are still listening to editor updates
            //packages containing code can break the coroutine!
            StartEditorUpdates();
        }
        #endregion


        private void UpdatePackages()
        {
            m_packageInfos.Clear();
            m_configSettings = SetupUtils.GetOrCreatePackageConfigSettings();
            m_appConfig = SetupUtils.GetAppConfig("Gaia");

            //If we cannot read version information, we assume it is version 0.0.0 and do not treat it like an update
            m_majorVersion = 0;
            m_minorVersion = 0;
            m_patchVersion = 0;

            //Update major minor patch version from this config
            if (m_appConfig != null)
            {
                try
                {
                    int.TryParse(m_appConfig.MajorVersion, out m_majorVersion);
                    int.TryParse(m_appConfig.MinorVersion, out m_minorVersion);
                    int.TryParse(m_appConfig.PatchVersion, out m_patchVersion);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "")
                    { }
                    Debug.LogError("Error while reading version information from the config file!");
                }
            }

            string[] allPWInfoGUIDs = AssetDatabase.FindAssets("t:PWPackageInfo");
            for (int i = 0; i < allPWInfoGUIDs.Length; i++)
            {
                bool actionChange = false;
                string guid = allPWInfoGUIDs[i];
                PWPackageInfo pWPackageInfo = (PWPackageInfo)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(PWPackageInfo));
                if (pWPackageInfo != null)
                {
                    //Init first, so we got a current state of the package
                    pWPackageInfo.Initialize();

                    m_packageInfos.Add(pWPackageInfo);
                    PWPackageConfig configEntry = null;
                    if (m_configSettings != null && m_configSettings.m_pWPackageConfigs != null && m_configSettings.m_pWPackageConfigs.Count > 0)
                    {
                        configEntry = m_configSettings.m_pWPackageConfigs.Find(x => x.m_packageID == pWPackageInfo.m_ID);
                    }
                    pWPackageInfo.m_updateAvailable = false;
                    //If there is a config entry, and the version of the application is higher
                    //than what is stored in that entry, we should inherit the last executed action
                    //In this way when the user installs an update, the setup window will remember
                    //the last choices and it is a one-click update.
                    if (configEntry != null && IsCurrentVersionHigher(configEntry, pWPackageInfo, m_appConfig))
                    {
                        //The package is currently installed, but the source package is deleted
                        // => suggest "Update and Delete" option
                        if (pWPackageInfo.m_status == PWPackageStatus.Installed && (configEntry.m_lastAction == PWPackageAction.UpdateAndDelete || configEntry.m_lastAction == PWPackageAction.DeleteSourcePackage))
                        {
                            pWPackageInfo.m_updateAvailable = true;
                            pWPackageInfo.m_currentAction = PWPackageAction.UpdateAndDelete;
                            actionChange = true;
                        }

                        //The package is currently not installed, and the last action was that the source package was deleted
                        // => suggest "Delete Source Package" option
                        if (pWPackageInfo.m_status == PWPackageStatus.NotInstalled && (configEntry.m_lastAction == PWPackageAction.UpdateAndDelete || configEntry.m_lastAction == PWPackageAction.DeleteSourcePackage))
                        {
                            pWPackageInfo.m_currentAction = PWPackageAction.DeleteSourcePackage;
                            actionChange = true;
                        }

                        //The package is currently installed, and the last action was to install the package
                        // => suggest "Install Package" option (this will be labeled as "Update Package" in the UI automatically)
                        if (pWPackageInfo.m_status == PWPackageStatus.Installed && (configEntry.m_lastAction == PWPackageAction.Install))
                        {
                            pWPackageInfo.m_updateAvailable = true;
                            pWPackageInfo.m_currentAction = PWPackageAction.Install;
                            actionChange = true;
                        }

                    }
                    else
                    {
                        //no updates - we only suggest deleting the install sources for already installed packages
                        if (pWPackageInfo.m_status == PWPackageStatus.Installed && pWPackageInfo.m_unityPackage != null)
                        {
                            pWPackageInfo.m_currentAction = PWPackageAction.DeleteSourcePackage;
                            actionChange = true;
                        }
                    }


                    //if there is no config entry, we can assume / apply the default action for the package
                    if (configEntry == null)
                    {
                        if (pWPackageInfo.m_status != PWPackageStatus.Installed)
                        {
                            pWPackageInfo.m_currentAction = (PWPackageAction)pWPackageInfo.m_defaultAction;
                            actionChange = true;
                        }
                        else
                        {
                            //suggest deleting the source if it is still there
                            if (pWPackageInfo.m_unityPackage != null)
                            {
                                pWPackageInfo.m_currentAction = PWPackageAction.DeleteSourcePackage;
                                actionChange = true;
                            }
                            else
                            {
                                //no to do.
                                pWPackageInfo.m_currentAction = PWPackageAction.Nothing;
                                actionChange = true;
                            }
                        }
                    }

                    if (actionChange)
                    {
#if UNITY_EDITOR
                        EditorUtility.SetDirty(pWPackageInfo);
#endif
                    }
                }
            }
            SortPackagesByName();

        }

        private bool IsCurrentVersionHigher(PWPackageConfig configEntry, PWPackageInfo packageInfo, PWCommon5.AppConfig appConfig)
        {
            //The following are the three possible constellations where the application config version number would be higher
            //The major version is higher
            if (m_majorVersion > configEntry.m_majorVersion)
            {
                return true;
            }
            //The major version is equal, but minor version is higher
            else if (m_majorVersion == configEntry.m_majorVersion && m_minorVersion > configEntry.m_minorVersion)
            {
                return true;
            }
            //Major & minor are equal, but patch version is higher
            else if (m_majorVersion == configEntry.m_majorVersion && m_minorVersion == configEntry.m_minorVersion && m_patchVersion > configEntry.m_patchVersion)
            {
                return true;
            }

            //Last check - is the file creation date and time higher than the time the install was performed - this would also mean a newer package.
            if (packageInfo.m_unityPackage != null)
            {
                FileInfo fi = new FileInfo(AssetDatabase.GetAssetPath(packageInfo.m_unityPackage));
                if (configEntry.m_installTimeStamp < new System.DateTimeOffset(fi.CreationTimeUtc).ToUnixTimeMilliseconds())
                {
                    return true;
                }
            }
            return false;
        }

        private void SortPackagesByName()
        {
            m_packageInfos.Sort(delegate (PWPackageInfo x, PWPackageInfo y)
            {
                if (x.m_displayName == null && y.m_displayName == null) return 0;
                else if (x.m_displayName == null) return -1;
                else if (y.m_displayName == null) return 1;
                else return x.m_displayName.CompareTo(y.m_displayName);
            });
            ;
        }

        private void SortActionPackagesByInstallOrder()
        {
            m_actionPackages.Sort(delegate (PWPackageInfo x, PWPackageInfo y)
            {
                return x.m_installOrder.CompareTo(y.m_installOrder);
            });
            ;
        }
    }

    internal class SetupEditorWindowPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            SetupEditorWindow.m_waitingForLinear = false;
        }
    }

}
