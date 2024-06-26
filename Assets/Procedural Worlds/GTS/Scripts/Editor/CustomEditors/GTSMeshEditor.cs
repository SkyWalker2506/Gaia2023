using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    [CustomEditor(typeof(GTSMesh))]
    public class GTSMeshEditor : GTSEditor
    {
        public static bool m_mainPanel = true;
        public static bool m_profilePanel = false;
        public static bool m_terrainSettingsPanel = true;
        private GTSMesh mesh;
        private Editor m_profileEditor;
        private Vector2 scrollPosition;
        private GTSProfile profile
        {
            get => mesh != null ? mesh.profile : null;
            set
            {
                if (mesh != null)
                {
                    bool changed = mesh.profile != value;
                    mesh.profile = value;
                    if (changed)
                    {
                        RefreshProfileInspector();
                        EditorUtility.SetDirty(mesh);
                    }
                }
            }
        }
        private void OnEnable()
        {
            mesh = target as GTSMesh;
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this);
            RefreshProfileInspector();
        }
        private void MarkMeshDirty()
        {
            EditorUtility.SetDirty(mesh);
            mesh.OnValidate();
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
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            {
                m_terrainSettingsPanel = m_editorUtils.Panel("TerrainSettingsPanel", TerrainSettingsPanel, m_terrainSettingsPanel);
                m_mainPanel = m_editorUtils.Panel("MainPanel", MainPanel, m_mainPanel);
                if (profile != null)
                    m_profilePanel = m_editorUtils.Panel(new GUIContent(profile.name), ProfilePanel, m_profilePanel);
            }
            if (EditorGUI.EndChangeCheck())
            {
                MarkMeshDirty();
            }
        }
        public void MainPanel(bool helpEnabled)
        {
            profile = (GTSProfile)m_editorUtils.ObjectField("GTSProfile", profile, typeof(GTSProfile), false, helpEnabled);
            mesh.material = (Material)m_editorUtils.ObjectField("GTSTerrainMaterial", mesh.material, typeof(Material), false, helpEnabled);
        }
        public void ProfilePanel(bool helpEnabled)
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
                mesh.colormap = (Texture2D)m_editorUtils.ObjectField("ColorMap", mesh.colormap, typeof(Texture2D), false, GUILayout.MaxHeight(16));
            }
            if (EditorGUI.EndChangeCheck())
            {
                MarkMeshDirty();
            }
        }
    }
}