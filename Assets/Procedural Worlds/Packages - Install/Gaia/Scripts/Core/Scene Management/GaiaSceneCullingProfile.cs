using UnityEngine;
using static Gaia.GaiaConstants;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class GaiaSceneCullingProfile : ScriptableObject
    {
        public enum ShadowCullingType
        {
            Small,
            Medium,
            Large
        }

        [Header("Global Settings")] 
        //public bool m_enableLayerCulling = true;
        public bool m_applyToEditorCamera = false;
        public bool m_realtimeUpdate = false;
        public float[] m_layerDistances = new float[32];
        public string[] m_layerNames = new string[32];
        public float[] m_shadowLayerDistances = new float[32];
        public float m_additionalCullingDistance = 0f;

        public void InitCulling(GaiaSettings gaiaSettings)
        {
            //Set Object & Shadow layer distances based on the current environment in the Gaia Settings
            //Default to desktop
            EnvironmentTarget target = EnvironmentTarget.Desktop;
            if (gaiaSettings != null)
            {
                target = gaiaSettings.m_currentEnvironment;
            }

            float[] layerDistances = new float[32];
            float[] shadowDistances = new float[32];
            for (int i = 0; i < layerDistances.Length; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                switch (layerName)
                {
                    case "Default":
                    case "Water":
                    case "PW_VFX":
                        layerDistances[i] = 0f;
                        shadowDistances[i] = 0f;
                        break;
                    case "PW_Object_Small":
                        switch (target)
                        {
                            case EnvironmentTarget.UltraLight:
                                layerDistances[i] = 10f;
                                shadowDistances[i] = 5f;
                                break;
                            case EnvironmentTarget.MobileAndVR:
                                layerDistances[i] = 25f;
                                shadowDistances[i] = 10f;
                                break;
                            case EnvironmentTarget.Desktop:
                                layerDistances[i] = 100f;
                                shadowDistances[i] = 100f;
                                break;
                            case EnvironmentTarget.PowerfulDesktop:
                                layerDistances[i] = 200f;
                                shadowDistances[i] = 150f;
                                break;
                            case EnvironmentTarget.Custom:
                                layerDistances[i] = 200f;
                                shadowDistances[i] = 150f;
                                break;
                        }
                        break;
                    case "PW_Object_Medium":
                        switch (target)
                        {
                            case EnvironmentTarget.UltraLight:
                                layerDistances[i] = 20f;
                                shadowDistances[i] = 10f;
                                break;
                            case EnvironmentTarget.MobileAndVR:
                                layerDistances[i] = 50f;
                                shadowDistances[i] = 20f;
                                break;
                            case EnvironmentTarget.Desktop:
                                layerDistances[i] = 500f;
                                shadowDistances[i] = 500f;
                                break;
                            case EnvironmentTarget.PowerfulDesktop:
                                layerDistances[i] = 800f;
                                shadowDistances[i] = 800f;
                                break;
                            case EnvironmentTarget.Custom:
                                layerDistances[i] = 1000f;
                                shadowDistances[i] = 900f;
                                break;
                        }
                        break;
                    case "PW_Object_Large":
                        switch (target)
                        {
                            case EnvironmentTarget.UltraLight:
                                layerDistances[i] = 75f;
                                shadowDistances[i] = 30f;
                                break;
                            case EnvironmentTarget.MobileAndVR:
                                layerDistances[i] = 100f;
                                shadowDistances[i] = 100f;
                                break;
                            case EnvironmentTarget.Desktop:
                                layerDistances[i] = 1000f;
                                shadowDistances[i] = 1000f;
                                break;
                            case EnvironmentTarget.PowerfulDesktop:
                                layerDistances[i] = 2000f;
                                shadowDistances[i] = 2000f;
                                break;
                            case EnvironmentTarget.Custom:
                                layerDistances[i] = 4000f;
                                shadowDistances[i] = 3000f;
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            m_layerDistances = layerDistances;
            m_shadowLayerDistances = shadowDistances;
        }


        public void UpdateCulling(GaiaSettings gaiaSettings)
        {
            //if (!GaiaUtils.CheckIfSceneProfileExists())
            //{
            //    return;
            //}
            //if (GaiaGlobal.Instance.m_mainCamera == null)
            //{
            //    GaiaGlobal.Instance.m_mainCamera = GaiaUtils.GetCamera();
            //}

            float farClipPlane = 2000f;
            if (Camera.main != null)
            {
                farClipPlane = Camera.main.farClipPlane;
            }

            //if (GaiaGlobal.Instance.SceneProfile.m_sunLight == null)
            //{
            //    GaiaGlobal.Instance.SceneProfile.m_sunLight = GaiaUtils.GetMainDirectionalLight(false);
            //}

            //if (GaiaGlobal.Instance.SceneProfile.m_moonLight == null)
            //{
            //    GaiaGlobal.Instance.SceneProfile.m_moonLight = GaiaUtils.GetMainMoonLight(false);
            //}

            Terrain terrain = TerrainHelper.GetActiveTerrain();

            //Objects
            m_layerDistances = new float[32];
            for (int i = 0; i < m_layerDistances.Length; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                switch (layerName)
                {
                    case "Default":
                    case "Water":
                    case "PW_VFX":
                        m_layerDistances[i] = 0f;
                        break;
                    case "PW_Object_Small":
                        if (terrain != null)
                        {
                            m_layerDistances[i] = GaiaUtils.CalculateCameraCullingLayerValue(terrain, gaiaSettings.m_currentEnvironment, 5f);
                        }
                        else
                        {
                            m_layerDistances[i] = 200;
                        }
                        break;
                    case "PW_Object_Medium":
                        if (terrain != null)
                        {
                            m_layerDistances[i] = GaiaUtils.CalculateCameraCullingLayerValue(terrain, gaiaSettings.m_currentEnvironment, 3f);
                        }
                        else
                        {
                            m_layerDistances[i] = 500;
                        }
                        break;
                    case "PW_Object_Large":
                        if (terrain != null)
                        {
                            m_layerDistances[i] = GaiaUtils.CalculateCameraCullingLayerValue(terrain, gaiaSettings.m_currentEnvironment);
                        }
                        else
                        {
                            m_layerDistances[i] = 1000;
                        }
                        break;
                    default:
                        m_layerDistances[i] = 0f;
                        break;
                }
            }
        }
        public void UpdateShadow()
        {
            //Shadows
            m_shadowLayerDistances = new float[32];
            for (int i = 0; i < m_shadowLayerDistances.Length; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                switch (layerName)
                {
                    case "Default":
                    case "Water":
                    case "PW_VFX":
                        m_shadowLayerDistances[i] = 0f;
                        break;
                    case "PW_Object_Small":
                        m_shadowLayerDistances[i] = 20f;
                        break;
                    case "PW_Object_Medium":
                        m_shadowLayerDistances[i] = 100f;
                        break;
                    case "PW_Object_Large":
                        m_shadowLayerDistances[i] = 250f;
                        break;
                    default:
                        m_shadowLayerDistances[i] = 0f;
                        break;
                }
            }
        }

        /// <summary>
        /// Create Gaia Culling System Profile asset
        /// </summary>
#if UNITY_EDITOR
        public static GaiaSceneCullingProfile CreateCullingProfile()
        {
            GaiaSceneCullingProfile asset = ScriptableObject.CreateInstance<GaiaSceneCullingProfile>();
            GaiaSettings gaiaSettings = GaiaUtils.GetGaiaSettings();
            asset.UpdateCulling(gaiaSettings);
            asset.UpdateShadow();
            AssetDatabase.CreateAsset(asset, "Assets/Gaia Scene Culling Profile.asset");
            AssetDatabase.SaveAssets();
            return asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/Gaia/Gaia Scene Culling Profile")]
        public static void CreateCullingProfileMenu()
        {
            GaiaSceneCullingProfile asset = ScriptableObject.CreateInstance<GaiaSceneCullingProfile>();
            GaiaSettings gaiaSettings = GaiaUtils.GetGaiaSettings();
            asset.UpdateCulling(gaiaSettings);
            asset.UpdateShadow();
            AssetDatabase.CreateAsset(asset, "Assets/Gaia Scene Culling Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
#endif
    }
}