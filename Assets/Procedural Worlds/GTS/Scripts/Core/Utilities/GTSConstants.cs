using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    public static class GTSConstants
    {
        public const string ContentConfFileName = "GTS Content.conf";
        public const string UserDataFolderPath = "Assets/GTS User Data";
        public const string BuiltinResourcesPath = "Resources/unity_builtin_extra";
        public const string BuiltinExtraResourcesPath = "Library/unity default resources";
        /// <summary>
        /// Global format all temporary textures will be created in before the final compression format will be applied
        /// </summary>
        public const TextureFormat WorkTextureFormat = TextureFormat.RGBA32;
        private static string m_contentFolderPath = null;
        private static string m_profilesFolderPath = null;
        private static string m_texturesFolderPath = null;
        private static string m_detailFolderPath = null;
        private static string m_snowFolderPath = null;
        private static string m_rainFolderPath = null;
        private static string m_geoFolderPath = null;
        private static string m_variationFolderPath = null;
#if UNITY_EDITOR
        public static string GetAssetPath(string fileName)
        {
            foreach (string asset in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName), (string[])null))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset);
                if (Path.GetFileName(assetPath) == fileName)
                    return assetPath;
            }
            return "";
        }
#endif
        public static string GetUserDataFolder(string subFolderName = "")
        {
            string fullPath = $"{UserDataFolderPath}/{subFolderName}";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            return fullPath;
        }
        #region Properties
        public static string ContentFolderPath
        {
            get
            {
#if UNITY_EDITOR
                if (m_contentFolderPath == null)
                {
                    m_contentFolderPath = GetAssetPath(ContentConfFileName);
                    m_contentFolderPath = Path.GetDirectoryName(m_contentFolderPath);
                }
#endif
                return m_contentFolderPath;
            }
        }
        public static string GTSDefaultsPath => $"{ContentFolderPath}/GTS Defaults.asset";
        public static string GTSManagerPath => $"{ContentFolderPath}/GTS Manager.asset";
        public static string GTSMaintenanceTokenPath => $"{ContentFolderPath}/GTS Maintenance Token.dat";
        public static string GTSDefaultProfilePath => $"{ProfilesFolderPath}/GTS Default Profile.asset";
        public static string GTSLowPolyProfilePath => $"{ProfilesFolderPath}/GTS Low Poly Profile.asset";
        public static string ProfilesFolderPath
        {
            get
            {
                if (m_profilesFolderPath == null)
                    m_profilesFolderPath = $"{ContentFolderPath}/Profiles";
                return m_profilesFolderPath;
            }
        }
        public static string TexturesFolderPath
        {
            get
            {
                if (m_texturesFolderPath == null)
                    m_texturesFolderPath = $"{ContentFolderPath}/Textures";
                return m_texturesFolderPath;
            }
        }
        #region Texture Folders
        public static string DetailFolderPath
        {
            get
            {
                if (m_detailFolderPath == null)
                    m_detailFolderPath = $"{TexturesFolderPath}/Detail";
                return m_detailFolderPath;
            }
        }
        public static string SnowFolderPath
        {
            get
            {
                if (m_snowFolderPath == null)
                    m_snowFolderPath = $"{TexturesFolderPath}/Snow";
                return m_snowFolderPath;
            }
        }
        public static string RainFolderPath
        {
            get
            {
                if (m_rainFolderPath == null)
                    m_rainFolderPath = $"{TexturesFolderPath}/Rain";
                return m_rainFolderPath;
            }
        }
        public static string GeoFolderPath
        {
            get
            {
                if (m_geoFolderPath == null)
                    m_geoFolderPath = $"{TexturesFolderPath}/Geo";
                return m_geoFolderPath;
            }
        }
        public static string VariationFolderPath
        {
            get
            {
                if (m_variationFolderPath == null)
                    m_variationFolderPath = $"{TexturesFolderPath}/Variation";
                return m_variationFolderPath;
            }
        }
        #endregion
        #region Texture Paths
        // Detail
        public static string DetailNormal1TexturePath => $"{DetailFolderPath}/T_GTS_Detail_N_1.png";
        public static string DetailNormal2TexturePath => $"{DetailFolderPath}/T_GTS_Detail_N_2.png";
        public static string DetailNormal3TexturePath => $"{DetailFolderPath}/T_GTS_Detail_N_3.png";
        // Geo
        public static string GeoAlbedoTexturePath => $"{GeoFolderPath}/T_Geo_02_soft.png";
        public static string GeoNormalTexturePath => $"{GeoFolderPath}/T_Geo_02_N.png";
        // Snow
        public static string SnowAlbedoTexturePath => $"{SnowFolderPath}/T_GTS_Snow_A.png";
        public static string SnowNormalTexturePath => $"{SnowFolderPath}/T_GTS_Snow_N.png";
        public static string SnowMaskTexturePath => $"{SnowFolderPath}/T_GTS_Snow_M.png";
        //Rain
        public static string RainTexturePath => $"{RainFolderPath}/T_GTS_RainData.png";
        // Variation
        public static string VariationTexturePath => $"{VariationFolderPath}/T_GTS_Variation.png";
        #endregion
        #region Low Poly Texture Paths
        // Detail
        public static string LowPolyDetailNormalTexturePath => $"{DetailFolderPath}/T_GTS_Detail_LP_N.png";
        // Geo
        public static string LowPolyGeoAlbedoTexturePath => $"{GeoFolderPath}/T_Geo_10.png";
        public static string LowPolyGeoNormalTexturePath => $"{GeoFolderPath}/T_Geo_01_N.png";
        // Snow
        public static string LowPolySnowAlbedoTexturePath => $"{SnowFolderPath}/T_GTS_Snow_LP_A.png";
        public static string LowPolySnowNormalTexturePath => $"{SnowFolderPath}/T_GTS_Snow_LP_N.png";
        public static string LowPolySnowMaskTexturePath => $"{SnowFolderPath}/T_GTS_Snow_LP_M.png";
        // Variation
        public static string LowPolyVariationTexturePath => $"{VariationFolderPath}/T_GTS_Variation_LP.png";
        #endregion
        #endregion
    }
}