using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using System.Text;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    public static class GTSUtility
    {
        #region Shaders & Materials
        // Setup our post processing converting materials
        private static Shader m_convertToAlbedoShader;
        public static Shader convertToAlbedoShader
        {
            get
            {
                if (m_convertToAlbedoShader == null)
                    m_convertToAlbedoShader = Shader.Find("Hidden/GTS_CombineAlbedoMetallic");
                return m_convertToAlbedoShader;
            }
        }
        private static Shader m_convertToNormalShader;
        public static Shader convertToNormalShader
        {
            get
            {
                if (m_convertToNormalShader == null)
                    m_convertToNormalShader = Shader.Find("Hidden/GTS_CombineNormalMask");
                return m_convertToNormalShader;
            }
        }
        private static Shader m_convertMaskMapShader;
        public static Shader convertToMaskMapShader
        {
            get
            {
                if (m_convertMaskMapShader == null)
                    m_convertMaskMapShader = Shader.Find("Hidden/GTS_ConvertMaskMap");
                return m_convertMaskMapShader;
            }
        }
        private static Shader m_createNormalMapShader;
        public static Shader createNormalMapShader
        {
            get
            {
                if (m_createNormalMapShader == null)
                    m_createNormalMapShader = Shader.Find("Hidden/GTS_CreateBaseNormal");
                return m_createNormalMapShader;
            }
        }
        private static Shader m_createAlbedoMapShader;
        public static Shader createAlbedoMapShader
        {
            get
            {
                if (m_createAlbedoMapShader == null)
                    m_createAlbedoMapShader = Shader.Find("Hidden/GTS_CreateBaseColor");
                return m_createAlbedoMapShader;
            }
        }
        private static Shader m_createBakedAlbedoMapShader;
        public static Shader createBakedAlbedoMapShader
        {
            get
            {
                if (m_createBakedAlbedoMapShader == null)
                    m_createBakedAlbedoMapShader = Shader.Find("Hidden/GTS_CreateBakedAlbedo");
                return m_createBakedAlbedoMapShader;
            }
        }
        private static Shader m_createWeightedIndexMapShader;
        public static Shader createWeightedIndexMapShader
        {
            get
            {
                if (m_createWeightedIndexMapShader == null)
                    m_createWeightedIndexMapShader = Shader.Find("Hidden/GTS_WeightedIndex");
                return m_createWeightedIndexMapShader;
            }
        }
        private static Shader m_createWorldNormalMapShader;
        public static Shader createWorldNormalMapShader
        {
            get
            {
                if (m_createWorldNormalMapShader == null)
                    m_createWorldNormalMapShader = Shader.Find("Hidden/GTS_TerrainWorldNormalCurvature");
                return m_createWorldNormalMapShader;
            }
        }
        private static Material m_convertToAlbedoMaterial;
        public static Material convertToAlbedoMaterial
        {
            get
            {
                if (m_convertToAlbedoMaterial == null)
                    m_convertToAlbedoMaterial = new Material(convertToAlbedoShader);
                return m_convertToAlbedoMaterial;
            }
        }
        private static Material m_convertToNormalMaterial;
        public static Material convertToNormalMaterial
        {
            get
            {
                if (m_convertToNormalMaterial == null)
                    m_convertToNormalMaterial = new Material(convertToNormalShader);
                return m_convertToNormalMaterial;
            }
        }
        private static Material m_convertToMaskMapMaterial;
        public static Material convertToMaskMapMaterial
        {
            get
            {
                if (m_convertToMaskMapMaterial == null)
                    m_convertToMaskMapMaterial = new Material(convertToMaskMapShader);
                return m_convertToMaskMapMaterial;
            }
        }
        private static Material m_createNormalMapMaterial;
        public static Material createNormalMapMaterial
        {
            get
            {
                if (m_createNormalMapMaterial == null)
                    m_createNormalMapMaterial = new Material(createNormalMapShader);
                return m_createNormalMapMaterial;
            }
        }
        private static Material m_createAlbedoMapMaterial;
        public static Material createAlbedoMapMaterial
        {
            get
            {
                if (m_createAlbedoMapMaterial == null)
                    m_createAlbedoMapMaterial = new Material(createAlbedoMapShader);
                return m_createAlbedoMapMaterial;
            }
        }
        private static Material m_createBakedAlbedoMapMaterial;
        public static Material createBakedAlbedoMapMaterial
        {
            get
            {
                if (m_createBakedAlbedoMapMaterial == null)
                    m_createBakedAlbedoMapMaterial = new Material(createBakedAlbedoMapShader);
                return m_createBakedAlbedoMapMaterial;
            }
        }
        private static Material m_createWorldNormalMapMaterial;
        public static Material createWorldNormalMapMaterial
        {
            get
            {
                if (m_createWorldNormalMapMaterial == null)
                    m_createWorldNormalMapMaterial = new Material(createWorldNormalMapShader);
                return m_createWorldNormalMapMaterial;
            }
        }
        private static Material m_createWeightedIndexMapMaterial;
        public static Material createWeightedIndexMapMaterial
        {
            get
            {
                if (m_createWeightedIndexMapMaterial == null)
                    m_createWeightedIndexMapMaterial = new Material(createWeightedIndexMapShader);
                return m_createWeightedIndexMapMaterial;
            }
        }
#if UNITY_EDITOR
        private static GTSProfile m_defaultProfile;
        public static GTSProfile defaultProfile
        {
            get
            {
                if (m_defaultProfile == null)
                    m_defaultProfile = AssetDatabase.LoadAssetAtPath<GTSProfile>(GTSConstants.GTSDefaultProfilePath);
                return m_defaultProfile;
            }
        }
        private static GTSDefaults m_defaultSettings;
        public static GTSDefaults defaults
        {
            get
            {
                if (m_defaultSettings == null)
                    m_defaultSettings = AssetDatabase.LoadAssetAtPath<GTSDefaults>(GTSConstants.GTSDefaultsPath);
                return m_defaultSettings;
            }
        }
#endif
        #endregion
        #region Material Pipeline Checks
        public static GTSPipeline GetCurrentPipeline()
        {
            RenderPipelineAsset pipelineAsset = GraphicsSettings.renderPipelineAsset;
            if (pipelineAsset == null)
                return GTSPipeline.BuiltIn;
            Type rpType = pipelineAsset.GetType();
            if (rpType.FullName.Contains("HighDefinition"))
                return GTSPipeline.HDRP;
            return GTSPipeline.URP;
        }
        public static void WriteTexture2D(RenderTexture renderTexture, string path)
        {
            Texture2D texture2D = renderTexture.ToTexture2D();
            if (texture2D != null)
            {
                byte[] fileData = texture2D.EncodeToPNG();
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(fileData, 0, fileData.Length);
                }
#if UNITY_EDITOR
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
#endif
            }
        }
        public static void WriteTexture2D(Texture2D replacementTexture2D, string path)
        {
            if (replacementTexture2D == null)
                return;
            RenderTexture tempRT = new RenderTexture(replacementTexture2D.width, replacementTexture2D.height, 0);
            tempRT.useMipMap = true;
            tempRT.autoGenerateMips = true;
            tempRT.Create();
            Graphics.Blit(replacementTexture2D, tempRT);
            WriteTexture2D(tempRT, path);
            tempRT.Release();
        }
        /// <summary>
        /// Gets the appropriate Shaders for the current pipeline.
        /// Note: Gets this from the GTS-Pipeline-Material and Default-Pipeline-Material.
        /// </summary>
        /// <param name="gtsShader">Output GTS Shader</param>
        /// <param name="unityShader">Output Unity Shader</param>
        /// <returns>Returns true if both shaders are valid.</returns>
        public static bool GetPipelineShaders(out Shader gtsShader, out Shader unityShader)
        {
            gtsShader = null;
            unityShader = null;
            switch (GetCurrentPipeline())
            {
                case GTSPipeline.BuiltIn:
                    gtsShader = Shader.Find("PW/GTS_BuiltIn");
                    unityShader = Shader.Find("Nature/Terrain/Standard");
                    break;
                case GTSPipeline.URP:
                    gtsShader = Shader.Find("PW/GTS_URP_Compiled");
                    unityShader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                    break;
                case GTSPipeline.HDRP:
                    gtsShader = Shader.Find("PW/GTS_HDRP_Compiled");
                    unityShader = Shader.Find("HDRP/TerrainLit");
                    break;
                default:
                    return false;
            }
            return gtsShader != null && unityShader != null;
        }
        #endregion
        #region Texture Utilities
        /// <summary>
        /// Outputs a Texture2D as a .png file for debug purposes
        /// </summary>
        /// <param name="path">The path to write to, including filename ending in .png</param>
        /// <param name="sourceTexture">The texture to export</param>
        public static void WriteTexture2D(string path, Texture2D sourceTexture)
        {
            byte[] exrBytes = ImageConversion.EncodeToPNG(sourceTexture);
            if (exrBytes != null)
            {
                PWCommon5.Utils.WriteAllBytes(path, exrBytes);
            }
        }
        public static Texture2D Copy(Texture2D src)
        {
            RenderTexture renderTexture = src.ToRenderTexture();
            Texture2D copyTexture = renderTexture.ToTexture2D();
            renderTexture.Release();
            return copyTexture;
        }
        public static Texture2D Copy(RenderTexture src, Texture2D dst)
        {
            if (dst != null)
                dst.Reinitialize(src.width, src.height);
            else
                dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32, src.useMipMap);
            RenderTexture.active = src;
            dst.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            dst.Apply();
            RenderTexture.active = null;
            return dst;
        }
        /// <summary>
        /// Resize the supplied texture, also handles non rw textures and makes them rm
        /// </summary>
        /// <param name="texture">Source texture</param>
        /// <param name="format">Texture Format of new Texture</param>
        /// <param name="aniso">Aniso Level of new Texture</param>
        /// <param name="width">Width of new texture</param>
        /// <param name="height">Height of new texture</param>
        /// <param name="mipmap">Generate mipmaps</param>
        /// <param name="linear">Use linear colour conversion</param>
        /// <param name="compress">Compresses Texture using Editor Compression.</param>
        /// <returns>New texture</returns>
        public static Texture2D ResizeTexture(Texture2D texture, TextureFormat format, int aniso, int width, int height, bool mipmap, bool linear, bool compress)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            bool prevRgbConversionState = GL.sRGBWrite;
            GL.sRGBWrite = !linear;
            Graphics.Blit(texture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D newTexture = new Texture2D(width, height, GTSConstants.WorkTextureFormat, mipmap, linear);
            newTexture.name = texture.name + " X";
            newTexture.anisoLevel = aniso;
            newTexture.filterMode = texture.filterMode;
            newTexture.wrapMode = texture.wrapMode;
            newTexture.mipMapBias = texture.mipMapBias;
            newTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            newTexture.Apply(true);
            newTexture = CompressTexture(newTexture, format);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            GL.sRGBWrite = prevRgbConversionState;
            return newTexture;
        }
        /// <summary>
        /// Returns a compressed Texture2D in the desired output format.
        /// </summary>
        /// <param name="texture">The input texture.</param>
        /// <param name="textureFormat">The compression format to apply.</param>
        /// <returns>Texture2D in the desired format.</returns>
        private static Texture2D CompressTexture(Texture2D texture, TextureFormat textureFormat)
        {
#if UNITY_EDITOR
            // Anything other than TextureCompressionQuality.Fast resulted in extreme (>2 hours!) 
            // Texture baking times for certain compression formats. Change only if truly needed.
#if UNITY_2018_3_OR_NEWER
            EditorUtility.CompressTexture(texture, textureFormat, UnityEditor.TextureCompressionQuality.Fast);
#else
            EditorUtility.CompressTexture(texture, textureFormat, TextureCompressionQuality.Fast);
#endif
#else
            // Fallback in case someone creates profiles during runtime
            texture.Compress(true);
#endif
            texture.Apply(true);
            return texture;
        }
        #endregion
