using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{

    /// <summary>
    /// Holds the settings from the Unity Lighting Window  Environment tab
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Gaia/BuiltIn-URP Environment Settings")]
    public class EnvironmentBuiltInURP : ScriptableObject
    {
        public Material m_skyBoxMaterial;

        public Color m_realtimeShadowColor;

        public AmbientMode m_ambientMode;

        public Color m_skyColor;
        public Color m_equatorColor;
        public Color m_groundColor;

        public bool m_fogEnabled;
        public Color m_fogColor;
        public FogMode m_fogMode;
        public float m_fogDensity;

        public Texture2D m_haloTexture;
        public float m_haloStrength;
        public float m_flareFadeSpeed;
        public float m_flareStrength;
        public Texture2D m_spotCookie;


        public void Apply()
        {
            if (m_skyBoxMaterial != null)
            {
                //Do not apply the skybox directly into that slot, so that changes
                //by the user do not destroy the original material.
                RenderSettings.skybox = Instantiate<Material>(m_skyBoxMaterial);
            }

            RenderSettings.subtractiveShadowColor = m_realtimeShadowColor;

            RenderSettings.ambientMode = m_ambientMode;

            RenderSettings.ambientSkyColor = m_skyColor;
            RenderSettings.ambientEquatorColor = m_equatorColor;
            RenderSettings.ambientGroundColor = m_groundColor;

            RenderSettings.fog = m_fogEnabled;
            RenderSettings.fogColor = m_fogColor;
            RenderSettings.fogMode = m_fogMode;
            RenderSettings.fogDensity = m_fogDensity;

            RenderSettings.haloStrength = m_haloStrength;
            RenderSettings.flareFadeSpeed = m_flareFadeSpeed;
            RenderSettings.flareStrength = m_flareStrength;

        }

        public void IngestFromScene()
        {
#if UNITY_EDITOR
            if (RenderSettings.skybox != null)
            {
                Material originalSkyBox = RenderSettings.skybox;
                string currentPath = AssetDatabase.GetAssetPath(this);
                currentPath = currentPath.Substring(0, currentPath.LastIndexOf("/"));
                string savePath = currentPath + "/" + this.name + " SkyBox.mat";
                //Do not create the asset based on the skybox in the render settings, otherwise the scene will
                //reference that newly created asset instead!
                Material skyBoxCopy = Instantiate<Material>(originalSkyBox);
                AssetDatabase.CreateAsset(skyBoxCopy, savePath);
                m_skyBoxMaterial = (Material)AssetDatabase.LoadAssetAtPath(savePath, typeof(Material));
                RenderSettings.skybox = Instantiate<Material>(m_skyBoxMaterial); ;
            }
            else
            {
                m_skyBoxMaterial = null;
            }
#endif


            m_realtimeShadowColor = RenderSettings.subtractiveShadowColor;

            m_ambientMode = RenderSettings.ambientMode;

            m_skyColor = RenderSettings.ambientSkyColor;
            m_equatorColor = RenderSettings.ambientEquatorColor;
            m_groundColor = RenderSettings.ambientGroundColor;

            m_fogEnabled = RenderSettings.fog;
            m_fogColor = RenderSettings.fogColor;
            m_fogMode = RenderSettings.fogMode;
            m_fogDensity = RenderSettings.fogDensity;

            m_haloStrength = RenderSettings.haloStrength;
            m_flareFadeSpeed = RenderSettings.flareFadeSpeed;
            m_flareStrength = RenderSettings.flareStrength;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif

        }
    }
}
