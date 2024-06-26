using ProceduralWorlds.Setup;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    public class lpBuiltInDropDownEntry { public int m_ID; public string m_name; public LightingPresetBuiltIn m_preset; }
    public class lpURPDropDownEntry { public int m_ID; public string m_name; public LightingPresetURP m_preset; }
    public class lpHDRPDropDownEntry { public int m_ID; public string m_name; public LightingPresetHDRP m_preset; }

    public class GRC_LightingPresets : GaiaRuntimeComponent
    {
        public bool m_addPPLayertoCamera = true;

        private int m_selectedID;

        private List<LightingPresetBuiltIn> m_lightingPresetsBuiltIn = new List<LightingPresetBuiltIn>();
        private List<LightingPresetURP> m_lightingPresetsURP = new List<LightingPresetURP>();
        private List<LightingPresetHDRP> m_lightingPresetsHDRP = new List<LightingPresetHDRP>();

        private List<lpBuiltInDropDownEntry> m_lpBuiltInDropDownEntries = new List<lpBuiltInDropDownEntry>();
        private List<lpURPDropDownEntry> m_lpURPDropDownEntries = new List<lpURPDropDownEntry>();
        private List<lpHDRPDropDownEntry> m_lpHDRPDropDownEntries = new List<lpHDRPDropDownEntry>();

        private GUIContent m_presetDropdownLabel;
        private GUIContent m_generalHelpLink;

        private GUIContent m_panelLabel;
        public override GUIContent PanelLabel
        {
            get
            {
                if (m_panelLabel == null || m_panelLabel.text == "")
                {
                    m_panelLabel = new GUIContent("Lighting Presets", "Apply a lighting preset to your scene.");
                }
                return m_panelLabel;
            }
        }

        public override void Initialize()
        {
            m_orderNumber = 300;

            if (m_presetDropdownLabel == null || m_presetDropdownLabel.text == "")
            {
                m_presetDropdownLabel = new GUIContent("Lighting Preset", "Select a Lighting Preset from the list to apply to the scene as part of your lighting setup.");
            }
            if (m_generalHelpLink == null || m_generalHelpLink.text == "")
            {
                m_generalHelpLink = new GUIContent("Lighting Presets Module on Canopy", "Opens the Canopy Online Help Article for the Lighting Presets Module");
            }

            //Get all lighting presets
#if UNITY_EDITOR

#if HDPipeline
            m_lightingPresetsHDRP.Clear();
            m_lpHDRPDropDownEntries.Clear();
            string[] allGUIDs = AssetDatabase.FindAssets("t:LightingPresetHDRP", new string[1] { SetupUtils.GetInstallRootPath() });
#elif UPPipeline
                m_lightingPresetsURP.Clear();
                m_lpURPDropDownEntries.Clear();
                string[] allGUIDs = AssetDatabase.FindAssets("t:LightingPresetURP", new string[1] { SetupUtils.GetInstallRootPath() });
#else
            m_lightingPresetsBuiltIn.Clear();
            m_lpBuiltInDropDownEntries.Clear();
            string[] allGUIDs = AssetDatabase.FindAssets("t:LightingPresetBuiltIn", new string[1] { SetupUtils.GetInstallRootPath() });
#endif

            for (int i = 0; i < allGUIDs.Length; i++)
            {

#if HDPipeline
                string guid = allGUIDs[i];
                LightingPresetHDRP lpHD = (LightingPresetHDRP)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(LightingPresetHDRP));
                m_lightingPresetsHDRP.Add(lpHD);
                m_lpHDRPDropDownEntries.Add(new lpHDRPDropDownEntry { m_ID = i, m_name = lpHD.m_displayName, m_preset = lpHD });
#elif UPPipeline
                    string guid = allGUIDs[i];
                    LightingPresetURP lpu = (LightingPresetURP)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(LightingPresetURP));
                    m_lightingPresetsURP.Add(lpu);
                    m_lpURPDropDownEntries.Add(new lpURPDropDownEntry { m_ID = i, m_name = lpu.m_displayName, m_preset = lpu });
#else
                string guid = allGUIDs[i];
                LightingPresetBuiltIn lpb = (LightingPresetBuiltIn)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(LightingPresetBuiltIn));
                m_lightingPresetsBuiltIn.Add(lpb);
                m_lpBuiltInDropDownEntries.Add(new lpBuiltInDropDownEntry { m_ID = i, m_name = lpb.m_displayName, m_preset = lpb });
#endif

            }

#if HDPipeline
            m_lightingPresetsHDRP.Sort(delegate (LightingPresetHDRP x, LightingPresetHDRP y)
            {
                if (x.m_displayName == null && y.m_displayName == null) return 0;
                else if (x.m_displayName == null) return -1;
                else if (y.m_displayName == null) return 1;
                else return x.m_displayName.CompareTo(y.m_displayName);
            });
            m_lpHDRPDropDownEntries.Sort(delegate (lpHDRPDropDownEntry x, lpHDRPDropDownEntry y)
            {
                if (x.m_preset.m_displayName == null && y.m_preset.m_displayName == null) return 0;
                else if (x.m_preset.m_displayName == null) return -1;
                else if (y.m_preset.m_displayName == null) return 1;
                else return x.m_preset.m_displayName.CompareTo(y.m_preset.m_displayName);
            });

            for (int i = 0; i < m_lpHDRPDropDownEntries.Count; i++)
            {
                lpHDRPDropDownEntry entry = m_lpHDRPDropDownEntries[i];
                entry.m_ID = i;
            }

