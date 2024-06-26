using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{

    [CustomEditor(typeof(EnvironmentBuiltInURP))]
    public class EnvironmentBuiltInURPEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EnvironmentBuiltInURP environmentBuiltIn = (EnvironmentBuiltInURP)target;
            base.OnInspectorGUI();

#if HDPipeline
            EditorGUILayout.HelpBox("Please note that these settings can only be ingested using URP or Built-In - you are using HDRP at the moment!", MessageType.Warning);
            GUI.enabled = false;
#endif


            if (GUILayout.Button("Ingest From Scene"))
            {
                environmentBuiltIn.IngestFromScene();
            }
            if (GUILayout.Button("Apply To Scene"))
            {
                environmentBuiltIn.Apply();
            }
        }
    }
}
