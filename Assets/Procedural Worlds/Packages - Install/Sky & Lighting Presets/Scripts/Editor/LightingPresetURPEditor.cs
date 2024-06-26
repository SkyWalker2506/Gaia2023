using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{

    [CustomEditor(typeof(LightingPresetURP))]
    public class LightingPresetURPEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            LightingPresetURP lightingPresetURP = (LightingPresetURP)target;
            base.OnInspectorGUI();

#if !UPPipeline
            EditorGUILayout.HelpBox("Please note that these settings can only be ingested using URP - you are using a different render pipeline at the moment!", MessageType.Warning);
            GUI.enabled = false;
#endif

            if (GUILayout.Button("Ingest From Scene"))
            {
                lightingPresetURP.IngestFromScene();
            }
            if (GUILayout.Button("Apply To Scene"))
            {
                lightingPresetURP.Apply();
            }
        }
    }
}
