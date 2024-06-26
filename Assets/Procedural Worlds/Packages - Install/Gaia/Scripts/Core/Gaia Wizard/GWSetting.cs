using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


public enum GWSettingStatus { Unknown, OK, Info, Warning, Error }

/// <summary>
/// Base class for a "Gaia Wizard" setting. A "Gaia Wizard" settings can be a certain project specific setting or aspect that
/// we want to check against in the Gaia Manager and, if possible, offer to fix automatically if the setting is currently configured
/// in a unfavorable way.
/// Example: Project is not set up to use linear color space 
/// 
/// </summary>

namespace Gaia
{

    [System.Serializable]
    public class GWSetting : ScriptableObject, IComparable<GWSetting>
    {
        public bool m_foldedOut;
        public bool m_wasFixed;
        public bool m_canAutoFix = true;

        internal string m_name;

        public bool m_ignored = false;
        internal bool m_RPBuiltIn;
        internal bool m_RPHDRP;
        internal bool m_RPURP;
        
        internal bool m_canRestore = false;
        internal string m_infoTextOK;
        internal string m_infoTextIssue;
        internal string m_link ="";
        internal string m_linkDisplayText = "";

        private GUIStyle m_linkStyle;
        private GUIContent m_linkContent;
        private static GUIContent m_iconOKContent;
        private static GUIContent m_iconWarningContent;
        private static GUIContent m_iconErrorContent;

        private GWSettingStatus m_status = GWSettingStatus.Unknown;
        public GWSettingStatus Status
        {
            get
            {
                return m_status;
            }
            set
            {
                GWSettingStatus oldValue = m_status;
                m_status = value;
                if (m_status != oldValue && OnGWSettingStatusChanged != null)
                {
                    OnGWSettingStatusChanged(this);
                }
            }
        }

        public delegate void GWSettingStatusChanged(GWSetting changedSetting);
        public static event GWSettingStatusChanged OnGWSettingStatusChanged;

        private static GaiaSettings m_gaiaSettings;
        private static GaiaSettings GaiaSettings
        {
            get
            {
                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }
                return m_gaiaSettings;
            }
        }

        private static GaiaConstants.EnvironmentRenderer m_activePipeline;
        

        public virtual void Initialize()
        {
            if (m_link != "")
            {
                if (m_linkDisplayText != "")
                {
                    m_linkContent = new GUIContent(m_linkDisplayText, m_link);
                }
                else 
                {
                    m_linkContent = new GUIContent(m_link, m_link);
                }
            }

            if (m_iconOKContent == null)
            {
                m_iconOKContent = new GUIContent(GaiaSettings.m_IconOK, "OK - This setting is configured correctly");
            }

            if (m_iconWarningContent == null)
            {
                m_iconWarningContent = new GUIContent(GaiaSettings.m_IconWarning, "Warning - This setting might cause issues with Gaia");
            }

            if (m_iconErrorContent == null)
            {
                m_iconErrorContent = new GUIContent(GaiaSettings.m_IconError, "ERROR - This setting will cause severe issues / conflicts with Gaia");
            }
        }

        /// <summary>
        /// Checks whether the setting is in an appropiate state for using Gaia
        /// </summary>
        /// <returns></returns>
        public virtual bool PerformCheck()
        {
            return true;
        }

        /// <summary>
        /// Fixes the setting to a default value that is compatible with Gaia
        /// </summary>
        /// <returns></returns>
        public virtual bool FixNow(bool autoFix = false)
        {
            return true;
        }

        /// <summary>
        /// Restores the original value for the setting before the fix was applied
        /// </summary>
        /// <returns></returns>
        public virtual bool RestoreOriginalValue()
        {
            return true;
        }

        /// <summary>
        /// Gets a string representation of the original value that was stored when the fix button was pressed
        /// </summary>
        /// <returns></returns>
        public virtual string GetOriginalValueString()
        {
            return "Unknown";
        }

