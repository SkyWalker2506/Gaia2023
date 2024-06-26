using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Base class for a "Gaia Runtime Component" setting. A "Gaia Runtime Component" is an element added to the runtime of the scene, e.g. Lighting,
/// a player controller, etc. This class is a base class that can be derived from to implement the addition and the removal of said component to the scene.
/// It also draws the UI in the Gaia Manager Runtime tab.
/// 
/// </summary>

namespace Gaia
{

    [System.Serializable]
    abstract public class GaiaRuntimeComponent : ScriptableObject
    {
        /// <summary>
        /// Controls / stores whether the UI for this component is unfolded in the Gaia Manager or not
        /// </summary>
        public bool m_unfolded;

        /// <summary>
        /// When the component is "active", it will be applied with all other runtime settings from the Gaia Manager when runtime is generated
        /// </summary>
        public bool m_isActive = true;

        /// <summary>
        /// Tracks whether the "help" button is pressed for this component on the Gaia Manager
        /// </summary>
        public bool m_helpEnabled = false;

        /// <summary>
        /// This value determines the position of this component in the Gaia Manager GUI. The lower the number, the higher up it appears.
        /// </summary>
        public int m_orderNumber = int.MaxValue;

        /// <summary>
        /// Styling for help box links
        /// </summary>
        private GUIStyle m_helpLinkStyle;

        /// <summary>
        /// Styling for the help box
        /// </summary>
        private GUIStyle m_helpBoxStyle;


        /// <summary>
        /// Overrideable label property for the panel box in the Gaia Manager runtime tab
        /// </summary>
        abstract public GUIContent PanelLabel
        {
            get;
        }

        /// <summary>
        /// Initializes the component (if necessary), this is being called when the Gaia Manager opens
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Draws the UI for this component in the Gaia Manager Runtime tab
        /// </summary>
        public virtual void DrawUI()
        {
            
        }

        /// <summary>
        /// Adds this runtime component to the current scene
        /// </summary>
        public virtual void AddToScene()
        {
        }

        /// <summary>
        /// Removes this runtime component to the current scene
        /// </summary>
        public virtual void RemoveFromScene()
        {
        }


        /// <summary>
        /// Displays a help box with text and an optional link
        /// </summary>
        /// <param name="helpText"></param>
        /// <param name="linkContent"></param>
        public void DisplayHelp(string helpText, GUIContent linkContent = null, string linkTargetURL ="")
        {
            if (!m_helpEnabled)
            {
                return;
            }

            if (m_helpLinkStyle == null || (m_helpLinkStyle.normal.textColor.r + m_helpLinkStyle.normal.textColor.g + m_helpLinkStyle.normal.textColor.b == 0))
            {
                m_helpLinkStyle = new GUIStyle(GUI.skin.label);
                m_helpLinkStyle.fontStyle = FontStyle.Normal;
                m_helpLinkStyle.wordWrap = false;
                m_helpLinkStyle.normal.textColor = Color.white;
                m_helpLinkStyle.stretchWidth = false;
            }

            if (m_helpBoxStyle == null || (m_helpBoxStyle.normal.textColor.r + m_helpBoxStyle.normal.textColor.g + m_helpBoxStyle.normal.textColor.b == 0))
            {
                m_helpBoxStyle = new GUIStyle(GUI.skin.box);
                m_helpBoxStyle.margin = new RectOffset(5, 5, 5, 5);
                m_helpBoxStyle.padding = new RectOffset(0, 0, 0, 0);
                m_helpBoxStyle.alignment = TextAnchor.UpperLeft;
                m_helpBoxStyle.stretchWidth = true;
                m_helpBoxStyle.richText = true;
                m_helpBoxStyle.wordWrap = true;
            }


            GUILayout.BeginVertical(m_helpBoxStyle);
            {
                GUILayout.Label(helpText, m_helpBoxStyle);
                if (linkContent != null)
                {
 #if UNITY_EDITOR
                    if (ClickableHeaderCustomStyle(linkContent, m_helpLinkStyle))
                    {
                        Application.OpenURL(linkTargetURL);
                    }
#endif
                }
            }
            GUILayout.EndVertical();
        
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

    }
}
