using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
namespace ProceduralWorlds.GTS
{
    [CustomEditor(typeof(GTSProfile))]
    public class GTSProfileEditor : GTSEditor
    {
        public static bool m_globalSettingsPanel = true;
        public static bool m_profileSettingsPanel = true;
        public static bool m_meshSettingsPanel = false;
        public static bool m_runtimeSettingsPanel = false;
        public static bool m_textureArraySettingsPanel = false;
        public static bool m_textureLayerSettingsPanel = false;
        private GTSProfile profile;
        private ReorderableList gtsLayersList;
        private GTSPipeline currentPipeline;
        private bool m_gtsLayerHelpEnabled = false;
        private bool m_showActions = true;
        public bool ShowActions
        {
            get => m_showActions;
            set => m_showActions = value;
        }
        private void OnDrawGTSLayersHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "GTS Layers");
        }
        private void OnDrawGTSLayersElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            GTSGlobalSettings globalSettings = profile.globalSettings;
            List<GTSTerrainLayer> gtsLayers = profile.gtsLayers;
            if (gtsLayers.Count == 0)
                return;
            GTSTerrainLayer gtsLayer = gtsLayers[index];
            // The 'level' property
            // The label field for level (width 100, height of a single line)
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), gtsLayer.name);
            if (isActive)
            {
                int oldIndent = EditorGUI.indentLevel;
                EditorGUILayout.BeginVertical(PWStyles.gpanel);
                {
                    EditorGUI.indentLevel = 0;
                    m_editorUtils.HeadingNonLocalized("Overview");
                    EditorGUI.indentLevel++;
                    {
                        gtsLayer.name = m_editorUtils.TextField("GTSLayerName", gtsLayer.name, m_gtsLayerHelpEnabled);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(PWStyles.gpanel);
                {
                    m_editorUtils.HeadingNonLocalized("Textures");
                    EditorGUI.indentLevel++;
                    {
                        gtsLayer.albedoMapTexture = (Texture2D)m_editorUtils.ObjectField("GTSLayerAlbedoMapTexture", gtsLayer.albedoMapTexture, typeof(Texture2D), false, m_gtsLayerHelpEnabled, GUILayout.MaxHeight(16));
                        gtsLayer.normalMapTexture = (Texture2D)m_editorUtils.ObjectField("GTSLayerNormalMapTexture", gtsLayer.normalMapTexture, typeof(Texture2D), false, m_gtsLayerHelpEnabled, GUILayout.MaxHeight(16));
                        gtsLayer.maskMapTexture = (Texture2D)m_editorUtils.ObjectField("GTSLayerMaskMapTexture", gtsLayer.maskMapTexture, typeof(Texture2D), false, m_gtsLayerHelpEnabled, GUILayout.MaxHeight(16));
                    }
                    EditorGUI.indentLevel--;
                    m_editorUtils.HeadingNonLocalized("Layer Adjustments");
                    EditorGUI.indentLevel++;
                    {
                        gtsLayer.tint = EditorGUILayout.ColorField(new GUIContent("Tint"), gtsLayer.tint, true, true, true);
                        m_editorUtils.InlineHelp("GTSLayerTint", m_gtsLayerHelpEnabled);
                        gtsLayer.tileSize = m_editorUtils.Vector2Field("GTSLayerTileSize", gtsLayer.tileSize, m_gtsLayerHelpEnabled);
                        gtsLayer.tileOffset = m_editorUtils.Vector2Field("GTSLayerTileOffset", gtsLayer.tileOffset, m_gtsLayerHelpEnabled);
                        gtsLayer.normalStrength = m_editorUtils.Slider("GTSLayerNormalStrength", gtsLayer.normalStrength, 0f, 10f, m_gtsLayerHelpEnabled);
                        m_editorUtils.MinMaxSliderWithFields("GTSLayerAmbientOcclusionMinMax", ref gtsLayer.maskMapRemapMin.y, ref gtsLayer.maskMapRemapMax.y, 0f, 1f, m_gtsLayerHelpEnabled);
                        m_editorUtils.MinMaxSliderWithFields("GTSLayerSmoothnessMinMax", ref gtsLayer.maskMapRemapMin.w, ref gtsLayer.maskMapRemapMax.w, 0f, 1f, m_gtsLayerHelpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    if (profile.detailSettings.enabled)
                    {
                        m_editorUtils.HeadingNonLocalized("Detail");
                        EditorGUI.indentLevel++;
                        {
                            gtsLayer.detailAmount = m_editorUtils.Slider("GTSLayerDetailAmount", gtsLayer.detailAmount, 0f, 1f, m_gtsLayerHelpEnabled);
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (profile.geoSettings.enabled)
                    {
                        m_editorUtils.HeadingNonLocalized("Geo");
                        EditorGUI.indentLevel++;
                        {
                            gtsLayer.geoAmount = m_editorUtils.Slider("GTSLayerGeoAmount", gtsLayer.geoAmount, 0f, 1f, m_gtsLayerHelpEnabled);
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (profile.heightSettings.enabled)
                    {
                        m_editorUtils.HeadingNonLocalized("Height Blending");
                        EditorGUI.indentLevel++;
                        {
                            gtsLayer.heightContrast = m_editorUtils.Slider("GTSLayerHeightContrast", gtsLayer.heightContrast, 0f, 2f, m_gtsLayerHelpEnabled);
                            gtsLayer.heightBrightness = m_editorUtils.Slider("GTSLayerHeightBrightness", gtsLayer.heightBrightness, 0f, 2f, m_gtsLayerHelpEnabled);
                            gtsLayer.heightIncrease = m_editorUtils.Slider("GTSLayerHeightIncrease", gtsLayer.heightIncrease, 0f, 10f, m_gtsLayerHelpEnabled);
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (currentPipeline == GTSPipeline.HDRP && profile.tessellationSettings.enabled)
                    {
                        m_editorUtils.HeadingNonLocalized("Tessellation");
                        EditorGUI.indentLevel++;
                        {
                            gtsLayer.displacementContrast = m_editorUtils.Slider("GTSLayerDisplacementContrast", gtsLayer.displacementContrast, 0f, 2f, m_gtsLayerHelpEnabled);
                            gtsLayer.displacementBrightness = m_editorUtils.Slider("GTSLayerDisplacementBrightness", gtsLayer.displacementBrightness, 0f, 2f, m_gtsLayerHelpEnabled);
                            gtsLayer.displacementIncrease = m_editorUtils.Slider("GTSLayerDisplacementIncrease", gtsLayer.displacementIncrease, 0f, 2f, m_gtsLayerHelpEnabled);
                            gtsLayer.tessellationAmount = m_editorUtils.Slider("GTSLayerTessellationAmount", gtsLayer.tessellationAmount, 0f, 100f, m_gtsLayerHelpEnabled);
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (globalSettings.targetPlatform != GTSTargetPlatform.MobileAndVR)
                    {
                        m_editorUtils.HeadingNonLocalized("Tri Planar");
                        EditorGUI.indentLevel++;
                        {
                            gtsLayer.triPlanar = m_editorUtils.Toggle("GTSLayerTriPlanar", gtsLayer.triPlanar, m_gtsLayerHelpEnabled);
                            gtsLayer.triPlanarSize = m_editorUtils.Vector2Field("GTSLayerTriPlanarSize", gtsLayer.triPlanarSize, m_gtsLayerHelpEnabled);
                        }
                        EditorGUI.indentLevel--;
                        if (!gtsLayer.triPlanar)
                        {
                            m_editorUtils.HeadingNonLocalized("Make Seamless (Stochastic)");
                            EditorGUI.indentLevel++;
                            {
                                gtsLayer.stochastic = m_editorUtils.Toggle("GTSLayerStochastic", gtsLayer.stochastic, m_gtsLayerHelpEnabled);
                            }
                            EditorGUI.indentLevel--;
                        }
                    }

                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel = oldIndent;
            }
        }
        private void RefreshGTSLayersList()
        {
            if (profile == null)
                return;
            gtsLayersList = new ReorderableList(profile.gtsLayers, typeof(GTSTerrainLayer), false, true, false, false);
            gtsLayersList.drawHeaderCallback = OnDrawGTSLayersHeader;
            gtsLayersList.drawElementCallback = OnDrawGTSLayersElement;
        }
        private void OnEnable()
        {
            profile = target as GTSProfile;
            RefreshGTSLayersList();
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this);
            // Pipeline Check
            currentPipeline = GTSUtility.GetCurrentPipeline();
        }
        public void DrawActionButtons(bool helpEnabled)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_editorUtils.GetContent("ApplyProfile"), PWStyles.maxHalfWidth, PWStyles.minButtonHeight))
                {
                    profile.AddGTSToTerrains();

                    profile.ApplyProfile();


                    if (profile.refreshTexturesArrays)
                    {
                        if (profile.TextureArraysEmpty)
                        {
                            if (EditorUtility.DisplayDialog("Missing Texture Arrays", "The Texture Arrays for this profile don't exist, would you like to generate them now?", "Yes", "No"))
                            {
                                profile.CreateTextureArrays();
                                profile.refreshTexturesArrays = false;
                            }
                        }

                        else if (EditorUtility.DisplayDialog("Update Texture Arrays", "The Texture Arrays don't match the updated terrain layers, would you like to re-generate them now?", "Yes", "No"))
                        {
                            profile.CreateTextureArrays();
                            profile.refreshTexturesArrays = false;
                        }
                    }

                }
                if (GUILayout.Button(m_editorUtils.GetContent("RemoveProfile"), PWStyles.maxHalfWidth, PWStyles.minButtonHeight))
                {
                    profile.RemoveProfile();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(m_editorUtils.GetContent("RegenerateData"), PWStyles.maxFullWidth, PWStyles.minButtonHeight))
                {
                    if (EditorUtility.DisplayDialog("Regenerate Data Warning",
                            "Regenerating the GTS data is a destructive operation, this will delete all current GTS textures and materials for this profile, are you sure you want to generate them now?",
                            "Yes", "No"))
                    {
                        profile.RegenerateData();
                    }
                    
                }
            }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("ApplyProfile", helpEnabled);
            m_editorUtils.InlineHelp("RemoveProfile", helpEnabled);
            m_editorUtils.InlineHelp("RegenerateData", helpEnabled);
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_editorUtils.GUIHeader();
            #region Perform Maintenance Check
            bool oldEnabled = GUI.enabled;
            if (!GTSUtility.GetPipelineShaders(out Shader gtsShader, out Shader unityShader))
            {
                EditorGUILayout.HelpBox("GTS has detected that a maintenance is required." +
                                        "\nClick the button below to perform maintenance now.", MessageType.Warning);
                Color oldBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Perform Maintenance"))
                {
                    GTSEditorUtility.PerformMaintenance();
                }
                GUI.backgroundColor = oldBackgroundColor;
                GUI.enabled = false;
            }
            #endregion
            // Label Prefix
            GUIContent iconContent = new GUIContent();
            GUIContent labelContent = new GUIContent("- Active");
            bool isConnected = profile.IsAppliedToSceneTerrains(out List<GTSTerrain> gtsTerrains);
            // Missing from build scenes
            if (isConnected)
            {
                iconContent = EditorGUIUtility.IconContent("lightMeter/greenLight");
                int terrainCount = gtsTerrains.Count;
                string terrainTitle = "Terrain";
                if (terrainCount > 1)
                    terrainTitle += "s";
                labelContent.text = $"Profile applied to '{terrainCount}' {terrainTitle} in the active Scenes.";
                string appliedTerrainsText = $"Profile applied to the following {terrainTitle}:";
                foreach (GTSTerrain gtsTerrain in gtsTerrains)
                {
                    appliedTerrainsText += $"\n  {gtsTerrain.name}";
                }
                labelContent.tooltip = appliedTerrainsText;
            }
            else
            {
                iconContent = EditorGUIUtility.IconContent("lightMeter/redLight");
                labelContent.text = "Profile not active in Scene";
                labelContent.tooltip = "Click on 'Apply Profile' to apply this profile to the Scene.";
            }
            // // In build scenes and disabled
            // else
            // {
            //     iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_min");
            //     labelContent.text = "BuildIndex: " + buildScene.buildIndex;
            //     labelContent.tooltip = "This scene is in build settings and DISABLED.\nIt will be NOT included in builds.";
            // }
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(iconContent, GUILayout.Width(iconContent.image.width));
                EditorGUILayout.LabelField(labelContent);
            }
            EditorGUILayout.EndHorizontal();
            m_globalSettingsPanel = m_editorUtils.Panel("GlobalSettingsPanel", GlobalSettingsPanel, m_globalSettingsPanel);
            m_meshSettingsPanel = m_editorUtils.Panel("MeshSettingsPanel", MeshSettingsPanel, m_meshSettingsPanel);
            m_profileSettingsPanel = m_editorUtils.Panel("ProfileSettingsPanel", ProfileSettingsPanel, m_profileSettingsPanel);
            m_textureLayerSettingsPanel = m_editorUtils.Panel("TextureLayerSettingsPanel", TextureLayerSettingsPanel, m_textureLayerSettingsPanel);
            m_textureArraySettingsPanel = m_editorUtils.Panel("TextureArraySettingsPanel", TextureArraySettingsPanel, m_textureArraySettingsPanel);
            m_runtimeSettingsPanel = m_editorUtils.Panel("RuntimeSettingsPanel", RuntimeSettingsPanel, m_runtimeSettingsPanel);
            if (ShowActions)
            {
                DrawActionButtons(m_gtsLayerHelpEnabled);
            }
            if (GUI.changed)
            {
                MarkProfileDirty();
            }

            GUI.enabled = oldEnabled;
        }
        public void MarkProfileDirty()
        {
            EditorUtility.SetDirty(target);
            profile.OnValidate();
        }
        public void DrawProfileSettings(string nameKey, GTSProfileSettings profileSettings, Action<bool> panel, bool helpEnabled)
        {
            // EditorUtilities.DrawSplitter();
            bool displayContent = m_editorUtils.DrawHeader(
                nameKey,
                profileSettings,
                helpEnabled,
                profileSettings.Reset,
                null
            );
            if (displayContent)
            {
                using (new EditorGUI.DisabledScope(!profileSettings.enabled))
                {
                    EditorGUI.indentLevel++;
                    panel?.Invoke(helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.Space();
        }
        public void GlobalSettingsPanel(bool helpEnabled)
        {
            GTSGlobalSettings globalSettings = profile.globalSettings;
            bool currentGUIState = GUI.enabled;
            {
                GUI.enabled = false;
                m_editorUtils.EnumPopup("CurrentPipeline", currentPipeline);
                GUI.enabled = currentGUIState;
                m_editorUtils.InlineHelp("CurrentPipeline", helpEnabled);
                globalSettings.blendDistance = m_editorUtils.Slider("GlobalBlendDistance", globalSettings.blendDistance, 0f, 1f, helpEnabled);
                globalSettings.blendRange = m_editorUtils.Slider("GlobalBlendRange", globalSettings.blendRange, 0f, 1.0f, helpEnabled);
                globalSettings.targetPlatform = (GTSTargetPlatform)m_editorUtils.EnumPopup("TargetPlatform", globalSettings.targetPlatform);
                if (globalSettings.targetPlatform == GTSTargetPlatform.MobileAndVR)
                {
                    // Label Prefix
                    GUIContent iconContent = new GUIContent();
                    GUIContent labelContent = new GUIContent("- Active");
                    // Missing from build scenes

                    if (profile.IsMoreThanFourLayers())
                    {
                        iconContent = EditorGUIUtility.IconContent("lightMeter/redLight");
                        labelContent.text = "Unsupported number of terrain layers on 1 or more terrains.";
                        labelContent.tooltip = "Please ensure all terrains using this profile do not exceed 4 terrain layers.";

                    }
                    else
                    {
                        iconContent = EditorGUIUtility.IconContent("lightMeter/greenLight");
                        labelContent.text = "Maximum supported terrain layers is 4";
                        labelContent.tooltip = "All terrains using this profile have a maximum count of 4 terrain layers";
                    }
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(iconContent, GUILayout.Width(iconContent.image.width));
                        EditorGUILayout.LabelField(labelContent);
                    }
                    EditorGUILayout.EndHorizontal();



                }
                globalSettings.uvTarget = (GTSUVTarget)m_editorUtils.EnumPopup("UVSpace", globalSettings.uvTarget);
            }
            GUI.enabled = currentGUIState;
        }
        public void HeightSettingsPanel(bool helpEnabled)
        {
            GTSHeightSettings heightSettings = profile.heightSettings;
            heightSettings.blendFactor = m_editorUtils.Slider("HeightBlendFactor", heightSettings.blendFactor, 0f, 1.0f, helpEnabled);
        }
        public void TessellationSettingsPanel(bool helpEnabled)
        {
            GTSTessellationSettings tessellationSettings = profile.tessellationSettings;
            tessellationSettings.multiplier = m_editorUtils.Slider("TessellationMultiplier", tessellationSettings.multiplier, 0f, 10f, helpEnabled);
            float minDistance = tessellationSettings.minDistance;
            float maxDistance = tessellationSettings.maxDistance;
            m_editorUtils.MinMaxSliderWithFields("TessellationMinMax", ref minDistance, ref maxDistance, 0f, 100f, helpEnabled);
            tessellationSettings.minDistance = minDistance;
            tessellationSettings.maxDistance = maxDistance;
        }
        public void SnowSettingsPanel(bool helpEnabled)
        {
            GTSSnowSettings snowSettings = profile.snowSettings;
            if (profile.globalSettings.uvTarget == GTSUVTarget.ObjectSpace)
            {
                snowSettings.objectSpace = m_editorUtils.Toggle("SnowObjectSpace", snowSettings.objectSpace, helpEnabled);
            }
            snowSettings.power = m_editorUtils.Slider("SnowPower", snowSettings.power, 0f, 1f, helpEnabled);
            snowSettings.minHeight = m_editorUtils.FloatField("SnowMinHeight", snowSettings.minHeight, helpEnabled);
            snowSettings.blendRange = m_editorUtils.Slider("SnowBlendRange", snowSettings.blendRange, 0f, 40f, helpEnabled);
            snowSettings.slopeBlend = m_editorUtils.Slider("SnowSlopeBlend", snowSettings.slopeBlend, 0f, 100f, helpEnabled);
            snowSettings.age = m_editorUtils.Slider("SnowAge", snowSettings.age, 0f, 1f, helpEnabled);
            snowSettings.scale = m_editorUtils.Slider("SnowScale", snowSettings.scale, 0.5f, 10f, helpEnabled);
            snowSettings.albedoTexture = (Texture2D)m_editorUtils.ObjectField("SnowAlbedoTexture", snowSettings.albedoTexture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
            snowSettings.normalTexture = (Texture2D)m_editorUtils.ObjectField("SnowNormalTexture", snowSettings.normalTexture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
            snowSettings.maskTexture = (Texture2D)m_editorUtils.ObjectField("SnowMaskTexture", snowSettings.maskTexture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
            snowSettings.normalStrength = m_editorUtils.Slider("SnowNormalStrength", snowSettings.normalStrength, 0f, 10f, helpEnabled);
            Vector4 maskRemapMin = snowSettings.maskRemapMin;
            Vector4 maskRemapMax = snowSettings.maskRemapMax;
            m_editorUtils.MinMaxSliderWithFields("SnowAmbientOcclusionMinMax", ref maskRemapMin.y, ref maskRemapMax.y, 0f, 1f, helpEnabled);
            m_editorUtils.MinMaxSliderWithFields("SnowSmoothnessMinMax", ref maskRemapMin.w, ref maskRemapMax.w, 0f, 1f, helpEnabled);
            snowSettings.maskRemapMin = maskRemapMin;
            snowSettings.maskRemapMax = maskRemapMax;
            snowSettings.color = EditorGUILayout.ColorField(m_editorUtils.GetContent("SnowColor"), snowSettings.color, true, true, true);
            m_editorUtils.InlineHelp("SnowColor", helpEnabled);
            //snowSettings.heightContrast = m_editorUtils.Slider("SnowHeightContrast", snowSettings.heightContrast, 0f, 2f, helpEnabled);
            //snowSettings.heightBrightness = m_editorUtils.Slider("SnowHeightBrightness", snowSettings.heightBrightness, 0f, 2f, helpEnabled);
            //snowSettings.heightIncrease = m_editorUtils.Slider("SnowHeightIncrease", snowSettings.heightIncrease, 0f, 10f, helpEnabled);
            if (currentPipeline == GTSPipeline.HDRP && profile.tessellationSettings.enabled)
            {
                snowSettings.displacementContrast = m_editorUtils.Slider("SnowDisplacementContrast", snowSettings.displacementContrast, 0f, 2f, helpEnabled);
                snowSettings.displacementBrightness = m_editorUtils.Slider("SnowDisplacementBrightness", snowSettings.displacementBrightness, 0f, 2f, helpEnabled);
                snowSettings.displacementIncrease = m_editorUtils.Slider("SnowDisplacementIncrease", snowSettings.displacementIncrease, 0f, 10f, helpEnabled);
                snowSettings.tessellationAmount = m_editorUtils.Slider("SnowTessellationAmount", snowSettings.tessellationAmount, 0f, 100f, helpEnabled);
            }
        }
        public void RainSettingsPanel(bool helpEnabled)
        {
            GTSRainSettings rainSettings = profile.rainSettings;
            rainSettings.Power = m_editorUtils.Slider("RainPower", rainSettings.Power, 0f, 1f, helpEnabled);
            rainSettings.MinHeight = m_editorUtils.FloatField("RainMinHeight", rainSettings.MinHeight, helpEnabled);
            rainSettings.MaxHeight = m_editorUtils.FloatField("RainMaxHeight", rainSettings.MaxHeight, helpEnabled);
            rainSettings.Speed = m_editorUtils.Slider("RainSpeed", rainSettings.Speed, 0f, 1f, helpEnabled);
            rainSettings.Darkness = m_editorUtils.Slider("RainDarkness", rainSettings.Darkness, 0f, 1f, helpEnabled);
            rainSettings.Smoothness = m_editorUtils.Slider("RainSmoothness", rainSettings.Smoothness, 0f, 1f, helpEnabled);
            rainSettings.Scale = m_editorUtils.Slider("RainScale", rainSettings.Scale, 0f, 10f, helpEnabled);
            rainSettings.rainDataTexture = (Texture2D)m_editorUtils.ObjectField("RainTexture", rainSettings.rainDataTexture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
        }
        public void DetailSettingsPanel(bool helpEnabled)
        {
            GTSDetailSettings detailSettings = profile.detailSettings;
            if (profile.globalSettings.uvTarget == GTSUVTarget.ObjectSpace)
            {
                detailSettings.objectSpace = m_editorUtils.Toggle("DetailObjectSpace", detailSettings.objectSpace, helpEnabled);
            }
            detailSettings.normalTexture = (Texture2D)m_editorUtils.ObjectField("DetailNormalTexture", detailSettings.normalTexture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));

            // Label Prefix
            GUIContent sRGBContent = new GUIContent("");
            GUIContent normalMapContent = new GUIContent("");

            if (detailSettings.normalTexture != null)
            {
                string detailNormal_AssetPath = AssetDatabase.GetAssetPath(detailSettings.normalTexture);
                var detailNormal_Importer = AssetImporter.GetAtPath(detailNormal_AssetPath) as TextureImporter;
                EditorGUILayout.BeginHorizontal();
                {
                    if (detailNormal_Importer.sRGBTexture)
                    {
                        sRGBContent.text = "Provided texture is set to 'sRGB'. Please uncheck 'sRGB' in texture settings.";
                        sRGBContent.tooltip = "Please ensure texture is not 'sRGB'.";
                        EditorGUILayout.LabelField(sRGBContent);
                    }
                    if (detailNormal_Importer.textureType == TextureImporterType.NormalMap)
                    {
                        normalMapContent.text = "Provided texture is set to 'NormalMap'. Please ensure type is 'Default'.";
                        normalMapContent.tooltip = "Please ensure texture is set to type 'Default'.";
                        EditorGUILayout.LabelField(normalMapContent);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }


            detailSettings.nearTiling = m_editorUtils.Slider("DetailNearTiling", detailSettings.nearTiling, 0f, 100f, helpEnabled);
            detailSettings.nearStrength = m_editorUtils.Slider("DetailNearStrength", detailSettings.nearStrength, 0f, 2f, helpEnabled);
            detailSettings.farTiling = m_editorUtils.Slider("DetailFarTiling", detailSettings.farTiling, 100f, 1000f, helpEnabled);
            detailSettings.farStrength = m_editorUtils.Slider("DetailFarStrength", detailSettings.farStrength, 0f, 2f, helpEnabled);
        }
        public void GeoSettingsPanel(bool helpEnabled)
        {
            GTSGeoSettings geoSettings = profile.geoSettings;
            if (profile.globalSettings.uvTarget == GTSUVTarget.ObjectSpace)
            {
                geoSettings.objectSpace = m_editorUtils.Toggle("GeoObjectSpace", geoSettings.objectSpace, helpEnabled);
            }
            geoSettings.albedoTexture = (Texture2D)m_editorUtils.ObjectField("GeoAlbedoTexture", geoSettings.albedoTexture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
            geoSettings.normalTexture = (Texture2D)m_editorUtils.ObjectField("GeoNormalTexture", geoSettings.normalTexture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
            // Near
            geoSettings.nearStrength = m_editorUtils.Slider("GeoNearStrength", geoSettings.nearStrength, 0f, 1f, helpEnabled);
            geoSettings.nearNormalStrength = m_editorUtils.Slider("GeoNormalStrength", geoSettings.nearNormalStrength, 0f, 1f, helpEnabled);
            geoSettings.nearScale = m_editorUtils.Slider("GeoNearScale", geoSettings.nearScale, 0f, 100f, helpEnabled);
            geoSettings.nearOffset = m_editorUtils.Slider("GeoNearOffset", geoSettings.nearOffset, 0f, 1f, helpEnabled);
            // Far
            geoSettings.farStrength = m_editorUtils.Slider("GeoFarStrength", geoSettings.farStrength, 0f, 1f, helpEnabled);
            geoSettings.farNormalStrength = m_editorUtils.Slider("GeoFarNormalStrength", geoSettings.farNormalStrength, 0f, 1f, helpEnabled);
            geoSettings.farScale = m_editorUtils.Slider("GeoFarScale", geoSettings.farScale, 100f, 500f, helpEnabled);
            geoSettings.farOffset = m_editorUtils.Slider("GeoFarOffset", geoSettings.farOffset, 0f, 1f, helpEnabled);
        }
        public void VariationSettingsPanel(bool helpEnabled)
        {
            GTSVariationSettings variationSettings = profile.variationSettings;
            if (profile.globalSettings.uvTarget == GTSUVTarget.ObjectSpace)
            {
                variationSettings.objectSpace = m_editorUtils.Toggle("VariationObjectSpace", variationSettings.objectSpace, helpEnabled);
            }
            variationSettings.texture = (Texture2D)m_editorUtils.ObjectField("VariationTexture", variationSettings.texture, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
            variationSettings.sizeA = m_editorUtils.Slider("VariationSizeA", variationSettings.sizeA, 0f, 10f, helpEnabled);
            variationSettings.sizeB = m_editorUtils.Slider("VariationSizeB", variationSettings.sizeB, 0f, 10f, helpEnabled);
            variationSettings.sizeC = m_editorUtils.Slider("VariationSizeC", variationSettings.sizeC, 0f, 10f, helpEnabled);
            variationSettings.intensity = m_editorUtils.Slider("VariationIntensity", variationSettings.intensity, 0f, 1f, helpEnabled);
        }

        public void ColorMapSettingsPanel(bool helpEnabled)
        {
            GTSColorMapSettings colorMapSettings = profile.colorMapSettings;
            //colorMapSettings.colorMapTex = (Texture)m_editorUtils.ObjectField("ColorMapTex", colorMapSettings.colorMapTex, typeof(Texture), false, helpEnabled, GUILayout.MaxHeight(16));
            //colorMapSettings.colorMapNormalTex = (Texture)m_editorUtils.ObjectField("ColorMapNormalTex", colorMapSettings.colorMapNormalTex, typeof(Texture), false, helpEnabled, GUILayout.MaxHeight(16));
            colorMapSettings.alphaIntensity = m_editorUtils.Slider("ColorMapAlphaIntensity", colorMapSettings.alphaIntensity, 0f, 2f, helpEnabled);
            colorMapSettings.colorIntensity = m_editorUtils.Slider("ColorMapIntensity", colorMapSettings.colorIntensity, 0f, 10f, helpEnabled);
            colorMapSettings.nearIntensity = m_editorUtils.Slider("ColorMapNearIntensity", colorMapSettings.nearIntensity, 0f, 1f, helpEnabled);
            colorMapSettings.farIntensity = m_editorUtils.Slider("ColorMapFarIntensity", colorMapSettings.farIntensity, 0f, 1f, helpEnabled);
            //colorMapSettings.nearNormalIntensity = m_editorUtils.Slider("ColorMapNearNormalIntensity", colorMapSettings.nearNormalIntensity, 0f, 1f, helpEnabled);
            //colorMapSettings.farNormalIntensity = m_editorUtils.Slider("ColorMapFarNormalIntensity", colorMapSettings.farNormalIntensity, 0f, 1f, helpEnabled);
        }

        public void VegetationMapSettingsPanel(bool helpEnabled)
        {
            GTSVegetationMapSettings vegetationMapSettings = profile.vegetationMapSettings;
            vegetationMapSettings.useFloraColormap = m_editorUtils.Toggle("VegetationMapUseFlora", vegetationMapSettings.useFloraColormap, helpEnabled);
            if (!vegetationMapSettings.useFloraColormap)
            {
                vegetationMapSettings.colorMapTex = (Texture)m_editorUtils.ObjectField("VegetationMapTex", vegetationMapSettings.colorMapTex, typeof(Texture), false, helpEnabled, GUILayout.MaxHeight(16));
                vegetationMapSettings.colorMapNormalTex = (Texture)m_editorUtils.ObjectField("VegetationMapNormalTex", vegetationMapSettings.colorMapNormalTex, typeof(Texture), false, helpEnabled, GUILayout.MaxHeight(16));
            }

            vegetationMapSettings.alphaIntensity = m_editorUtils.Slider("VegetationMapAlphaIntensity", vegetationMapSettings.alphaIntensity, 0f, 2f, helpEnabled);
            vegetationMapSettings.colorIntensity = m_editorUtils.Slider("VegetationMapIntensity", vegetationMapSettings.colorIntensity, 0f, 10f, helpEnabled);
            vegetationMapSettings.nearIntensity = m_editorUtils.Slider("VegetationMapNearIntensity", vegetationMapSettings.nearIntensity, 0f, 1f, helpEnabled);
            vegetationMapSettings.farIntensity = m_editorUtils.Slider("VegetationMapFarIntensity", vegetationMapSettings.farIntensity, 0f, 1f, helpEnabled);
            vegetationMapSettings.nearNormalIntensity = m_editorUtils.Slider("VegetationMapNearNormalIntensity", vegetationMapSettings.nearNormalIntensity, 0f, 1f, helpEnabled);
            vegetationMapSettings.farNormalIntensity = m_editorUtils.Slider("VegetationMapFarNormalIntensity", vegetationMapSettings.farNormalIntensity, 0f, 1f, helpEnabled);
        }

        public void TextureLayerSettingsPanel(bool helpEnabled)
        {
            EditorGUI.indentLevel++;
            {
                m_gtsLayerHelpEnabled = helpEnabled;
                EditorGUILayout.BeginHorizontal();
                {
                    float halfWidth = EditorGUIUtility.currentViewWidth * 0.5f;
                    if (GUILayout.Button(m_editorUtils.GetContent("RefreshGTSLayers"), PWStyles.maxHalfWidth))
                    {
                        Terrain[] terrains = Terrain.activeTerrains;
                        if (terrains.Length > 0)
                        {
                            bool anyHasLayers = terrains.Any(t =>
                            {
                                TerrainData terrainData = t.terrainData;
                                if (terrainData != null)
                                {
                                    TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                                    if (terrainLayers != null)
                                    {
                                        return terrainLayers.Length > 0;
                                    }
                                }
                                return false;
                            });
                            if (anyHasLayers)
                            {
                                bool refreshLayers = true;
                                List<GTSTerrainLayer> gtsLayers = profile.gtsLayers;
                                if (gtsLayers.Count > 0)
                                {
                                    refreshLayers = EditorUtility.DisplayDialog("GTS - Refresh Layers", "Are you sure you want to refresh GTS Layers? This is a destructive operation that will reset the data in the GTS Layers to match the Unity Terrain Layers. \nNote: This cannot be undone.", "Yes", "Cancel");
                                }
                                if (refreshLayers)
                                {
                                    profile.RefreshTerrainLayers(terrains, true);
                                    //profile.CreateTextureArrays();
                                    if (profile.refreshTexturesArrays)
                                    {
                                        if (EditorUtility.DisplayDialog("Update Texture Arrays", "The Texture Arrays don't match the updated terrain layers, would you like to re-generate them now?", "Yes", "No"))
                                        {
                                            profile.CreateTextureArrays();
                                            profile.refreshTexturesArrays = false;
                                        }
                                    }
                                    MarkProfileDirty();
                                    EditorGUIUtility.ExitGUI();

                                }
                            }
                            else
                            {
                                if (EditorUtility.DisplayDialog("GTS - Refresh Layers", "Warning: None of the Terrains in the Scene have any Terrain Layers. Please add at least one terrain layer.", "Ok"))
                                {
                                    EditorGUIUtility.ExitGUI();
                                }
                            }
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("GTS - Refresh Layers", "Warning: There are no Terrains in the Scene to get Terrain Layers from.", "Ok"))
                            {
                                EditorGUIUtility.ExitGUI();
                            }
                        }
                    }
                    bool enabledGUIState = GUI.enabled;
                    GUI.enabled = gtsLayersList.count > 0;
                    if (GUILayout.Button(m_editorUtils.GetContent("ClearGTSLayers"), PWStyles.maxHalfWidth))
                    {
                        if (EditorUtility.DisplayDialog("GTS - Clear Layers", "Are you sure you want to clear all GTS Layers? This will remove all copies of the Terrain Layers in the Profile only. \nNote: This cannot be undone.", "Yes", "Cancel"))
                        {
                            profile.ClearTerrainLayers();
                            MarkProfileDirty();
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                    GUI.enabled = enabledGUIState;
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("RefreshGTSLayers", helpEnabled);
                m_editorUtils.InlineHelp("ClearGTSLayers", helpEnabled);
                EditorGUILayout.Space();
                gtsLayersList.DoLayoutList();
            }
            EditorGUI.indentLevel--;
        }
        public void TextureArraySettingsPanel(bool helpEnabled)
        {
            GTSTextureArraySettings textureArraySettings = profile.textureArraySettings;
            textureArraySettings.anisoLevel = m_editorUtils.IntSlider("AnisoLevel", textureArraySettings.anisoLevel, 1, 16, helpEnabled);
            textureArraySettings.mipMapBias = m_editorUtils.Slider("MipMapBias", textureArraySettings.mipMapBias, -1f, 1f, helpEnabled);
            textureArraySettings.maxTextureSize = (GTSTextureSize)m_editorUtils.EnumPopup("MaxTextureSize", textureArraySettings.maxTextureSize, helpEnabled);
            textureArraySettings.compressed = m_editorUtils.Toggle("Compressed", textureArraySettings.compressed, helpEnabled);
            if (textureArraySettings.compressed)
            {
                EditorGUI.indentLevel++;
                {
                    textureArraySettings.compressionFormat = (GTSCompressionFormat)m_editorUtils.EnumPopup("CompressionFormat", textureArraySettings.compressionFormat, helpEnabled);
                }
                EditorGUI.indentLevel--;
            }
            if (m_editorUtils.Button("CreateTextureArrays", helpEnabled))
                profile.CreateTextureArrays();
            textureArraySettings.albedoArray = (Texture2DArray)m_editorUtils.ObjectField("AlbedoArray", textureArraySettings.albedoArray, typeof(Texture2DArray), false, helpEnabled, GUILayout.MaxHeight(16));
            textureArraySettings.normalArray = (Texture2DArray)m_editorUtils.ObjectField("NormalArray", textureArraySettings.normalArray, typeof(Texture2DArray), false, helpEnabled, GUILayout.MaxHeight(16));
        }

        public void MeshSettingsPanel(bool helpEnabled)
        {
            GTSMeshSettings meshSettings = profile.meshSettings;
            meshSettings.saveResolution = (SaveResolution)m_editorUtils.EnumPopup("TerrainResolution", meshSettings.saveResolution);
            meshSettings.lodCount = m_editorUtils.IntSlider("NumberOfLODs", meshSettings.lodCount, 1, 4, helpEnabled);
            for (int lodIndex = 0; lodIndex < meshSettings.lodCount; lodIndex++)
            {
                EditorGUI.indentLevel++;
                string lodName = "LOD" + lodIndex + "Simplification";
                meshSettings.lodQuality[lodIndex] = m_editorUtils.Slider(lodName, meshSettings.lodQuality[lodIndex], 0, 100, helpEnabled);
                EditorGUI.indentLevel--;
            }
            meshSettings.subTiles = m_editorUtils.IntSlider("SubdivisionLevel", meshSettings.subTiles, 0, 5, helpEnabled);

            if (GUILayout.Button(m_editorUtils.GetContent("ConvertToMesh")))
            {
                profile.ConvertToMesh();
            }

            string toggleTerrainName;
            if (profile.terrainsHidden)
            {
                toggleTerrainName = "ShowOriginalTerrains";
            }
            else
            {
                toggleTerrainName = "ShowMeshTerrains";
            }



            if (GUILayout.Button(m_editorUtils.GetContent(toggleTerrainName)))
            {
                if (profile.terrainsHidden)
                {
                    profile.UnhideTerrains();
                    profile.HideMeshTerrains();
                }
                else
                {
                    profile.HideTerrains();
                    profile.UnHideMeshTerrains();
                }
            }

        }


        public void RuntimeSettingsPanel(bool helpEnabled)
        {
            if (m_editorUtils.Button("CreateRuntimeData", helpEnabled))
            {
                profile.SetRuntimeData();
            }
            profile.runtime = (GTSRuntime)m_editorUtils.ObjectField("RuntimeData", profile.runtime, typeof(GTSRuntime), false, helpEnabled);
        }


        public void ProfileSettingsPanel(bool helpEnabled)
        {
            DrawProfileSettings("DetailSettingsPanel", profile.detailSettings, DetailSettingsPanel, helpEnabled);
            DrawProfileSettings("GeoSettingsPanel", profile.geoSettings, GeoSettingsPanel, helpEnabled);
            DrawProfileSettings("HeightSettingsPanel", profile.heightSettings, HeightSettingsPanel, helpEnabled);
            DrawProfileSettings("SnowSettingsPanel", profile.snowSettings, SnowSettingsPanel, helpEnabled);
            DrawProfileSettings("RainSettingsPanel", profile.rainSettings, RainSettingsPanel, helpEnabled);
            DrawProfileSettings("ColorMapSettingsPanel", profile.colorMapSettings, ColorMapSettingsPanel, helpEnabled);
            //DrawProfileSettings("VegetationMapSettingsPanel", profile.vegetationMapSettings, VegetationMapSettingsPanel, helpEnabled);
            if (currentPipeline == GTSPipeline.HDRP)
            {
                DrawProfileSettings("TessellationSettingsPanel", profile.tessellationSettings, TessellationSettingsPanel, helpEnabled);
            }
            DrawProfileSettings("VariationSettingsPanel", profile.variationSettings, VariationSettingsPanel, helpEnabled);
        }
    }
}