        /// <summary>
        /// Ignores the setting so that it is not constantly highlighted as a problem in the UI
        /// </summary>
        public void ToggleIgnore()
        {
            m_ignored = !m_ignored;
            if (m_ignored)
            {
                m_foldedOut = false;
            }
            if (OnGWSettingStatusChanged != null)
            {
                OnGWSettingStatusChanged(this);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Draws the UI for this entry in the Setup tab
        /// </summary>
        public void DrawUI()
        {
#if UNITY_EDITOR

            if (m_linkStyle == null || (m_linkStyle.normal.textColor.r + m_linkStyle.normal.textColor.g + m_linkStyle.normal.textColor.b == 0))
            {
                m_linkStyle = new GUIStyle(GUI.skin.label);
                m_linkStyle.fontStyle = FontStyle.Normal;
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = Color.white;
                m_linkStyle.stretchWidth = false;
            }

            EditorGUILayout.BeginHorizontal();

            if (m_status == GWSettingStatus.OK || m_ignored)
            {
                GUILayout.Label(m_iconOKContent);
            }
            else
            if (m_status == GWSettingStatus.Error)
            {
                GUILayout.Label(m_iconErrorContent);
            }
            else
            if (m_status == GWSettingStatus.Warning)
            {
                GUILayout.Label(m_iconWarningContent);
            }

            GUILayout.Space(5);

            m_foldedOut = EditorGUILayout.Foldout(m_foldedOut, m_name, true);

            GUILayout.FlexibleSpace();
            if ((m_status == GWSettingStatus.Error || m_status == GWSettingStatus.Warning) && !m_ignored)
            {
                if (GUILayout.Button("Ignore", GUILayout.Width(50)))
                {
                    ToggleIgnore();
                }
            }

            if ((m_status == GWSettingStatus.Error || m_status == GWSettingStatus.Warning) && !m_ignored)
            {
                if (GUILayout.Button("Fix", GUILayout.Width(50)))
                {
                   m_wasFixed = FixNow();
                    if (m_wasFixed)
                    {
                        m_foldedOut = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (m_foldedOut)
            {

                EditorGUI.indentLevel+=3;

                GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
                myStyle.fontSize = 12;

                if (m_status == GWSettingStatus.OK)
                {
                    EditorGUILayout.HelpBox(m_infoTextOK, MessageType.Info);
                }
                if (m_status == GWSettingStatus.Error)
                {
                    EditorGUILayout.HelpBox(m_infoTextIssue, MessageType.Error);
                }
                if (m_status == GWSettingStatus.Warning)
                {
                    EditorGUILayout.HelpBox(m_infoTextIssue, MessageType.Warning);
                }

                if (m_ignored)
                {
                    EditorGUILayout.HelpBox("You are currently ignoring this issue.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    if (GUILayout.Button("Stop Ignoring"))
                    {
                        ToggleIgnore();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (m_status== GWSettingStatus.OK && m_wasFixed && m_canRestore)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    GUILayout.Label($"Original Value: {GetOriginalValueString()}");
                   
                    GUILayout.Space(45);
                    if (GUILayout.Button("Restore"))
                    {
                        RestoreOriginalValue();
                    }
                    EditorGUILayout.EndHorizontal();
                }


                if (m_link != "")
                {
                    if (m_linkStyle == null || m_linkContent == null)
                    {
                        Initialize();
                    }

                    //EditorGUILayout.LabelField("Please find more information by following this link:");
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    if (ClickableHeaderCustomStyle(m_linkContent, m_linkStyle))
                    {
                        Application.OpenURL(m_link);
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
                EditorGUI.indentLevel-=3;
            }
#endif
        }
#if UNITY_EDITOR
    bool ClickableHeaderCustomStyle(GUIContent content, GUIStyle style, GUILayoutOption[] options = null)
    {
        var position = GUILayoutUtility.GetRect(content, style, options);
        Handles.BeginGUI();
        Color oldColor = Handles.color;
        Handles.color = style.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = oldColor;
        Handles.EndGUI();
        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
        return GUI.Button(position, content, style);
    }
#endif

        public static void RefreshActivePipeline()
        {
            m_activePipeline = GaiaUtils.GetActivePipeline();

            //We have to take both the render pipeline asset & scripting defines into account
            switch (m_activePipeline)
            {
                case GaiaConstants.EnvironmentRenderer.Universal:
#if !UPPipeline
                    m_activePipeline = GaiaConstants.EnvironmentRenderer.BuiltIn;
#endif
                    break;
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
#if !HDPipeline
                    m_activePipeline = GaiaConstants.EnvironmentRenderer.BuiltIn;
#endif
                    break;
            }

        }

        public bool RelevantForRenderPipeline()
        {
            //sort out generic parent classes, we do not want those to show up in the overview
            if (this.GetType() == typeof(GWS_HDRPBoolCheck))
            {
                return false;
            }

            switch (m_activePipeline)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                    if (m_RPBuiltIn)
                        return true;
                    break;
                case GaiaConstants.EnvironmentRenderer.Lightweight:
                    return false;
                case GaiaConstants.EnvironmentRenderer.Universal:
                    if (m_RPURP)
                        return true;
                    break;
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                    if (m_RPHDRP)
                        return true;
                    break;
            }
            return false; 
        }

        public int CompareTo(GWSetting otherSetting)
        {
            //sort warning items first, otherwise go by name of the item
            if (otherSetting.m_status == GWSettingStatus.OK && m_status == GWSettingStatus.Warning)
            {
                return -1;
            }
            else if (otherSetting.m_status == GWSettingStatus.Warning && m_status == GWSettingStatus.OK)
            {
                return 1;
            }
            else
            {
                return this.m_name.CompareTo(otherSetting.m_name);
            }
        }
    }
}
