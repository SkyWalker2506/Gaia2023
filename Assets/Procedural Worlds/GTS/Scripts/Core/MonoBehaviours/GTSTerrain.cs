using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace ProceduralWorlds.GTS
{
    /// <summary>
    /// The GTS Terrain is a script that can be attached to Unity Terrain GameObjects.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Terrain))]
    public class GTSTerrain : GTSComponent
    {
        #region Static

        private static GTSTerrain[] m_activeTerrains = Array.Empty<GTSTerrain>();
        private static GTSTerrain m_activeTerrain;

        public static GTSTerrain[] activeTerrains
        {
            get
            {
                // Is any of the items null?
                if (m_activeTerrains.Any(item => item == null))
                {
                    // Remove the nulls
                    List<GTSTerrain> terrains = new List<GTSTerrain>(m_activeTerrains);
                    terrains.RemoveAll(item => item == null);
                    m_activeTerrains = terrains.ToArray();
                }

                return m_activeTerrains;
            }
        }

        public static GTSTerrain activeTerrain => m_activeTerrain;

        private static void AddTerrain(GTSTerrain terrain)
        {
            if (m_activeTerrains.Contains(terrain))
                return;
            List<GTSTerrain> terrains = new List<GTSTerrain>(m_activeTerrains);
            terrains.Add(terrain);
            m_activeTerrains = terrains.ToArray();
            if (m_activeTerrains.Length > 0)
                m_activeTerrain = m_activeTerrains[0];
            GTSProfile.RefreshProfiles();
        }

        private static void RemoveTerrain(GTSTerrain terrain)
        {
            if (!m_activeTerrains.Contains(terrain))
                return;
            // Remove the nulls
            List<GTSTerrain> terrains = new List<GTSTerrain>(m_activeTerrains);
            terrains.Remove(terrain);
            m_activeTerrains = terrains.ToArray();
            if (m_activeTerrains.Length > 0)
                m_activeTerrain = m_activeTerrains[0];
            else
                m_activeTerrain = null;
            GTSProfile.RefreshProfiles();
        }

        #endregion

        #region Variables

        private Terrain m_terrain;
        private TerrainData m_terrainData;
        [SerializeField] private Vector3 lastTerrainPosition;

        #endregion

        #region Properties

        public Terrain terrain
        {
            get
            {
                if (m_terrain == null)
                    m_terrain = GetComponent<Terrain>();
                return m_terrain;
            }
        }

        public TerrainData terrainData
        {
            get
            {
                if (m_terrainData == null)
                    m_terrainData = terrain != null ? terrain.terrainData : null;
                return m_terrainData;
            }
        }

        public override Material material
        {
            get
            {
                if (terrain != null)
                    return terrain.materialTemplate;
                return null;
            }
            set
            {
                if (terrain != null)
                {
                    terrain.materialTemplate = value;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(terrain);
#endif
                }
            }
        }

        public override Vector2 position
        {
            get
            {
                Vector3 worldPosition = terrain != null ? terrain.transform.position : Vector3.zero;
                return new Vector2(worldPosition.x, worldPosition.z);
            }
        }

        public override Vector2 size
        {
            get
            {
                Vector3 worldSize = terrainData != null ? terrainData.size : Vector3.one;
                return new Vector2(worldSize.x, worldSize.z);
            }
        }

        public override Vector3 worldSize
        {
            get
            {
                if (terrainData != null)
                    return terrainData.size;
                return Vector2.one;
            }
        }

        public override int heightmapResolution
        {
            get
            {
                if (terrainData != null)
                    return terrainData.heightmapResolution;
                return 512;
            }
        }

        public bool IsApplied
        {
            get
            {
                if (terrain.materialTemplate != null)
                    if (terrain.materialTemplate == gtsTerrainMaterial)
                        return true;
                return false;
            }
        }

        #endregion

        #region Methods

        #region Unity Events

        public void SubscribeEvents()
        {
#if UNITY_EDITOR
            TerrainCallbacks.textureChanged += OnTerrainTextureChanged;
            TerrainCallbacks.heightmapChanged += OnTerrainHeightmapChanged;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
#endif
#if FLORA_2022
            FloraGlobals.onUpdateFloramap += OnUpdateFloraMap;
#endif
        }

        public void UnSubscribeEvents()
        {
#if UNITY_EDITOR
            TerrainCallbacks.textureChanged -= OnTerrainTextureChanged;
            TerrainCallbacks.heightmapChanged -= OnTerrainHeightmapChanged;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
#endif
        }

        private void OnEnable()
        {
            AddTerrain(this);
            SubscribeEvents();
            lastTerrainPosition = terrain.GetPosition();
        }

        private void OnUpdateFloraMap(Vector3 terrainPosition, Vector3 terrainSize, Texture floraMap,
            Texture floraNormal)
        {
            if (vegetationMapSettings.useFloraColormap)
            {
                material.SetTexture(GTSShaderID.VegetationMapTexture, floraMap);
                material.SetTexture(GTSShaderID.VegetationMapNormalTexture, floraNormal);
            }
        }

        private void OnUpdateVegetationMap(Texture vegetationMap, Texture vegetationNormal)
        {
            material.SetTexture(GTSShaderID.VegetationMapTexture, vegetationMap);
            material.SetTexture(GTSShaderID.VegetationMapNormalTexture, vegetationNormal);
        }

        private void OnUpdateColorMap(Texture colorMap,
            Texture colorNormal)
        {
            material.SetTexture(GTSShaderID.ColorMapTexture, colorMap);
            material.SetTexture(GTSShaderID.ColorMapNormalTexture, colorNormal);
        }

        private void OnDisable()
        {
            UnSubscribeEvents();
            RemoveTerrain(this);
        }

        private void OnDestroy()
        {
            UnSubscribeEvents();
            RemoveTerrain(this);
        }

        public override void UpdateProfile()
        {
            base.UpdateProfile();
#if UNITY_EDITOR
            m_updateBakedAlbedoMaps = true;
            MarkBakedAlbedoMapDirty();
#endif
        }
        
                public override void RegenerateData()
                {
                    base.RegenerateData();
                    DeleteAllTextures();
        
                    #if UNITY_EDITOR
                    if (name != null)
                    {
                        string assetFileName = $"{name}.mat";
                        string assetFilePath = GetAssetPath(assetFileName);
                        if (File.Exists(assetFilePath))
                        {
                            AssetDatabase.DeleteAsset(assetFilePath);
                        }
                    }
                    #endif
        
                    ApplyProfile();
                }
        
        
                public void DeleteAllTextures()
                {
                    #if UNITY_EDITOR
                    //Delete Controls
                    if (gtsControlTextures == null)
                        return;
                    int count = gtsControlTextures.Length;
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (gtsControlTextures[i] != null)
                            {
                                string name = gtsControlTextures[i].name;
                                string PNGAssetFilePath = $"{name}.png";
                                string PNGFilePath = $"{GetGTSUserDataFolder()}/{PNGAssetFilePath}";
                                if (File.Exists(PNGFilePath))
                                {
                                    AssetDatabase.DeleteAsset(PNGFilePath);
                                }
                            }
                            
                        }
                    }
                    
                    //Delete Weight Splats
                    if (weightSplatIndex2D != null)
                    {
                        string name = weightSplatIndex2D.name;
                        string weightSplatPNGAssetFilePath = $"{name}.png";
                        string weightSplatPNGFilePath = $"{GetGTSUserDataFolder()}/{weightSplatPNGAssetFilePath}";
                        if (File.Exists(weightSplatPNGFilePath))
                        {
                            AssetDatabase.DeleteAsset(weightSplatPNGFilePath);
                        }
                    }
        
                    //Delete World Normal Maps
                    if (worldNormalMap2D != null)
                    {
                        string name = worldNormalMap2D.name;
                        string worldNormalMapPNGAssetFilePath = $"{name}.png";
                        string worldNormalMapPNGFilePath = $"{GetGTSUserDataFolder()}/{worldNormalMapPNGAssetFilePath}";
                        if (File.Exists(worldNormalMapPNGFilePath))
                        {
                            AssetDatabase.DeleteAsset(worldNormalMapPNGFilePath);
                        }
                    }
                    
                    //Delete Albedo Map
                    if (bakedAlbedoMap2D != null)
                    {
                        string name = bakedAlbedoMap2D.name;
                        string bakedAlbedoMapPNGAssetFilePath = $"{name}.png";
                        string bakedAlbedoMapPNGFilePath = $"{GetGTSUserDataFolder()}/{bakedAlbedoMapPNGAssetFilePath}";
                        if (File.Exists(bakedAlbedoMapPNGFilePath))
                        {
                            AssetDatabase.DeleteAsset(bakedAlbedoMapPNGFilePath);
                        }
                    }
                    #endif
                }
        
        
