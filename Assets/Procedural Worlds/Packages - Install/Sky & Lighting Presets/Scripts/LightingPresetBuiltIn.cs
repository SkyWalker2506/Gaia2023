using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace Gaia
{
    /// <summary>
    /// Holds all relevant settings and prefab references for setting up lighting in the built-in render pipeline
    /// </summary>
    /// 
    [CreateAssetMenu(menuName = "Procedural Worlds/Gaia/Lighting Preset BuiltIn")]
    public class LightingPresetBuiltIn : ScriptableObject
    {
        public string m_displayName;
        public GameObject m_directionalLightPrefab;
        public GameObject m_globalPostProcessingPrefab;
#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile m_globalPostProcessingProfile;
        private bool m_overwriteProfilePermission;
        private bool m_keepProfilePermission;
#endif
        public EnvironmentBuiltInURP m_environmentBuiltIn;


        private string m_lastCreatedProfile;

        public void Apply(bool addPPLayerToCam = true)
        {
#if UNITY_EDITOR
            GameObject lightingObject = GaiaUtils.GetLightingObject(false);

            //Destroy old lighting, if any
            RemoveFromScene();
            lightingObject = GaiaUtils.GetLightingObject(true);

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
#if UNITY_POST_PROCESSING_STACK_V2
            // Variable for copied profiles
            PostProcessProfile copiedPostProcessingProfile = null;

            // Check for Valid VolumeProfile assets
            if (m_globalPostProcessingProfile != null)
            {
                string targetFolder = GaiaDirectories.GetSRPLightingProfilePathForSession();
                // Create the target folder if not found
                if (!AssetDatabase.IsValidFolder(targetFolder))
                {
                    GaiaDirectories.GetSRPLightingProfilePathForSession();
                    AssetDatabase.ImportAsset(targetFolder);
                }


                copiedPostProcessingProfile = CopyVolumeProfileAsset(AssetDatabase.GetAssetPath(m_globalPostProcessingProfile), targetFolder);

            }
            m_overwriteProfilePermission = false;
            m_keepProfilePermission = false;
#endif



            if (m_directionalLightPrefab != null)
            {
                GameObject newGO = GameObject.Instantiate(m_directionalLightPrefab, lightingObject.transform);
                newGO.name = newGO.name.Replace("(Clone)", "");
            }
            if (m_globalPostProcessingPrefab != null)
            {
#if UNITY_POST_PROCESSING_STACK_V2
                GameObject newGO = GameObject.Instantiate(m_globalPostProcessingPrefab, lightingObject.transform);
                newGO.name = newGO.name.Replace("(Clone)", "");
                PostProcessVolume vol = newGO.GetComponent<PostProcessVolume>();
                if (copiedPostProcessingProfile != null)
                {
                    vol.profile = copiedPostProcessingProfile;
                }
#endif
            }
            if (m_environmentBuiltIn != null)
            {
                m_environmentBuiltIn.Apply();
            }
            if (addPPLayerToCam)
            {
#if UNITY_POST_PROCESSING_STACK_V2

                Camera cam = Camera.main;

                GameObject playerObj = GaiaUtils.GetPlayerObject(false);
                if (playerObj != null)
                {
                    Camera playerCam = playerObj.GetComponentInChildren<Camera>();
                    if (playerCam != null)
                    {
                        cam = playerCam;
                    }
                }

                if (cam != null)
                {
                    PostProcessLayer layer = cam.GetComponent<PostProcessLayer>();
                    if (layer == null)
                    {
                        layer = cam.gameObject.AddComponent<PostProcessLayer>();
                    }

                    layer.fog.enabled = true;
                    layer.fog.excludeSkybox = true;
                    layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                    layer.volumeLayer = 2;
                    layer.volumeTrigger = cam.transform;
                }
                else
                {
                    Debug.LogWarning("Could not find a camera: Post-Processing was NOT added to the camera. You will need to add a post-processing layer component to your camera or re-apply the lighting preset once there is a camera in the scene.");
                }

#endif
            }
            EditorSceneManager.MarkSceneDirty(lightingObject.gameObject.scene);
#endif
        }

#if UNITY_EDITOR && UNITY_POST_PROCESSING_STACK_V2

        private PostProcessProfile CopyVolumeProfileAsset(string originalPath, string targetFolder)
        {

            // Check if the original asset exists
            if (!AssetDatabase.IsValidFolder(originalPath) && !AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(originalPath))
            {
                Debug.LogError($"Error: Original asset at path {originalPath} not found. Unable to copy.");
                return null;
            }

            // Get the name of the original asset
            string originalFileName = System.IO.Path.GetFileName(originalPath);

            // Create the full destination path including the file name
            string destinationPath = targetFolder + "/" + originalFileName;

            // Check if the file exists in the target folder, if yes then display a dialog for overwriting or creating a new copy
            if (!System.IO.File.Exists(destinationPath))
            {
                // Create a copy of the object
                AssetDatabase.CopyAsset(originalPath, destinationPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(destinationPath);
                m_lastCreatedProfile = destinationPath; // Update the last created profile
            }
            else
            {
                if (!m_overwriteProfilePermission && !m_keepProfilePermission)
                {
                    if (EditorUtility.DisplayDialog("Lighting settings already exist", "Do you want to overwrite the current lighting settings or create a new one", "Overwrite", "Create"))
                    {
                        m_overwriteProfilePermission = true;

                    }
                    else
                    {
                        m_keepProfilePermission = true;
                    }
                }

                if (m_overwriteProfilePermission)
                {
                    // Overwrite the last created profile
                    if (!string.IsNullOrEmpty(m_lastCreatedProfile))
                    {
                        AssetDatabase.CopyAsset(originalPath, m_lastCreatedProfile);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.ImportAsset(m_lastCreatedProfile);
                    }
                }
                if (m_keepProfilePermission)
                {
                    // Make a new profile profile
                    destinationPath = AssetDatabase.GenerateUniqueAssetPath(targetFolder + "/" + originalFileName);
                    AssetDatabase.CopyAsset(originalPath, destinationPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(destinationPath);
                    m_lastCreatedProfile = destinationPath; // Update the last created profile
                }

            }
            return (PostProcessProfile)AssetDatabase.LoadAssetAtPath(m_lastCreatedProfile, typeof(PostProcessProfile));
        }
#endif
        public void RemoveFromScene()
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
        }

        public void IngestFromScene()
        {
#if UNITY_EDITOR
            string currentPath = AssetDatabase.GetAssetPath(this);
            currentPath = currentPath.Substring(0, currentPath.LastIndexOf("/"));
            //Assuming there is a directional light in the scene, we will grab the first occurence and create a prefab from it
            var allLights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < allLights.Length; i++)
            {
                Light light = allLights[i];
                if (light.type == LightType.Directional)
                {

                    string savePath = currentPath + "/" + this.name + " DirectionalLight.prefab";
                    PrefabUtility.SaveAsPrefabAsset(light.gameObject, savePath);
                    m_directionalLightPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(savePath, typeof(GameObject));
                    break;
                }
            }

#if UNITY_POST_PROCESSING_STACK_V2
            //Assuming there is a global post processing volume in the scene, we will grab the first occurence and create a prefab from it
            var allPPVolumes = GameObject.FindObjectsByType<PostProcessVolume>(FindObjectsSortMode.None);
            for (int i = 0; i < allPPVolumes.Length; i++)
            {
                PostProcessVolume ppv = allPPVolumes[i];
                if (ppv.isGlobal)
                {
#if UNITY_EDITOR
                    string savePath = currentPath + "/" + this.name + " GlobalPostProcessing.prefab";
                    PrefabUtility.SaveAsPrefabAsset(ppv.gameObject, savePath);
                    m_globalPostProcessingPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(savePath, typeof(GameObject));
#endif
                    break;
                }
            }
#endif

            if (m_environmentBuiltIn == null)
            {
                string savePath = currentPath + "/" + this.name + " Environment.asset";
                EnvironmentBuiltInURP eb = ScriptableObject.CreateInstance<EnvironmentBuiltInURP>();
                AssetDatabase.CreateAsset(eb, currentPath + "/" + this.name + " Environment.asset");
                m_environmentBuiltIn = (EnvironmentBuiltInURP)AssetDatabase.LoadAssetAtPath(savePath, typeof(EnvironmentBuiltInURP));
            }

            m_environmentBuiltIn.IngestFromScene();
#endif
        }
    }
}
