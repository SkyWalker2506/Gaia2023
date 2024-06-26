using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Gaia
{

    [CustomEditor(typeof(LightingPresetHDRP))]
    public class LightingPresetHDRPEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            LightingPresetHDRP lightingPresetHDRP = (LightingPresetHDRP)target;
            base.OnInspectorGUI();

#if !HDPipeline
            EditorGUILayout.HelpBox("Please note that these settings can only be ingested using HDRP - you are using a different render pipeline at the moment!", MessageType.Warning);
            GUI.enabled = false;
#endif

            if (GUILayout.Button("Ingest From Scene"))
            {
                lightingPresetHDRP.IngestFromScene();
            }
            if (GUILayout.Button("Apply To Scene"))
            {
                lightingPresetHDRP.Apply();
            }
        }
    }
}