#if UNITY_EDITOR
        public void EditorUpdate()
        {
            if (m_updateSplatIndexMaps)
            {
                m_updateSplatIndexMaps = false;
                UpdateSplatIndexMaps();
            }

            if (m_updateWorldNormalMaps)
            {
                m_updateWorldNormalMaps = false;
                UpdateWorldNormalMaps();
            }

            if (m_updateBakedAlbedoMaps)
            {
                m_updateBakedAlbedoMaps = false;
                UpdateBakedAlbedoMaps();
            }

            if (m_saveBakedAlbedoMapTexture)
            {
                m_saveBakedAlbedoMapTexture = false;
                UpdateBakedAlbedoMaps();
                SaveBakedAlbedoMaps();
                m_isDirty = true;
            }

            if (m_saveTextures)
            {
                m_saveTextures = false;
                UpdateAllTextures();
                SaveAllTextures();
                m_isDirty = true;
            }

            UpdateTerrainPosition();
        }
        
        public void SaveTexturesIfDirty()
        {
            if (m_updateSplatIndexMaps)
            {
                m_updateSplatIndexMaps = false;
                UpdateSplatIndexMaps();
            }

            if (m_updateWorldNormalMaps)
            {
                m_updateWorldNormalMaps = false;
                UpdateWorldNormalMaps();
            }

            if (m_updateBakedAlbedoMaps)
            {
                m_updateBakedAlbedoMaps = false;
                UpdateBakedAlbedoMaps();
            }

            if (m_saveBakedAlbedoMapTexture)
            {
                m_saveBakedAlbedoMapTexture = false;
                UpdateBakedAlbedoMaps();
                SaveBakedAlbedoMaps();
                m_isDirty = true;
            }

            if (m_saveTextures)
            {
                m_saveTextures = false;
                UpdateAllTextures();
                SaveAllTextures();
                m_isDirty = true;
            }

            UpdateTerrainPosition();
        }

        public void CheckForMissingTextures()
        {
            
            //Check Controls
            if (gtsControlTextures == null)
                return;
            int count = gtsControlTextures.Length;
            if (count > 0)
            {
                bool regenerateControlTextures = false;
                for (int i = 0; i < count; i++)
                {
                    if (gtsControlTextures[i] != null)
                    {
                        string name = gtsControlTextures[i].name;
                        string PNGAssetFilePath = $"{name}.png";
                        string PNGFilePath = $"{GetGTSUserDataFolder()}/{PNGAssetFilePath}";

                        if (!File.Exists(PNGFilePath))
                        {
                            regenerateControlTextures = true;
                        }
                    }
                    else
                    {
                        regenerateControlTextures = true;
                    }
                }

                if (regenerateControlTextures)
                {
                    UpdateControlTextures();
                    SaveControlTextures();
                }
            }
            
            //Check Weight Splats
            if (weightSplatIndex2D != null)
            {
                string name = weightSplatIndex2D.name;
                string weightSplatPNGAssetFilePath = $"{name}.png";
                string weightSplatPNGFilePath = $"{GetGTSUserDataFolder()}/{weightSplatPNGAssetFilePath}";
                if (!File.Exists(weightSplatPNGFilePath))
                {
                    UpdateSplatIndexMaps();
                    SaveSplatIndexMaps();
                }
            }
            else
            {
                UpdateSplatIndexMaps();
                SaveSplatIndexMaps();
            }

            //Check World Normal Maps
            if (worldNormalMap2D != null)
            {
                string name = worldNormalMap2D.name;
                string worldNormalMapPNGAssetFilePath = $"{name}.png";
                string worldNormalMapPNGFilePath = $"{GetGTSUserDataFolder()}/{worldNormalMapPNGAssetFilePath}";
                if (!File.Exists(worldNormalMapPNGFilePath))
                {
                    UpdateWorldNormalMaps();
                    SaveWorldNormalMaps();
                }
            }
            else
            {
                UpdateWorldNormalMaps();
                SaveWorldNormalMaps();
            }
            
            //Check Albedo Map
            if (bakedAlbedoMap2D != null)
            {
                string name = bakedAlbedoMap2D.name;
                string bakedAlbedoMapPNGAssetFilePath = $"{name}.png";
                string bakedAlbedoMapPNGFilePath = $"{GetGTSUserDataFolder()}/{bakedAlbedoMapPNGAssetFilePath}";
                if (!File.Exists(bakedAlbedoMapPNGFilePath))
                {
                    UpdateBakedAlbedoMaps();
                    SaveBakedAlbedoMaps();
                }
            }
            else
            {
                UpdateBakedAlbedoMaps();
                SaveBakedAlbedoMaps();
            }
        }
