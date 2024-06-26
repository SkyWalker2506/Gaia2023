using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using PWCommon5;
using Gaia.Internal;
using Gaia;

namespace ProceduralWorlds.WaterSystem
{
    /// <summary>
    /// Editor for the PWS_WaterReflections
    /// </summary>
    [CustomEditor(typeof(PWS_WaterSystem))]
    public class PWS_WaterReflectionsEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private PWS_WaterSystem m_waterSystem;
        private GUIContent m_layers;

        Material m_waterMat;


        private void OnEnable()
        {
            m_waterSystem = (PWS_WaterSystem)target;

            m_waterMat = m_waterSystem.m_waterMaterialInstances.Find(x => !x.name.Contains("Under"));

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
        }

        #region Inspector Region

        /// <summary>
        /// Custom editor for PWS_WaterReflections
        /// </summary
        public override void OnInspectorGUI()
        {
            if (m_layers == null)
            {
                m_layers = new GUIContent("Reflection Layers", "Select the Layers which should be visible in the water reflection.");
            }

            EditorGUI.BeginChangeCheck();

            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

            if (m_waterSystem == null)
            {
                m_waterSystem = (PWS_WaterSystem)target;
            }

            m_editorUtils.Panel("GlobalSettings", GlobalSettings, true);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_waterSystem);
            }
        }

        #endregion

        #region Panel

        /// <summary>
        /// Global Main Panel
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void GlobalSettings(bool helpEnabled)
        {
            m_editorUtils.Heading("Setup");
            EditorGUI.indentLevel++;
            m_waterSystem.SunLight = (Light)m_editorUtils.ObjectField("SunLight", m_waterSystem.SunLight, typeof(Light), true, helpEnabled);
            m_waterSystem.m_player = (Transform)m_editorUtils.ObjectField("Player", m_waterSystem.m_player, typeof(Transform), true, helpEnabled);
            m_waterSystem.SeaLevel = m_editorUtils.FloatField("SeaLevel", m_waterSystem.SeaLevel, helpEnabled);

            EditorGUI.BeginChangeCheck();
            {
                //Note we need to revert the toggle since the flag is for disabling
                m_waterSystem.m_disableAllReflections = !m_editorUtils.Toggle("EnableReflections", !m_waterSystem.m_disableAllReflections);
                if (!m_waterSystem.m_disableAllReflections)
                {
                    m_waterSystem.m_useCustomRenderDistance = m_editorUtils.Toggle("UseCustomRenderDistance", m_waterSystem.m_useCustomRenderDistance, helpEnabled);
                    if (m_waterSystem.m_useCustomRenderDistance)
                    {
                        m_waterSystem.m_customRenderDistance = m_editorUtils.FloatField("CustomRenderDistance", m_waterSystem.m_customRenderDistance, helpEnabled);
                    }
                    m_waterSystem.m_reflectedLayers = GaiaEditorUtils.LayerMaskField(m_layers, m_waterSystem.m_reflectedLayers);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (!m_waterSystem.m_disableAllReflections)
                {
                    m_waterSystem.ResetPosandRot();
                    m_waterSystem.OnRenderObjectUpdate();
                }
            }


            m_waterSystem.InfiniteMode = m_editorUtils.Toggle("InfiniteMode", m_waterSystem.InfiniteMode, helpEnabled);



            if (m_waterMat == null)
            {
                EditorGUILayout.HelpBox("Could not find Water Material on this water surface, locking the surface options.", MessageType.Warning);
                if (GUILayout.Button("Check Again"))
                {
                    m_waterMat = m_waterSystem.m_waterMaterialInstances.Find(x => !x.name.Contains("Under"));
                }
            }
            else
            {
                m_waterMat.SetTexture("_WaterDepthRamp", (Texture2D)m_editorUtils.ObjectField("ShaderWaterDepth", m_waterMat.GetTexture("_WaterDepthRamp"), typeof(Texture2D), helpEnabled));
                m_waterMat.SetFloat("_TransparentDepth", m_editorUtils.Slider("ShaderDepth", m_waterMat.GetFloat("_TransparentDepth"), 0, 0.5f, helpEnabled));
                m_waterMat.SetFloat("_Metallic", m_editorUtils.Slider("ShaderMetallic", m_waterMat.GetFloat("_Metallic"), 0, 1, helpEnabled));
                m_waterMat.SetFloat("_Smoothness", m_editorUtils.Slider("ShaderSmoothness", m_waterMat.GetFloat("_Smoothness"), 0, 1, helpEnabled));
                m_waterMat.SetTexture("_NormalLayer0", (Texture2D)m_editorUtils.ObjectField("ShaderNormal1", m_waterMat.GetTexture("_NormalLayer0"), typeof(Texture2D), helpEnabled));
                m_waterMat.SetFloat("_NormalLayer0Scale", m_editorUtils.Slider("ShaderNormal1Scale", m_waterMat.GetFloat("_NormalLayer0Scale"), 0, 3, helpEnabled));
                m_waterMat.SetTexture("_NormalLayer1", (Texture2D)m_editorUtils.ObjectField("ShaderNormal2", m_waterMat.GetTexture("_NormalLayer1"), typeof(Texture2D), helpEnabled));
                m_waterMat.SetFloat("_NormalLayer1Scale", m_editorUtils.Slider("ShaderNormal2Scale", m_waterMat.GetFloat("_NormalLayer1Scale"), 0, 3, helpEnabled));
                m_waterMat.SetTexture("_NormalLayer2", (Texture2D)m_editorUtils.ObjectField("ShaderNormalDistance", m_waterMat.GetTexture("_NormalLayer2"), typeof(Texture2D), helpEnabled));
                m_waterMat.SetFloat("_NormalLayer2Scale", m_editorUtils.Slider("ShaderNormalDistanceScale", m_waterMat.GetFloat("_NormalLayer2Scale"), 0, 3, helpEnabled));
                m_waterMat.SetFloat("_NormalTile", m_editorUtils.Slider("ShaderNormalTile", m_waterMat.GetFloat("_NormalTile"), 0, 2048, helpEnabled));
                m_waterMat.SetFloat("_NormalFadeStart", m_editorUtils.FloatField("ShaderNormalFadeStart", m_waterMat.GetFloat("_NormalFadeStart"), helpEnabled));
                m_waterMat.SetFloat("_NormalFadeDistance", m_editorUtils.FloatField("ShaderNormalFadeDistance", m_waterMat.GetFloat("_NormalFadeDistance"), helpEnabled));
                m_waterMat.SetFloat("_WaveLength", m_editorUtils.FloatField("ShaderWaveLength", m_waterMat.GetFloat("_WaveLength"), helpEnabled));
                m_waterMat.SetFloat("_WaveSteepness", m_editorUtils.FloatField("ShaderWaveSteepness", m_waterMat.GetFloat("_WaveSteepness"), helpEnabled));
                m_waterMat.SetFloat("_WaveSpeed", m_editorUtils.FloatField("ShaderWaveSpeed", m_waterMat.GetFloat("_WaveSpeed"), helpEnabled));
                if (m_waterMat.HasFloat("_WaveShoreClamp"))
                {
                    m_waterMat.SetFloat("_WaveShoreClamp", m_editorUtils.Slider("ShaderWaveShoreClamp", m_waterMat.GetFloat("_WaveShoreClamp"), 0, 1, helpEnabled));
                }
                m_waterMat.SetTexture("_FoamTex", (Texture2D)m_editorUtils.ObjectField("ShaderFoamTexture", m_waterMat.GetTexture("_FoamTex"), typeof(Texture2D), helpEnabled));
                m_waterMat.SetFloat("_FoamTexTile", m_editorUtils.FloatField("ShaderFoamTexTile", m_waterMat.GetFloat("_FoamTexTile"), helpEnabled));
                m_waterMat.SetFloat("_FoamDepth", m_editorUtils.Slider("ShaderFoamDepth", m_waterMat.GetFloat("_FoamDepth"), 0, 16, helpEnabled));
                m_waterMat.SetFloat("_FoamStrength", m_editorUtils.Slider("ShaderFoamStrength", m_waterMat.GetFloat("_FoamStrength"), 0, 2, helpEnabled));
            }

            EditorGUI.indentLevel--;

            /*m_editorUtils.Heading("SurfaceSettings");
            EditorGUI.indentLevel++;
            WaterReflections.m_minSurfaceLight = m_editorUtils.FloatField("MinSurfaceLight", WaterReflections.m_minSurfaceLight, helpEnabled);
            if (WaterReflections.m_minSurfaceLight > WaterReflections.m_maxSurfaceLight)
            {
                WaterReflections.m_minSurfaceLight = WaterReflections.m_maxSurfaceLight - 0.1f;
            }

            if (WaterReflections.m_minSurfaceLight < 0f)
            {
                WaterReflections.m_minSurfaceLight = 0f;
            }
            WaterReflections.m_maxSurfaceLight = m_editorUtils.FloatField("MaxSurfaceLight", WaterReflections.m_maxSurfaceLight, helpEnabled);
            if (WaterReflections.m_maxSurfaceLight < 0.1f)
            {
                WaterReflections.m_maxSurfaceLight = 0.1f;
            }
            EditorGUI.indentLevel--;*/

            //if (m_editorUtils.Button("EditReflectionSettings"))
            //{
            //    GaiaUtils.FocusWaterProfile();
            //}
        }

        #endregion
    }
}
