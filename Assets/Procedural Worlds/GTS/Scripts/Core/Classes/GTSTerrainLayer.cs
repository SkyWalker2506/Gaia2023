using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSTerrainLayer
    {
        public string name = "No Name";
        public Texture2D albedoMapTexture;
        public Texture2D normalMapTexture;
        public Texture2D maskMapTexture;
        public Color tint;
        public Vector2 tileSize;
        public Vector2 tileOffset;
        public float normalStrength;
        public Vector4 maskMapRemapMin;
        public Vector4 maskMapRemapMax;
        public float detailAmount = 1;
        public float geoAmount = 1f;
        public float heightContrast;
        public float heightBrightness;
        public float heightIncrease;
        public float displacementContrast;
        public float displacementBrightness;
        public float displacementIncrease;
        public float tessellationAmount;
        public bool triPlanar;
        public Vector2 triPlanarSize;
        public bool stochastic;
        public RenderTexture albedoPackedMap;
        public RenderTexture normalPackedMap;
        public GTSTerrainLayer()
        {
        }
        public GTSTerrainLayer(TerrainLayer terrainLayer)
        {
            ConvertFrom(terrainLayer);
        }
        /// <summary>
        /// Converts TerrainLayer settings to the GTSTerrainLayer.
        /// </summary>
        /// <param name="terrainLayer"></param>
        public void ConvertFrom(TerrainLayer terrainLayer)
        {
            // Shared with Terrain Layer
            if (terrainLayer != null)
            {
                normalStrength = terrainLayer.normalScale;
                albedoMapTexture = terrainLayer.diffuseTexture;
                normalMapTexture = terrainLayer.normalMapTexture;
                maskMapTexture = terrainLayer.maskMapTexture;
                normalStrength = terrainLayer.normalScale;
                tileSize = terrainLayer.tileSize;
                tileOffset = terrainLayer.tileOffset;
                maskMapRemapMin = terrainLayer.maskMapRemapMin;
                maskMapRemapMax = terrainLayer.maskMapRemapMax;

                string title = "";
                if (albedoMapTexture != null)
                {
                    title = albedoMapTexture.name;
                }
                else
                {
                    if (normalMapTexture != null)
                        title = normalMapTexture.name;
                    else if (maskMapTexture != null)
                        title = maskMapTexture.name;
                }
                name = string.IsNullOrEmpty(title) ? terrainLayer.name : title;
            }
            else
            {
                normalStrength = 1;
                tileSize = new Vector2(8, 8);
                maskMapRemapMin = new Vector4(0, 0, 0, 0);
                maskMapRemapMax = new Vector4(1, 1, 1, 1);
            }
            // Specific to GTS
            tint = new Color(1, 1, 1, 1);
            heightBrightness = 1;
            heightContrast = 1;
            heightIncrease = 0;
            displacementContrast = 0.1f;
            displacementBrightness = 0.1f;
            displacementIncrease = 0;
            tessellationAmount = 25;
            triPlanar = false;
            triPlanarSize = new Vector2(1f, 1f);
        }
        #region Misc
        /// <summary>
        /// Applies the current GTSTerrainLayer settings to the given TerrainLayer object. 
        /// </summary>
        /// <param name="terrainLayer"></param>
        public void ApplyToTerrainLayer(TerrainLayer terrainLayer)
        {
            // If there is no terrain layer.
            if (terrainLayer == null)
                return; // Exit early.
            // Shared with Terrain Layer
            terrainLayer.name = name;
            terrainLayer.normalScale = normalStrength;
            terrainLayer.diffuseTexture = albedoMapTexture;
            terrainLayer.normalMapTexture = normalMapTexture;
            terrainLayer.maskMapTexture = maskMapTexture;
            terrainLayer.normalScale = normalStrength;
            terrainLayer.tileSize = tileSize;
            terrainLayer.tileOffset = tileOffset;
            terrainLayer.maskMapRemapMin = maskMapRemapMin;
            terrainLayer.maskMapRemapMax = maskMapRemapMax;
        }
        #endregion
        public void ConvertTexturesToPackedFormat()
        {
            

            // Setup our post processing converting materials
            Material convertToAlbedoMaterial = GTSUtility.convertToAlbedoMaterial;
            Material convertToNormalMaterial = GTSUtility.convertToNormalMaterial;
            Material createAlbedoMapMaterial = GTSUtility.createAlbedoMapMaterial;
            Material createNormalMapMaterial = GTSUtility.createNormalMapMaterial;
            int defaultSize = 2048;
            RenderTexture albedoRef;
            if (albedoMapTexture == null)
            {
                albedoRef = new RenderTexture(defaultSize, defaultSize, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                albedoRef.Create();
                Graphics.Blit(null, albedoRef, createAlbedoMapMaterial);
            }
            else
            {
                albedoRef = new RenderTexture(albedoMapTexture.width, albedoMapTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                albedoRef.Create();
                Graphics.Blit(albedoMapTexture, albedoRef);
            }


            // Create Temp Mask Map Texture
            RenderTexture convertedMaskMap = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);

            // No Mask Map provided
            if (maskMapTexture == null)
            {
                Material createMaskMap = GTSUtility.convertToMaskMapMaterial;

                // Does Albedo Have Smoothness in Alpha?
                bool hasAlpha = false;
                if (albedoMapTexture != null)
                {
#if UNITY_EDITOR
                    string albedoMapPath = AssetDatabase.GetAssetPath(albedoMapTexture);
                    TextureImporter importer = AssetImporter.GetAtPath(albedoMapPath) as TextureImporter;
                    hasAlpha = importer.DoesSourceTextureHaveAlpha();
                    createMaskMap.SetTexture("_Albedo", albedoMapTexture);
#endif
                }
                else
                {
                    createMaskMap.SetTexture("_Albedo", albedoRef);
                }
                createMaskMap.SetInt("_HasAlbedo", hasAlpha ? 1 : 0);

                Graphics.Blit(null, convertedMaskMap, createMaskMap);
            }
            else
            {
                Graphics.Blit(maskMapTexture, convertedMaskMap);
            }


            RenderTexture convertedAlbedo = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            convertedAlbedo.name = "convertedAlbedo";
            convertedAlbedo.enableRandomWrite = true;
            convertedAlbedo.useMipMap = true;
            convertedAlbedo.Create();
            convertToAlbedoMaterial.SetTexture("_Albedo", albedoRef);
            convertToAlbedoMaterial.SetTexture("_MaskMap", convertedMaskMap);
            Graphics.Blit(albedoRef, convertedAlbedo, convertToAlbedoMaterial);
            albedoPackedMap = convertedAlbedo;
            RenderTexture normalRef;
            if (normalMapTexture == null)
            {
                normalRef = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default);
                normalRef.Create();
                Graphics.Blit(null, normalRef, createNormalMapMaterial);
            }
            else
            {
                normalRef = new RenderTexture(normalMapTexture.width, normalMapTexture.width, 0, RenderTextureFormat.Default);
                normalRef.Create();
                Graphics.Blit(normalMapTexture, normalRef);
            }
            RenderTexture convertedNormal = new RenderTexture(normalRef.width, normalRef.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            convertedAlbedo.name = "convertedNormal";
            convertedNormal.enableRandomWrite = true;
            convertedNormal.useMipMap = true;
            convertedNormal.Create();
            convertToNormalMaterial.SetTexture("_Normal", normalRef);
            convertToNormalMaterial.SetTexture("_MaskMap", convertedMaskMap);
            Graphics.Blit(normalRef, convertedNormal, convertToNormalMaterial);
            normalPackedMap = convertedNormal;
        }
    }
}