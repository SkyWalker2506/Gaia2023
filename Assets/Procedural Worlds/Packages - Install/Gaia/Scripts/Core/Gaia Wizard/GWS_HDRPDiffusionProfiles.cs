using Gaia.Pipeline;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.Linq;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class GWS_HDRPDiffusionProfiles : GWSetting
    {

        private GaiaSettings m_gaiaSettings;
        private GaiaSettings GaiaSettings
        {
            get
            {
                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }
                return m_gaiaSettings;
            }
        }

        private void OnEnable()
        {
            m_canRestore = false;
            m_RPBuiltIn = false;
            m_RPHDRP = true;
            m_RPURP = false;
            m_name = "HDRP Diffusion Profiles";
            m_infoTextOK = "The Gaia HDRP Diffusion profiles are included in the HDRP Global Settings / Default Volume Profile.";
            m_infoTextIssue = "The Gaia HDRP Diffusion profiles are missing from the HDRP Global Settings / Default Volume Profile. This can lead to colors being displayed wrongly, especially with vegetation, when Gaia shaders are being used.";
            m_link = "https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Default-Settings-Window.html#volume-profiles";
            m_linkDisplayText = "Volume Profiles in the HDRP Global Settings";
            Initialize();
        }

        public override bool PerformCheck()
        {
#if HDPipeline && UNITY_EDITOR

            //For this check we need to compare Gaias list of "official" volume profiles against the current configuration
            //in the HDRP Global settings. Those are unfortunately difficult to access via scripting (see below....)

            UnityPipelineProfile upp = GaiaSettings.m_pipelineProfile; 
            RenderPipelineGlobalSettings gaiaSettings = GetGaiaRenderPipelineGlobalSettings(upp);
            RenderPipelineGlobalSettings currentSettings = GraphicsSettings.GetSettingsForRenderPipeline<UnityEngine.Rendering.HighDefinition.HDRenderPipeline>();
            if (gaiaSettings == null)
            {
                //if no official Gaia settings exist, there is nothing to check against
                Status = GWSettingStatus.Warning;
                return false;
            }
            //There are official Gaia settings for this unity version - check now if there is a different "other" configuration
            //active in the current settings that is not equal to the gaia settings

            if (currentSettings != null)
            {
                if (currentSettings == gaiaSettings)
                {
                    //the current settings equal the gaia settings, no issue here
                    Status = GWSettingStatus.OK;
                    return false;
                }
            }

            //There are other settings, is there a default volume profile in there as well?

            SerializedObject currentSettingsSO = new SerializedObject(currentSettings);
            SerializedProperty currentVolumeProfileProperty = currentSettingsSO.FindProperty("m_DefaultVolumeProfile");

            if (currentVolumeProfileProperty == null)
            {
                //There is no default volume profile property in the current settings, this is an issue
                Status = GWSettingStatus.Warning;
                return true;
            }
            //There is a default volume profile, let's load up both the current configuration and the Gaia config as Volume profiles

            SerializedObject gaiaSettingsSO = new SerializedObject(gaiaSettings);
            SerializedProperty gaiaVolumeProfileProperty = gaiaSettingsSO.FindProperty("m_DefaultVolumeProfile");

            VolumeProfile currentVP = (VolumeProfile)currentVolumeProfileProperty.objectReferenceValue;
            VolumeProfile gaiaVP = (VolumeProfile)gaiaVolumeProfileProperty.objectReferenceValue;

            if (currentVP == null)
            {
                //The property could not be converted into an actual volume profile, this is an issue
                Status = GWSettingStatus.Warning;
                return true;
            }
            //Check if there is a diffusion profile list in the default volume profile as well

            DiffusionProfileList currentDiffusionProfileList = (DiffusionProfileList)currentVP.components.Find(x => x.GetType() == typeof(DiffusionProfileList));
            DiffusionProfileList gaiaDiffusionProfileList = (DiffusionProfileList)gaiaVP.components.Find(x => x.GetType() == typeof(DiffusionProfileList));

            if (currentDiffusionProfileList == null)
            {
                //There is no diffusion profile list to begin with in the current config, this is an issue
                Status = GWSettingStatus.Warning;
                return true;
            }

            //We've established that there is a current diffusion profile list in the default volume profile
            //Now loop through all entries of the "official" gaia diffusion profiles to see if they are in the current config already
            //if not, this is our "issue"
            for (int j = 0; j < gaiaDiffusionProfileList.diffusionProfiles.value.Length; j++)
            {
                DiffusionProfileSettings gaiaDPSettings = gaiaDiffusionProfileList.diffusionProfiles.value[j];
                bool found = false;
                for (int i = 0; i < currentDiffusionProfileList.diffusionProfiles.value.Length; i++)
                {
                    DiffusionProfileSettings currentDPSettings = currentDiffusionProfileList.diffusionProfiles.value[i];
                    if (currentDPSettings == gaiaDPSettings)
                    {
                        //this gaia diffusion profile is part of the config already, all good for this entry
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    //the profile from the gaia list was not found, this is an issue
                    Status = GWSettingStatus.Warning;
                    return true;
                }
            }

            //if we went through all profiles and all the gaia profiles were there, there is no issue, we are done with the check
            Status = GWSettingStatus.OK;
            return false;
#else
            return true;
#endif
        }

#if UNITY_EDITOR && HDPipeline
        public HDRenderPipelineAsset GetHDRPAsset()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                return (HDRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;


            }
            else
            {
                Status = GWSettingStatus.Unknown;
                m_infoTextIssue = "Could not find the HD Render Pipeline Asset!";
                return null;
            }
        }
#endif