#endif
        protected override void Update()
        {
#if UNITY_EDITOR
            EditorUpdate();
#endif
            UpdateTerrainPosition();
            base.Update();
        }
#if UNITY_EDITOR
        public void MarkBakedAlbedoMapDirty()
        {
            m_bakedAlbedoMapDirty = true;
            MarkSceneDirty();
        }

        public void MarkSceneDirty()
        {
            if (Application.isPlaying)
                return;
            Scene scene = gameObject.scene;
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        public void OnTerrainTextureChanged(Terrain terrain, string name, RectInt region, bool synced)
        {
            if (terrain != this.terrain)
                return;
            m_updateSplatIndexMaps = true;
            m_updateBakedAlbedoMaps = true;
            m_texturesDirty = true;
            MarkSceneDirty();
        }

        public void OnTerrainHeightmapChanged(Terrain terrain, RectInt region, bool synced)
        {
            if (terrain != this.terrain)
                return;
            m_updateWorldNormalMaps = true;
        }

        public void OnSceneSaving(Scene scene, string path)
        {
            if (gameObject.scene != scene)
                return;
            if (m_bakedAlbedoMapDirty)
            {
                m_bakedAlbedoMapDirty = false;
                m_saveBakedAlbedoMapTexture = true;
                material.SetTexture(GTSShaderID.SplatmapIndexLowRes, bakedAlbedoMap2D);
            }

            if (m_texturesDirty)
            {
                m_texturesDirty = false;
                m_saveTextures = true;
            }

            EditorApplication.delayCall += () =>
            {
                SaveTexturesIfDirty();
                CheckForMissingTextures();
            };
        }

        public void OnPlaymodeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (m_bakedAlbedoMapDirty)
                    {
                        m_bakedAlbedoMapDirty = false;
                        m_saveBakedAlbedoMapTexture = true;
                    }

                    if (m_texturesDirty)
                    {
                        m_texturesDirty = false;
                        m_saveTextures = true;
                    }

                    EditorUpdate();
                    break;
            }
        }
