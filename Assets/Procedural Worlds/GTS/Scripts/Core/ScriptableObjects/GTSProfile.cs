using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
#if GAIA_PRO_PRESENT
using Gaia;
#endif

namespace ProceduralWorlds.GTS
{
    /// <summary>
    /// The GTS Profile is a collection of data that can be applied to many GTS Terrains.
    /// </summary>
    [CreateAssetMenu(fileName = "GTS Profile", menuName = "Procedural Worlds/GTS/Profile", order = 1)]
    public class GTSProfile : ScriptableObject
    {
        #region Static

        private static GTSProfile[] m_activeProfiles = Array.Empty<GTSProfile>();
        private static GTSProfile m_activeProfile;

        public static GTSProfile[] activeProfiles
        {
            get
            {
                // Is any of the items null?
                if (m_activeProfiles.All(item => item != null))
                    return m_activeProfiles;

                // Remove the nulls
                List<GTSProfile> profiles = new List<GTSProfile>(m_activeProfiles);
                profiles.RemoveAll(item => item == null);
                m_activeProfiles = profiles.ToArray();


                return m_activeProfiles;
            }
        }

        public static GTSProfile activeProfile => m_activeProfile;

        public static void RefreshProfiles()
        {
            List<GTSProfile> result = new List<GTSProfile>();
            GTSTerrain[] activeTerrains = GTSTerrain.activeTerrains;
            foreach (GTSTerrain terrain in activeTerrains)
            {
                GTSProfile profile = terrain.profile;
                if (profile == null)
                    continue;
                if (result.Contains(profile))
                    continue;
                result.Add(profile);
            }

            m_activeProfiles = result.ToArray();
            if (m_activeProfiles.Length > 0)
                m_activeProfile = m_activeProfiles.First();
        }

        #endregion

        public const string ALBEDO_ARRAY_NAME = "Albedo Texture Array";
        public const string NORMAL_ARRAY_NAME = "Normal Texture Array";
        public bool TextureArraysEmpty => textureArraySettings.TextureArraysEmpty;
        public List<GTSTerrainLayer> gtsLayers = new List<GTSTerrainLayer>();
        public GTSGlobalSettings globalSettings = new GTSGlobalSettings();
        public GTSMeshSettings meshSettings = new GTSMeshSettings();
        public GTSHeightSettings heightSettings = new GTSHeightSettings();
        public GTSTessellationSettings tessellationSettings = new GTSTessellationSettings();
        public GTSSnowSettings snowSettings = new GTSSnowSettings();
        public GTSRainSettings rainSettings = new GTSRainSettings();
        public GTSDetailSettings detailSettings = new GTSDetailSettings();
        public GTSGeoSettings geoSettings = new GTSGeoSettings();
        public GTSColorMapSettings colorMapSettings = new GTSColorMapSettings();
        public GTSVegetationMapSettings vegetationMapSettings = new GTSVegetationMapSettings();
        public GTSTextureArraySettings textureArraySettings = new GTSTextureArraySettings();
        public GTSVariationSettings variationSettings = new GTSVariationSettings();
        public bool terrainsHidden = false;
        private GTSExportTerrainSettings meshExportSettings;
        public bool refreshTexturesArrays = false;
        public bool exceededMaxTerrainLayers = false;
        public GTSRuntime runtime;

#if UNITY_EDITOR
        [NonSerialized] private bool isLowPoly = false;
#endif
        

        private List<GTSComponent> GetGTSComponents()
        {
            List<GTSComponent> gtscomponents = new List<GTSComponent>();
            bool dynamicallyLoaded = IsGaiaTerrainLoadedScene();
            GTSComponent[] foundComponents = GTSUtility.FindObjectsByType<GTSComponent>(dynamicallyLoaded);
            foreach (GTSComponent gtsComponent in foundComponents)
            {
                if (gtsComponent.profile == this)
                    gtscomponents.Add(gtsComponent);
            }

            return gtscomponents;
        }

        public bool IsGaiaTerrainLoadedScene()
        {
#if GAIA_PRO_PRESENT
            return GaiaUtils.HasDynamicLoadedTerrains();
#else
            return false;
#endif
        }

        public bool IsAppliedToSceneTerrains(out List<GTSTerrain> ouputTerrains)
        {
            ouputTerrains = new List<GTSTerrain>();
            GTSTerrain[] foundTerrains = GTSUtility.FindObjectsByType<GTSTerrain>(true);
            foreach (GTSTerrain gtsTerrain in foundTerrains)
            {
                if (gtsTerrain.profile != this)
                    continue;
                if (gtsTerrain.IsApplied)
                    ouputTerrains.Add(gtsTerrain);
            }

            return ouputTerrains.Count > 0;
        }

        public bool IsMoreThanFourLayers()
        {
            Terrain[] terrains = GTSUtility.FindObjectsByType<Terrain>(true);

            bool isMoreThanFour = false;
            foreach (Terrain terrain in terrains)
            {
                if (terrain.terrainData.terrainLayers.Length > 4)
                {
                    isMoreThanFour = true;
                }
            }

            return isMoreThanFour;
        }

