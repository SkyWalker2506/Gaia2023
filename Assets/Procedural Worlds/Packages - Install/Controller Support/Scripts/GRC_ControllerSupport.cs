using Gaia;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System;
#endif
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
#if UPPipeline
using UnityEngine.Rendering.Universal;
#endif
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Gaia
{
    public enum ControllerType
    {
        FlyCam,
        [InspectorName("First Person - Unity Starter Assets")]
        FirstPerson,
        [InspectorName("Third Person - Unity Starter Assets")]
        ThirdPerson,
        [InspectorName("Custom - Configure your own Controller")]
        Custom,
        RuntimeGenerated
    }

    public class GRCControllerSupport : GaiaRuntimeComponent
    {

        public ControllerType m_selectedControllerType = ControllerType.FlyCam;
        public bool m_activateTouchUI = false;
        public bool m_addLocationSystem = false;
        public bool m_addAudioManager = false;
        public bool m_customAdjustDrawDistance = true;
        public bool m_customSetupPostProcessing = true;
        public bool m_addVFXManager = false;
        public GameObject m_customCharacter;
        public Camera m_customCamera;

        private GRC_ControllerSettings m_controllerSettings;
        public GRC_ControllerSettings ControllerSettings
        {
            get
            {
                if (m_controllerSettings == null)
                {
                    m_controllerSettings = GetControllerSettings();
                }
                return m_controllerSettings;
            }
        }


        private GUIContent m_touchUILabel;
        private GUIContent m_locationSystemLabel;
        private GUIContent m_AudioManagerLabel;
        private GUIContent m_adjustDrawDistanceLabel;
        private GUIContent m_setupPostProcessingLabel;
        private GUIContent m_controllerDropdownLabel;
        private GUIContent m_customCharacterLabel;
        private GUIContent m_customCameraLabel;
        private GUIContent m_firstPersonControllerLink;
        private GUIContent m_thirdPersonControllerLink;
        private GUIContent m_generalHelpLink;

        private GUIStyle m_linkStyle;

        private GUIContent m_panelLabel;
        public override GUIContent PanelLabel
        {
            get
            {
                if (m_panelLabel == null || m_panelLabel.text == "")
                {
                    m_panelLabel = new GUIContent("Controller Support", "Add Unity's first person or third person controller into the scene, or configure your own.");
                }
                return m_panelLabel;
            }
        }

        public override void Initialize()
        {
            m_orderNumber = 100;

            if (m_controllerDropdownLabel == null || m_controllerDropdownLabel.text == "")
            {
                m_controllerDropdownLabel = new GUIContent("Controller Type", "Select the type of controller you want to add to your scene.");
            }
            if (m_touchUILabel == null || m_touchUILabel.text == "")
            {
                m_touchUILabel = new GUIContent("Activate Touch UI", "Activate UI controls for mobile devices with a touch screen.");
            }
            if (m_locationSystemLabel == null || m_locationSystemLabel.text == "")
            {
                m_locationSystemLabel = new GUIContent("Add Location System", "Adds a script to the camera that allows you to bookmark locations in the Location Manager.");
            }
            if (m_AudioManagerLabel == null || m_AudioManagerLabel.text == "")
            {
                m_AudioManagerLabel = new GUIContent("Add Audio Manager", "Adds a script to the camera that tracks and enables audio sources spawned by Gaia.");
            }
            if (m_adjustDrawDistanceLabel == null || m_adjustDrawDistanceLabel.text == "")
            {
                m_adjustDrawDistanceLabel = new GUIContent("Adjust Draw Distance", "Increases the draw distance on the camera to make sure all relevant elements in the scene are being drawn.");
            }
            if (m_setupPostProcessingLabel == null || m_setupPostProcessingLabel.text == "")
            {
                m_setupPostProcessingLabel = new GUIContent("Setup Post Processing", "Sets up Post Processing on the camera so Post Fx are rendered on it correctly.");
            }
            if (m_customCameraLabel == null || m_customCameraLabel.text == "")
            {
                m_customCameraLabel = new GUIContent("Custom Camera", "Put your main camera from the scene into this slot. All the necessary settings and references will then be automatic applied when you create runtime.");
            }
            if (m_customCharacterLabel == null || m_customCharacterLabel.text == "")
            {
                m_customCharacterLabel = new GUIContent("Custom Character", "Put your character from the scene into this slot. If there is no dedicated character, you can usually assign the camera object in here. All the necessary settings and references will then be automatic applied when you create runtime.");
            }
            if (m_firstPersonControllerLink == null || m_firstPersonControllerLink.text == "")
            {
                m_firstPersonControllerLink = new GUIContent("Starter Assets - First Person Character Controller", "Opens the Unity Asset Store");
            }
            if (m_thirdPersonControllerLink == null || m_thirdPersonControllerLink.text == "")
            {
                m_thirdPersonControllerLink = new GUIContent("Starter Assets - Third Person Character Controller", "Opens the Unity Asset Store");
            }
            if (m_generalHelpLink == null || m_generalHelpLink.text == "")
            {
                m_generalHelpLink = new GUIContent("Controller Support Module on Canopy", "Opens the Canopy Online Help Article for the Controller Support Module");
            }


        }

        public override void DrawUI()
        {
#if UNITY_EDITOR
            if (m_linkStyle == null || (m_linkStyle.normal.textColor.r == 0 && m_linkStyle.normal.textColor.g == 0 && m_linkStyle.normal.textColor.b == 0))
            {
                m_linkStyle = new GUIStyle(GUI.skin.label);
                m_linkStyle.fontStyle = FontStyle.Normal;
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = Color.white;
                m_linkStyle.stretchWidth = false;
            }
            bool originalGUIstate = GUI.enabled;
            bool dependencyInstalled = true;

            string helpText = "The Controller Support Module allows you to add pre-made controllers to the scene, or configure your own camera / character to work correctly with all other runtime systems of Gaia.";
            DisplayHelp(helpText, m_generalHelpLink, "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/creating_runtime/runtime-module-controller-support-r161/");

            EditorGUI.BeginChangeCheck();
            {
                m_selectedControllerType = (ControllerType)EditorGUILayout.EnumPopup(m_controllerDropdownLabel, m_selectedControllerType);
                DisplayHelp("Select the controller type you want to add here.");
                switch (m_selectedControllerType)
                {
                    case ControllerType.FlyCam:
                        m_addLocationSystem = EditorGUILayout.Toggle(m_locationSystemLabel, m_addLocationSystem);
                        DisplayHelp("Adds a script to bookmark locations to the Fly cam. You can then load these locations afterwards both in edit and play mode to quickly jump to interesting spots.");
#if GAIA_2023_PRO
                        m_addAudioManager = EditorGUILayout.Toggle(m_AudioManagerLabel, m_addAudioManager);
                        DisplayHelp("Adds a script to the camera to manage audio sources spawned by Gaia. This script will disable and control audio volumes for those procedurally spawned audio sources.");
#endif

                        break;
                    case ControllerType.FirstPerson:
                        if (ControllerSettings.m_firstPersonControllerPrefab == null)
                        {
                            dependencyInstalled = false;
                            EditorGUILayout.HelpBox("The first person controller is based on the free Unity Asset 'Starter Assets - First Person Character Controller'. Please Install this asset into your project from the link below to use it with Gaia.", MessageType.Warning);
                            if (ClickableHeaderCustomStyle(m_firstPersonControllerLink, m_linkStyle))
                            {
                                Application.OpenURL("https://assetstore.unity.com/packages/essentials/starter-assets-first-person-character-controller-urp-196525");
                            }
                        }
                        else
                        {
                            m_activateTouchUI = EditorGUILayout.Toggle(m_touchUILabel, m_activateTouchUI);
                            DisplayHelp("Adds a touch User Interface designed for touch screeen devices.");
#if GAIA_2023_PRO
                            m_addAudioManager = EditorGUILayout.Toggle(m_AudioManagerLabel, m_addAudioManager);
                            DisplayHelp("Adds a script to the camera to manage audio sources spawned by Gaia. This script will disable and control audio volumes for those procedurally spawned audio sources.");
#endif

#if !UPPipeline
                            EditorGUILayout.HelpBox("Please note that the First Person Character Starter Asset is only compatible with the Universal Rendering Pipeline according to the asset store description - it can however easily be adapted to work in other pipelines. Please refer to the link below for more", MessageType.Info);
                            if (ClickableHeaderCustomStyle(m_generalHelpLink, m_linkStyle))
                            {
                                Application.OpenURL("https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/creating_runtime/runtime-module-controller-support-r161/");
                            }
#endif

                        }
                        break;
                    case ControllerType.ThirdPerson:
                        if (ControllerSettings.m_thirdPersonControllerPrefab == null)
                        {
                            dependencyInstalled = false;
                            EditorGUILayout.HelpBox("The third person controller is based on the free Unity Asset 'Starter Assets - Third Person Character Controller'. Please Install this asset into your project from the link below to use it with Gaia.", MessageType.Warning);
                            if (ClickableHeaderCustomStyle(m_thirdPersonControllerLink, m_linkStyle))
                            {
                                Application.OpenURL("https://assetstore.unity.com/packages/essentials/starter-assets-third-person-character-controller-urp-196526");
                            }
                        }
                        else
                        {
                            m_activateTouchUI = EditorGUILayout.Toggle(m_touchUILabel, m_activateTouchUI);
                            DisplayHelp("Adds a touch User Interface designed for touch screeen devices.");
#if GAIA_2023_PRO
                            m_addAudioManager = EditorGUILayout.Toggle(m_AudioManagerLabel, m_addAudioManager);
                            DisplayHelp("Adds a script to the camera to manage audio sources spawned by Gaia. This script will disable and control audio volumes for those procedurally spawned audio sources.");
#endif
#if !UPPipeline
                            EditorGUILayout.HelpBox("Please note that the Third Person Character Starter Asset is only compatible with the Universal Rendering Pipeline according to the asset store description - it can however easily be adapted to work in other pipelines. Please refer to the link below for more.", MessageType.Info);
                            if (ClickableHeaderCustomStyle(m_generalHelpLink, m_linkStyle))
                            {
                                Application.OpenURL("https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/creating_runtime/runtime-module-controller-support-r161/");
                            }
#endif
                        }
                        break;
                    case ControllerType.Custom:
                        EditorGUILayout.HelpBox("Drop your own character and camera reference from the scene into these slots. Gaia will then apply the necessary configuration to them. If you do not use a dedicated character object, please put the camera object in the character slot as well. If you dynamically create your player during runtime, please see the info under the controller type 'Runtime Generated'.", MessageType.Info);
                        m_customCharacter = (GameObject)EditorGUILayout.ObjectField(m_customCharacterLabel, m_customCharacter, typeof(GameObject), true);
                        DisplayHelp("Put your own character in this slot. If you do not have a game object that acts as a Camera, you can also put the camera Game Object into here.");
                        m_customCamera = (Camera)EditorGUILayout.ObjectField(m_customCameraLabel, m_customCamera, typeof(Camera), true);
                        DisplayHelp("Put your own camera in this slot. If you do not have a camera, please refer to the online documentation.");
#if GAIA_2023_PRO
                        m_addAudioManager = EditorGUILayout.Toggle(m_AudioManagerLabel, m_addAudioManager);
                        DisplayHelp("Adds a script to the camera to manage audio sources spawned by Gaia. This script will disable and control audio volumes for those procedurally spawned audio sources.");
#endif
#if !HDPIPELINE                         
                        m_customAdjustDrawDistance = EditorGUILayout.Toggle(m_adjustDrawDistanceLabel, m_customAdjustDrawDistance);
                        DisplayHelp("Increases the draw distance (far clipping plane) on the camera to at least 2000 meters to include the Procedural Worlds Sky into rendering.");
                        m_customSetupPostProcessing = EditorGUILayout.Toggle(m_setupPostProcessingLabel, m_customSetupPostProcessing);
                        DisplayHelp("Adds the Post-Processing Layer component (Built-In) or enables Post Processing (URP) on the camera so that Post-Processing FX are rendered correctly. This can have great impact on the look of your scene.");
#else
                        m_adjustDrawDistance = false;
#endif
                        break;
                    case ControllerType.RuntimeGenerated:
                        EditorGUILayout.HelpBox("If you dynamically spawn your player / camera from code during runtime, you can use the following Gaia API call to configure it during runtime: GaiaAPI.SetRuntimePlayerAndCamera", MessageType.Info);
                        break;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }

            //Lock buttons if the dependent package is not installed, we cannot spawn the character in this case
            if (!dependencyInstalled)
            {
                GUI.enabled = false;
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


        bool ClickableHeaderCustomStyle(GUIContent content, GUIStyle style, GUILayoutOption[] options = null)
        {
#if UNITY_EDITOR
            var position = GUILayoutUtility.GetRect(content, style, options);
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = style.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = oldColor;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, content, style);
#else
            return false;
#endif
        }

        /// <summary>
        /// Return Controller Settings or null;
        /// </summary>
        /// <returns>Gaia settings or null if not found</returns>
        public static GRC_ControllerSettings GetControllerSettings()
        {
            return GaiaUtils.GetAsset("Controller Settings.asset", typeof(GRC_ControllerSettings)) as GRC_ControllerSettings;
        }

        public override void AddToScene()
        {
            //Remove old instances first
            RemoveFromScene();

            if (m_selectedControllerType == ControllerType.RuntimeGenerated)
            {
                //Nothing to do here, the runtime generated controller type serves only as pointer towards the API call in the UI
                return;
            }


            if (m_selectedControllerType == ControllerType.Custom)
            {
                //Abort, if camera and character are not given
                if (m_customCamera == null || m_customCharacter == null)
                {
                    Debug.LogWarning("Could not set up custom controller for Gaia, there was no character / camera referenced!");
                    return;
                }
            }

            GameObject gaiaPlayerObj = null;

            //Create / get Gaia Player obj
            if (m_selectedControllerType != ControllerType.Custom)
            {
                gaiaPlayerObj = GaiaUtils.GetPlayerObject();
            }

            GameObject spawnedObject = null;
            GameObject character = null;
            Camera camera = null;

            //Create the prefab associated with the controller type, or set up the custom player
            switch (m_selectedControllerType)
            {
                case ControllerType.FlyCam:
                    spawnedObject = GameObject.Instantiate(ControllerSettings.m_flyCamPrefab);
                    //The flycam is both camera & character
                    character = spawnedObject;
                    camera = spawnedObject.GetComponent<Camera>();
                    break;
                case ControllerType.FirstPerson:

                    if (ControllerSettings.m_firstPersonControllerPrefab == null)
                    {
                        return;
                    }
                    spawnedObject = GameObject.Instantiate(ControllerSettings.m_firstPersonControllerPrefab);
                    CharacterController cc1 = spawnedObject.GetComponentInChildren<CharacterController>();
                    if (cc1 != null)
                    {
                        character = cc1.gameObject;
                    }
                    Camera cam1 = spawnedObject.GetComponentInChildren<Camera>();
                    if (cam1 != null)
                    {
                        camera = cam1;
                    }
                    break;
                case ControllerType.ThirdPerson:
                    if (ControllerSettings.m_thirdPersonControllerPrefab == null)
                    {
                        return;
                    }
                    spawnedObject = GameObject.Instantiate(ControllerSettings.m_thirdPersonControllerPrefab);
                    CharacterController cc2 = spawnedObject.GetComponentInChildren<CharacterController>();
                    if (cc2 != null)
                    {
                        character = cc2.gameObject;
                    }
                    Camera cam2 = spawnedObject.GetComponentInChildren<Camera>();
                    if (cam2 != null)
                    {
                        camera = cam2;
                    }
                    break;
                case ControllerType.Custom:
                    camera = m_customCamera;
                    character = m_customCharacter;

                    if (m_customAdjustDrawDistance)
                    {
                        camera.farClipPlane = Mathf.Max(2000f, camera.farClipPlane);
                    }

                    if (m_customSetupPostProcessing)
                    {
                        SetupPostProcessing(camera);
                    }
                    break;
            }

            if (spawnedObject != null)
            {
                spawnedObject.name = spawnedObject.name.Replace("(Clone)", "");
            }

            //Deavtive the default Unity "Main Camera" at the root of the scene, if any
            if (Camera.main != null && Camera.main.name == "Main Camera" && Camera.main.transform.parent == null && Camera.main != camera)
            {
                Camera.main.gameObject.SetActive(false);
                Debug.Log("Gaia found the default 'Main Camera' camera object in the scene and deactivated it in favor of the new controller setup.");
            }

            //Check for First / Third person characters whether the touch UI is supposed to be displayed
            if (m_selectedControllerType == ControllerType.ThirdPerson || m_selectedControllerType == ControllerType.FirstPerson)
            {
                foreach (Transform t in spawnedObject.transform)
                {
                    if (t.name.StartsWith("UI"))
                    {
                        t.gameObject.SetActive(m_activateTouchUI);
                    }
                }

#if GAIA_2023_PRO
                if (GaiaUtils.HasDynamicLoadedTerrains())
                {
                    //Force add the "wait for terrain loading script" - these characters does not have a rigidbody, but a controller script with "artificial" gravity
                    RigidbodyWaitForTerrainLoad wait = character.AddComponent<RigidbodyWaitForTerrainLoad>();
                    foreach (MonoBehaviour com in character.GetComponents<MonoBehaviour>())
                    {
                        if (com.GetType().Name.StartsWith("Third") || (com.GetType().Name.StartsWith("First")))
                        {
                            wait.m_activateDelay = 0.5f;
                            wait.m_componentsToActivate.Add(com);
                            com.enabled = false;
                        }
                    }
                }
#endif

                //Increase the render distance to 2000 - this makes sure that e.g. the Procedural Worlds Sky clouds are being rendered
#if GAIA_CINEMACHINE
                if (spawnedObject != null)
                {
                    Cinemachine.CinemachineVirtualCamera cVCam = spawnedObject.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
                    if (cVCam != null)
                    {
                        cVCam.m_Lens.FarClipPlane = 2000;
                    }
                }
#endif
            }

            if (character != null)
            {
                //We assume the custom character is in the right spot already, if it is a custom controller
                if (m_selectedControllerType != ControllerType.Custom)
                {
                    //parent to the "Gaia Player" game object
                    if (spawnedObject != null)
                    {
                        spawnedObject.transform.parent = gaiaPlayerObj.transform;
                    }

                    Vector3 location = Gaia.TerrainHelper.GetWorldCenter(true);

                    location = character.transform.position;

                    Terrain t = TerrainHelper.GetTerrain(location);
                    if (t != null)
                    {
                        float height = t.SampleHeight(location);
                        height += 2f;
                        location = new Vector3(location.x, height, location.z);
                    }

                    //get the sea level, and make sure we do not spawn under water for the case that water exists & we spawned right into a lake.
                    float seaLevel = GaiaSessionManager.GetSessionManager().GetSeaLevel();
                    location = new Vector3(location.x, Mathf.Max(location.y, seaLevel + 2f), location.z);

                    character.transform.position = location;
                }

                GaiaUtils.SetUpGaiaCharacter(character);


            }

            // Set up anti-aliasing (unless a custom camera is used)
            if (camera != null && m_selectedControllerType != ControllerType.Custom)
            {

#if HDPipeline

                HDAdditionalCameraData hdacd = camera.GetComponent<HDAdditionalCameraData>();
                if (hdacd == null)
                {
                    hdacd = camera.gameObject.AddComponent<HDAdditionalCameraData>();
                }
                if (hdacd != null)
                {
                    //Using FXAA for now since there is / was an issue with motion vectors on terrain details in HDRP, breaking TAA essentially
                    hdacd.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                }
#endif
                SetupPostProcessing(camera);

            }

            GaiaUtils.SetupGaiaCamera(camera);

            if (m_selectedControllerType == ControllerType.FlyCam && m_addLocationSystem)
            {
                LocationSystem ls = camera.GetComponent<LocationSystem>();
                if (ls == null)
                {
                    ls = camera.gameObject.AddComponent<LocationSystem>();
                }
                ls.m_camera = camera.transform;
                ls.m_player = character.transform;
            }

#if GAIA_2023_PRO
            if (m_addAudioManager)
            {
                GaiaAudioManager gma = camera.GetComponent<GaiaAudioManager>();
                if (gma == null)
                {
                    gma = camera.gameObject.AddComponent<GaiaAudioManager>();
                }
            }
#endif

        }

        private void SetupPostProcessing(Camera camera)
        {
#if UPPipeline
                   UniversalAdditionalCameraData uacd = camera.GetComponent<UniversalAdditionalCameraData>();
                    if (uacd == null)
                    {
                        uacd = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                    }
                    if (uacd != null)
                    {
                        uacd.renderPostProcessing = true;
                    }
#endif
#if !UPPipeline && !HDPIpeline

#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessLayer layer = camera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = camera.gameObject.AddComponent<PostProcessLayer>();
            }
            layer.fog.enabled = true;
            layer.fog.excludeSkybox = true;
            layer.volumeLayer = 2;
            layer.volumeTrigger = camera.transform;
#endif

#endif
        }

        public override void RemoveFromScene()
        {
            GameObject gaiaPlayerObj = GaiaUtils.GetPlayerObject();
            if (gaiaPlayerObj != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(gaiaPlayerObj);
                }
                else
                {
                    GameObject.DestroyImmediate(gaiaPlayerObj);
                }
            }
        }

    }
}