#endif
        public void UpdateTerrainPosition()
        {
            if (m_terrain.transform.position != lastTerrainPosition)
            {
                material.SetVector(GTSShaderID.TerrainPosSize, new Vector4(position.x, position.y, size.x, size.y));
                lastTerrainPosition = m_terrain.transform.position;
            }
        }

        #endregion

        #region Generate

        public bool GenerateGTSMaterial()
        {
            if (GTSUtility.GetPipelineShaders(out Shader gtsShader, out Shader unityShader))
            {
                if (gtsTerrainMaterial == null || gtsTerrainMaterial.shader != gtsShader)
                {
                    // Get Material currently on Terrain
                    Material terrainMaterial = terrain.materialTemplate;
                    if (terrainMaterial == null || terrainMaterial.shader != gtsShader)
                    {
                        name = string.IsNullOrEmpty(name) ? "Terrain Material" : name;
                        Material newMaterial = new Material(gtsShader)
                        {
                            name = name
                        };
                        newMaterial.enableInstancing = false;
#if UNITY_EDITOR
                        string assetFileName = $"{name}.mat";
                        string assetFilePath = GetAssetPath(assetFileName);
                        // assetFilePath = AssetDatabase.GenerateUniqueAssetPath(assetFilePath);
                        AssetDatabase.CreateAsset(newMaterial, assetFilePath);
                        gtsTerrainMaterial = newMaterial;
                        m_saveTextures = true;
#endif
                    }
                }
            }
            else
            {
                GTSDebug.LogError("FATAL: GTS Shaders do not exist! To fix this, please re-import the GTS package.");
                return false;
            }

            return gtsTerrainMaterial != null;
        }

        public void GetDefaulTerrainMaterial()
        {
            // if (defaultTerrainMaterial == null)
            // {
            GTSUtility.GetPipelineShaders(out Shader gtsShader, out Shader unityShader);
            Material terrainMaterial = terrain.materialTemplate;
            if (terrainMaterial != null)
                if (terrainMaterial.shader != gtsShader)
                    defaultTerrainMaterial = terrainMaterial;
#if UNITY_EDITOR
                else
                    defaultTerrainMaterial = GTSDefaults.GetDefaultTerrainMaterial();
#endif
            // }
        }

        #endregion

        #region Add / Update / Remove Profile

        public override void ApplyProfile()
        {
            if (profile == null)
                return;
            profile.RefreshTerrainLayers(this);
            GetDefaulTerrainMaterial();
            if (GenerateGTSMaterial())
                SetTerrainToGTS();
#if UNITY_EDITOR
            m_updateSplatIndexMaps = true;
            m_updateBakedAlbedoMaps = true;
#endif
        }

        public override void RemoveProfile()
        {
            SetTerrainToDefaults();
        }

        #endregion

        #region Start / Stop Profile

        public override void StartProfile(GTSProfile profile)
        {
            profile.RefreshTerrainLayers(this);
        }

        public override void StopProfile(GTSProfile profile)
        {
        }

        #endregion

        #region File Storage

        public string GetGTSUserDataFolder()
        {
            Scene scene = gameObject.scene;
            string scenePath = GTSConstants.GetUserDataFolder("Scenes");
            string sceneName = "Untitled Scene";
            if (scene.IsValid())
            {
                if (!string.IsNullOrEmpty(scene.name))
                    sceneName = scene.name;
                else
                    sceneName = "Unsaved Scene";
            }

            string folderPath = $"{scenePath}/{sceneName}";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (terrain != null)
            {
                folderPath += $"/{terrain.name}";
            }

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        public string GetAssetPath(string assetFileName) => $"{GetGTSUserDataFolder()}/{assetFileName}";

        public Texture2D SaveTexture2DToDisk(Texture2D texture2D, FilterMode filterMode = FilterMode.Bilinear,
            bool generateMipMaps = true)
        {
            if (texture2D == null)
                return texture2D;
#if UNITY_EDITOR
            string filePath = AssetDatabase.GetAssetPath(texture2D);
            if (string.IsNullOrEmpty(filePath))
            {
                string assetFilePath = $"{texture2D.name}.png";
                filePath = $"{GetGTSUserDataFolder()}/{assetFilePath}";
            }

            if (!texture2D.isReadable)
            {
                RenderTexture tempRT = texture2D.ToRenderTexture();
                texture2D = tempRT.ToTexture2D();
                tempRT.Release();
            }

            GTSUtility.WriteTexture2D(filePath, texture2D);
            AssetDatabase.ImportAsset(filePath);
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = false;
                importer.ignorePngGamma = false;
                // -> Advanced
                importer.npotScale = TextureImporterNPOTScale.ToNearest;
                importer.isReadable = true;
                importer.streamingMipmaps = false;
                importer.vtOnly = false;
                importer.mipmapEnabled = true;
                importer.borderMipmap = false;
                importer.mipmapFilter = TextureImporterMipFilter.BoxFilter;
                importer.mipMapsPreserveCoverage = false;
                importer.mipmapEnabled = generateMipMaps;
                importer.fadeout = false;
                // <- Advanced
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = filterMode;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                // -> Importer Platform Settings 
                TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
                settings.maxTextureSize = 2048;
                settings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);
                // <- Importer Platform Settings
                AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
            }
            
            texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            
            
