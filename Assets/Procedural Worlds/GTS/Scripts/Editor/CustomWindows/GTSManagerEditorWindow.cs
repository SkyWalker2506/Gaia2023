using System;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    public class GTSManagerEditorWindow : EditorWindow
    {
        #region Variables
        [NonSerialized] protected bool m_loaded = false;
        public bool ManagerNotFound => m_manager == null && m_loaded;
        private GTSManager m_manager;
        public GTSManager manager
        {
            get
            {
                if (m_manager == null)
                {
                    if (!m_loaded)
                    {
                        m_manager = AssetDatabase.LoadAssetAtPath<GTSManager>(GTSConstants.GTSManagerPath);
                        m_loaded = true;
                    }
                }
                return m_manager;
            }
        }
        public GTSProfile profile
        {
            get => manager != null ? profile : null;
            set
            {
                if (manager != null)
                    profile = value;
            }
        }
        private GTSManagerEditor m_managerEditor;
        #endregion
        #region Properties
        #endregion
        #region Methods
        private void RefreshManagerInspector()
        {
            if (m_managerEditor != null)
                DestroyImmediate(m_managerEditor);
            m_managerEditor = manager != null ? Editor.CreateEditor(manager, typeof(GTSManagerEditor)) as GTSManagerEditor : null;
        }
        private void OnEnable()
        {
            RefreshManagerInspector();
        }
        private void OnDisable()
        {
            if (m_managerEditor != null)
                DestroyImmediate(m_managerEditor);
        }
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        private void OnFocus()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }
        private void OnGUI()
        {
            if (ManagerNotFound)
            {
                EditorGUILayout.LabelField("Manager not found! Please ensure you have a GTS Manager!");
                return;
            }
            if (m_managerEditor != null)
                m_managerEditor.OnInspectorGUI();
        }
        public void OnSceneGUI(SceneView sceneView)
        {
            if (ManagerNotFound)
                return;
            if (m_managerEditor != null)
                m_managerEditor.OnSceneGUI();
        }
        #endregion
    }
}