#if HDPipeline && UNITY_EDITOR
        public static RenderPipelineGlobalSettings GetGaiaRenderPipelineGlobalSettings(UnityPipelineProfile profile)
        {
            GaiaPackageVersion unityVersion = GaiaUtils.GetPackageVersion();
            UnityVersionPipelineAsset mapping = profile.m_highDefinitionPipelineProfiles.Find(x => x.m_unityVersion == unityVersion);

            string globalSettingsAssetName = "";
            if (mapping != null)
            {
                globalSettingsAssetName = mapping.m_globalSettingsAssetName;
            }
            else
            {
                //No mapping? This is most likely a new, untested unity version. Try latest entry in this case since this is most likely to work.
                globalSettingsAssetName = profile.m_highDefinitionPipelineProfiles.Last().m_globalSettingsAssetName;
            }
            return AssetDatabase.LoadAssetAtPath<RenderPipelineGlobalSettings>(GaiaUtils.GetAssetPath(globalSettingsAssetName + GaiaConstants.gaiaFileFormatAsset));

        }
#endif

    public override bool FixNow(bool autoFix = false)
        {
#if HDPipeline && UNITY_EDITOR

            //The fix code is mostly copied from the "PerformCheck" code above, but it is slightly different here and there
            //very bad tradeoff in time / value to make this more re-usable

            UnityPipelineProfile upp = GaiaSettings.m_pipelineProfile;
            RenderPipelineGlobalSettings gaiaSettings = GetGaiaRenderPipelineGlobalSettings(upp);
            RenderPipelineGlobalSettings currentSettings = GraphicsSettings.GetSettingsForRenderPipeline<UnityEngine.Rendering.HighDefinition.HDRenderPipeline>();
            if (gaiaSettings == null)
            {
                //if no official Gaia settings exist, there is nothing to check against
                Status = GWSettingStatus.Warning;
                return false;
            }
            //There are official Gaia settings for this unity version - check now if there is a different "other" configuration
            //active in the current settings that is not equal to the gaia settings

            if (currentSettings != null)
            {
                if (currentSettings == gaiaSettings)
                {
                    //the current settings equal the gaia settings, nothing to fix here
                    Status = GWSettingStatus.OK;
                    return true;
                }
            }
 
            //There are other settings, is there a default volume profile in there as well?

            SerializedObject currentSettingsSO = new SerializedObject(currentSettings);
            SerializedProperty currentVolumeProfileProperty = currentSettingsSO.FindProperty("m_DefaultVolumeProfile");

            SerializedObject gaiaSettingsSO = new SerializedObject(gaiaSettings);
            SerializedProperty gaiaVolumeProfileProperty = gaiaSettingsSO.FindProperty("m_DefaultVolumeProfile");

            if (currentVolumeProfileProperty == null || currentVolumeProfileProperty.objectReferenceValue == null)
            {
                //There is no default volume profile in the current settings - let's take the gaia one then
                currentVolumeProfileProperty.objectReferenceValue = gaiaVolumeProfileProperty.objectReferenceValue;
                currentSettingsSO.ApplyModifiedProperties();
                Status = GWSettingStatus.OK;
                return true;

            }

            //There is a default volume profile, let's load up both the current configuration and the Gaia config as Volume profiles

            VolumeProfile currentVP = (VolumeProfile)currentVolumeProfileProperty.objectReferenceValue;
            VolumeProfile gaiaVP = (VolumeProfile)gaiaVolumeProfileProperty.objectReferenceValue;

            //Check if there is a diffusion profile list in the default volume profile as well

            DiffusionProfileList currentDiffusionProfileList = (DiffusionProfileList)currentVP.components.Find(x => x.GetType() == typeof(DiffusionProfileList));
            DiffusionProfileList gaiaDiffusionProfileList = (DiffusionProfileList)gaiaVP.components.Find(x => x.GetType() == typeof(DiffusionProfileList));

            if (currentDiffusionProfileList == null)
            {
                //no diffusion profile list component on the current default volume profile? Need to add one
                currentDiffusionProfileList = currentVP.Add<DiffusionProfileList>();
                currentDiffusionProfileList.diffusionProfiles.value = new DiffusionProfileSettings[0];
            }

            //We've established that there is a current diffusion profile list in the default volume profile
            //Now loop through all entries of the "official" gaia diffusion profiles to see if they are in the current config already
            //if not, we will copy them over
            for (int j = 0; j < gaiaDiffusionProfileList.diffusionProfiles.value.Length; j++)
            {
                DiffusionProfileSettings gaiaDPSettings = gaiaDiffusionProfileList.diffusionProfiles.value[j];
                bool found = false;
                for (int i = 0; i < currentDiffusionProfileList.diffusionProfiles.value.Length; i++)
                {
                    DiffusionProfileSettings currentDPSettings = currentDiffusionProfileList.diffusionProfiles.value[i];
                    if (currentDPSettings == gaiaDPSettings)
                    {
                        //this gaia diffusion profile is part of the config already, all good for this entry
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    //the profile from the gaia list was not found, it needs to be copied into the current configuration
                    currentDiffusionProfileList.diffusionProfiles.value = GaiaUtils.AddElementToArray<DiffusionProfileSettings>(currentDiffusionProfileList.diffusionProfiles.value, gaiaDPSettings);
                }
            }
            //if we went through all profiles and added them if needed we are done with the fix
            currentSettingsSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(currentVP);
            AssetDatabase.SaveAssetIfDirty(currentVP);
            Status = GWSettingStatus.OK;
            PerformCheck();
            return true;
            
#else
            return false;
#endif
        }

        public override string GetOriginalValueString()
        {
            return "Gaia Diffusion Profiles were not present";
        }

    }
}
