using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    [CustomEditor(typeof(GaiaSceneReadMe))]
    public class GaiaSceneReadMeEditor : Editor
    {
        GUIStyle m_messageStyle;

        public override void OnInspectorGUI()
        {
            if(m_messageStyle == null || (m_messageStyle.normal.textColor.r == 0 && m_messageStyle.normal.textColor.g == 0 && m_messageStyle.normal.textColor.b == 0))
            {
                m_messageStyle = new GUIStyle(GUI.skin.box);
                m_messageStyle.alignment = TextAnchor.UpperLeft;
            }

            string message = "Gaia Scene ReadMe\r\n\r\n";
            message += "Gaia creates three different main scene objects in your scene:\r\n\r\n";
            message += "Gaia Tools: Contains the tools that you use to create your world. You can delete those when you are done with editing your terrains.\r\n\r\n";
            message += "Gaia Runtime: Contains runtime elements such as the lighting setup, water, etc. (if you use them).\r\n\r\n";
            message += "Gaia Terrains: Contains the terrains and the objects on them.\r\n\r\n";
            message += "Ultimatively the output of your terrain creation work is stored under Gaia Terrains. It is save to delete the tools if you do not need them anymore. You may delete Gaia Runtime as well if you do not intend to use any of the elements found in there. Gaia Runtime might restore itself as soon as you start to use Gaia again in the scene / project.";

            EditorGUILayout.LabelField(message, m_messageStyle);
            GUILayout.Space(10);
            if (GUILayout.Button("Delete this Readme Message"))
            {
                ((GaiaSceneReadMe)target).DeleteAll();
            }
        }
    }
}
