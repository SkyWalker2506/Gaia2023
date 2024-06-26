using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public abstract class GTSComponent : MonoBehaviour
    {
        #region Variables
        [SerializeField] protected GTSProfile m_profile;
        [SerializeField] protected Texture2D m_colorMap;
        // Only Serialize the GUIDs of the assets being used so we can 'rehydrate' the script.
        [SerializeField] protected GTSAssetID<Material> gtsTerrainMaterialID = new GTSAssetID<Material>();
        [SerializeField] protected List<GTSAssetID<Texture2D>> gtsControlTexturesIDs = new List<GTSAssetID<Texture2D>>();
        [SerializeField] protected GTSAssetID<Material> defaultTerrainMaterialID = new GTSAssetID<Material>();
        [SerializeField] protected GTSAssetID<TerrainData> defaultTerrainDataID = new GTSAssetID<TerrainData>();
        [SerializeField] protected GTSAssetID<Texture2D> worldNormalMap2DID = new GTSAssetID<Texture2D>();
        [SerializeField] protected GTSAssetID<Texture2D> weightSplatIndex2DID = new GTSAssetID<Texture2D>();
        [FormerlySerializedAs("weightSplatIndexLowRes2DID")] [SerializeField]
        protected GTSAssetID<Texture2D> bakedAlbedoMap2DID = new GTSAssetID<Texture2D>();
        public GTSIndexSettings indexSettings = new GTSIndexSettings();
        protected Material m_gtsTerrainMaterial;
        protected Texture2D[] m_gtsControlTextures;
        protected Material m_defaultTerrainMaterial;
        protected TerrainData m_defaultTerrainData;
        protected Texture2D m_worldNormalMap2D;
        protected Texture2D m_weightSplatIndex2D;
        protected Texture2D m_bakedAlbedoMap2D;
        protected RenderTexture m_worldNormalMap;
        protected RenderTexture m_weightSplatIndex;
        protected RenderTexture m_bakedAlbedoMap;
        protected bool m_isDirty = true;
#if UNITY_EDITOR
        protected bool m_updateSplatIndexMaps = false;
        protected bool m_updateWorldNormalMaps = false;
        protected bool m_updateBakedAlbedoMaps = false;
        protected bool m_bakedAlbedoMapDirty = false;
        protected bool m_texturesDirty = false;
        protected bool m_saveBakedAlbedoMapTexture = false;
        protected bool m_saveTextures = false;
#endif
        #endregion
        #region Properties
        public GTSProfile profile
        {
            get => m_profile;
            set => m_profile = value;
        }
        public bool ShouldRegenerate => m_worldNormalMap2D == null || m_weightSplatIndex2D == null || m_bakedAlbedoMap2D == null;
        public GTSGlobalSettings globalSettings => profile != null ? profile.globalSettings : null;
        public GTSDetailSettings detailSettings => profile != null ? profile.detailSettings : null;
        public GTSGeoSettings geoSettings => profile != null ? profile.geoSettings : null;
        public GTSVariationSettings variationSettings => profile != null ? profile.variationSettings : null;
        public GTSSnowSettings snowSettings => profile != null ? profile.snowSettings : null;
        public GTSRainSettings rainSettings => profile != null ? profile.rainSettings : null;
        public GTSTessellationSettings tessellationSettings => profile != null ? profile.tessellationSettings : null;
        public GTSHeightSettings heightSettings => profile != null ? profile.heightSettings : null;
        public GTSColorMapSettings colorMapSettings => profile != null ? profile.colorMapSettings : null;
        public GTSVegetationMapSettings vegetationMapSettings => profile != null ? profile.vegetationMapSettings : null;
        public GTSTextureArraySettings textureArraySettings => profile != null ? profile.textureArraySettings : null;

        public Texture2D colormap
        {
            get => m_colorMap;
            set => m_colorMap = value;
        }
        public bool IsDirty
        {
            get => m_isDirty;
            set => m_isDirty = value;
        }
        public abstract Material material { get; set; }
        public abstract Vector2 position { get; }
        public abstract Vector2 size { get; }
        public abstract Vector3 worldSize { get; }
        public abstract int heightmapResolution { get; }
        protected Material gtsTerrainMaterial
        {
            get
            {
#if UNITY_EDITOR
                if (m_gtsTerrainMaterial == null)
                    m_gtsTerrainMaterial = gtsTerrainMaterialID?.LoadAsset();
#endif
                return m_gtsTerrainMaterial;
            }
            set
            {
                m_gtsTerrainMaterial = value;
#if UNITY_EDITOR
                gtsTerrainMaterialID.SaveAsset(m_gtsTerrainMaterial);
#endif
            }
        }
        protected Material defaultTerrainMaterial
        {
            get
            {
#if UNITY_EDITOR
                if (m_defaultTerrainMaterial == null)
                    m_defaultTerrainMaterial = defaultTerrainMaterialID?.LoadAsset();
#endif
                return m_defaultTerrainMaterial;
            }
            set
            {
                m_defaultTerrainMaterial = value;
#if UNITY_EDITOR
                defaultTerrainMaterialID.SaveAsset(m_defaultTerrainMaterial);
#endif
            }
        }
        protected TerrainData defaultTerrainData
        {
            get
            {
#if UNITY_EDITOR
                if (m_defaultTerrainData == null)
                    m_defaultTerrainData = defaultTerrainDataID?.LoadAsset();
#endif
                return m_defaultTerrainData;
            }
            set
            {
                m_defaultTerrainData = value;
#if UNITY_EDITOR
                defaultTerrainDataID.SaveAsset(m_defaultTerrainData);
#endif
            }
        }
        protected Texture2D[] gtsControlTextures
        {
            get
            {
#if UNITY_EDITOR
                if (m_gtsControlTextures == null)
                {
                    int count = gtsControlTexturesIDs.Count;
                    if (count > 0)
                    {
                        m_gtsControlTextures = new Texture2D[count];
                        for (int i = 0; i < count; i++)
                        {
                            GTSAssetID<Texture2D> guid = gtsControlTexturesIDs[i];
                            m_gtsControlTextures[i] = guid?.LoadAsset();
                        }
                    }
                }
#endif
                return m_gtsControlTextures;
            }
            set
            {
                int count = value != null ? value.Length : 0;
                if (count > 0)
                {
                    // TODO : Isaac : The system only supports a maximum of 2 control textures currently.
                    if (count >= 2)
                        count = 2;
                    m_gtsControlTextures = new Texture2D[count];
                    for (int i = 0; i < count; i++)
                        gtsControlTextures[i] = GTSUtility.Copy(value[i]);
                }
                else
                {
                    m_gtsControlTextures = Array.Empty<Texture2D>();
                }
            }
        }
        protected Texture2D worldNormalMap2D
        {
            get
            {
#if UNITY_EDITOR
                if (m_worldNormalMap2D == null)
                    m_worldNormalMap2D = worldNormalMap2DID?.LoadAsset();
#endif
                return m_worldNormalMap2D;
            }
            set
            {
                m_worldNormalMap2D = value;
#if UNITY_EDITOR
                worldNormalMap2DID.SaveAsset(m_worldNormalMap2D);
#endif
            }
        }
        protected Texture2D weightSplatIndex2D
        {
            get
            {
#if UNITY_EDITOR
                if (m_weightSplatIndex2D == null)
                    m_weightSplatIndex2D = weightSplatIndex2DID?.LoadAsset();
#endif
                return m_weightSplatIndex2D;
            }
            set
            {
                m_weightSplatIndex2D = value;
#if UNITY_EDITOR
                weightSplatIndex2DID.SaveAsset(m_weightSplatIndex2D);
#endif
            }
        }
        protected Texture2D bakedAlbedoMap2D
        {
            get
            {
#if UNITY_EDITOR
                if (m_bakedAlbedoMap2D == null)
                    m_bakedAlbedoMap2D = bakedAlbedoMap2DID?.LoadAsset();
#endif
                return m_bakedAlbedoMap2D;
            }
            set
            {
                m_bakedAlbedoMap2D = value;
#if UNITY_EDITOR
                bakedAlbedoMap2DID.SaveAsset(m_bakedAlbedoMap2D);
#endif
            }
        }
        #endregion
        #region Methods
        // Update is called once per frame
        protected virtual void Update()
        {
            if (m_isDirty)
            {
                m_isDirty = false;
                Refresh();
            }
        }
        public virtual void ApplyProfile()
        {
        }
        public virtual void UpdateProfile()
        {
            UpdateMaterial();
        }
        public virtual void RemoveProfile()
        {
        }
        public virtual void StartProfile(GTSProfile profile)
        {
        }
        public virtual void StopProfile(GTSProfile profile)
        {
        }
        public virtual void RegenerateData()
        {
        }
        public void SetTerrainToDefaults()
        {
            if (defaultTerrainMaterial != null)
            {
                material = defaultTerrainMaterial;
                #region Disable Snow
                // Turn snow off globally.
                Shader.SetGlobalFloat(GTSShaderID.GlobalSnowIntensity, 0);
                // Turn off any other compatible systems.
                Vector4 snowDataA = new Vector4(0, snowSettings.power, snowSettings.minHeight, snowSettings.age);
                Shader.SetGlobalVector(GTSShaderID.SnowDataA, snowDataA);
                #endregion
                #region Disable Rain
                //Turn rain off globally
                Vector4 rainDataA = new Vector4(0, Mathf.Min(rainSettings.Power, 0.9f), rainSettings.MinHeight, rainSettings.MaxHeight);
                #endregion
            }
        }
        public void SetTerrainToGTS()
        {
            if (gtsTerrainMaterial != null)
                material = gtsTerrainMaterial;
        }
        public void OnValidate()
        {
            m_isDirty = true;
        }
        public void Refresh()
        {
            UpdateMaterial();
            SetReferencedTextures();
        }
        public void SetReferencedTextures()
        {
            if (material == null)
                return;
            if (gtsControlTextures != null)
            {
                for (int i = 0; i < gtsControlTextures.Length; i++)
                    material.SetTexture(GTSShaderID.Controls[i], gtsControlTextures[i]);
            }
            if (worldNormalMap2D != null)
                material.SetTexture(GTSShaderID.WorldNormalMap, worldNormalMap2D);
            if (weightSplatIndex2D != null)
                material.SetTexture(GTSShaderID.SplatmapIndex, weightSplatIndex2D);
            if (bakedAlbedoMap2D != null)
                material.SetTexture(GTSShaderID.SplatmapIndexLowRes, bakedAlbedoMap2D);
        }
        private void SetKeyword(int keywordID, string keywordName, bool enabled)
        {
            material.SetInt(keywordID, enabled ? 1 : 0);
            if (enabled)
                material.EnableKeyword(keywordName);
            else
                material.DisableKeyword(keywordName);
        }
        public void UpdateMaterial()
        {
            if (profile == null)
                return;
            if (material == null)
                return;
            if (!material.IsGTSMaterial())
                return;
            SetKeyword(GTSShaderID.Tessellation, GTSShaderID.TessellationOn, tessellationSettings.enabled);
            SetKeyword(GTSShaderID.DetailNormals, GTSShaderID.DetailNormalsOn, detailSettings.enabled);
            SetKeyword(GTSShaderID.Snow, GTSShaderID.SnowOn, snowSettings.enabled);
            SetKeyword(GTSShaderID.Rain, GTSShaderID.RainOn, rainSettings.enabled);
            SetKeyword(GTSShaderID.Geo, GTSShaderID.GeoOn, geoSettings.enabled);
            SetKeyword(GTSShaderID.Variation, GTSShaderID.VariationOn, variationSettings.enabled);
            SetKeyword(GTSShaderID.HeightBlend, GTSShaderID.HeightBlendOn, heightSettings.enabled);
            SetKeyword(GTSShaderID.MobileVR, GTSShaderID.MobileVROn, globalSettings.targetPlatform == GTSTargetPlatform.MobileAndVR);
            SetKeyword(GTSShaderID.Colormap, GTSShaderID.ColormapOn, colorMapSettings.enabled);
            SetKeyword(GTSShaderID.Vegetationmap, GTSShaderID.VegetationmapOn, vegetationMapSettings.enabled);
            material.SetVector(GTSShaderID.TerrainPosSize, new Vector4(position.x, position.y, size.x, size.y));
            material.SetInt(GTSShaderID.WorldAlignedUVs, (int)globalSettings.uvTarget);
            float blendFactor = 1 - (((heightSettings.blendFactor) - 0) / (1 - 0) * (0.98f - 0) + 0);
            material.SetFloat(GTSShaderID.BlendFactor, blendFactor);
            Vector4 detailNearFarData = new Vector4(detailSettings.nearTiling,
                detailSettings.nearStrength,
                detailSettings.farTiling,
                detailSettings.farStrength);
            material.SetVector(GTSShaderID.DetailNearFarData, detailNearFarData);
            Vector4 GeoNearData = new Vector4(geoSettings.nearStrength, geoSettings.nearScale, geoSettings.nearOffset, geoSettings.nearNormalStrength);
            Vector4 GeoFarData = new Vector4(geoSettings.farStrength, geoSettings.farScale, geoSettings.farOffset, geoSettings.farNormalStrength);
            material.SetVector(GTSShaderID.GeoNearData, GeoNearData);
            material.SetVector(GTSShaderID.GeoFarData, GeoFarData);
            Vector4 MacroVariationData = new Vector4(variationSettings.sizeA / 1000,
                variationSettings.sizeB / 10000,
                variationSettings.sizeC / 100000,
                variationSettings.intensity);
            material.SetVector(GTSShaderID.MacroVariationData, MacroVariationData);
            material.SetFloat(GTSShaderID.TessellationMultiplier, tessellationSettings.multiplier);
            material.SetFloat(GTSShaderID.TessellationFactorMinDistance, tessellationSettings.minDistance);
            material.SetFloat(GTSShaderID.TessellationFactorMaxDistance, tessellationSettings.maxDistance);

            //Colormap settings
            Vector4 ColormapNearFarData = new Vector4(colorMapSettings.nearIntensity, colorMapSettings.farIntensity, colorMapSettings.nearNormalIntensity, colorMapSettings.farNormalIntensity);
            material.SetVector(GTSShaderID.ColorMapNearFarData, ColormapNearFarData);

            Vector4 ColormapDataA = new Vector4(colorMapSettings.alphaIntensity, colorMapSettings.colorIntensity, 1, 1);
            material.SetVector(GTSShaderID.ColorMapDataA, ColormapDataA);

            material.SetTexture(GTSShaderID.ColorMapTexture, colormap);

            //Object space data
            Vector4 objectSpaceDataA = new Vector4(
            detailSettings.objectSpace == true ? 1 : 0,
            geoSettings.objectSpace == true ? 1 : 0,
            snowSettings.objectSpace == true ? 1 : 0,
            variationSettings.objectSpace == true ? 1 : 0
            );

            material.SetVector(GTSShaderID.ObjectSpaceDataA, objectSpaceDataA);

            // Create Vector Arrays
            Vector4[] HeightDataArray = new Vector4[8];
            Vector4[] UVDataArray = new Vector4[8];
            Vector4[] MaskMapRemapMinArray = new Vector4[8];
            Vector4[] MaskMapRemapMaxArray = new Vector4[8];
            Vector4[] MaskMapRemapArray = new Vector4[8];
            Vector4[] ColorArray = new Vector4[8];
            Vector4[] TriPlanarDataArray = new Vector4[8];
            Vector4[] DisplacementDataArray = new Vector4[8];
            Vector4[] LayerDataAArray = new Vector4[8];
            List<GTSTerrainLayer> gtsLayers = profile.gtsLayers;
            for (int layerIndex = 0; layerIndex < gtsLayers.Count; layerIndex++)
            {
                GTSTerrainLayer gtsLayer = gtsLayers[layerIndex];
                // x: height contrast, y: height brightness, z: height increase, w: 0
                Vector4 heightData = new Vector4(gtsLayer.heightContrast, gtsLayer.heightBrightness, gtsLayer.heightIncrease, 0);
                // x: displacement contrast, y: displacement brightness, z: displacement increase, w: 0
                Vector4 displacmentData = new Vector4(gtsLayer.displacementContrast, gtsLayer.displacementBrightness, gtsLayer.displacementIncrease, gtsLayer.tessellationAmount);
                // x: tiling x, y: tiling y, z: offset x, w: offset y
                Vector4 tilingOffset = new Vector4(
                    worldSize.x / gtsLayer.tileSize.x,
                    worldSize.z / gtsLayer.tileSize.y,
                    gtsLayer.tileOffset.x,
                    gtsLayer.tileOffset.y
                );
                // x: tri planar size x, y: tri planar size y, z: tri planar enabled, w: 0
                Vector4 triPlanarData = new Vector4(gtsLayer.triPlanarSize.x * 10f, gtsLayer.triPlanarSize.y * 10f, gtsLayer.triPlanar == true ? 1 : 0, 0);
                // x: geo amount, y: stochastic, z: normalScale, w: detailNormalAmount
                Vector4 layerDataA = new Vector4(gtsLayer.geoAmount, gtsLayer.stochastic == true ? 1 : 0, gtsLayer.normalStrength, gtsLayer.detailAmount);
                Vector4 maskMapRemapData = new Vector4(gtsLayer.maskMapRemapMin.y, gtsLayer.maskMapRemapMin.w, gtsLayer.maskMapRemapMax.y, gtsLayer.maskMapRemapMax.w);

                // Update material properties
                material.SetVector(GTSShaderID.LayerHeightData[layerIndex], heightData);
                material.SetVector(GTSShaderID.MaskMapRemapMin[layerIndex], gtsLayer.maskMapRemapMin);
                material.SetVector(GTSShaderID.MaskMapRemapMax[layerIndex], gtsLayer.maskMapRemapMax);
                material.SetVector(GTSShaderID.Color[layerIndex], gtsLayer.tint);
                material.SetVector(GTSShaderID.DisplacementData[layerIndex], displacmentData);
                material.SetVector(GTSShaderID.LayerST[layerIndex], tilingOffset);
                material.SetVector(GTSShaderID.LayerDataA[layerIndex], layerDataA);
                material.SetVector(GTSShaderID.TriPlanarData[layerIndex], triPlanarData);
                material.SetVector(GTSShaderID.MaskMapRemap[layerIndex], maskMapRemapData);

                // Set Vector Arrays
                HeightDataArray[layerIndex] = heightData;
                UVDataArray[layerIndex] = tilingOffset;
                MaskMapRemapMinArray[layerIndex] = gtsLayer.maskMapRemapMin;
                MaskMapRemapMaxArray[layerIndex] = gtsLayer.maskMapRemapMax;
                ColorArray[layerIndex] = gtsLayer.tint;
                TriPlanarDataArray[layerIndex] = triPlanarData;
                DisplacementDataArray[layerIndex] = displacmentData;
                LayerDataAArray[layerIndex] = layerDataA;
                MaskMapRemapArray[layerIndex] = maskMapRemapData;
            }

            // Set Vector Arrays
            Shader.SetGlobalVectorArray(GTSShaderID.HeightDataArray, HeightDataArray);
            Shader.SetGlobalVectorArray(GTSShaderID.UVDataArray, UVDataArray);
            Shader.SetGlobalVectorArray(GTSShaderID.MaskMapRemapMinArray, MaskMapRemapMinArray);
            Shader.SetGlobalVectorArray(GTSShaderID.MaskMapRemapMaxArray, MaskMapRemapMaxArray);
            Shader.SetGlobalVectorArray(GTSShaderID.MaskMapRemapArray, MaskMapRemapArray);
            Shader.SetGlobalVectorArray(GTSShaderID.ColorArray, ColorArray);
            Shader.SetGlobalVectorArray(GTSShaderID.TriPlanarDataArray, TriPlanarDataArray);
            Shader.SetGlobalVectorArray(GTSShaderID.DisplacementDataArray, DisplacementDataArray);
            Shader.SetGlobalVectorArray(GTSShaderID.LayerDataAArray, LayerDataAArray);

            // Snow Settings
            Vector4 snowDataA = new Vector4(snowSettings.enabled ? snowSettings.power : 0, snowSettings.power, snowSettings.minHeight, snowSettings.age);
            Vector4 snowDataB = new Vector4(snowSettings.scale, snowSettings.blendRange, snowSettings.slopeBlend, snowSettings.normalStrength);
            Vector4 snowDisplacementData = new Vector4(snowSettings.displacementContrast, snowSettings.displacementBrightness, snowSettings.displacementIncrease, snowSettings.tessellationAmount);
            Vector4 snowHeightData = new Vector4(snowSettings.heightContrast, snowSettings.heightBrightness, snowSettings.heightIncrease, 0);
            Shader.SetGlobalVector(GTSShaderID.SnowDataA, snowDataA);
            Shader.SetGlobalVector(GTSShaderID.SnowDataB, snowDataB);
            Shader.SetGlobalVector(GTSShaderID.SnowDisplacementData, snowDisplacementData);
            Shader.SetGlobalVector(GTSShaderID.SnowHeightData, snowHeightData);
            Shader.SetGlobalVector(GTSShaderID.SnowColor, snowSettings.color);
            Shader.SetGlobalVector(GTSShaderID.SnowMaskRemapMin, snowSettings.maskRemapMin);
            Shader.SetGlobalVector(GTSShaderID.SnowMaskRemapMax, snowSettings.maskRemapMax);
            // Setting global textures is fine.
            Shader.SetGlobalTexture(GTSShaderID.SnowAlbedoMap, snowSettings.albedoTexture);
            Shader.SetGlobalTexture(GTSShaderID.SnowNormalMap, snowSettings.normalTexture);
            Shader.SetGlobalTexture(GTSShaderID.SnowMaskMap, snowSettings.maskTexture);

            // Set Legacy Snow Values
            Shader.SetGlobalFloat(GTSShaderID.GlobalSnowIntensity, snowSettings.enabled ? snowSettings.power : 0);
            Shader.SetGlobalFloat(GTSShaderID.GlobalCoverLayer1FadeStart, snowSettings.minHeight);
            Shader.SetGlobalFloat(GTSShaderID.GlobalCoverLayer1FadeDist, snowSettings.blendRange);
#if GAIA_PRO_PRESENT
            Gaia.ProceduralWorldsGlobalWeather weather = Gaia.ProceduralWorldsGlobalWeather.Instance;
            if (weather != null)
            {
                if (Gaia.GaiaUtils.CheckIfSceneProfileExists(out Gaia.SceneProfile profile))
                {
                    weather.PermanentSnowHeight = snowSettings.minHeight;
                    weather.SnowFadeHeight = snowSettings.blendRange;
                    if (profile.m_selectedLightingProfileValuesIndex <= profile.m_lightingProfiles.Count - 1)
                    {
                        profile.m_lightingProfiles[profile.m_selectedLightingProfileValuesIndex].m_pwSkyWeatherData.m_snowSettings.Save(weather);
                    }
                }
            }
#endif

            //Rain Settings
            Vector4 rainDataA = new Vector4(Mathf.Min(rainSettings.Power, 0.9f), Mathf.Min(rainSettings.Power, 0.9f), rainSettings.MinHeight, rainSettings.MaxHeight);
            Vector4 rainDataB = new Vector4(rainSettings.Speed, 1 - rainSettings.Darkness, rainSettings.Smoothness, rainSettings.Scale);
            Shader.SetGlobalVector(GTSShaderID.RainDataA, rainDataA);
            Shader.SetGlobalVector(GTSShaderID.RainDataB, rainDataB);
            Shader.SetGlobalTexture(GTSShaderID.RainMap, rainSettings.rainDataTexture);

            // Terrain Settings
            Vector4 recipSize = new Vector4(
                (float)1.0f / heightmapResolution,
                (float)1.0f / heightmapResolution,
                (float)1.0f / (heightmapResolution - 1),
                (float)1.0f / (heightmapResolution - 1));
            material.SetVector(GTSShaderID.TerrainHeightmapRecipSize, recipSize);
            Vector4 globalBlendData = new Vector4(globalSettings.blendDistance / 10000f,
                globalSettings.blendRange, 0, 0);
            material.SetVector(GTSShaderID.GlobalBlendData, globalBlendData);
            Shader.SetGlobalTexture(GTSShaderID.SnowAlbedoMap, snowSettings.albedoTexture);
            Shader.SetGlobalTexture(GTSShaderID.SnowNormalMap, snowSettings.normalTexture);
            Shader.SetGlobalTexture(GTSShaderID.SnowMaskMap, snowSettings.maskTexture);
            material.SetTexture(GTSShaderID.DetailNormalMap, detailSettings.normalTexture);
            material.SetTexture(GTSShaderID.GeoMap, geoSettings.albedoTexture);
            material.SetTexture(GTSShaderID.GeoNormal, geoSettings.normalTexture);
            material.SetTexture(GTSShaderID.MacroVariationMap, variationSettings.texture);

            // Texture Arrays
            material.SetTexture(GTSShaderID.TextureArrayAlbedo, textureArraySettings.albedoArray);
            material.SetTexture(GTSShaderID.TextureArrayNormal, textureArraySettings.normalArray);
        }
        #endregion
    }
}