using UnityEngine;
namespace ProceduralWorlds.GTS
{
    public static class GTSExtensionMethods
    {
        #region Terrain
        public static T AddComponent<T>(this Terrain terrain) where T : Component => terrain.gameObject.AddComponent<T>();
        #endregion
        #region RenderTexture
        public static Texture2D ToTexture2D(this RenderTexture rt)
        {
            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, rt.useMipMap);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            return texture;
        }
        public static RenderTexture ToRenderTexture(this Texture2D tex)
        {
            RenderTexture rt = new RenderTexture(tex.width, tex.height, 0);
            // Copy your texture ref to the render texture
            Graphics.Blit(tex, rt);
            return rt;
        }
        /// <summary>
        /// Converts this RenderTexture to a RenderTexture in ARGB32 format.
        /// Can also return the original RenderTexture if it's already in ARGB32 format.
        /// Note that the resulting temporary RenderTexture should be released if no longer used to prevent a memory leak.
        /// </summary>
        public static RenderTexture ConvertToARGB32(this RenderTexture self)
        {
            if (self.format == RenderTextureFormat.ARGB32)
                return self;
            RenderTexture result = RenderTexture.GetTemporary(self.width, self.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(self, result);
            return result;
        }
        #endregion
        #region Texture2D
        public static void Copy(this Texture2D texture2D, RenderTexture renderTexture)
        {
            RenderTexture oldRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = oldRenderTexture;
        }
        #endregion
        #region Material
        public static bool IsGTSMaterial(this Material material)
        {
            GTSUtility.GetPipelineShaders(out var gtsShader, out var unityShader);
            return material.shader == gtsShader;
        }
        #endregion
    }
}