#endif
            return texture2D;
        }

        #endregion

        #region Update

        public void UpdateSplatIndexMaps()
        {
            if (material == null)
                return;
            if (gtsControlTextures == null)
                return;
            if (terrainData == null)
                return;
            Texture2D[] alphamapTextures = terrainData.alphamapTextures;
            int numControls = alphamapTextures.Length;
            if (numControls == 0)
                return;
            // TODO : This is because the system doesn't support more than 8 layers on a terrain.
            if (numControls >= 2)
                numControls = 2;
            indexSettings.weightSplatIndexMat = GTSUtility.createWeightedIndexMapMaterial;
            Material weightSplatIndexMat = indexSettings.weightSplatIndexMat;
            float indexBlurDistance = indexSettings.blurDistance;
            float indexBlurSteps = indexSettings.blurSteps;
            Texture2D firstSplat = alphamapTextures[0];
            if (m_weightSplatIndex != null)
                m_weightSplatIndex.Release();
            m_weightSplatIndex = new RenderTexture(firstSplat.width, firstSplat.height, 0, RenderTextureFormat.ARGB32,
                firstSplat.mipmapCount)
            {
                name = "Weight Splat Index"
            };
            m_weightSplatIndex.Create();
            weightSplatIndexMat.SetInt(GTSShaderID.WeightIndex_NumSplats, numControls);
            weightSplatIndexMat.SetFloat(GTSShaderID.WeightIndex_BlurDistance, indexBlurDistance);
            weightSplatIndexMat.SetFloat(GTSShaderID.WeightIndex_BlurSteps, indexBlurSteps);
            for (int i = 0; i < numControls; i++)
                weightSplatIndexMat.SetTexture(GTSShaderID.WeightIndex_Splats[i], alphamapTextures[i]);
            Graphics.Blit(firstSplat, m_weightSplatIndex, weightSplatIndexMat);
            material.SetFloat(GTSShaderID.Resolution, firstSplat.width);
            material.SetTexture(GTSShaderID.SplatmapIndex, m_weightSplatIndex);
            material.SetTexture(GTSShaderID.SplatmapIndexLowRes, m_bakedAlbedoMap);
            for (int i = 0; i < numControls; i++)
                material.SetTexture(GTSShaderID.Controls[i], alphamapTextures[i]);
        }

        public void UpdateBakedAlbedoMaps()
        {
            if (material == null)
                return;
            if (gtsControlTextures == null)
                return;
            if (terrainData == null)
                return;
            Texture2D[] alphamapTextures = terrainData.alphamapTextures;
            int numControls = alphamapTextures.Length;
            if (numControls == 0)
                return;
            if (m_bakedAlbedoMap != null)
                m_bakedAlbedoMap.Release();
            m_bakedAlbedoMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32)
            {
                name = "Weight Splat Low Res Index"
            };
            m_bakedAlbedoMap.useMipMap = true;
            m_bakedAlbedoMap.autoGenerateMips = true;
            m_bakedAlbedoMap.Create();
            Material createAlbedoMat = GTSUtility.createBakedAlbedoMapMaterial;
            createAlbedoMat.SetVector(GTSShaderID.TerrainPosSize, new Vector4(position.x, position.y, size.x, size.y));
            if (m_weightSplatIndex == null)
            {
                createAlbedoMat.SetTexture(GTSShaderID.SplatmapIndex, weightSplatIndex2D);
            }
            else
            {
                createAlbedoMat.SetTexture(GTSShaderID.SplatmapIndex, m_weightSplatIndex);
            }

            for (int i = 0; i < numControls; i++)
            {
                if (i < 2 && alphamapTextures[i] != null)
                {
                    createAlbedoMat.SetTexture(GTSShaderID.Controls[i], alphamapTextures[i]);
                }
            }

            createAlbedoMat.SetTexture(GTSShaderID.TextureArrayAlbedo, textureArraySettings.albedoArray);
            createAlbedoMat.SetTexture(GTSShaderID.TextureArrayNormal, textureArraySettings.normalArray);
            Vector4 GeoFarData = new Vector4(geoSettings.farStrength, geoSettings.farScale / 2, geoSettings.farOffset,
                geoSettings.farNormalStrength);
            createAlbedoMat.SetVector(GTSShaderID.GeoFarData, GeoFarData);
            createAlbedoMat.SetTexture(GTSShaderID.GeoMap, geoSettings.albedoTexture);
            createAlbedoMat.SetInt(GTSShaderID.GeoOn, geoSettings.enabled ? 1 : 0);
            createAlbedoMat.SetInt(GTSShaderID.HeightBlendOn, heightSettings.enabled ? 1 : 0);
            float blendFactor = 1 - (((heightSettings.blendFactor) - 0) / (1 - 0) * (0.98f - 0) + 0);
            createAlbedoMat.SetFloat(GTSShaderID.BlendFactor, blendFactor);
            createAlbedoMat.SetTexture(GTSShaderID.HeightMap, terrain.terrainData.heightmapTexture);
            createAlbedoMat.SetFloat(GTSShaderID.HeightScale, terrain.terrainData.heightmapScale.y);
            List<GTSTerrainLayer> gtsLayers = profile.gtsLayers;
            Vector4[] UVDataArray = new Vector4[8];
            Vector4[] ColorArray = new Vector4[8];
            Vector4[] LayerDataAArray = new Vector4[8];
            Vector4[] HeightDataArray = new Vector4[8];
            Vector4[] MaskMapRemapMin = new Vector4[8];
            Vector4[] MaskMapRemapMax = new Vector4[8];
            Vector4[] MaskMapRemapArray = new Vector4[8];

            for (int layerIndex = 0; layerIndex < gtsLayers.Count; layerIndex++)
            {
                GTSTerrainLayer gtsLayer = gtsLayers[layerIndex];
                Vector4 tilingOffset = new Vector4(
                    worldSize.x / gtsLayer.tileSize.x,
                    worldSize.z / gtsLayer.tileSize.y,
                    gtsLayer.tileOffset.x,
                    gtsLayer.tileOffset.y
                );

                // x: geo amount, y: stochastic, z: normalScale, w: detailNormalAmount
                Vector4 layerDataA = new Vector4(gtsLayer.geoAmount, gtsLayer.stochastic ? 1 : 0,
                    gtsLayer.normalStrength, gtsLayer.detailAmount);
                // x: height contrast, y: height brightness, z: height increase, w: 0
                Vector4 heightData = new Vector4(gtsLayer.heightContrast, gtsLayer.heightBrightness,
                    gtsLayer.heightIncrease, 0);
                HeightDataArray[layerIndex] = heightData;
                UVDataArray[layerIndex] = tilingOffset;
                ColorArray[layerIndex] = gtsLayer.tint;
                LayerDataAArray[layerIndex] = layerDataA;
                Vector4 maskMapRemapData = new Vector4(gtsLayer.maskMapRemapMin.y, gtsLayer.maskMapRemapMin.w,
                    gtsLayer.maskMapRemapMax.y, gtsLayer.maskMapRemapMax.w);
                MaskMapRemapArray[layerIndex] = maskMapRemapData;
            }

            createAlbedoMat.SetVectorArray(GTSShaderID.HeightDataArray, HeightDataArray);
            createAlbedoMat.SetVectorArray(GTSShaderID.UVDataArray, UVDataArray);
            createAlbedoMat.SetVectorArray(GTSShaderID.ColorArray, ColorArray);
            createAlbedoMat.SetVectorArray(GTSShaderID.LayerDataAArray, LayerDataAArray);
            createAlbedoMat.SetVectorArray(GTSShaderID.MaskMapRemapArray, MaskMapRemapArray);

            Graphics.Blit(null, m_bakedAlbedoMap, createAlbedoMat);
            material.SetTexture(GTSShaderID.SplatmapIndexLowRes, m_bakedAlbedoMap);
        }

        public void UpdateControlTextures()
        {
            if (material == null)
                return;
            if (!material.IsGTSMaterial())
                return;
            if (terrainData == null)
                return;
            if (terrainData.alphamapTextureCount > 0)
            {
                Texture2D[] alphamapTextures = terrainData.alphamapTextures;
                gtsControlTextures = alphamapTextures;
            }

            if (gtsControlTextures == null)
                return;
            int numControls = gtsControlTextures.Length;
            if (numControls == 0)
                return;
            for (int i = 0; i < numControls; i++)
                material.SetTexture(GTSShaderID.Controls[i], gtsControlTextures[i]);
        }

        public void UpdateWorldNormalMaps()
        {
            if (material == null)
                return;
            // Get terrain normal
            bool oldDrawInstanced = terrain.drawInstanced;
            terrain.drawInstanced = true;
            RenderTexture normalmapTexture = terrain.normalmapTexture;
            if (normalmapTexture != null)
            {
                Material worldNormalMat = GTSUtility.createWorldNormalMapMaterial;
                worldNormalMat.SetTexture(GTSShaderID.WorldNormal, normalmapTexture);
                if (m_worldNormalMap != null)
                    m_worldNormalMap.Release();
                m_worldNormalMap = new RenderTexture(normalmapTexture.width, normalmapTexture.height, 0,
                    RenderTextureFormat.ARGB32);
                m_worldNormalMap.useMipMap = true;
                m_worldNormalMap.autoGenerateMips = true;
                m_worldNormalMap.Create();
                Graphics.Blit(normalmapTexture, m_worldNormalMap, worldNormalMat);
                material.SetTexture(GTSShaderID.WorldNormalMap, m_worldNormalMap);
            }

            terrain.drawInstanced = oldDrawInstanced;
        }

        public void UpdateAllTextures()
        {
            UpdateControlTextures();
            UpdateSplatIndexMaps();
            UpdateBakedAlbedoMaps();
            UpdateWorldNormalMaps();
        }

        #endregion

        #region Save

        public void SaveAllTextures()
        {
            SaveSplatIndexMaps();
            SaveWorldNormalMaps();
            SaveBakedAlbedoMaps();
            SaveControlTextures();
        }

        public void SaveControlTextures()
        {
            if (gtsControlTextures == null)
                return;
            int count = gtsControlTextures.Length;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
#if UNITY_EDITOR
                    if (i >= gtsControlTexturesIDs.Count)
                        gtsControlTexturesIDs.Add(new GTSAssetID<Texture2D>());
                    Texture2D existingTexture = gtsControlTexturesIDs[i]?.LoadAsset();
                    if (existingTexture != null)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(existingTexture);
                        AssetDatabase.DeleteAsset(assetPath);
                    }

                    if (gtsControlTextures[i] == null)
                        continue;
