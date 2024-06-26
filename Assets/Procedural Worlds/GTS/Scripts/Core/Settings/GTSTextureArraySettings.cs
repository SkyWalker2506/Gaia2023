using System;
using UnityEngine;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSTextureArraySettings : GTSProfileSettings
    {
        public Texture2DArray albedoArray;
        public Texture2DArray normalArray;
        public int anisoLevel = 1;
        public float mipMapBias = 0f;
        public GTSTextureSize maxTextureSize = GTSTextureSize.Is2048MetersSq;
        public bool compressed = false;
        public GTSCompressionFormat compressionFormat = GTSCompressionFormat.RGBA32;
        public bool TextureArraysEmpty => albedoArray == null || normalArray == null;
        public override void Reset()
        {
            base.Reset();
            anisoLevel = 1;
            mipMapBias = 0f;
            maxTextureSize = GTSTextureSize.Is2048MetersSq;
            compressed = false;
            compressionFormat = GTSCompressionFormat.RGBA32;
        }
    }
}