using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace ProceduralWorlds.GTS
{
    public class GTSPreprocessBuild : IProcessSceneWithReport
    {
        public int callbackOrder => int.MaxValue;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null)
                return;
            if (scene.isSubScene)
                return;
            Terrain[] terrains = GTSUtility.FindObjectsByType<Terrain>();
            foreach (Terrain terrain in terrains)
            {
                GTSTerrain gtsTerrain = terrain.GetComponent<GTSTerrain>();
                if (gtsTerrain == null)
                    continue;
                Material terrainMaterial = terrain.materialTemplate;
                if (terrainMaterial.IsGTSMaterial())
                {
                    // Make copy of Terrain Data without Layers
                    TerrainData terrainData = terrain.terrainData;
                    if (terrainData == null)
                        continue;
                    TerrainData copyData = Object.Instantiate(terrainData);
                    copyData.name = terrainData.name;
                    // Clear layers on Terrain Data
                    copyData.terrainLayers = null;
                    terrain.terrainData = copyData;
                    // If there's a Terrain Collider set it to the copy data
                    TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
                    if (terrainCollider != null)
                        terrainCollider.terrainData = copyData;
                    gtsTerrain.Refresh();
                }
                else
                {
                    // Remove reference to profile
                    gtsTerrain.profile = null;
                    // Object.DestroyImmediate(gtsTerrain);
                }
            }
        }
    }
}