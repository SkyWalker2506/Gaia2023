using UnityEngine;
using UnityEditor;
using PWCommon5;
using Gaia.Internal;
using System.Collections.Generic;

namespace Gaia
{
    [CustomEditor(typeof(InteriorWeatherController))]
    public class InteriorWeatherControllerEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private InteriorWeatherController m_profile;

        private void OnEnable()
        {
            //Get Profile object
            m_profile = (InteriorWeatherController)target;
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
        }

        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

            //Monitor for changes
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Panel("Settings", GlobalSettingsEnabled, true);

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profile, "Made changes");
                EditorUtility.SetDirty(m_profile);
            }
        }

        private void GlobalSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_editorUtils.Heading("TriggerSettings");
            m_profile.TriggerType = (CollisionDetectionType)m_editorUtils.EnumPopup("TriggerType", m_profile.TriggerType, helpEnabled);
            if (m_profile.TriggerType == CollisionDetectionType.Box)
            {
                m_profile.TriggerSize = m_editorUtils.Vector3Field("TriggerSize", m_profile.TriggerSize, helpEnabled);
            }
            else
            {
                m_profile.TriggerRadius = m_editorUtils.FloatField("TriggerRadius", m_profile.TriggerRadius, helpEnabled);
            }
            m_profile.m_triggerMode = (InteriorWeatherTriggerMode)m_editorUtils.EnumPopup("TriggerMode", m_profile.m_triggerMode, helpEnabled);
            switch (m_profile.m_triggerMode)
            {
                case InteriorWeatherTriggerMode.Trigger:
                {
                    m_profile.m_playerTag = m_editorUtils.TextField("PlayerTag", m_profile.m_playerTag, helpEnabled);
                    break;
                }
                case InteriorWeatherTriggerMode.Bounds:
                {
                    m_profile.m_playerTransform = (Transform)m_editorUtils.ObjectField("PlayerTransform", m_profile.m_playerTransform, typeof(Transform), true, helpEnabled);
                    break;
                }
            }
            EditorGUILayout.Space();

            m_editorUtils.Heading("ReverbSettings");
            m_profile.m_interiorAudioRevertPreset = (AudioReverbPreset)m_editorUtils.EnumPopup("InteriorReverbPreset", m_profile.m_interiorAudioRevertPreset, helpEnabled);
            m_profile.m_exteriorAudioRevertPreset = (AudioReverbPreset)m_editorUtils.EnumPopup("ExteriorReverbPreset", m_profile.m_exteriorAudioRevertPreset, helpEnabled);
            EditorGUILayout.Space();

            m_editorUtils.Heading("WeatherSettings");
            m_profile.m_controllerMode = (InteriorWeatherControllerMode)m_editorUtils.EnumPopup("ControllerMode", m_profile.m_controllerMode, helpEnabled);
            EditorGUI.indentLevel++;
            switch (m_profile.m_controllerMode)
            {
                case InteriorWeatherControllerMode.Collision:
                {
                    m_profile.m_colliderQuality = (ParticleSystemCollisionQuality)m_editorUtils.EnumPopup("ColliderQuality", m_profile.m_colliderQuality, helpEnabled);
                    if (GaiaUtils.GetActivePipeline() == GaiaConstants.EnvironmentRenderer.HighDefinition && m_profile.m_colliderQuality == ParticleSystemCollisionQuality.High)
                    {
                        EditorGUILayout.HelpBox("Collider Quality can be really expensive in HDRP and we recommend using Low or Medium quality. High will cause some performance issues", MessageType.Info);
                    }
                    m_profile.m_collideLayers = LayerMaskField(m_editorUtils, "ColliderLayers", m_profile.m_collideLayers, helpEnabled);
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("CollisionMode"), MessageType.Info);
                    break;
                }
                case InteriorWeatherControllerMode.DisableVFX:
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("DisableWeatherVFXMode"), MessageType.Info);
                    break;
                }
            }
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }

        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="label"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static LayerMask LayerMaskField(EditorUtils m_editorUtils, string label, LayerMask layerMask, bool helpEnabled)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }
            maskWithoutEmpty = m_editorUtils.MaskField(label, maskWithoutEmpty, layers.ToArray(), helpEnabled);
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }
            }
            layerMask.value = mask;
            return layerMask;
        }
    }
}