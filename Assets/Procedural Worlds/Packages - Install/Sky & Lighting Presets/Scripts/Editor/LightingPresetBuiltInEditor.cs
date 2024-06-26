using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{

    [CustomEditor(typeof(LightingPresetBuiltIn))]
    public class LightingPresetBuiltInEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LightingPresetBuiltIn lightingPresetBuiltIn = (LightingPresetBuiltIn)target;
            base.OnInspectorGUI();

#if UPPipeline || HDPipeline
            EditorGUILayout.HelpBox("Please note that these settings can only be ingested using built-in rendering - you are using a different render pipeline at the moment!", MessageType.Warning);
            GUI.enabled = false;
#endif

            if (GUILayout.Button("Ingest From Scene"))
            {
                lightingPresetBuiltIn.IngestFromScene();
            }
            if (GUILayout.Button("Apply To Scene"))
            {
                lightingPresetBuiltIn.Apply();
            }
        }
    }
}
