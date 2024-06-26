using UnityEngine;
using PWCommon5;
using UnityEditor.SceneManagement;
using UnityEditor;
using Gaia.Internal;

namespace Gaia
{
    [CustomEditor(typeof(TerrainDetailOverwrite))]
    public class TerrainDetailOverwriteEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private string m_version;
        private Color defaultBackground;
        private TerrainDetailOverwrite m_tdo;

        private void OnEnable()
        {
            //Get Gaia Lighting Profile object
            m_tdo = (TerrainDetailOverwrite)target;

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            m_version = PWApp.CONF.Version;

            if (m_tdo.m_terrain == null)
            {
                m_tdo.m_terrain = m_tdo.GetComponent<Terrain>();
            }

            m_tdo.m_unityDetailDistance = (int)m_tdo.m_terrain.detailObjectDistance;
            m_tdo.m_unityDetailDensity = m_tdo.m_terrain.detailObjectDensity;

            if (m_tdo.m_terrain.terrainData.detailResolutionPerPatch == 2)
            {
                m_tdo.m_detailQuality = GaiaConstants.TerrainDetailQuality.Ultra2;
            }
            else if (m_tdo.m_terrain.terrainData.detailResolutionPerPatch == 4)
            {
                m_tdo.m_detailQuality = GaiaConstants.TerrainDetailQuality.VeryHigh4;
            }
            else if (m_tdo.m_terrain.terrainData.detailResolutionPerPatch == 8)
            {
                m_tdo.m_detailQuality = GaiaConstants.TerrainDetailQuality.High8;
            }
            else if (m_tdo.m_terrain.terrainData.detailResolutionPerPatch == 16)
            {
                m_tdo.m_detailQuality = GaiaConstants.TerrainDetailQuality.Medium16;
            }
            else if (m_tdo.m_terrain.terrainData.detailResolutionPerPatch == 32)
            {
                m_tdo.m_detailQuality = GaiaConstants.TerrainDetailQuality.Low32;
            }
            else
            {
                m_tdo.m_detailQuality = GaiaConstants.TerrainDetailQuality.VeryLow64;
            }
        }

        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

            m_tdo.GetResolutionPatches();

            EditorGUILayout.LabelField("Version: " + m_version);

            m_editorUtils.Panel("DetailDistanceSettings", DetailTerrainDistance, true);
        }

        private void DetailTerrainDistance(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Text("DetailInfo");
            EditorGUILayout.Space();

            //switch (m_profile.m_detailQuality)
            //{
            //    case GaiaConstants.TerrainDetailQuality.Ultra2:
            //        EditorGUILayout.LabelField(m_editorUtils.GetTextValue("DetailPatchResolution") + "Ultra");
            //        break;
            //    case GaiaConstants.TerrainDetailQuality.VeryHigh4:
            //        EditorGUILayout.LabelField(m_editorUtils.GetTextValue("DetailPatchResolution") + "Very High");
            //        break;
            //    case GaiaConstants.TerrainDetailQuality.High8:
            //        EditorGUILayout.LabelField(m_editorUtils.GetTextValue("DetailPatchResolution") + "High");
            //        break;
            //    case GaiaConstants.TerrainDetailQuality.Medium16:
            //        EditorGUILayout.LabelField(m_editorUtils.GetTextValue("DetailPatchResolution") + "Medium");
            //        break;
            //    case GaiaConstants.TerrainDetailQuality.Low32:
            //        EditorGUILayout.LabelField(m_editorUtils.GetTextValue("DetailPatchResolution") + "Low");
            //        break;
            //    case GaiaConstants.TerrainDetailQuality.VeryLow64:
            //        EditorGUILayout.LabelField(m_editorUtils.GetTextValue("DetailPatchResolution") + "Very Low");
            //        break;
            //}
            //EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("DetailPatchResolutionHelp"), MessageType.Info);

            if (m_tdo.m_gaiaDetailFadeoutDistance + m_tdo.m_gaiaDetailDistance > m_tdo.m_unityDetailDistance)
            {
                EditorGUILayout.HelpBox("The Unity Detail Distance is smaller than the sum of Gaia Detail Distance and Gaia Fadeout Distance. You might see terrain details pop-in in a block style fashion. Try to keep the Unity Distance higher than the sum of the Gaia Distances to avoid this.", MessageType.Warning);
            }

            m_tdo.m_unityDetailDistance = m_editorUtils.IntField("DetailDistance", m_tdo.m_unityDetailDistance, helpEnabled);
            if (m_tdo.m_unityDetailDistance < 0)
            {
                m_tdo.m_unityDetailDistance = 0;
            }
            if (m_tdo.m_unityDetailDistance > 250)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("DetailDistanceHelp"), MessageType.Info);
            }

            m_tdo.m_gaiaDetailDistance = m_editorUtils.FloatField("GaiaDetailDistance", m_tdo.m_gaiaDetailDistance, helpEnabled);
            m_tdo.m_gaiaDetailFadeoutDistance = m_editorUtils.FloatField("GaiaFadeoutDistance", m_tdo.m_gaiaDetailFadeoutDistance, helpEnabled);

            m_tdo.m_unityDetailDensity = m_editorUtils.Slider("DetailDensity", m_tdo.m_unityDetailDensity, 0f, 1f, helpEnabled);
            if (m_tdo.m_unityDetailDistance > 250 && m_tdo.m_unityDetailDensity > 0.5f)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("DetailDensityHelp"), MessageType.Info);
            }

            if (m_editorUtils.Button("ApplyToAll"))
            {
                m_tdo.ApplySettings(true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_tdo, "Made changes");
                EditorUtility.SetDirty(m_tdo);
                m_tdo.ApplySettings(false);

                if (!Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }
    }
}