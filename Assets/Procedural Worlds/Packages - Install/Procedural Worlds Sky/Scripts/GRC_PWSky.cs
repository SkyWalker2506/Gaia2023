using UnityEditor;
using UnityEngine;

namespace Gaia
{
    public class GRC_PWSky : GaiaRuntimeComponent
    {
        private GUIContent m_generalHelpLink;

        private GRC_PWSkyPrefabs m_skyPrefabs;
        public GRC_PWSkyPrefabs SkyPrefabs
        {
            get
            {
                if (m_skyPrefabs == null)
                {
                    m_skyPrefabs = GetSkyPrefabs();
                }
                return m_skyPrefabs;
            }
        }

        private GUIContent m_panelLabel;
        public override GUIContent PanelLabel
        {
            get
            {
                if (m_panelLabel == null || m_panelLabel.text == "")
                {
                    m_panelLabel = new GUIContent("Procedural Worlds Sky", "Add a dynamic Sky system with changing time of day, layered clouds and weather to your scene..");
                }
                return m_panelLabel;
            }
        }

        public override void Initialize()
        {
            m_orderNumber = 400;

            if (m_generalHelpLink == null || m_generalHelpLink.text == "")
            {
                m_generalHelpLink = new GUIContent("Procedural Worlds Sky Module on Canopy", "Opens the Canopy Online Help Article for the Procedural Worlds Sky Module");
            }
        }

        public override void DrawUI()
        {
#if UNITY_EDITOR
            bool originalGUIState = GUI.enabled;

            DisplayHelp("This module adds the Procedural Worlds Sky to the scene. This is a dynamic time of day lighting system that allows you to simulate daytime & nighttime in your scene.", m_generalHelpLink, "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/creating_runtime/runtime-module-procedural-worlds-sky-r164/");


#if HDPipeline
            EditorGUILayout.HelpBox("The Procedural Worlds Sky is not available in the HD Render Pipeline. Please use the Lighting Presets instead, and / or use the HDRP environment volumes for outdoor lighting.", MessageType.Warning);
#endif

            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove"))
                {
                    RemoveFromScene();
                }
                GUILayout.Space(15);
#if HDPipeline
                GUI.enabled = false;
#endif
                if (GUILayout.Button("Apply"))
                {
                    AddToScene();
                }
                GUI.enabled = originalGUIState;
                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        public override void AddToScene()
        {
#if HDPipeline
            //Not available in the HD Pipeline, exit now
            return;
#else
            //Remove any old versions first
            RemoveFromScene();

            //Deactivate any remaining directional lights
            var allLights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < allLights.Length; i++)
            {
                Light light = allLights[i];
                if (light.type == LightType.Directional)
                {
                    light.gameObject.SetActive(false);
                }
            }

            if (SkyPrefabs.m_pwSkyBox != null)
            {
                //Do not apply the skybox directly into that slot, so that changes
                //by the user do not destroy the original material.
                RenderSettings.skybox = Instantiate<Material>(SkyPrefabs.m_pwSkyBox);
            }

            //Create the Lighting object
            GameObject lightingObject = GaiaUtils.GetLightingObject();

            GameObject dirLight = null;
            //Create the directional (sun) light below lighting
            if (SkyPrefabs.m_directionalLight != null)
            {
                dirLight = GameObject.Instantiate(SkyPrefabs.m_directionalLight, lightingObject.transform);
                dirLight.name = dirLight.name.Replace("(Clone)", "");
            }

            //Create PP prefab
#if UPPipeline
            if (SkyPrefabs.m_postProcessURPPrefab != null)
            {
                GameObject.Instantiate(SkyPrefabs.m_postProcessURPPrefab, lightingObject.transform);
            }

#else
#if UNITY_POST_PROCESSING_STACK_V2
            if (SkyPrefabs.m_postProcessbuiltInPrefab != null)
            {
                GameObject lightingGO = GameObject.Instantiate(SkyPrefabs.m_postProcessbuiltInPrefab, lightingObject.transform);
                lightingGO.name = lightingGO.name.Replace("(Clone)", "");
            }
#endif
#endif


            //Create all Weather / Sky VFX below

            if (SkyPrefabs.m_weather != null)
            {
                GameObject weatherGO = GameObject.Instantiate(SkyPrefabs.m_weather, lightingObject.transform);
                weatherGO.name = weatherGO.name.Replace("(Clone)", "");
            }

            //Create the sky object
            GameObject pwSkyObj = null;
            if (SkyPrefabs.m_pwSky != null)
            {
                pwSkyObj = GameObject.Instantiate(SkyPrefabs.m_pwSky, lightingObject.transform);
                pwSkyObj.name = pwSkyObj.name.Replace("(Clone)", "");
            }

            PWSkyStandalone pwss = pwSkyObj.GetComponent<PWSkyStandalone>();
            pwss.m_gaiaTimeOfDay.m_todMinutes = 0;
            pwss.m_gaiaTimeOfDay.m_todHour = 14;

#if UPPipeline
            pwss.m_profileValues.PostProcessProfileURP = SkyPrefabs.m_postprocessURP;
#else
#if UNITY_POST_PROCESSING_STACK_V2
            pwss.m_profileValues.PostProcessProfileBuiltIn = SkyPrefabs.m_postprocessBuiltIn;
#endif
#endif

            GaiaUtils.RefreshPlayerSetup();
        }

        public override void RemoveFromScene()
        {
            GameObject lightingObject = GaiaUtils.GetLightingObject(false);
            if (lightingObject != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(lightingObject);
                }
                else
                {
                    GameObject.DestroyImmediate(lightingObject);
                }
            }

            GameObject pwSkyObject = GaiaUtils.GetPWSkyObject();
            if (pwSkyObject != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(pwSkyObject);
                }
                else
                {
                    GameObject.DestroyImmediate(pwSkyObject);
                }
            }

            GameObject weatherObject = GaiaUtils.GetWeatherObject();
            if (weatherObject != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(weatherObject);
                }
                else
                {
                    GameObject.DestroyImmediate(weatherObject);
                }
            }
#endif
        }


        /// <summary>
        /// Return WaterSettings or null;
        /// </summary>
        /// <returns>Gaia settings or null if not found</returns>
        public static GRC_PWSkyPrefabs GetSkyPrefabs()
        {
            return GaiaUtils.GetAsset("PW Sky Prefabs.asset", typeof(GRC_PWSkyPrefabs)) as GRC_PWSkyPrefabs;
        }
    }
}
