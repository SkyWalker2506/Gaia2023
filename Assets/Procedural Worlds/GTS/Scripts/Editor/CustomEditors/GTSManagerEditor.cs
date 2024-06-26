using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    [CustomEditor(typeof(GTSManager))]
    public class GTSManagerEditor : GTSEditor
    {
        public static bool m_quickStartPanel = true;
        public static bool m_overviewPanel = true;
        public static bool m_profilePanel = true;
        private GTSManager m_manager;
        private Editor m_profileEditor;
        private Vector2 scrollPosition;
        private GTSProfile profile
        {
            get => m_manager != null ? m_manager.profile : null;
            set
            {
                if (m_manager != null)
                {
                    bool changed = m_manager.profile != value;
                    m_manager.profile = value;
                    if (changed)
                    {
                        RefreshProfileInspector();
                        EditorUtility.SetDirty(m_manager);
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
            m_manager = target as GTSManager;
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this);
            RefreshProfileInspector();
        }
        private void OnDisable()
        {
            if (m_profileEditor != null)
                DestroyImmediate(m_profileEditor);
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_editorUtils.GUIHeader();
            if (m_manager == null)
                return;
            serializedObject.Update();
            {
                m_quickStartPanel = m_editorUtils.Panel("QuickStartPanel", QuickStartPanel, m_quickStartPanel);
                m_overviewPanel = m_editorUtils.Panel("OverviewPanel", MainPanel, m_overviewPanel);
                if (profile != null)
                    m_profilePanel = m_editorUtils.Panel(new GUIContent(profile.name), ProfilePanel, m_profilePanel);
            }
            serializedObject.ApplyModifiedProperties();
        }
        public void QuickStartPanel(bool helpEnabled)
        {
            EditorGUILayout.LabelField("The manual is located here:", PWStyles.richText);
            GTSDefaults defaults = GTSDefaults.Instance;
            if (defaults != null)
            {
                Object documentationPDF = defaults.documentationPDF;
                if (documentationPDF != null)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(documentationPDF, typeof(Object), false);
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUILayout.LabelField("For more information and support, click on the buttons below.", PWStyles.richText);
            EditorGUILayout.BeginHorizontal();
            {
                if (m_editorUtils.Button("View Forums Btn", PWStyles.maxHalfWidth, GUILayout.Height(18f)))
                    Application.OpenURL(PWApp.CONF.SupportLink);
                if (m_editorUtils.Button("View Tutorials Btn", PWStyles.maxHalfWidth, GUILayout.Height(18f)))
                    Application.OpenURL(PWApp.CONF.TutorialsLink);
            }
            EditorGUILayout.EndHorizontal();
        }
        public void MainPanel(bool helpEnabled)
        {
            if (profile == null)
                EditorGUILayout.HelpBox("To get started with GTS, create a new profile by clicking 'New' below.", MessageType.Info);
            m_editorUtils.LabelField("Profile", helpEnabled);
            EditorGUILayout.BeginHorizontal();
            {
                GTSProfile oldProfile = profile;
                profile = (GTSProfile)EditorGUILayout.ObjectField(profile, typeof(GTSProfile), false);
                if (profile != oldProfile)
                {
                    RefreshProfileInspector();
                }
                if (profile != null)
                {
                    if (m_editorUtils.Button("ResetProfile", helpEnabled, GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("GTS - Reset Profile", "Are you sure you want to Reset the GTS Profile? \nNote: This cannot be undone.", "Yes", "Cancel"))
                        {
                            profile.Reset();
                            EditorUtility.SetDirty(profile);
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                }
                if (m_editorUtils.Button("NewProfile", helpEnabled, GUILayout.Width(50)))
                {
                    // Create a new profile here!
                    profile = GTSEditorUtility.CreateNewProfile();
                }
            }
            EditorGUILayout.EndHorizontal();
            if (profile != null)
            {
                GTSProfileEditor profileEditor = m_profileEditor as GTSProfileEditor;
                if (profileEditor != null)
                    profileEditor.DrawActionButtons(helpEnabled);
            }
        }
        private void ProfilePanel(bool helpEnabled)
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
    }
}