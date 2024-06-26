using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSVariationSettings : GTSProfileSettings
    {
        public Texture2D texture;
        public float sizeA = 10f;
        public float sizeB = 10f;
        public float sizeC = 10f;
        public float intensity = 0.5f;
        public bool objectSpace = false;
        public override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(isLowPoly ? GTSConstants.LowPolyVariationTexturePath : GTSConstants.VariationTexturePath);
#endif
            sizeA = 10f;
            sizeB = 10f;
            sizeC = 10f;
            intensity = 0.5f;
            objectSpace = false;
        }
    }
}