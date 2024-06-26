using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif
using UnityEngine;
using UnityEngine.Rendering;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Gaia
{
    public class GRC_UnityHDRPWater : GaiaRuntimeComponent
    {

        public bool m_activateFoam = false;
        public bool m_activateSSR = true;

        private GameObject m_currentWaterPrefab;
        private GUIContent m_generalHelpLink;
        private GUIContent m_activateFoamLabel;
        private GUIContent m_activateSSRLabel;

        private GRC_UnityHDRPWaterSettings m_waterSettings;
        public GRC_UnityHDRPWaterSettings WaterSettings
        {
            get
            {
                if (m_waterSettings == null)
                {
                    m_waterSettings = GetWaterSettings();
                }
                return m_waterSettings;
            }
        }

        private GUIContent m_panelLabel;
        public override GUIContent PanelLabel
        {
            get
            {
                if (m_panelLabel == null || m_panelLabel.text == "")
                {
                    m_panelLabel = new GUIContent("Unity HDRP Water", "Uses the Unity HDRP water system to create an ocean at the sea level of your scene. Only works in the HD Render Pipeline.");
                }
                return m_panelLabel;
            }
        }

        public override void Initialize()
        {
            m_orderNumber = 500;

            if (m_activateFoamLabel == null || m_activateFoamLabel.text == "")
            {
                m_activateFoamLabel = new GUIContent("Activate Foam", "Activates Water Foam Rendering for the Water Surface in the HDRP Quality settings.");
            }
            if (m_activateSSRLabel == null || m_activateSSRLabel.text == "")
            {
                m_activateSSRLabel = new GUIContent("Activate Reflections", "This will activate Transparent Screen Space Reflections in the HDRP Global Settings for a reflective water surface.");
            }
            if (m_generalHelpLink == null || m_generalHelpLink.text == "")
            {
                m_generalHelpLink = new GUIContent("Unity HDRP Water Module on Canopy", "Opens the Canopy Online Help Article for the Gaia Water Module");
            }
            if (WaterSettings != null)
            {
#if UNITY_2023_1_OR_NEWER
                m_currentWaterPrefab = WaterSettings.m_2023_1_WaterPrefab;
#else
                m_currentWaterPrefab = WaterSettings.m_2022_3_WaterPrefab;
#endif
            }
        }

        public override void DrawUI()
        {
#if UNITY_EDITOR
            DisplayHelp("This runtime module will utilize the Unity HDRP water system to create an ocean at the current sea level of your scene. You can further enhance the water surface with additional tools from the HDRP water system, please see the link for more.", m_generalHelpLink, "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/creating_runtime/runtime-module-unity-hdrp-water-r168/");

            bool originalGUIState = GUI.enabled;

#if !HDPipeline
            EditorGUILayout.HelpBox("The Unity HDRP Water system requires the HD render pipeline to be active in order to be used. Please install the HD render pipeline in your project and configure Gaia to the HD Pipeline from the Configuration tab.", MessageType.Warning);
            GUI.enabled = false;
#endif

            EditorGUI.BeginChangeCheck();
            {
#if UNITY_2023_1_OR_NEWER
                m_activateFoam = EditorGUILayout.Toggle(m_activateFoamLabel, m_activateFoam);
                DisplayHelp("Activates Water Foam Rendering in the HDRP Quality settings. This will render white foam on top of the waves.");
#endif
                m_activateSSR = EditorGUILayout.Toggle(m_activateSSRLabel, m_activateSSR);
                DisplayHelp("This will activate Transparent Screen Space Reflections in the HDRP Global Settings for a reflective water surface.");

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
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }

            GUI.enabled = originalGUIState;
#endif
        }

        public override void AddToScene()
        {
            //There are a couple of HDRP settings that need to be active for the HDRP water to render, see 
            //https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@16.0/manual/WaterSystem-use.html
            ConfigureHDRPWater();

#if UNITY_2023_2_OR_NEWER
            m_currentWaterPrefab = WaterSettings.m_2023_2_WaterPrefab;
#elif UNITY_2023_1_OR_NEWER
            m_currentWaterPrefab = WaterSettings.m_2023_1_WaterPrefab;
#else
            m_currentWaterPrefab = WaterSettings.m_2022_3_WaterPrefab;
#endif

            //Remove any old versions first
            RemoveFromScene();
            GameObject gaiaRuntimeObject = GaiaUtils.GetRuntimeSceneObject(true);
            if (m_currentWaterPrefab != null)
            {
                GameObject newWaterGO = GameObject.Instantiate(m_currentWaterPrefab, gaiaRuntimeObject.transform);
                newWaterGO.name = newWaterGO.name.Replace("(Clone)", "");

                float seaLevel = GaiaAPI.GetSeaLevel();
                newWaterGO.transform.position = new Vector3(0f, seaLevel, 0f);
            }

            //Deactivate sea plane on stamper, spawner, biome controller (ugly otherwise)
            foreach (var spawner in FindObjectsByType<Spawner>(FindObjectsSortMode.None))
            {
                spawner.m_showSeaLevelPlane = false;
            }
            foreach (var stamper in FindObjectsByType<Stamper>(FindObjectsSortMode.None))
            {
                stamper.m_showSeaLevelPlane = false;
            }
            foreach (var biomeController in FindObjectsByType<BiomeController>(FindObjectsSortMode.None))
            {
                biomeController.m_showSeaLevelPlane = false;
            }


        }

        private void ConfigureHDRPWater()
        {
#if HDPipeline && UNITY_EDITOR
            HDRenderPipelineAsset rpAsset = GetHDRPAsset();
            if (rpAsset == null)
            {
                return;
            }

            //Water needs to be activated in various spots in the HDRP config

            //Quality Settings > Rendering
            RenderPipelineSettings rps = rpAsset.currentPlatformRenderPipelineSettings;
            rps.supportWater = true;
#if UNITY_2023_1_OR_NEWER
            rps.supportWaterDeformation = true;
            rps.supportWaterExclusion = true;
            rps.supportWaterFoam = m_activateFoam;
#endif
            rpAsset.currentPlatformRenderPipelineSettings = rps;

            RenderPipelineGlobalSettings globalSettings = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
            SerializedObject globalSettingsSO = new SerializedObject(globalSettings);

            //Camera Frame settings > Rendering
            //these settings are serialized differently depending on unity / hdrp version
#if UNITY_2023_2_OR_NEWER
            SerializedProperty frameSettingsProp1 = globalSettingsSO.FindProperty("m_RenderingPath");
            SerializedProperty cameraSettings = frameSettingsProp1.FindPropertyRelative("m_Camera");
            FrameSettings frameSettings1 = (FrameSettings)cameraSettings.boxedValue;
#else
            SerializedProperty frameSettingsProp1 = globalSettingsSO.FindProperty("m_RenderingPathDefaultCameraFrameSettings");
            FrameSettings frameSettings1 = (FrameSettings)frameSettingsProp1.boxedValue;
#endif

            frameSettings1.SetEnabled(FrameSettingsField.Water, true);
#if UNITY_2023_1_OR_NEWER
            frameSettings1.SetEnabled(FrameSettingsField.WaterDeformation, true);
            frameSettings1.SetEnabled(FrameSettingsField.WaterExclusion, true);
#endif
            if (m_activateSSR)
            {
                frameSettings1.SetEnabled(FrameSettingsField.SSR, true);
                frameSettings1.SetEnabled(FrameSettingsField.TransparentSSR, true);
            }
            else
            {
                //Leave SSR enabled in General - can't tell if user still would use it for other things
                frameSettings1.SetEnabled(FrameSettingsField.TransparentSSR, false);
            }

            //Realtime Reflection Frame settings > Rendering
#if UNITY_2023_2_OR_NEWER
            cameraSettings.boxedValue = frameSettings1;

            SerializedProperty reflectionSettings = frameSettingsProp1.FindPropertyRelative("m_RealtimeReflection");
            FrameSettings frameSettings2 = (FrameSettings)reflectionSettings.boxedValue;

#else
            frameSettingsProp1.boxedValue = frameSettings1;

            SerializedProperty frameSettingsProp2 = globalSettingsSO.FindProperty("m_RenderingPathDefaultRealtimeReflectionFrameSettings");
            FrameSettings frameSettings2 = (FrameSettings)frameSettingsProp2.boxedValue;
#endif

            frameSettings2.SetEnabled(FrameSettingsField.Water, true);
#if UNITY_2023_1_OR_NEWER
            frameSettings2.SetEnabled(FrameSettingsField.WaterDeformation, true);
            frameSettings2.SetEnabled(FrameSettingsField.WaterExclusion, true);
#endif

            //Baked or Custom Reflection Frame settings > Rendering
#if UNITY_2023_2_OR_NEWER
            reflectionSettings.boxedValue = frameSettings2;

            SerializedProperty bakedReflectionSettings = frameSettingsProp1.FindPropertyRelative("m_CustomOrBakedReflection");
            FrameSettings frameSettings3 = (FrameSettings)bakedReflectionSettings.boxedValue;
#else
            frameSettingsProp2.boxedValue = frameSettings2;

            SerializedProperty frameSettingsProp3 = globalSettingsSO.FindProperty("m_RenderingPathDefaultBakedOrCustomReflectionFrameSettings");
            FrameSettings frameSettings3 = (FrameSettings)frameSettingsProp3.boxedValue;
#endif

            frameSettings3.SetEnabled(FrameSettingsField.Water, true);
#if UNITY_2023_1_OR_NEWER
            frameSettings3.SetEnabled(FrameSettingsField.WaterDeformation, true);
            frameSettings3.SetEnabled(FrameSettingsField.WaterExclusion, true);
#endif

#if UNITY_2023_2_OR_NEWER
            bakedReflectionSettings.boxedValue = frameSettings3;
#else
            frameSettingsProp3.boxedValue = frameSettings3;
#endif

            globalSettingsSO.ApplyModifiedProperties();

#if UNITY_EDITOR
            EditorUtility.SetDirty(rpAsset);
#endif

            bool foundVolume = false;

            //Look for a global rendering volumes in the scene, if available, activate water in there
            foreach (Volume v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (!v.isGlobal)
                {
                    continue;
                }
                foundVolume = true;
                WaterRendering wr = null;
                if (!v.sharedProfile.TryGet<WaterRendering>(out wr))
                {
                    wr = VolumeProfileFactory.CreateVolumeComponent<WaterRendering>(v.sharedProfile);
                }
                wr.active = true;
                wr.enable.value = true;
                wr.enable.overrideState = true;
                EditorUtility.SetDirty(v.sharedProfile);
            }
            if (!foundVolume)
            {
                Debug.LogError("Could not find a global HDRP Volume in the scene - the HDRP Water surface will not be rendered correctly. Please make sure your scene setup contains a global HDRP Environment Volume. If in doubt, use the Gaia Lighting Presets.");
            }
#endif
        }

#if UNITY_EDITOR && HDPipeline
        public HDRenderPipelineAsset GetHDRPAsset()
        {
            //Do we have a render pipeline asset at quality level? If yes, we take this one,
            //as it will override what is in the default settings in the "regular" pipeline asset
            HDRenderPipelineAsset asset = (HDRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel());
            if (asset != null)
            {
                return asset;
            }
            //otherwise: Get the default asset
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                return (HDRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
            }
            else
            {
                Debug.LogError("Error while getting Render Pipeline Asset for the HDRP Water Setup. Do you have a render pipeline asset assigned in the project Graphics Settings?");
                return null;
            }
        }
#endif

        public override void RemoveFromScene()
        {
            GameObject unityWaterObject = GaiaUtils.GetRuntimeChild(m_currentWaterPrefab.name, false);
            if (unityWaterObject != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(unityWaterObject);
                }
                else
                {
                    GameObject.DestroyImmediate(unityWaterObject);
                }
            }

            //Re-activate sea plane on stamper, spawner, biome controller (ugly otherwise)
            foreach (var spawner in FindObjectsByType<Spawner>(FindObjectsSortMode.None))
            {
                spawner.m_showSeaLevelPlane = true;
            }
            foreach (var stamper in FindObjectsByType<Stamper>(FindObjectsSortMode.None))
            {
                stamper.m_showSeaLevelPlane = true;
            }
            foreach (var biomeController in FindObjectsByType<BiomeController>(FindObjectsSortMode.None))
            {
                biomeController.m_showSeaLevelPlane = true;
            }
        }


        /// <summary>
        /// Return WaterSettings or null;
        /// </summary>
        /// <returns>Gaia settings or null if not found</returns>
        public static GRC_UnityHDRPWaterSettings GetWaterSettings()
        {
            return GaiaUtils.GetAsset("Unity HDRP Water Settings.asset", typeof(GRC_UnityHDRPWaterSettings)) as GRC_UnityHDRPWaterSettings;
        }
    }
}
