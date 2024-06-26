using Gaia.Internal;
using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace Gaia
{
    [CustomEditor(typeof(PWSkyStandalone))]
    public class PWSkyStandaloneEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
#if GAIA_2023_PRO
        private ProceduralWorldsGlobalWeather m_globalWeather;
          private ProceduralWorldsGlobalWeather GlobalWeather
        {
            get 
            {
                if (m_globalWeather == null)
                {
                    m_globalWeather = ProceduralWorldsGlobalWeather.Instance;
                }
                return m_globalWeather;
            }
        }


#endif
        private GaiaConstants.EnvironmentRenderer m_renderPipeline;
        private PWSkyStandalone m_pWSkyStandalone;

              public void OnEnable()
        {

            m_renderPipeline = GaiaUtils.GetActivePipeline();
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
        }

        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }


        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!
            m_pWSkyStandalone = (PWSkyStandalone)target;
            m_editorUtils.Panel("LightingProfileSettings", LightingProfileSettings, true); 

            


        }

        private void LightingProfileSettings(bool helpEnabled)
        {
#if GAIA_2023_PRO
            if (GlobalWeather != null)
            {

                float currentTimeAndDayValue =  PWSkyStandalone.GetTimeOfDayMainValue();

                EditorGUI.BeginChangeCheck();
                m_editorUtils.LabelField("TimeOfDay", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_pWSkyStandalone.m_gaiaTimeOfDay.m_todHour = m_editorUtils.IntSlider("TODHour", m_pWSkyStandalone.m_gaiaTimeOfDay.m_todHour, 0, 23, helpEnabled);
                if (m_pWSkyStandalone.m_gaiaTimeOfDay.m_todHour > 23)
                {
                    m_pWSkyStandalone.m_gaiaTimeOfDay.m_todHour = 0;
                }
                m_pWSkyStandalone.m_gaiaTimeOfDay.m_todMinutes = m_editorUtils.Slider("TODMinutes", m_pWSkyStandalone.m_gaiaTimeOfDay.m_todMinutes, 0f, 59f, helpEnabled);
                if (m_pWSkyStandalone.m_gaiaTimeOfDay.m_todMinutes > 60f)
                {
                    m_pWSkyStandalone.m_gaiaTimeOfDay.m_todMinutes = 0f;
                }
                m_pWSkyStandalone.m_gaiaTimeOfDay.m_todEnabled = m_editorUtils.Toggle("TODEnable", m_pWSkyStandalone.m_gaiaTimeOfDay.m_todEnabled, helpEnabled);
                if (m_pWSkyStandalone.m_gaiaTimeOfDay.m_todEnabled)
                {
                    EditorGUI.indentLevel++;
                    m_pWSkyStandalone.m_gaiaTimeOfDay.m_todDayTimeScale = m_editorUtils.Slider("TODScale", m_pWSkyStandalone.m_gaiaTimeOfDay.m_todDayTimeScale, 0f, 500f, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                if (EditorGUI.EndChangeCheck())
                {
                    m_pWSkyStandalone.UpdateGaiaTimeOfDay(false);
                    m_pWSkyStandalone.UpdateGaiaWeather();
                }

                //if (!m_pWSkyStandalone.m_profileValues.m_userCustomProfile && !m_lightingEditSettings)
                //{
                //    GUI.enabled = false;
                //}

                //if (m_enablePostProcessing)
                //{
                    if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.Universal)
                    {
                        m_editorUtils.Heading("PostProcessingSettings");
                        EditorGUI.indentLevel++;
#if UPPipeline
                        m_pWSkyStandalone.m_profileValues.PostProcessProfileURP = (VolumeProfile)m_editorUtils.ObjectField("PostProcessingProfile", m_pWSkyStandalone.m_profileValues.PostProcessProfileURP, typeof(VolumeProfile), false, helpEnabled);
#endif
                        EditorGUI.indentLevel--;
                    }
                    else if (m_renderPipeline == GaiaConstants.EnvironmentRenderer.BuiltIn)
                    {
                        m_editorUtils.Heading("PostProcessingSettings");
                        EditorGUI.indentLevel++;
#if UNITY_POST_PROCESSING_STACK_V2
                        m_pWSkyStandalone.m_profileValues.PostProcessProfileBuiltIn = (PostProcessProfile)m_editorUtils.ObjectField("PostProcessingProfile", m_pWSkyStandalone.m_profileValues.PostProcessProfileBuiltIn, typeof(PostProcessProfile), false, helpEnabled);
                        m_pWSkyStandalone.m_profileValues.m_directToCamera = m_editorUtils.Toggle("DirectToCamera", m_pWSkyStandalone.m_profileValues.m_directToCamera, helpEnabled);
                        if (m_pWSkyStandalone.m_profileValues.m_profileType == GaiaConstants.GaiaLightingProfileType.ProceduralWorldsSky)
                        {
                            if (GlobalWeather != null)
                            {
                                GlobalWeather.TODPostProcessExposure = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("PostProcessingExpsoure"), m_editorUtils.GetTooltip("PostProcessingExpsoure")), GlobalWeather.TODPostProcessExposure);
                                m_editorUtils.InlineHelp("PostProcessingExpsoure", helpEnabled);
                                GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue);
                            }
                        }
                        else
                        {
                            m_pWSkyStandalone.m_profileValues.m_postProcessExposure = m_editorUtils.FloatField("PostProcessingExpsoure", m_pWSkyStandalone.m_profileValues.m_postProcessExposure, helpEnabled);
                        }
#else
                                    EditorGUILayout.HelpBox("Post Processing is not installed. Install it from the package manager to use the post processing setup features.", MessageType.Info);
#endif
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
#if HDPipeline
                                    m_editorUtils.Heading("VolumeSettings");
                                    EditorGUI.indentLevel++;
                                    m_pWSkyStandalone.m_profileValues.PostProcessProfileHDRP = (VolumeProfile)m_editorUtils.ObjectField("PostProcessingProfile", m_pWSkyStandalone.m_profileValues.PostProcessProfileHDRP, typeof(VolumeProfile), false, helpEnabled);
                                    m_pWSkyStandalone.m_profileValues.EnvironmentProfileHDRP = (VolumeProfile)m_editorUtils.ObjectField("EnvironmentProfile", m_pWSkyStandalone.m_profileValues.EnvironmentProfileHDRP, typeof(VolumeProfile), false, helpEnabled);
                                    EditorGUI.indentLevel--;
#endif
                    }

                    GUILayout.Space(10f);
                //}

                EditorGUI.BeginChangeCheck();
                m_editorUtils.Heading("SkyboxSettings");
                EditorGUI.indentLevel++;
                m_pWSkyStandalone.m_profileValues.m_pwSkySunRotation = m_editorUtils.Slider("SunRotation", m_pWSkyStandalone.m_profileValues.m_pwSkySunRotation, 0f, 360f, helpEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    GaiaAPI.SetTimeOfDaySunRotation(m_pWSkyStandalone.m_profileValues.m_pwSkySunRotation);
                }

                if (GlobalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    if (GlobalWeather.TODHDRPGroundTint != null)
                    {
                        GlobalWeather.TODHDRPGroundTint = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODHDRPGroundTint"), m_editorUtils.GetTooltip("TODHDRPGroundTint")), GlobalWeather.TODHDRPGroundTint);
                        m_editorUtils.InlineHelp("TODHDRPGroundTint", helpEnabled);
                    }

                    GlobalWeather.TODSkyboxExposure = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSkyboxExposure"), m_editorUtils.GetTooltip("TODSkyboxExposure")), GlobalWeather.TODSkyboxExposure);
                    m_editorUtils.InlineHelp("TODSkyboxExposure", helpEnabled);
                }
                else
                {
                    GlobalWeather.TODSkyboxExposure = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSkyboxExposure"), m_editorUtils.GetTooltip("TODSkyboxExposure")), GlobalWeather.TODSkyboxExposure);
                    m_editorUtils.InlineHelp("TODSkyboxExposure", helpEnabled);

                    GlobalWeather.TODAtmosphereThickness = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODAtmosphereThickness"), m_editorUtils.GetTooltip("TODAtmosphereThickness")), GlobalWeather.TODAtmosphereThickness);
                    m_editorUtils.InlineHelp("TODAtmosphereThickness", helpEnabled);

                    GlobalWeather.TODSunSize = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunSize"), m_editorUtils.GetTooltip("TODSunSize")), GlobalWeather.TODSunSize);
                    m_editorUtils.InlineHelp("TODSunSize", helpEnabled);

                    GlobalWeather.TODSunSizeConvergence = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunSizeConvergence"), m_editorUtils.GetTooltip("TODSunSizeConvergence")), GlobalWeather.TODSunSizeConvergence);
                    m_editorUtils.InlineHelp("TODSunSizeConvergence", helpEnabled);

                    GlobalWeather.TODSkyboxTint = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODSkyboxTint"), m_editorUtils.GetTooltip("TODSkyboxTint")), GlobalWeather.TODSkyboxTint);
                    m_editorUtils.InlineHelp("TODSkyboxTint", helpEnabled);

                    GlobalWeather.TODSkyboxFogHeight = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSkyboxFogHeight"), m_editorUtils.GetTooltip("TODSkyboxFogHeight")), GlobalWeather.TODSkyboxFogHeight);
                    m_editorUtils.InlineHelp("TODSkyboxFogHeight", helpEnabled);

                    GlobalWeather.TODSkyboxFogGradient = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSkyboxFogGradient"), m_editorUtils.GetTooltip("TODSkyboxFogGradient")), GlobalWeather.TODSkyboxFogGradient);
                    m_editorUtils.InlineHelp("TODSkyboxFogGradient", helpEnabled);
                }
                EditorGUI.indentLevel--;
                if (GlobalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 1.2f);
                }
                else
                {
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 6.7f);
                }
                EditorGUILayout.Space();

                m_editorUtils.Heading("SunSettings");
                EditorGUI.indentLevel++;
                GlobalWeather.TODSunColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODSunColor"), m_editorUtils.GetTooltip("TODSunColor")), GlobalWeather.TODSunColor);
                m_editorUtils.InlineHelp("TODSunColor", helpEnabled);

                GlobalWeather.TODSunIntensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunIntensity"), m_editorUtils.GetTooltip("TODSunIntensity")), GlobalWeather.TODSunIntensity);
                m_editorUtils.InlineHelp("TODSunIntensity", helpEnabled);

                if (GlobalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 1.2f);
                }
                else
                {
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 1.2f);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                m_editorUtils.Heading("ShadowSettings");
                EditorGUI.indentLevel++;
                if (GlobalWeather.m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GlobalWeather.TODSunShadowStrength = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODSunShadowStregth"), m_editorUtils.GetTooltip("TODSunShadowStregth")), GlobalWeather.TODSunShadowStrength);
                    m_editorUtils.InlineHelp("TODSunShadowStregth", helpEnabled);
                }
                GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue);
                m_pWSkyStandalone.m_profileValues.m_shadowCastingMode = (LightShadows)m_editorUtils.EnumPopup("SunShadowCastingMode", m_pWSkyStandalone.m_profileValues.m_shadowCastingMode, helpEnabled);
                m_pWSkyStandalone.m_profileValues.m_sunShadowResolution = (LightShadowResolution)m_editorUtils.EnumPopup("SunShadowResolution", m_pWSkyStandalone.m_profileValues.m_sunShadowResolution, helpEnabled);
                m_pWSkyStandalone.m_profileValues.m_shadowDistance = m_editorUtils.Slider("ShadowDistance", m_pWSkyStandalone.m_profileValues.m_shadowDistance, 0f, 10000f, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                if (GlobalWeather.m_renderPipeline != GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    m_editorUtils.Heading("AmbientSettings");
                    EditorGUI.indentLevel++;
                    switch (RenderSettings.ambientMode)
                    {
                        case AmbientMode.Skybox:
                            GlobalWeather.TODAmbientIntensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientIntensity"), m_editorUtils.GetTooltip("TODAmbientIntensity")), GlobalWeather.TODAmbientIntensity);
                            m_editorUtils.InlineHelp("TODAmbientIntensity", helpEnabled);
                            break;
                        case AmbientMode.Trilight:
                            GlobalWeather.TODAmbientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientSkyColor"), m_editorUtils.GetTooltip("TODAmbientSkyColor")), GlobalWeather.TODAmbientSkyColor, true);
                            m_editorUtils.InlineHelp("TODAmbientSkyColor", helpEnabled);

                            GlobalWeather.TODAmbientEquatorColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientEquatorColor"), m_editorUtils.GetTooltip("TODAmbientEquatorColor")), GlobalWeather.TODAmbientEquatorColor, true);
                            m_editorUtils.InlineHelp("TODAmbientEquatorColor", helpEnabled);

                            GlobalWeather.TODAmbientGroundColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientGroundColor"), m_editorUtils.GetTooltip("TODAmbientGroundColor")), GlobalWeather.TODAmbientGroundColor, true);
                            m_editorUtils.InlineHelp("TODAmbientGroundColor", helpEnabled);
                            break;
                        default:
                            GlobalWeather.TODAmbientSkyColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODAmbientSkyColor"), m_editorUtils.GetTooltip("TODAmbientSkyColor")), GlobalWeather.TODAmbientSkyColor, true);
                            m_editorUtils.InlineHelp("TODAmbientSkyColor", helpEnabled);
                            break;
                    }
                    EditorGUI.indentLevel--;
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 2.2f);
                    EditorGUILayout.Space();
                }

                m_editorUtils.Heading("FogSettings");
                EditorGUI.indentLevel++;
                if (GlobalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GlobalWeather.TODFogColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODFogColor"), m_editorUtils.GetTooltip("TODFogColor")), GlobalWeather.TODFogColor);
                    m_editorUtils.InlineHelp("TODFogColor", helpEnabled);
                    GlobalWeather.TODHDRPFogAlbedo = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODHDRPFogAlbedo"), m_editorUtils.GetTooltip("TODHDRPFogAlbedo")), GlobalWeather.TODHDRPFogAlbedo);
                    m_editorUtils.InlineHelp("TODHDRPFogAlbedo", helpEnabled);
                    GlobalWeather.TODFogEndDistance = m_editorUtils.CurveField("TODFogEndDistance", GlobalWeather.TODFogEndDistance, helpEnabled);
                    GlobalWeather.TODHDRPFogAnisotropy = m_editorUtils.CurveField("TODHDRPFogAnisotropy", GlobalWeather.TODHDRPFogAnisotropy, helpEnabled);
                    GlobalWeather.TODHDRPFogBaseHeight = m_editorUtils.CurveField("TODHDRPFogBaseHeight", GlobalWeather.TODHDRPFogBaseHeight, helpEnabled);
                    GlobalWeather.TODHDRPFogDepthExtent = m_editorUtils.CurveField("TODHDRPFogDepthExtent", GlobalWeather.TODHDRPFogDepthExtent, helpEnabled);
                    GlobalWeather.TODHDRPFogLightProbeDimmer = m_editorUtils.CurveField("TODHDRPFogLightProbeDimmer", GlobalWeather.TODHDRPFogLightProbeDimmer, helpEnabled);
                }
                else
                {
                    FogMode fogMode = m_pWSkyStandalone.m_profileValues.m_fogMode;
                    fogMode = (FogMode)m_editorUtils.EnumPopup("FogMode", fogMode, helpEnabled);
                    if (fogMode != m_pWSkyStandalone.m_profileValues.m_fogMode)
                    {
                        m_pWSkyStandalone.m_profileValues.m_fogMode = fogMode;
                        GlobalWeather.UpdateFogMode(m_pWSkyStandalone.m_profileValues.m_fogMode);
                    }
                    GlobalWeather.TODFogColor = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue("TODFogColor"), m_editorUtils.GetTooltip("TODFogColor")), GlobalWeather.TODFogColor);
                    m_editorUtils.InlineHelp("TODFogColor", helpEnabled);

                    if (RenderSettings.fogMode == FogMode.Linear)
                    {
                        GlobalWeather.TODFogStartDistance = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODFogStartDistance"), m_editorUtils.GetTooltip("TODFogStartDistance")), GlobalWeather.TODFogStartDistance);
                        m_editorUtils.InlineHelp("TODFogStartDistance", helpEnabled);

                        GlobalWeather.TODFogEndDistance = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODFogEndDistance"), m_editorUtils.GetTooltip("TODFogEndDistance")), GlobalWeather.TODFogEndDistance);
                        m_editorUtils.InlineHelp("TODFogEndDistance", helpEnabled);
                    }
                    else
                    {
                        GlobalWeather.TODFogDensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODFogDensity"), m_editorUtils.GetTooltip("TODFogDensity")), GlobalWeather.TODFogDensity);
                        m_editorUtils.InlineHelp("TODFogDensity", helpEnabled);
                    }
                }
                EditorGUI.indentLevel--;
                if (GlobalWeather.m_renderPipeline == GaiaConstants.EnvironmentRenderer.HighDefinition)
                {
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 6.8f);
                }
                else
                {
                    if (RenderSettings.fogMode == FogMode.Linear)
                    {
                        GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 2.2f);
                    }
                    else
                    {
                        GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 1.2f);
                    }
                }
                EditorGUILayout.Space();

                m_editorUtils.Heading("CloudSettings");
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();

                GlobalWeather.EnableClouds = m_editorUtils.Toggle("EnableClouds", GlobalWeather.EnableClouds, helpEnabled);
                if (GlobalWeather.EnableClouds)
                {
                    EditorGUI.indentLevel++;
                    m_pWSkyStandalone.m_profileValues.m_pwSkyAtmosphereData.CloudGPUInstanced = m_editorUtils.Toggle("CloudGPUInstancing", m_pWSkyStandalone.m_profileValues.m_pwSkyAtmosphereData.CloudGPUInstanced, helpEnabled);
                    m_pWSkyStandalone.m_profileValues.m_pwSkyAtmosphereData.CloudRenderQueue = (GaiaConstants.CloudRenderQueue)m_editorUtils.EnumPopup("CloudRenderQueue", m_pWSkyStandalone.m_profileValues.m_pwSkyAtmosphereData.CloudRenderQueue, helpEnabled);
                    GlobalWeather.CloudRotationSpeedLow = m_editorUtils.Slider("CloudRotationSpeedLow", GlobalWeather.CloudRotationSpeedLow, -5f, 5f, helpEnabled);
                    GlobalWeather.CloudRotationSpeedMiddle = m_editorUtils.Slider("CloudRotationSpeedMiddle", GlobalWeather.CloudRotationSpeedMiddle, -5f, 5f, helpEnabled);
                    GlobalWeather.CloudRotationSpeedFar = m_editorUtils.Slider("CloudRotationSpeedFar", GlobalWeather.CloudRotationSpeedFar, -5f, 5f, helpEnabled);
                    GlobalWeather.CloudHeight = m_editorUtils.IntField("CloudHeight", GlobalWeather.CloudHeight, helpEnabled);
                    if (GlobalWeather.CloudHeight < 0)
                    {
                        GlobalWeather.CloudHeight = 0;
                    }
                    GlobalWeather.CloudScale = m_editorUtils.Slider("CloudScale", GlobalWeather.CloudScale, 1f, 5f, helpEnabled);
                    GlobalWeather.CloudOffset = m_editorUtils.Slider("CloudOffset", GlobalWeather.CloudOffset, 1f, 500f, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    m_editorUtils.LabelField("LightingSettings", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    //GlobalWeather.CloudBrightness = m_editorUtils.Slider("CloudBrightness", GlobalWeather.CloudBrightness, 0f, 8f, helpEnabled);
                    GlobalWeather.CloudDomeBrightness = m_editorUtils.CurveField("CloudDomeBrightness", GlobalWeather.CloudDomeBrightness, helpEnabled);
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue);
                    GlobalWeather.CloudFade = m_editorUtils.Slider("CloudFadeDistance", GlobalWeather.CloudFade, 0f, 500f, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    m_editorUtils.LabelField("CloudSetup", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    GlobalWeather.TODCloudHeightLevelDensity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudHeightLevelDensity"), m_editorUtils.GetTooltip("TODCloudHeightLevelDensity")), GlobalWeather.TODCloudHeightLevelDensity);
                    m_editorUtils.InlineHelp("TODCloudHeightLevelDensity", helpEnabled);

                    GlobalWeather.TODCloudHeightLevelThickness = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudHeightLevelThickness"), m_editorUtils.GetTooltip("TODCloudHeightLevelThickness")), GlobalWeather.TODCloudHeightLevelThickness);
                    m_editorUtils.InlineHelp("TODCloudHeightLevelThickness", helpEnabled);

                    GlobalWeather.TODCloudHeightLevelSpeed = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudHeightLevelSpeed"), m_editorUtils.GetTooltip("TODCloudHeightLevelSpeed")), GlobalWeather.TODCloudHeightLevelSpeed);
                    m_editorUtils.InlineHelp("TODCloudHeightLevelSpeed", helpEnabled);

                    GlobalWeather.TODCloudOpacity = EditorGUILayout.CurveField(new GUIContent(m_editorUtils.GetTextValue("TODCloudOpacity"), m_editorUtils.GetTooltip("TODCloudOpacity")), GlobalWeather.TODCloudOpacity);
                    m_editorUtils.InlineHelp("TODCloudOpacity", helpEnabled);
                    GaiaEditorUtils.DrawTimeOfDayLine(currentTimeAndDayValue, 3.2f);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                if (EditorGUI.EndChangeCheck())
                {

                    m_pWSkyStandalone.m_profileValues.m_pwSkyCloudData.Save(GlobalWeather);
                }

                EditorGUILayout.Space();
                GUI.enabled = true;
                m_editorUtils.LabelField("Weather", EditorStyles.boldLabel);

                EditorGUILayout.HelpBox("You are using Procedural Worlds Sky which has time of day and weather. To edit Weather/Time Of Day setup go to 'Advanced Weather Settings'", MessageType.Info);

                GUILayout.BeginHorizontal();
                if (m_editorUtils.Button("OpenWeatherSettings"))
                {
                    GaiaUtils.FocusWeatherObject();
                }
                GUILayout.EndHorizontal();
                 }
#endif

        
        }
    }
}