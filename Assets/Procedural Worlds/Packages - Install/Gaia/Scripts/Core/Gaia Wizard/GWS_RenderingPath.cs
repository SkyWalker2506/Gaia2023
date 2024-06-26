using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
#if UNITY_EDITOR
using UnityEditor.Rendering;
using UnityEditor;
#endif
namespace Gaia
{
    public class GWS_RenderingPath : GWSetting
    {
        private void OnEnable()
        {
            m_RPBuiltIn = true;
            m_RPHDRP = false;
            m_RPURP = false;
            m_name = "Rendering Path";
            m_infoTextOK = "This project uses Deferred Rendering";
            m_infoTextIssue = "This project uses Forward Rendering. Deffered Rendering is preferred for most projects.";
            Status = GWSettingStatus.Warning;
            m_link = "https://docs.unity3d.com/Manual/RenderingPaths.html";
            m_linkDisplayText = "Rendering paths in the Built-in Render Pipeline";
            Initialize();
        }

        public override bool PerformCheck()
        {
#if UNITY_EDITOR
            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
            if (tier1.renderingPath != RenderingPath.DeferredShading)
            {
                Status = GWSettingStatus.Warning;
                return true;
            }
#endif
            Status = GWSettingStatus.OK;
            return false;
        }

        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            if (autoFix || EditorUtility.DisplayDialog("Switch to Deferred Rendering?","Do you want to enable deferred rendering now?","Continue", "Cancel"))
            {
                var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
                var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
                var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
                tier1.renderingPath = RenderingPath.DeferredShading;
                EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1, tier1);

                tier2.renderingPath = RenderingPath.DeferredShading;
                EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2, tier2);

                tier3.renderingPath = RenderingPath.DeferredShading;
                EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3, tier3);
                LightingSettings currentLightingSettings = new LightingSettings();
                if (!Lightmapping.TryGetLightingSettings(out currentLightingSettings))
                {
                    LightingSettings lightingSettings = new LightingSettings();
                    if (!Lightmapping.TryGetLightingSettings(out lightingSettings))
                    {
                        Lightmapping.lightingSettings = new LightingSettings() { name = "Gaia Lighting Settings" };
                        string path = GaiaDirectories.GetSessionSubFolderPath(GaiaSessionManager.GetSessionManager().m_session, true);
                        AssetDatabase.CreateAsset(Lightmapping.lightingSettings, path + Path.DirectorySeparatorChar + GaiaConstants.lightingSettingsName);
                        AssetDatabase.SaveAssets();
                    }
                    Lightmapping.lightingSettings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
                    Lightmapping.lightingSettings.realtimeGI = true;
                    Lightmapping.lightingSettings.bakedGI = false;
                    Lightmapping.lightingSettings.indirectResolution = 2f;
                    Lightmapping.lightingSettings.lightmapResolution = 40f;
                    Lightmapping.lightingSettings.indirectScale = 2f;
#if !UNITY_2023_2_OR_NEWER
                    if (Lightmapping.lightingSettings.autoGenerate == true)
                    {
                        Lightmapping.lightingSettings.autoGenerate = false;
                    }
#endif
                }

                RenderSettings.defaultReflectionResolution = 256;
                if (QualitySettings.shadowDistance < 350f)
                {
                    QualitySettings.shadowDistance = 350f;
                }

                if (GameObject.Find("Directional light") != null)
                {
                    RenderSettings.sun = GameObject.Find("Directional light").GetComponent<Light>();
                }
                else if (GameObject.Find("Directional Light") != null)
                {
                    RenderSettings.sun = GameObject.Find("Directional Light").GetComponent<Light>();
                }
                PerformCheck();
                return true;
            }
#endif
                    return false;
        }
    }
}
