using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class GWS_ShaderVariantLimit : GWSetting
    {
        [SerializeField]
        private int m_originalVariantSetting;

        private void OnEnable()
        {
            m_canRestore = true;
            m_RPBuiltIn = true;
            m_RPHDRP = true;
            m_RPURP = true;
            m_canAutoFix = false;
            m_name = "Shader Variant Limit";
            m_infoTextOK = "The Shader Graph Variant Limit is set to 256 or higher. This is ideal for Gaia.";
            m_infoTextIssue = "The Shader Graph Variant Limit is less than 256 for this machine in Unity Preferences. Gaia needs this increased to 256 or higher to work correctly.";
            m_link = "https://docs.unity3d.com/Packages/com.unity.shadergraph@16.0/manual/Shader-Graph-Preferences.html";
            m_linkDisplayText = "Shader Graph Preferences (see 'Shader Variant Limit')";
            Initialize();
        }

        public override bool PerformCheck()
        {
#if UNITY_EDITOR
            if (!EditorPrefs.HasKey("UnityEditor.ShaderGraph.VariantLimit") || EditorPrefs.GetInt("UnityEditor.ShaderGraph.VariantLimit") < 256)
            {
                Status = GWSettingStatus.Warning;
                return true;
            }
#endif
            Status = GWSettingStatus.OK;
            return false;
        }

        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            if (autoFix || EditorUtility.DisplayDialog("Set Shader Variant Limit?",
            "Do you want to set the shader variant limit to 256 now? This will require a restart of the Unity Editor, please make sure to save your scenes to not lose any work.",
            "Continue", "Cancel"))
            {
                if (EditorPrefs.HasKey("UnityEditor.ShaderGraph.VariantLimit"))
                {
                    m_originalVariantSetting = EditorPrefs.GetInt("UnityEditor.ShaderGraph.VariantLimit");
                }
                else
                {
                    m_originalVariantSetting = 0;
                }
                EditorPrefs.SetInt("UnityEditor.ShaderGraph.VariantLimit", 256);
                GaiaSettings settings = GaiaUtils.GetGaiaSettings();
                settings.m_forceShaderReimport = true;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                EditorApplication.OpenProject(Environment.CurrentDirectory);
                return true;
            }
#endif
            return false;
        }

        public override string GetOriginalValueString()
        {
            return m_originalVariantSetting.ToString();
        }

        public override bool RestoreOriginalValue()
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Restore Shader Variant Limit?",
            $"Do you want to restore the shader variant limit to its original value {m_originalVariantSetting} now? This will require a restart of the Unity Editor, please make sure to save your scenes to not lose any work.",
            "Continue", "Cancel"))
            {
                if (m_originalVariantSetting == 0)
                {
                    EditorPrefs.DeleteKey("UnityEditor.ShaderGraph.VariantLimit");
                }
                else
                {
                    EditorPrefs.SetInt("UnityEditor.ShaderGraph.VariantLimit", m_originalVariantSetting);
                }
                GaiaSettings settings = GaiaUtils.GetGaiaSettings();
                settings.m_forceShaderReimport = true;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                EditorApplication.OpenProject(Environment.CurrentDirectory);
                return true;
            }
#endif
            return false;
        }

    }
}
