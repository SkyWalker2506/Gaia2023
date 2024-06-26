using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    [CustomEditor(typeof(GTSTerrain))]
    public class GTSTerrainEditor : GTSEditor
    {
        public static bool m_mainPanel = true;
        public static bool m_settingsPanel = false;
        public static bool m_terrainSettingsPanel = true;
        private GTSTerrain gtsTerrain;
        private Editor m_profileEditor;
        private Vector2 scrollPosition;
        private GTSProfile profile
        {
            get => gtsTerrain != null ? gtsTerrain.profile : null;
            set
            {
                if (gtsTerrain != null)
                {
                    bool changed = gtsTerrain.profile != value;
                    gtsTerrain.profile = value;
                    if (changed)
                    {
                        RefreshProfileInspector();
                        EditorUtility.SetDirty(gtsTerrain);
                    }
                }
            }
        }
        private void RefreshProfileInspector()
        {
            if (m_profileEditor != null)
                DestroyImmediate(m_profileEditor);
            m_profileEditor = profile != null ? CreateEditor(profile) : null;
            GTSProfileEditor profileEditor = m_profileEditor as GTSProfileEditor;
            if (profileEditor != null)
                profileEditor.ShowActions = false;
        }
        private void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this);
            gtsTerrain = target as GTSTerrain;
            RefreshProfileInspector();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            {
                m_terrainSettingsPanel = m_editorUtils.Panel("TerrainSettingsPanel", TerrainSettingsPanel, m_terrainSettingsPanel);
                m_mainPanel = m_editorUtils.Panel("MainPanel", MainPanel, m_mainPanel);
                if (profile != null)
                    m_settingsPanel = m_editorUtils.Panel(new GUIContent(profile.name), SettingsPanel, m_settingsPanel);

            }
            serializedObject.ApplyModifiedProperties();
        }
        public void MainPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                //gtsTerrain.colormap = (Texture2D)m_editorUtils.ObjectField("ColorMap", gtsTerrain.colormap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));

                profile = (GTSProfile)m_editorUtils.ObjectField("Profile", profile, typeof(GTSProfile), false, helpEnabled);
                if (profile != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(m_editorUtils.GetContent("ApplyProfile"), PWStyles.maxHalfWidth, PWStyles.minButtonHeight))
                        {
                            GTSProfile profile = gtsTerrain.profile;
                            if (profile != null)
                            {
                                if (profile.TextureArraysEmpty)
                                {
                                    if (EditorUtility.DisplayDialog("MissingTexture Arrays", "The Texture Arrays for this profile don't exist, would you like to generate them now?", "Yes", "No"))
                                    {
                                        profile.CreateTextureArrays();
                                    }
                                }
                            }
                            gtsTerrain.ApplyProfile();
                        }
                        if (GUILayout.Button(m_editorUtils.GetContent("RemoveProfile"), PWStyles.maxHalfWidth, PWStyles.minButtonHeight))
                            gtsTerrain.RemoveProfile();
                    }
                    EditorGUILayout.EndHorizontal();
                    m_editorUtils.InlineHelp("ApplyProfile", helpEnabled);
                    m_editorUtils.InlineHelp("RemoveProfile", helpEnabled);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(gtsTerrain);
            }
        }
        public void SettingsPanel(bool helpEnabled)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                if (m_profileEditor != null)
                {
                    m_profileEditor.OnInspectorGUI();
                    m_editorUtils.InlineHelp("ApplyProfile", helpEnabled);
                    m_editorUtils.InlineHelp("RemoveProfile", helpEnabled);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        public void TerrainSettingsPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                gtsTerrain.colormap = (Texture2D)m_editorUtils.ObjectField("ColorMap", gtsTerrain.colormap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16));
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(gtsTerrain);
            }
        }
    }
}