        public void HideTerrains()
        {
            Terrain[] terrains = GTSUtility.FindObjectsByType<Terrain>(true);

            foreach (Terrain terrain in terrains)
            {
                terrain.drawHeightmap = false;
            }

            terrainsHidden = true;
        }

        public void UnhideTerrains()
        {
            Terrain[] terrains = GTSUtility.FindObjectsByType<Terrain>(true);

            foreach (Terrain terrain in terrains)
            {
                terrain.drawHeightmap = true;
            }

            terrainsHidden = false;
        }

        public void HideMeshTerrains()
        {
            // Get a list of all components using this profile
            GTSMeshComponent[] gtsComponents = GTSUtility.FindObjectsByType<GTSMeshComponent>(true);
            // Loop through all components
            foreach (GTSMeshComponent component in gtsComponents)
            {
                component.Hide();
            }
        }

        public void UnHideMeshTerrains()
        {
            // Get a list of all components using this profile
            GTSMeshComponent[] gtsComponents = GTSUtility.FindObjectsByType<GTSMeshComponent>(true);
            // Loop through all components
            foreach (GTSMeshComponent component in gtsComponents)
            {
                component.Unhide();
            }
        }

        public static string GetGTSTerrainUserDataFolder(Terrain terrain)
        {
            // Create save directory
            Scene scene = terrain.gameObject.scene;
            string scenePath = GTSConstants.GetUserDataFolder("Scenes");
            string sceneName = "Untitled Scene";
            if (scene.IsValid())
            {
                sceneName = string.IsNullOrEmpty(scene.name) ? "Unsaved Scene" : scene.name;
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

            string filePath = $"{folderPath}/";

            return filePath;
        }


        public void ConvertToMesh()
        {
            globalSettings.uvTarget = GTSUVTarget.ObjectSpace;
#if UNITY_EDITOR
            GTSExportTerrainSettings exportSettings =
                AssetDatabase.LoadAssetAtPath<GTSExportTerrainSettings>(
                    "Assets/Procedural Worlds/GTS/Content Resources/GTS MeshTerrainSimplifySettings.asset");

            //Clear Mesh Data
            GTSMeshComponent[] meshComponents = GTSUtility.FindObjectsByType<GTSMeshComponent>(true);
            foreach (GTSMeshComponent component in meshComponents)
            {
                DestroyImmediate(component.gameObject);
            }

            List<MeshToSave> meshesToSave = new List<MeshToSave>();

            if (IsGaiaTerrainLoadedScene())
            {
#if GAIA_PRO_PRESENT
                void Act(Terrain t)
                {
                    ConvertTerrainToMesh(t, null, meshSettings.lodQuality, exportSettings, ref meshesToSave);
                }

                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(Act, true, null, "Exporting Meshes in Terrain Scenes...");
#endif
            }
            else
            {
                GameObject rootMeshTerrains = new GameObject("GTS_MeshTerrains")
                {
                    transform =
                    {
                        position = new Vector3(0, 0, 0)
                    }
                };
                rootMeshTerrains.AddComponent<GTSMeshComponent>();

                //Get a list of all components using this profile
                List<GTSComponent> gtsComponents = GetGTSComponents();
                //Loop through all components
                foreach (GTSComponent component in gtsComponents)
                {
                    //Get the terrain that this component is attached to
                    Terrain terrain = component.gameObject.GetComponent<Terrain>();
                    ConvertTerrainToMesh(terrain, rootMeshTerrains, meshSettings.lodQuality, exportSettings,
                        ref meshesToSave);
                }
            }

            HideTerrains();
            HideMeshTerrains();
            UnHideMeshTerrains();

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (MeshToSave meshToSave in meshesToSave)
                {
                    AssetDatabase.CreateAsset(meshToSave.mesh, meshToSave.path);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif
        }

        public struct MeshToSave
        {
            public string path;
            public Mesh mesh;
        }

        public void ConvertTerrainToMesh(Terrain terrain, GameObject rootMeshTerrains, float[] simplyQuality,
            GTSExportTerrainSettings exportSettings, ref List<MeshToSave> meshesToSave)
        {
            GTSPipeline currentPipeline = GTSUtility.GetCurrentPipeline();
            if (currentPipeline == GTSPipeline.BuiltIn)
            {
                Material materialTemplate = terrain.materialTemplate;
                materialTemplate.DisableKeyword("UNITY_INSTANCING_ENABLED");
                materialTemplate.enableInstancing = false;
            }
#if UNITY_EDITOR
            if (rootMeshTerrains == null)
            {
                rootMeshTerrains = new GameObject("GTS_MeshTerrains")
                {
                    transform =
                    {
                        position = new Vector3(0, 0, 0),
                        parent = terrain.transform.root
                    }
                };
                rootMeshTerrains.AddComponent<GTSMeshComponent>();
            }

            // Get save path
            string folderPath = GetGTSTerrainUserDataFolder(terrain);

            // Create new folder if not already existing for meshes
            string meshFolderPath = $"{folderPath}/Meshes";
            if (!Directory.Exists(meshFolderPath))
                Directory.CreateDirectory(meshFolderPath);

            // Export terrain full resolution, no splits
            int saveRes = (int)meshSettings.saveResolution;

            // Get a reference to exported terrain
            Mesh fullResTerrain = GTSUtility.BuildUnityMesh(terrain, saveRes);

            GTSMeshSplitter meshSplitter = new GTSMeshSplitter(fullResTerrain);
            List<Mesh> terrainSubTileList = meshSplitter.Split(meshSettings.subTiles);

            GameObject meshTerrainRoot = new GameObject(terrain.name + " SubTiles")
            {
                transform =
                {
                    position = new Vector3(0, 0, 0),
                    parent = rootMeshTerrains.transform
                }
            };

            // Create LODs for sub tiles
            for (int terrainTileIndex = 0; terrainTileIndex < terrainSubTileList.Count; terrainTileIndex++)
            {
                GameObject subMeshTerrainRoot = new GameObject(terrain.name + " SubTile" + terrainTileIndex)
                {
                    transform =
                    {
                        position = new Vector3(0, 0, 0),
                        parent = meshTerrainRoot.transform
                    }
                };

                // Store a list of newly created LODed GameObjects
                List<GameObject> lodGameObjects = new List<GameObject>();

                // Get List of LOD Settings
                List<GTSExportTerrainLODSettings> lodSettings = exportSettings.m_exportTerrainLODSettingsSourceTerrains;

                Mesh previousMesh = terrainSubTileList[terrainTileIndex];

                // Create LODs for this sub tile
                for (int lodIndex = 0; lodIndex < meshSettings.lodCount; lodIndex++)
                {
                    GameObject subMeshTerrainLOD =
                        new GameObject(terrain.name + " SubTile" + terrainTileIndex + "_LOD_" + lodIndex)
                        {
                            transform =
                            {
                                position = new Vector3(0, 0, 0),
                                parent = subMeshTerrainRoot.transform
                            }
                        };

                    // Simplify mesh
                    MeshSimplifier meshSimplifier = new MeshSimplifier();
                    lodSettings[lodIndex].m_simplifyQuality = meshSettings.lodQuality[lodIndex];
                    meshSimplifier.SimplificationOptions = lodSettings[lodIndex].m_simplificationOptions;
                    meshSimplifier.Initialize(previousMesh);
                    meshSimplifier.SimplifyMesh(simplyQuality[lodIndex] / 100f);
                    Mesh lodMesh = meshSimplifier.ToMesh();
                    lodMesh.name = terrain.name + "_SubTile_" + terrainTileIndex + lodIndex;

                    meshesToSave.Add(new MeshToSave
                    {
                        path = $"{meshFolderPath}/{terrain.name + "_SubTile_" + terrainTileIndex + lodIndex}.mesh",
                        mesh = lodMesh
                    });

                    // Assign a mesh filter
                    MeshFilter lodMeshFilter = subMeshTerrainLOD.GetComponent<MeshFilter>();
                    if (lodMeshFilter == null)
                    {
                        lodMeshFilter = subMeshTerrainLOD.AddComponent<MeshFilter>();
                    }

                    lodMeshFilter.sharedMesh = lodMesh;

                    // Assign a mesh renderer
                    MeshRenderer lodMeshRenderer = subMeshTerrainLOD.GetComponent<MeshRenderer>();
                    if (lodMeshRenderer == null)
                    {
                        lodMeshRenderer = subMeshTerrainLOD.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                    }

                    if (lodMeshRenderer != null)
                        lodMeshRenderer.sharedMaterial = terrain.materialTemplate;

                    // Add this lod mesh to the exported list
                    lodGameObjects.Add(subMeshTerrainLOD);

                    // Add mesh support for this object
                    GTSMesh gtsMesh = subMeshTerrainLOD.AddComponent<GTSMesh>();
                    gtsMesh.profile = this;
                    gtsMesh.material = terrain.materialTemplate;
                    gtsMesh.colormap = terrain.GetComponent<GTSTerrain>().colormap;

                    // Make sure we simplify the previous lod for the next loop
                    previousMesh = lodMesh;
                }

                // Create a new lod group
                LODGroup lodGroup = subMeshTerrainRoot.GetComponent<LODGroup>();
                if (lodGroup == null)
                {
                    lodGroup = subMeshTerrainRoot.AddComponent<LODGroup>();
                }

                float[] lodPercentages = { 0.95f, 0.7f, 0.6f, 0.02f };

                // Setup each lod within the group
                LOD[] lodArray = new LOD[meshSettings.lodCount];
                for (int lodIndex = 0; lodIndex < meshSettings.lodCount; lodIndex++)
                {
                    lodArray[lodIndex] = new LOD
                    {
                        renderers = lodGameObjects[lodIndex].GetComponentsInChildren<Renderer>()
                    };
                    float lodScreenSize = lodIndex == meshSettings.lodCount - 1 ? 0.02f : lodPercentages[lodIndex];
                    lodArray[lodIndex].screenRelativeTransitionHeight = lodScreenSize;
                }

                // Add lod settings to the lod group
                lodGroup.SetLODs(lodArray);

                // Set position of terrain parent
                Vector3 terrainPosition = terrain.GetPosition();
                subMeshTerrainRoot.transform.position = new Vector3(terrainPosition.x, 0, terrainPosition.z);

                terrain.drawHeightmap = false;
            }
#endif
        }

        public void ApplyProfile()
        {
            StartProfile();
            bool canProceed = true;
            if (exceededMaxTerrainLayers)
            {
#if UNITY_EDITOR
                if (!EditorUtility.DisplayDialog("Exceeded supported number of terrain layers",
                        "The number of max terrain layers has been exceeded. Please ensure only 8 are present. Visual artifacts will appear with more than 8 terrain layers. Proceed?",
                        "Yes", "No"))
                {
                    canProceed = false;
                }
#endif
            }


            if (canProceed)
            {
                if (IsGaiaTerrainLoadedScene())
                {
#if GAIA_PRO_PRESENT
                    Action<Terrain> act = t =>
                    {
                        GameObject gameObject = t.gameObject;
                        //GTSComponent gtsComponent = gameObject.GetComponent<GTSComponent>();
                        GTSTerrain gtsTerrain = gameObject.GetComponent<GTSTerrain>();
                        if (gtsTerrain != null)
                            if (gtsTerrain.profile == this)
                            {
                                gtsTerrain.ApplyProfile();
                                gtsTerrain.SaveAllTextures();
                            }
                                
                        
                    };
                    GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true);
#endif
                }
                else
                {
                    List<GTSComponent> gtsComponents = GetGTSComponents();
                    foreach (GTSComponent component in gtsComponents)
                    {
                        component.ApplyProfile();
                    }
                }

                UpdateProfile();
            }
        }

        public void RemoveProfile()
        {
            StopProfile();
            List<GTSComponent> gtsComponents = GetGTSComponents();
            foreach (GTSComponent component in gtsComponents)
            {
                component.RemoveProfile();
            }

            UpdateProfile();
        }

        public void UpdateProfile()
        {
            List<GTSComponent> gtsComponents = GetGTSComponents();
            foreach (GTSComponent component in gtsComponents)
            {
                component.UpdateProfile();
            }
        }

        public void StartProfile()
        {
            List<GTSComponent> gtsComponents = GetGTSComponents();
            foreach (GTSComponent component in gtsComponents)
            {
                component.StartProfile(this);
            }
        }

        public void StopProfile()
        {
            List<GTSComponent> gtsComponents = GetGTSComponents();
            foreach (GTSComponent component in gtsComponents)
            {
                component.StopProfile(this);
            }
        }
        public void RegenerateData()
        {
            List<GTSComponent> gtsComponents = GetGTSComponents();
            foreach (GTSComponent component in gtsComponents)
            {
                component.RegenerateData();
            }
        }

        public void OnValidate()
        {
            UpdateProfile();
#if UNITY_EDITOR
            isLowPoly = name.Contains("Low Poly");
#endif
        }

        public void Reset()
        {
#if UNITY_EDITOR
            detailSettings.isLowPoly = isLowPoly;
            geoSettings.isLowPoly = isLowPoly;
            heightSettings.isLowPoly = isLowPoly;
            snowSettings.isLowPoly = isLowPoly;
            rainSettings.isLowPoly = isLowPoly;
            tessellationSettings.isLowPoly = isLowPoly;
            variationSettings.isLowPoly = isLowPoly;
            textureArraySettings.isLowPoly = isLowPoly;
#endif
            detailSettings.Reset();
            geoSettings.Reset();
            heightSettings.Reset();
            snowSettings.Reset();
            rainSettings.Reset();
            tessellationSettings.Reset();
            variationSettings.Reset();
            textureArraySettings.Reset();
            // Clears & Refreshes the Terrain Layers
            ClearTerrainLayers();
            Terrain[] terrains = Terrain.activeTerrains;
            if (terrains.Length > 0)
            {
                RefreshTerrainLayers(terrains);
            }
#if UNITY_EDITOR
            if (AssetDatabase.Contains(this))
            {
                string assetPath = AssetDatabase.GetAssetPath(this);
                name = Path.GetFileNameWithoutExtension(assetPath);
                string albedoArrayPath = GetAssetPath(ALBEDO_ARRAY_NAME);
                string normalArrayPath = GetAssetPath(NORMAL_ARRAY_NAME);
                if (!string.IsNullOrEmpty(albedoArrayPath))
                    textureArraySettings.albedoArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(albedoArrayPath);
                if (!string.IsNullOrEmpty(normalArrayPath))
                    textureArraySettings.normalArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(normalArrayPath);
                EditorUtility.SetDirty(this);
            }
#endif
            UpdateProfile();
        }
#if UNITY_EDITOR
        public string GetGTSDataFolder()
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            string directoryName = Path.GetDirectoryName(assetPath);
            return $"{directoryName}/{name}_GTSData";
        }