#endif
                    gtsControlTextures[i].name = $"Control{i}";
                    gtsControlTextures[i] = SaveTexture2DToDisk(gtsControlTextures[i], FilterMode.Bilinear, false);
#if UNITY_EDITOR
                    gtsControlTexturesIDs[i]?.SaveAsset(gtsControlTextures[i]);
#endif
                }
            }

            int numControls = gtsControlTextures.Length;
            if (numControls == 0)
                return;
            for (int i = 0; i < numControls; i++)
                material.SetTexture(GTSShaderID.Controls[i], gtsControlTextures[i]);
        }

        public void SaveSplatIndexMaps()
        {
            if (profile == null)
                return;
            if (material == null)
                return;
            if (!material.IsGTSMaterial())
                return;
            if (m_weightSplatIndex != null)
                weightSplatIndex2D = GTSUtility.Copy(m_weightSplatIndex, weightSplatIndex2D);
            if (weightSplatIndex2D != null)
            {
                weightSplatIndex2D.name = "Weight Splat Index";
                weightSplatIndex2D = SaveTexture2DToDisk(weightSplatIndex2D, FilterMode.Point, false);
                material.SetTexture(GTSShaderID.SplatmapIndex, weightSplatIndex2D);
            }
        }

        public void SaveBakedAlbedoMaps()
        {
            if (profile == null)
                return;
            if (material == null)
                return;
            if (!material.IsGTSMaterial())
                return;
            if (m_bakedAlbedoMap != null)
                bakedAlbedoMap2D = GTSUtility.Copy(m_bakedAlbedoMap, bakedAlbedoMap2D);
            if (bakedAlbedoMap2D != null)
            {
                bakedAlbedoMap2D.name = "Baked Albedo Map";
                bakedAlbedoMap2D = SaveTexture2DToDisk(bakedAlbedoMap2D, FilterMode.Point);
                material.SetTexture(GTSShaderID.SplatmapIndexLowRes, bakedAlbedoMap2D);
            }
        }

        public void SaveWorldNormalMaps()
        {
            if (profile == null)
                return;
            if (material == null)
                return;
            if (!material.IsGTSMaterial())
                return;
            if (m_worldNormalMap == null)
                return;
            worldNormalMap2D = GTSUtility.Copy(m_worldNormalMap, worldNormalMap2D);
            worldNormalMap2D.name = "World Normal Map";
            worldNormalMap2D = SaveTexture2DToDisk(worldNormalMap2D);
            material.SetTexture(GTSShaderID.WorldNormalMap, worldNormalMap2D);
        }

        #endregion

        #endregion
    }
}