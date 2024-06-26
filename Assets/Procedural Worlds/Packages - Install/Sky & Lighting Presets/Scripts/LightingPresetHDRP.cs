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
using Object = UnityEngine.Object;
#endif
#if UPPipeline
using UnityEngine.Rendering.Universal;
#endif

namespace Gaia
{
    /// <summary>
    /// Holds all relevant settings and prefab references for setting up lighting in the HD render pipeline
    /// </summary>
    /// 
    [CreateAssetMenu(menuName = "Procedural Worlds/Gaia/Lighting Preset HDRP")]
    public class LightingPresetHDRP : ScriptableObject
    {
        public string m_displayName;  
        public GameObject m_directionalLightPrefab;
        public GameObject m_environmentPrefab;
        public GameObject m_globalPostProcessingPrefab;
        public GameObject m_worldDensityPrefab;
#if HDPipeline
        public VolumeProfile m_envVolumeProfile;
        public VolumeProfile m_gppVolumeProfile;
#endif

        private bool m_overwriteProfilePermission;
        private bool m_keepProfilePermission;
        private string m_lastCreatedProfile;


        public void Apply()
        {
#if UNITY_EDITOR && HDPipeline
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

            // Variable for copied profiles
            VolumeProfile copiedLightingVolume = null;
            VolumeProfile copiedPostProcessingVolume = null;

            // Check for Valid VolumeProfile assets
            if (m_gppVolumeProfile != null && m_envVolumeProfile != null)
            {
                string targetFolder = GaiaDirectories.GetHDRPLightingProfilePathForSession();
                // Create the target folder if not found
                if (!AssetDatabase.IsValidFolder(targetFolder))
                {
                    GaiaDirectories.GetHDRPLightingProfilePathForSession();
                    AssetDatabase.ImportAsset(targetFolder);
                    Debug.Log("TargetFolder = " + targetFolder);
                }

               
                    copiedLightingVolume = CopyVolumeProfileAsset(AssetDatabase.GetAssetPath(m_envVolumeProfile), targetFolder);
               
                    copiedPostProcessingVolume = CopyVolumeProfileAsset(AssetDatabase.GetAssetPath(m_gppVolumeProfile), targetFolder);
               
            }
           
            m_overwriteProfilePermission = false;
            m_keepProfilePermission = false;

            if (m_directionalLightPrefab != null)
            {
                GameObject.Instantiate(m_directionalLightPrefab, lightingObject.transform);
            }
            if (m_environmentPrefab != null)
            {
               GameObject envGO =  GameObject.Instantiate(m_environmentPrefab, lightingObject.transform);
                Volume vol = envGO.GetComponent<Volume>();
                if (copiedLightingVolume != null)
                {
                    vol.sharedProfile = copiedLightingVolume;
                }
            }
            if (m_globalPostProcessingPrefab != null)
            {
                GameObject postProcessingGO = GameObject.Instantiate(m_globalPostProcessingPrefab, lightingObject.transform);
                Volume vol = postProcessingGO.GetComponent<Volume>();
                if(copiedPostProcessingVolume != null)
                {
                    vol.sharedProfile = copiedPostProcessingVolume;
                }
            }
            if (m_worldDensityPrefab != null)
            {
                GameObject.Instantiate(m_worldDensityPrefab, lightingObject.transform);
            }
            EditorSceneManager.MarkSceneDirty(lightingObject.gameObject.scene);
#endif
        }

#if UNITY_EDITOR && HDPipeline
        private VolumeProfile CopyVolumeProfileAsset(string originalPath, string targetFolder)
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
            return (VolumeProfile)AssetDatabase.LoadAssetAtPath(m_lastCreatedProfile, typeof(VolumeProfile));
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

#if HDPipeline
            //Assuming there is a Density volume controller script in the scene, we will grab the first occurence and create a prefab from it
            HDRPDensityVolumeController dvc = GameObject.FindFirstObjectByType<HDRPDensityVolumeController>();

            if (dvc != null )
            {
                string savePath = currentPath + "/" + this.name + " World Density.prefab";
                PrefabUtility.SaveAsPrefabAsset(dvc.gameObject, savePath);
                m_worldDensityPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(savePath, typeof(GameObject));
            }
            //Assuming there is an environment and a post processing volume in the scene, looking for those to turn them into prefabs
            var allPPVolumes = GameObject.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < allPPVolumes.Length; i++)
            {
                Volume volume = allPPVolumes[i];
                if (volume.name.Equals(GaiaConstants.HDRPEnvironmentObject))
                {
                    string savePath = currentPath + "/" + this.name + " Environment.prefab";
                    PrefabUtility.SaveAsPrefabAsset(volume.gameObject, savePath);
                    m_environmentPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(savePath, typeof(GameObject));
                }

                if (volume.name.Equals(GaiaConstants.HDRPPostProcessingObject))
                {
                    string savePath = currentPath + "/" + this.name + " Post Processing.prefab";
                    PrefabUtility.SaveAsPrefabAsset(volume.gameObject, savePath);
                    m_globalPostProcessingPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(savePath, typeof(GameObject));
                }
            }
#endif
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