#if UNITY_EDITOR
        #region Editor Utilities
        public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                existingAsset = asset;
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
            }
            return existingAsset;
        }
        #endregion
#endif


        public static string ExportToObj(Terrain terrain, float terrainRes, string savePath)
        {
            //string fileName = ".obj";
            Vector3 terrainPos = terrain.transform.position;
            int w = terrain.terrainData.heightmapResolution;
            int h = terrain.terrainData.heightmapResolution;
            Vector3 meshScale = terrain.terrainData.size;
            int tRes = (int)Mathf.Pow(2, (int)terrainRes);
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            float[,] tData = terrain.terrainData.GetHeights(0, 0, w, h);

            w = (w - 1) / tRes + 1;
            h = (h - 1) / tRes + 1;
            Vector3[] tVertices = new Vector3[w * h];

            Vector2[] tUV = new Vector2[w * h];
            int[] tPolys;

            bool exportTris = false;
            Vector3[] tNormals;

            if (exportTris)
            {
                tPolys = new int[(w - 1) * (h - 1) * 6];
                tNormals = new Vector3[tPolys.Length * 6];
            }
            else
            {
                tPolys = new int[(w - 1) * (h - 1) * 4];
                tNormals = new Vector3[tPolys.Length * 4];
            }



            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x));
                    tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                }
            }

            int index = 0;
            if (exportTris)
            {
                // Build triangle indices: 3 indices into vertex array for each triangle
                for (int y = 0; y < h - 1; y++)
                {
                    for (int x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output two triangles
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = (y * w) + x + 1;


                        //calculate normal for this face
                        Vector3 v1 = tVertices[((y + 1) * w) + x] - tVertices[(y * w) + x];
                        Vector3 v2 = tVertices[(y * w) + x + 1] - tVertices[(y * w) + x];
                        Vector3 normal = Vector3.Cross(v1, v2);
                        tNormals[index] = normal;
                        tNormals[index-1] = normal;
                        tNormals[index-2] = normal;

                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;

                        //calculate normal for that face
                        v1 = tVertices[((y + 1) * w) + x + 1] - tVertices[((y + 1) * w) + x];
                        v2 = tVertices[(y * w) + x + 1] - tVertices[((y + 1) * w) + x];
                        normal = Vector3.Cross(v1, v2);
                        tNormals[index] = normal;
                        tNormals[index - 1] = normal;
                        tNormals[index - 2] = normal;
                    }
                }
            }
            else
            {
                // Build quad indices: 4 indices into vertex array for each quad
                for (int y = 0; y < h - 1; y++)
                {
                    for (int x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output one quad
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;

                        tNormals[index] = new Vector3(0, 1, 0);
                        tNormals[index - 1] = new Vector3(0, 1, 0);
                        tNormals[index - 2] = new Vector3(0, 1, 0);
                        tNormals[index - 3] = new Vector3(0, 1, 0);

                    }
                }
            }

            // Export to .obj
            StreamWriter sw = new StreamWriter(savePath);
            try
            {
                sw.WriteLine("# Unity terrain OBJ File");

                // Write vertices
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                float totalCount = (tVertices.Length * 2 + (exportTris ? tPolys.Length / 3 : tPolys.Length / 4)) / 1;
                for (int i = 0; i < tVertices.Length; i++)
                {
                    //UpdateProgress();
                    StringBuilder sb = new StringBuilder("v ", 20);
                    // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                    // Which is important when you're exporting huge terrains.
                    sb.Append(tVertices[i].x.ToString()).Append(" ").
                        Append(tVertices[i].y.ToString()).Append(" ").
                        Append(tVertices[i].z.ToString());
                    sw.WriteLine(sb);
                }
                // Write UVs
                for (int i = 0; i < tUV.Length; i++)
                {
                    //UpdateProgress();
                    StringBuilder sb = new StringBuilder("vt ", 22);
                    sb.Append(tUV[i].y.ToString()).Append(" ").
                        Append(tUV[i].x.ToString());
                    sw.WriteLine(sb);
                }
                // Write Normals
                for (int i = 0; i < tNormals.Length; i++)
                {
                    //UpdateProgress();
                    StringBuilder sb = new StringBuilder("vn ", 22);
                    sb.Append(tNormals[i].x.ToString()).Append(" ").
                        Append(tNormals[i].y.ToString()).Append(" ").
                        Append(tNormals[i].z.ToString());
                    sw.WriteLine(sb);
                }
                if (exportTris)
                {
                    // Write triangles
                    for (int i = 0; i < tPolys.Length; i += 3)
                    {
                        //UpdateProgress();
                        StringBuilder sb = new StringBuilder("f ", 43);
                        sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                            Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                            Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
                        sw.WriteLine(sb);
                    }
                }
                else
                {
                    // Write quads
                    for (int i = 0; i < tPolys.Length; i += 4)
                    {
                        //UpdateProgress();
                        StringBuilder sb = new StringBuilder("f ", 57);
                        sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                            Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                            Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                            Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
                        sw.WriteLine(sb);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log("Error saving file: " + err.Message);
            }
            sw.Close();
#if UNITY_EDITOR
            AssetDatabase.ImportAsset(savePath);
#endif
            return savePath;
        }

        public static Mesh BuildUnityMesh(Terrain terrain, float terrainRes)
        {
            Vector3 terrainPos = terrain.transform.position;
            int w = terrain.terrainData.heightmapResolution;
            int h = terrain.terrainData.heightmapResolution;
            Vector3 meshScale = terrain.terrainData.size;
            int tRes = (int)Mathf.Pow(2, (int)terrainRes);
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            float[,] tData = terrain.terrainData.GetHeights(0, 0, w, h);

            w = (w - 1) / tRes + 1;
            h = (h - 1) / tRes + 1;
            Vector3[] tVertices = new Vector3[w * h];

            Vector2[] tUV = new Vector2[w * h];
            int[] tPolys;
            tPolys = new int[(w - 1) * (h - 1) * 6];


            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    //tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(y, tData[x * tRes, y * tRes], x));// - terrain.transform.position;
                    tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(y, tData[x * tRes, y * tRes], x));
                    tUV[y * w + x] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);
                }
            }

            int index = 0;

            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    //tPolys[index++] = (y * w) + x;
                    //tPolys[index++] = ((y + 1) * w) + x;
                    //tPolys[index++] = (y * w) + x + 1;
                    //
                    //tPolys[index++] = ((y + 1) * w) + x;
                    //tPolys[index++] = ((y + 1) * w) + x + 1;
                    //tPolys[index++] = (y * w) + x + 1;


                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;


                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;

                }
            }

            int numPolys = tPolys.Length - 1;
            
            int[] tPolysFlipped = new int[tPolys.Length];
            for (int i = numPolys; i >= 0; i--)
            {
                tPolysFlipped[i] = tPolys[numPolys - i];
            }
            tPolys = tPolysFlipped;

            Mesh returnMesh = new Mesh();
            returnMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            returnMesh.vertices = tVertices;
            returnMesh.uv = tUV;
            returnMesh.triangles = tPolys;

            returnMesh.RecalculateNormals();

            return returnMesh;
        }

        public static long GetUnixTimestamp()
        {
            return new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
        
        public static T[] FindObjectsByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2022_3_OR_NEWER
            FindObjectsInactive findInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            FindObjectsSortMode sortMode = FindObjectsSortMode.None;
            T[] result = Object.FindObjectsByType<T>(findInactive, sortMode);
#else
            T[] result = Object.FindObjectsOfType<T>(includeInactive);
#endif
            return result;
        }
    }
}