        public string GetAssetPath(string assetName)
        {
            string folderPath = GetGTSDataFolder();
            return $"{folderPath}/{assetName}.asset";
        }

        public string GetOrCreateAssetPath(string assetName)
        {
            string folderPath = GetGTSDataFolder();
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return $"{folderPath}/{assetName}.asset";
        }
#endif
        public GTSTerrain[] AddGTSToTerrains()
        {
#if GAIA_PRO_PRESENT
            if (IsGaiaTerrainLoadedScene())
            {
                List<GTSTerrain> gtsTerrains = new List<GTSTerrain>();
                Action<Terrain> act = t =>
                {
                    GTSTerrain gtsTerrain = AddGTSToTerrain(t);
                    gtsTerrains.Add(gtsTerrain);
                };
                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true);
                return gtsTerrains.ToArray();
            }
#endif
            return AddGTSToTerrains(Terrain.activeTerrains);
        }

        public GTSTerrain[] AddGTSToTerrains(Terrain[] terrains)
        {
            GTSTerrain[] gtsTerrains = new GTSTerrain[terrains.Length];
            for (int i = 0; i < terrains.Length; i++)
                gtsTerrains[i] = AddGTSToTerrain(terrains[i]);
            return gtsTerrains;
        }

        public GTSTerrain AddGTSToTerrain(Terrain terrain)
        {
            GTSTerrain gtsTerrain = terrain.GetComponent<GTSTerrain>();
#if UNITY_EDITOR
            if (gtsTerrain == null)
            {
                gtsTerrain = terrain.AddComponent<GTSTerrain>();
                GameObject gameObject = terrain.gameObject;
                Scene scene = gameObject.scene;
                if (scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            gtsTerrain.profile = this;
#endif
            return gtsTerrain;
        }

        #region RemoveGTSFromTerrain

        public void RemoveGTSFromTerrains() => RemoveGTSFromTerrains(Terrain.activeTerrains);

        public void RemoveGTSFromTerrains(Terrain[] terrains)
        {
            foreach (Terrain terrain in terrains)
                RemoveGTSFromTerrain(terrain);
        }

        public void RemoveGTSFromTerrain(Terrain terrain)
        {
            GTSTerrain gtsTerrain = terrain.GetComponent<GTSTerrain>();
            if (gtsTerrain != null)
                GTSEvents.Destroy(gtsTerrain);
        }

        #endregion

        public void ClearTerrainLayers()
        {
            gtsLayers.Clear();
            UpdateProfile();
        }

        public void RevertToUnityTerrain()
        {
        }

        public void RefreshTerrainLayers(Terrain[] terrains, bool updateExisting = false)
        {
            foreach (Terrain terrain in terrains)
            {
                GTSTerrain gtsTerrain = terrain.GetComponent<GTSTerrain>();
                if (gtsTerrain == null)
                    continue;
                RefreshTerrainLayers(gtsTerrain, updateExisting);
            }

            UpdateProfile();
        }

        public void RefreshTerrainLayers(GTSTerrain[] gtsTerrains, bool updateExisting = false)
        {
            foreach (GTSTerrain gtsTerrain in gtsTerrains)
                RefreshTerrainLayers(gtsTerrain, updateExisting);
        }

        public void RefreshTerrainLayers(GTSTerrain gtsTerrain, bool updateExisting = false)
        {
            TerrainData terrainData = gtsTerrain.terrainData;
            if (terrainData != null)
            {
                TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                // TODO : The system only currently supports 8 terrain layers.
                if (terrainLayers.Length > 8)
                {
                    exceededMaxTerrainLayers = true;
                }
                else
                {
                    exceededMaxTerrainLayers = false;
                }

                if (gtsLayers.Count > terrainLayers.Length)
                {
                    int difference = gtsLayers.Count - terrainLayers.Length;
                    for (int i = 0; i < difference; i++)
                    {
                        gtsLayers.RemoveAt(gtsLayers.Count - 1);
                    }
                }

                if (gtsLayers.Count != terrainLayers.Length)
                {
                    refreshTexturesArrays = true;
                }

                int length = Mathf.Min(terrainLayers.Length, 8);
                for (int i = 0; i < length; i++)
                {
                    TerrainLayer terrainLayer = terrainLayers[i];

                    if (gtsLayers.Count != 0)
                    {
                        if (i < gtsLayers.Count)
                        {
                            //Check if we should single smoothness value
                            bool useSmoothnessValue = false;

                            if (gtsLayers[i] != null)
                            {
                                if (terrainLayer.diffuseTexture != null)
                                {
                                    //Update texture if it has been swapped
                                    if (gtsLayers[i].albedoMapTexture != null)
                                    {
                                        if (gtsLayers[i].albedoMapTexture.name != terrainLayer.diffuseTexture.name)
                                        {
                                            gtsLayers[i].albedoMapTexture = terrainLayer.diffuseTexture;
                                            gtsLayers[i].name = terrainLayer.diffuseTexture.name;
                                            refreshTexturesArrays = true;
                                        }
                                    }
                                    //Set new albedo map texture if before it was null
                                    else
                                    {
                                        gtsLayers[i].albedoMapTexture = terrainLayer.diffuseTexture;
                                        refreshTexturesArrays = true;
                                    }

                                }
                                //If terrain layer diffuse map texture is null
                                else
                                {
                                    //Set gts layer albedo map texture to null as well
                                    if (gtsLayers[i].albedoMapTexture != null)
                                    {
                                        gtsLayers[i].albedoMapTexture = null;
                                        refreshTexturesArrays = true;
                                    }
                                }

                                if (terrainLayer.normalMapTexture != null)
                                {

                                    //Update texture if it has been swapped
                                    if (gtsLayers[i].normalMapTexture != null)
                                    {
                                        if (gtsLayers[i].normalMapTexture.name != terrainLayer.normalMapTexture.name)
                                        {
                                            gtsLayers[i].normalMapTexture = terrainLayer.normalMapTexture;
                                            refreshTexturesArrays = true;
                                        }
                                    }

                                    //Set new normal map texture if before it was null
                                    else
                                    {
                                        gtsLayers[i].normalMapTexture = terrainLayer.normalMapTexture;
                                        refreshTexturesArrays = true;
                                    }

                                }
                                //If terrain layer normal map texture is null
                                else
                                {
                                    //Set gts layer normal map texture to null as well
                                    if (gtsLayers[i].normalMapTexture != null)
                                    {
                                        gtsLayers[i].normalMapTexture = null;
                                        refreshTexturesArrays = true;
                                    }
                                }

                                if (terrainLayer.maskMapTexture != null)
                                {
                                    if (gtsLayers[i].maskMapTexture != null)
                                    {
                                        //Update texture if it has been swapped
                                        if (gtsLayers[i].maskMapTexture.name != terrainLayer.maskMapTexture.name)
                                        {
                                            gtsLayers[i].maskMapTexture = terrainLayer.maskMapTexture;
                                            refreshTexturesArrays = true;
                                        }
                                    }

                                    //Set new mask map texture if before it was null
                                    else
                                    {
                                        gtsLayers[i].maskMapTexture = terrainLayer.maskMapTexture;
                                        refreshTexturesArrays = true;
                                    }

                                }

                                //If terrain layer mask map texture is null
                                else
                                {
                                    //Set gts layer mask map texture to null as well
                                    if (gtsLayers[i].maskMapTexture != null)
                                    {
                                        gtsLayers[i].maskMapTexture = null;
                                        refreshTexturesArrays = true;
                                    }

                                    //Check for what type of smoothness is being used
                                    if (terrainLayer.diffuseTexture != null)
                                    {
#if UNITY_EDITOR
                                        string albedoMapPath = AssetDatabase.GetAssetPath(terrainLayer.diffuseTexture);
                                        TextureImporter importer =
                                            AssetImporter.GetAtPath(albedoMapPath) as TextureImporter;
                                        useSmoothnessValue = !importer.DoesSourceTextureHaveAlpha();
#endif
                                    }

                                }
                            }

                            if (updateExisting)
                            {
                                if (gtsLayers[i] != null)
                                {
                                    

                                    if (gtsLayers[i].maskMapRemapMin != terrainLayer.maskMapRemapMin)
                                    {
                                        gtsLayers[i].maskMapRemapMin = terrainLayer.maskMapRemapMin;
                                    }

                                    if (gtsLayers[i].maskMapRemapMax != terrainLayer.maskMapRemapMax)
                                    {
                                        gtsLayers[i].maskMapRemapMax = terrainLayer.maskMapRemapMax;
                                    }

                                    if (useSmoothnessValue)
                                    {
                                        gtsLayers[i].maskMapRemapMin.w = terrainLayer.smoothness;
                                        gtsLayers[i].maskMapRemapMax.w = 1f;
                                    }

                                    if (gtsLayers[i].normalStrength != terrainLayer.normalScale)
                                    {
                                        gtsLayers[i].normalStrength = terrainLayer.normalScale;
                                    }

                                    if (gtsLayers[i].tileSize != terrainLayer.tileSize)
                                    {
                                        gtsLayers[i].tileSize = terrainLayer.tileSize;
                                    }

                                    if (gtsLayers[i].tileOffset != terrainLayer.tileOffset)
                                    {
                                        gtsLayers[i].tileOffset = terrainLayer.tileOffset;
                                    }
                                }
                            }
                            
                        }
                    }

                    if (terrainLayer == null)
                        continue;
                    if (i >= gtsLayers.Count)
                        gtsLayers.Add(new GTSTerrainLayer(terrainLayer));
                }
            }
        }

        public void ConvertLayerTexturesToPackedFormat()
        {
            foreach (GTSTerrainLayer layer in gtsLayers)
                layer.ConvertTexturesToPackedFormat();
        }

        public void CreateTextureArrays()
        {
#if UNITY_2022_2_OR_NEWER
            int prevMasterTextureLimit = QualitySettings.globalTextureMipmapLimit;
            QualitySettings.globalTextureMipmapLimit = 0;
#else
            int prevMasterTextureLimit = QualitySettings.masterTextureLimit;
            QualitySettings.masterTextureLimit = 0;
#endif
            StartProfile();
            if (gtsLayers.Count == 0)
            {
                GTSDebug.Log("Texture Array Generation Failed: One or more Terrains do not contain any Layers.");
                return;
            }

            ConvertLayerTexturesToPackedFormat();
            RenderTexture albedoPackedMap = gtsLayers[0].albedoPackedMap;
            int maxTextureSize = (int)textureArraySettings.maxTextureSize;
            int anisoLevel = textureArraySettings.anisoLevel;
            bool compressed = textureArraySettings.compressed;
            GTSCompressionFormat compressionFormat = textureArraySettings.compressionFormat;
            TextureFormat textureFormat = compressed ? (TextureFormat)compressionFormat : TextureFormat.RGBA32;
            // Albedo Array
            Texture2DArray newTextureArray = new Texture2DArray(maxTextureSize, maxTextureSize, gtsLayers.Count,
                textureFormat, true, false);
            newTextureArray.anisoLevel = anisoLevel;
            for (int i = 0; i < gtsLayers.Count; i++)
            {
                albedoPackedMap = gtsLayers[i].albedoPackedMap;
                RenderTexture.active = albedoPackedMap;
                Texture2D tempTex = new Texture2D(albedoPackedMap.width, albedoPackedMap.height,
                    GTSConstants.WorkTextureFormat, true, true);
                tempTex.mipMapBias = textureArraySettings.mipMapBias;
                tempTex.ReadPixels(new Rect(0, 0, albedoPackedMap.width, albedoPackedMap.height), 0, 0);
                tempTex.Apply();
                tempTex = GTSUtility.ResizeTexture(tempTex, textureFormat, textureArraySettings.anisoLevel,
                    maxTextureSize, maxTextureSize, true, true, compressed);
                for (int m = 0; m < tempTex.mipmapCount; m++)
                {
                    Graphics.CopyTexture(tempTex, 0, m, newTextureArray, i, m);
                }

                Object.DestroyImmediate(tempTex);
            }

            textureArraySettings.albedoArray = newTextureArray;
            textureArraySettings.albedoArray.name = ALBEDO_ARRAY_NAME;
            // Normal Array
            RenderTexture normalPackedMap = gtsLayers[0].normalPackedMap;
            newTextureArray =
                new Texture2DArray(maxTextureSize, maxTextureSize, gtsLayers.Count, textureFormat, true, true);
            newTextureArray.anisoLevel = anisoLevel;
            for (int i = 0; i < gtsLayers.Count; i++)
            {
                normalPackedMap = gtsLayers[i].normalPackedMap;
                RenderTexture.active = normalPackedMap;
                Texture2D tempTex = new Texture2D(normalPackedMap.width, normalPackedMap.height,
                    GTSConstants.WorkTextureFormat, true, false);
                tempTex.mipMapBias = textureArraySettings.mipMapBias;
                tempTex.ReadPixels(new Rect(0, 0, normalPackedMap.width, normalPackedMap.height), 0, 0, false);
                tempTex.Apply();
                tempTex = GTSUtility.ResizeTexture(tempTex, textureFormat, textureArraySettings.anisoLevel,
                    maxTextureSize, maxTextureSize, true, false, compressed);
                for (int m = 0; m < tempTex.mipmapCount; m++)
                {
                    Graphics.CopyTexture(tempTex, 0, m, newTextureArray, i, m);
                }

                Object.DestroyImmediate(tempTex);
            }

            textureArraySettings.normalArray = newTextureArray;
            textureArraySettings.normalArray.name = NORMAL_ARRAY_NAME;
#if UNITY_EDITOR

            // try
            // {
            //     AssetDatabase.StartAssetEditing();
            string albedoArrayPath = GetOrCreateAssetPath(ALBEDO_ARRAY_NAME);
            string normalArrayPath = GetOrCreateAssetPath(NORMAL_ARRAY_NAME);
            textureArraySettings.albedoArray =
                GTSUtility.CreateOrReplaceAsset(textureArraySettings.albedoArray, albedoArrayPath);
            textureArraySettings.normalArray =
                GTSUtility.CreateOrReplaceAsset(textureArraySettings.normalArray, normalArrayPath);
            // }
            // finally
            // {
            //     AssetDatabase.StopAssetEditing();
            EditorUtility.SetDirty(this);

            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();

            // textureArraySettings.albedoArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(albedoArrayPath);
            // textureArraySettings.normalArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(normalArrayPath);

            // }
#endif

#if UNITY_2022_2_OR_NEWER
            QualitySettings.globalTextureMipmapLimit = prevMasterTextureLimit;
#else
            QualitySettings.masterTextureLimit = prevMasterTextureLimit;
#endif
            UpdateProfile();
        }

        public void SetRuntimeData()
        {
            if (runtime == null)
            {
                runtime = ScriptableObject.CreateInstance<GTSRuntime>();
                runtime.name = name + "_Runtime";
            }
            Vector3 worldSize = Vector3.zero;

            Terrain[] terrains = GTSUtility.FindObjectsByType<Terrain>(true);
            if (terrains != null)
            {
                if (terrains.Length > 0 && terrains[0] != null)
                {
                    worldSize = terrains[0].terrainData.size;
                }
            }

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

            for (int layerIndex = 0; layerIndex < gtsLayers.Count; layerIndex++)
            {
                GTSTerrainLayer gtsLayer = gtsLayers[layerIndex];
                // x: height contrast, y: height brightness, z: height increase, w: 0
                Vector4 heightData = new Vector4(gtsLayer.heightContrast, gtsLayer.heightBrightness,
                    gtsLayer.heightIncrease, 0);
                // x: displacement contrast, y: displacement brightness, z: displacement increase, w: 0
                Vector4 displacmentData = new Vector4(gtsLayer.displacementContrast, gtsLayer.displacementBrightness,
                    gtsLayer.displacementIncrease, gtsLayer.tessellationAmount);
                // x: tiling x, y: tiling y, z: offset x, w: offset y
                Vector4 tilingOffset = new Vector4(
                    worldSize.x / gtsLayer.tileSize.x,
                    worldSize.z / gtsLayer.tileSize.y,
                    gtsLayer.tileOffset.x,
                    gtsLayer.tileOffset.y
                );
                // x: tri planar size x, y: tri planar size y, z: tri planar enabled, w: 0
                Vector4 triPlanarData = new Vector4(gtsLayer.triPlanarSize.x * 10f, gtsLayer.triPlanarSize.y * 10f,
                    gtsLayer.triPlanar == true ? 1 : 0, 0);
                // x: geo amount, y: stochastic, z: normalScale, w: detailNormalAmount
                Vector4 layerDataA = new Vector4(gtsLayer.geoAmount, gtsLayer.stochastic == true ? 1 : 0,
                    gtsLayer.normalStrength, gtsLayer.detailAmount);
                Vector4 maskMapRemapData = new Vector4(gtsLayer.maskMapRemapMin.y, gtsLayer.maskMapRemapMin.w,
                    gtsLayer.maskMapRemapMax.y, gtsLayer.maskMapRemapMax.w);

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

            runtime.HeightDataArray = HeightDataArray;
            runtime.UVDataArray = UVDataArray;
            runtime.MaskMapRemapMinArray = MaskMapRemapMinArray;
            runtime.MaskMapRemapMaxArray = MaskMapRemapMaxArray;
            runtime.MaskMapRemapArray = MaskMapRemapArray;
            runtime.ColorArray = ColorArray;
            runtime.TriPlanarDataArray = TriPlanarDataArray;
            runtime.DisplacementDataArray = DisplacementDataArray;
            runtime.LayerDataAArray = LayerDataAArray;


            // Snow Settings
            Vector4 snowDataA = new Vector4(snowSettings.enabled ? snowSettings.power : 0, snowSettings.power, snowSettings.minHeight, snowSettings.age);
            Vector4 snowDataB = new Vector4(snowSettings.scale, snowSettings.blendRange, snowSettings.slopeBlend, snowSettings.normalStrength);
            Vector4 snowDisplacementData = new Vector4(snowSettings.displacementContrast, snowSettings.displacementBrightness, snowSettings.displacementIncrease, snowSettings.tessellationAmount);
            Vector4 snowHeightData = new Vector4(snowSettings.heightContrast, snowSettings.heightBrightness, snowSettings.heightIncrease, 0);

            runtime.SnowDataA = snowDataA;
            runtime.SnowDataB = snowDataB;
            runtime.SnowDisplacementData = snowDisplacementData;
            runtime.SnowHeightData = snowHeightData;
            runtime.SnowColor = snowSettings.color;
            runtime.SnowMaskRemapMin = snowSettings.maskRemapMin;
            runtime.SnowMaskRemapMax = snowSettings.maskRemapMax;

            runtime.SnowAlbedoMap = snowSettings.albedoTexture;
            runtime.SnowNormalMap = snowSettings.normalTexture;
            runtime.SnowMaskMap = snowSettings.maskTexture;

            runtime.GlobalSnowIntensity = snowSettings.enabled ? snowSettings.power : 0;
            runtime.GlobalCoverLayer1FadeStart = snowSettings.minHeight;
            runtime.GlobalCoverLayer1FadeDist = snowSettings.blendRange;

            //Rain Settings
            Vector4 rainDataA = new Vector4(Mathf.Min(rainSettings.Power, 0.9f), Mathf.Min(rainSettings.Power, 0.9f), rainSettings.MinHeight, rainSettings.MaxHeight);
            Vector4 rainDataB = new Vector4(rainSettings.Speed, 1 - rainSettings.Darkness, rainSettings.Smoothness, rainSettings.Scale);

            runtime.RainDataA = rainDataA;
            runtime.RainDataB = rainDataB;
            runtime.RainDataTexture = rainSettings.rainDataTexture;

#if UNITY_EDITOR
            string runtimePath = GetOrCreateAssetPath(name + "_Runtime");
            GTSUtility.CreateOrReplaceAsset(runtime, runtimePath);
#endif
        }





    }


}
