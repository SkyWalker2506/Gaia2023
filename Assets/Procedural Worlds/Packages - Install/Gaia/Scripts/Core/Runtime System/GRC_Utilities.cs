using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    public class GRC_Utilities : GaiaRuntimeComponent
    {

        public bool m_addWind = true;
        public bool m_addScreenshotter = true;
        public bool m_addLoadingScreen = true;
        public GaiaConstants.GaiaGlobalWindType m_windType;

        private GUIContent m_addWindLabel;
        private GUIContent m_windTypeLabel;
        private GUIContent m_screenshotterLabel;
        private GUIContent m_loadingScreenLabel;
        private GUIContent m_generalHelpLink;



        private GUIContent m_panelLabel;
        public override GUIContent PanelLabel
        {
            get
            {
                if (m_panelLabel == null || m_panelLabel.text == "")
                {
                    m_panelLabel = new GUIContent("Utilities", "Add smaller Utilities like a wind zone or a screenshot tool to your scene.");
                }
                return m_panelLabel;
            }
        }

        public override void Initialize()
        {
            m_orderNumber = 600;
            if (m_addWindLabel == null || m_addWindLabel.text == "")
            {
                m_addWindLabel = new GUIContent("Add Wind", "Adds a wind zone object to the scene that syncs the wind movement with the Gaia Vegetation Shaders.");
            }
            if (m_windTypeLabel == null || m_windTypeLabel.text == "")
            {
                m_windTypeLabel = new GUIContent("Wind Type", "Select the wind type preset to use in the wind zone.");
            }
            if (m_screenshotterLabel == null || m_screenshotterLabel.text == "")
            {
                m_screenshotterLabel = new GUIContent("Add Screenshotter", "Adds a script to the camera that allows you to take screenshots (Default Key = F12).");
            }
            if (m_loadingScreenLabel == null || m_loadingScreenLabel.text == "")
            {
                m_loadingScreenLabel = new GUIContent("Add Loading Screen", "Adds a loading screen that is shown at the beginning of the scene until all terrains are loaded in. (Gaia Pro & Terrain Loading only).");
            }
            if (m_generalHelpLink == null || m_generalHelpLink.text == "")
            {
                m_generalHelpLink = new GUIContent("Utilities Module on Canopy", "Opens the Canopy Online Help Article for the Utilities Module");
            }
        }

        public override void DrawUI()
        {
            DisplayHelp("The Utilities Module is a collection of smaller helpers / tools that you can add to your scene.", m_generalHelpLink, "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/creating_runtime/runtime-module-utilities-r165/");

#if UNITY_EDITOR
            bool originalGUIstate = GUI.enabled;

            EditorGUI.BeginChangeCheck();
            {
                m_addWind = EditorGUILayout.Toggle(m_addWindLabel, m_addWind);
                DisplayHelp("Will add a windzone to the scene that contains a script that will propagate the wind values to the Gaia shaders so that e.g. trees react to wind correctly.");

                GUI.enabled = m_addWind;
                m_windType = (GaiaConstants.GaiaGlobalWindType)EditorGUILayout.EnumPopup(m_windTypeLabel, m_windType);
                DisplayHelp("Select a wind preset for the wind zone here.");

                GUI.enabled = originalGUIstate;
                m_addScreenshotter = EditorGUILayout.Toggle(m_screenshotterLabel, m_addScreenshotter);
                DisplayHelp("Will add a small tool to the scene that allows you to take screenshots during runtime (Default key: F12)");
                m_addLoadingScreen = EditorGUILayout.Toggle(m_loadingScreenLabel, m_addLoadingScreen);
                DisplayHelp("Will add a loading screen to the scene. This only has an effect in scenes that use Terrain Loading (Gaia Pro Feature)");

            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);

            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove"))
            {
                RemoveFromScene();
            }
            GUILayout.Space(15);
            if (GUILayout.Button("Apply"))
            {
                AddToScene();
            }
            GUILayout.EndHorizontal();

            GUI.enabled = originalGUIstate;
#endif
        }

        public override void AddToScene()
        {
            if (m_addWind)
            {
                CreateWindZone();
            }
            else
            {
                WindManager.RemoveWindManager();
            }

            if (m_addScreenshotter)
            {
                CreateScreenShotter();
            }
            else
            {
                DestroyScreenShotter();
            }
            if (m_addLoadingScreen && GaiaUtils.HasDynamicLoadedTerrains())
            {
                CreateLoadingScreen();
            }
            else
            {
                DestroyLoadingScreen();
            }
        }

        public static void CreateLoadingScreen()
        {
#if GAIA_2023_PRO

            GameObject loadingScreenObject = GameObject.Find(GaiaConstants.loadingScreenName);
            if (loadingScreenObject == null)
            {
                GameObject runtimeObj = GaiaUtils.GetRuntimeSceneObject(false);
                foreach (Transform t in runtimeObj.transform)
                {
                    if (t.name == GaiaConstants.loadingScreenName)
                    {
                        loadingScreenObject = t.gameObject;
                        break;
                    }
                }
            }

            if (loadingScreenObject == null)
            {
                GameObject loadingScreenPrefab = GaiaUtils.GetAssetPrefab(GaiaConstants.loadingScreenName);
                if (loadingScreenPrefab != null)
                {
                    loadingScreenObject = GameObject.Instantiate(loadingScreenPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    if (loadingScreenObject != null)
                    {
                        loadingScreenObject.name = loadingScreenObject.name.Replace("(Clone)", "");
                        GameObject runtimeParent = GaiaUtils.GetRuntimeSceneObject();
                        if (runtimeParent != null)
                        {
                            loadingScreenObject.transform.SetParent(runtimeParent.transform);
                        }
                        TerrainLoaderManager.Instance.m_loadingScreen = loadingScreenObject.GetComponent<GaiaLoadingScreen>();
                        loadingScreenObject.SetActive(false);
                    }
                }
            }
#endif
        }

        public static void DestroyLoadingScreen()
        {
            GaiaLoadingScreen loadingScreen = FindAnyObjectByType<GaiaLoadingScreen>(FindObjectsInactive.Include);
            if (loadingScreen != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(loadingScreen.gameObject);
                }
                else
                {
                    DestroyImmediate(loadingScreen.gameObject);
                }
            }
        }

        private void DestroyScreenShotter()
        {
            ScreenShotter screenshotter = FindAnyObjectByType<ScreenShotter>(FindObjectsInactive.Include);
            if (screenshotter != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(screenshotter.gameObject);
                }
                else
                {
                    DestroyImmediate(screenshotter.gameObject);
                }
            }
        }


        public override void RemoveFromScene()
        {
            GameObject shotterObj = GameObject.Find(GaiaConstants.gaiaScreenshotter);
            if (shotterObj != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(shotterObj);
                }
                else
                {
                    GameObject.DestroyImmediate(shotterObj);
                }
            }

            GameObject loadingScreenObject = GameObject.Find(GaiaConstants.loadingScreenName);
            if (loadingScreenObject != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(loadingScreenObject);
                }
                else
                {
                    GameObject.DestroyImmediate(loadingScreenObject);
                }
            }

            //Only remove wind if there is no PW sky in ths scene, it will depend on it!
            PWSkyStandalone pws = GameObject.FindAnyObjectByType<PWSkyStandalone>();
            if (pws == null)
            {
                GameObject windZone = GameObject.Find("PW Wind Zone");
                if (windZone != null)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(windZone);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(windZone);
                    }
                }
            }

        }


        private void CreateScreenShotter()
        {
            GameObject shotterObj = GameObject.Find(GaiaConstants.gaiaScreenshotter);
            if (shotterObj == null)
            {
                GaiaSettings gaiaSettings = GaiaUtils.GetGaiaSettings();
                shotterObj = new GameObject(GaiaConstants.gaiaScreenshotter);
                Gaia.ScreenShotter shotter = shotterObj.AddComponent<Gaia.ScreenShotter>();
                shotter.m_targetDirectory = gaiaSettings.m_screenshotsDirectory.Replace("Assets/", "");
                shotter.m_watermark = GaiaUtils.GetAsset("Made With Gaia Watermark.png", typeof(Texture2D)) as Texture2D;
                shotter.m_mainCamera = GaiaUtils.GetCamera();
                GameObject gaiaObj = GaiaUtils.GetRuntimeSceneObject();
                shotterObj.transform.parent = gaiaObj.transform;
                shotterObj.transform.position = Gaia.TerrainHelper.GetActiveTerrainCenter(false);
            }

        }

        private void CreateWindZone()
        {
            GameObject gaiaObj = GaiaUtils.GetRuntimeSceneObject();

            WindZone globalWind = gaiaObj.GetComponentInChildren<WindZone>();
            if (globalWind == null)
            {
                GameObject windZoneObj = new GameObject("PW Wind Zone");
                windZoneObj.transform.Rotate(new Vector3(25f, 0f, 0f));
                globalWind = windZoneObj.AddComponent<WindZone>();
                windZoneObj.transform.SetParent(gaiaObj.transform);
            }
            switch (m_windType)
            {
                case GaiaConstants.GaiaGlobalWindType.Calm:
                    globalWind.windMain = 0.35f;
                    globalWind.windTurbulence = 0.35f;
                    globalWind.windPulseMagnitude = 0.2f;
                    globalWind.windPulseFrequency = 0.05f;
                    break;
                case GaiaConstants.GaiaGlobalWindType.Moderate:
                    globalWind.windMain = 0.55f;
                    globalWind.windTurbulence = 0.45f;
                    globalWind.windPulseMagnitude = 0.2f;
                    globalWind.windPulseFrequency = 0.1f;
                    break;
                case GaiaConstants.GaiaGlobalWindType.Strong:
                    globalWind.windMain = 0.75f;
                    globalWind.windTurbulence = 0.5f;
                    globalWind.windPulseMagnitude = 0.2f;
                    globalWind.windPulseFrequency = 0.25f;
                    break;
                case GaiaConstants.GaiaGlobalWindType.None:
                    globalWind.windMain = 0f;
                    globalWind.windTurbulence = 0f;
                    globalWind.windPulseMagnitude = 0f;
                    globalWind.windPulseFrequency = 0f;
                    break;
            }

            WindManager windManager = globalWind.GetComponent<WindManager>();
            if (windManager == null)
            {
                windManager = globalWind.gameObject.AddComponent<WindManager>();
            }

            if (windManager.m_windAudioClip == null)
            {
#if UNITY_EDITOR
                windManager.m_windAudioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(GaiaUtils.GetAssetPath("Gaia Ambient Wind.mp3"));
#endif
            }
            windManager.InstantWindApply();


        }

    }
}
