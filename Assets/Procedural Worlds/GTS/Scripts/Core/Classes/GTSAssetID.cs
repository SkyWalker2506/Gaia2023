using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSAssetID<T> where T : Object
    {
        public string guid;
        public long localID;
#if UNITY_EDITOR
        public static bool IsBuiltInAsset(string assetPath)
        {
            return assetPath.Equals(GTSConstants.BuiltinResourcesPath) || assetPath.Equals(GTSConstants.BuiltinExtraResourcesPath);
        }
        private T LoadAsset(string guid, long localID) 
        {
            if (string.IsNullOrEmpty(guid))
                return null;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return null;
            if (IsBuiltInAsset(path))
            {
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(GTSConstants.BuiltinResourcesPath);
                foreach (Object asset in allAssets)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string assetGUID, out long assetLocalID))
                    {
                        if (assetLocalID == localID)
                            return asset as T;
                    }
                }
                Object[] allExtraAssets = AssetDatabase.LoadAllAssetsAtPath(GTSConstants.BuiltinExtraResourcesPath);
                foreach (Object asset in allExtraAssets)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string assetGUID, out long assetLocalID))
                    {
                        if (assetLocalID == localID)
                            return asset as T;
                    }
                }
            }
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }        
        public void SaveAsset(T @object)
        {
            if (@object == null)
                return;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(@object, out guid, out localID);
        }
        public T LoadAsset()
        {
            if (string.IsNullOrEmpty(guid))
                return null;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return null;
            if (IsBuiltInAsset(path))
            {
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(GTSConstants.BuiltinResourcesPath);
                foreach (Object asset in allAssets)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string assetGUID, out long assetLocalID))
                    {
                        if (assetLocalID == localID)
                            return asset as T;
                    }
                }
                Object[] allExtraAssets = AssetDatabase.LoadAllAssetsAtPath(GTSConstants.BuiltinExtraResourcesPath);
                foreach (Object asset in allExtraAssets)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string assetGUID, out long assetLocalID))
                    {
                        if (assetLocalID == localID)
                            return asset as T;
                    }
                }
            }
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}