#elif UPPipeline
            m_lightingPresetsURP.Sort(delegate (LightingPresetURP x, LightingPresetURP y)
            {
                if (x.m_displayName == null && y.m_displayName == null) return 0;
                else if (x.m_displayName == null) return -1;
                else if (y.m_displayName == null) return 1;
                else return x.m_displayName.CompareTo(y.m_displayName);
            });
            m_lpURPDropDownEntries.Sort(delegate (lpURPDropDownEntry x, lpURPDropDownEntry y)
            {
                if (x.m_preset.m_displayName == null && y.m_preset.m_displayName == null) return 0;
                else if (x.m_preset.m_displayName == null) return -1;
                else if (y.m_preset.m_displayName == null) return 1;
                else return x.m_preset.m_displayName.CompareTo(y.m_preset.m_displayName);
            });

             for (int i = 0; i < m_lpURPDropDownEntries.Count; i++)
            {
                lpURPDropDownEntry entry = m_lpURPDropDownEntries[i];
                entry.m_ID = i;
            }
#else

            m_lightingPresetsBuiltIn.Sort(delegate (LightingPresetBuiltIn x, LightingPresetBuiltIn y)
            {
                if (x.m_displayName == null && y.m_displayName == null) return 0;
                else if (x.m_displayName == null) return -1;
                else if (y.m_displayName == null) return 1;
                else return x.m_displayName.CompareTo(y.m_displayName);
            });
            m_lpBuiltInDropDownEntries.Sort(delegate (lpBuiltInDropDownEntry x, lpBuiltInDropDownEntry y)
            {
                if (x.m_preset.m_displayName == null && y.m_preset.m_displayName == null) return 0;
                else if (x.m_preset.m_displayName == null) return -1;
                else if (y.m_preset.m_displayName == null) return 1;
                else return x.m_preset.m_displayName.CompareTo(y.m_preset.m_displayName);
            });

            for (int i = 0; i < m_lpBuiltInDropDownEntries.Count; i++)
            {
                lpBuiltInDropDownEntry entry = m_lpBuiltInDropDownEntries[i];
                entry.m_ID = i;
            }
#endif

#endif
        }

        public override void DrawUI()
        {
            bool originalGUIState = GUI.enabled;
#if UNITY_EDITOR
            EditorGUI.BeginChangeCheck();
            {
                string helpText = "The Lighting Preset Module allows you to quickly apply a pre-made lighting setup to your scene. This can be used to determine how your terrain would look like under different lighting conditions, or as a starting point to develop your lighting setup for the scene. It is possible to develop your own presets to the selection as well.";
                DisplayHelp(helpText, m_generalHelpLink, "https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/creating_runtime/runtime-module-lighting-presets-r163/");
#if HDPipeline
                m_selectedID = EditorGUILayout.IntPopup("Lighting Preset", m_selectedID, m_lpHDRPDropDownEntries.Select(x => x.m_name).ToArray(), m_lpHDRPDropDownEntries.Select(x => x.m_ID).ToArray());
#elif UPPipeline
            m_selectedID = EditorGUILayout.IntPopup("Lighting Preset", m_selectedID, m_lpURPDropDownEntries.Select(x => x.m_name).ToArray(), m_lpURPDropDownEntries.Select(x => x.m_ID).ToArray());
#else
                m_selectedID = EditorGUILayout.IntPopup("Lighting Preset", m_selectedID, m_lpBuiltInDropDownEntries.Select(x => x.m_name).ToArray(), m_lpBuiltInDropDownEntries.Select(x => x.m_ID).ToArray());
#endif
                DisplayHelp("Select the lighting preset you want to apply here.");

#if !UNITY_POST_PROCESSING_STACK_V2
            GUI.enabled = false;
#endif
                m_addPPLayertoCamera = EditorGUILayout.Toggle("Add PP Layer", m_addPPLayertoCamera);
                DisplayHelp("Should Gaia automatically add a Post-Processing Layer component to the camera? Without the Post-Processing Layer on the camera, Post-Processing Effects do not work in the built-in rendering pipeline.");
                GUI.enabled = originalGUIState;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove"))
                {
                    RemoveFromScene();
                }
                GUILayout.Space(15);
                if (GUILayout.Button("Apply"))
                {
                    AddToScene();
                }
                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        public override void AddToScene()
        {
#if HDPipeline
            m_lightingPresetsHDRP[m_selectedID].Apply();
#elif UPPipeline
            m_lightingPresetsURP[m_selectedID].Apply(m_addPPLayertoCamera);
#else
            m_lightingPresetsBuiltIn[m_selectedID].Apply(m_addPPLayertoCamera);
#endif
        }

        public override void RemoveFromScene()
        {
#if HDPipeline
            m_lightingPresetsHDRP[m_selectedID].RemoveFromScene();
#elif UPPipeline
            m_lightingPresetsURP[m_selectedID].RemoveFromScene();
#else
            m_lightingPresetsBuiltIn[m_selectedID].RemoveFromScene();
#endif
        }

    }
}
