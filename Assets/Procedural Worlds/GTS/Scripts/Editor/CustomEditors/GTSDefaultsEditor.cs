using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.GTS
{
    [CustomEditor(typeof(GTSDefaults))]
    public class GTSDefaultsEditor : GTSEditor
    {
        public static bool m_mainPanel = true;
        private GTSDefaults defaults;

        private void OnEnable()
        {
            defaults = target as GTSDefaults;
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            {
                m_mainPanel = m_editorUtils.Panel("MainPanel", MainPanel, m_mainPanel);
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(defaults);
            }
        }

        public void MainPanel(bool helpEnabled)
        {
            defaults.debugEnabled = m_editorUtils.Toggle("DebugEnabled", defaults.debugEnabled, helpEnabled);
            defaults.builtInMaterial = (Material)m_editorUtils.ObjectField("BuiltInMaterial", defaults.builtInMaterial, typeof(Material), false, helpEnabled);
            defaults.urpMaterial = (Material)m_editorUtils.ObjectField("URPMaterial", defaults.urpMaterial, typeof(Material), false, helpEnabled);
            defaults.hdrpMaterial = (Material)m_editorUtils.ObjectField("HDRPMaterial", defaults.hdrpMaterial, typeof(Material), false, helpEnabled);
            defaults.urpPackage = (Object)m_editorUtils.ObjectField("URPPackage", defaults.urpPackage, typeof(Object), false, helpEnabled);
            defaults.hdrpPackage = (Object)m_editorUtils.ObjectField("HDRPPackage", defaults.hdrpPackage, typeof(Object), false, helpEnabled);
            defaults.hdrp2022Package = (Object)m_editorUtils.ObjectField("HDRP2022Package", defaults.hdrp2022Package, typeof(Object), false, helpEnabled);
            defaults.hdrp2022_3_Package = (Object)m_editorUtils.ObjectField("HDRP2022_3_Package", defaults.hdrp2022_3_Package, typeof(Object), false, helpEnabled);
            defaults.hdrp2023_1_Package = (Object)m_editorUtils.ObjectField("HDRP2023_1_Package", defaults.hdrp2023_1_Package, typeof(Object), false, helpEnabled);
            defaults.documentationPDF = (Object)m_editorUtils.ObjectField("DocumentationPDF", defaults.documentationPDF, typeof(Object), false, helpEnabled);
            defaults.shaderFolder = (Object)m_editorUtils.ObjectField("ShaderFolder", defaults.shaderFolder, typeof(Object), false, helpEnabled);
        }
    }
}