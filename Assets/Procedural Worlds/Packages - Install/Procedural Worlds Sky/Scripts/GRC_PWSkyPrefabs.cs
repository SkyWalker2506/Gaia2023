using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

[CreateAssetMenu(menuName = "Procedural Worlds/Gaia/PW Sky Prefabs")]
public class GRC_PWSkyPrefabs : ScriptableObject
{
    public GameObject m_weather;
    public GameObject m_directionalLight;
    public GameObject m_pwSky;
    public Material m_pwSkyBox;
    public GameObject m_postProcessURPPrefab;
    public GameObject m_postProcessbuiltInPrefab;
#if UPPipeline
    public VolumeProfile m_postprocessURP;
#endif
#if UNITY_POST_PROCESSING_STACK_V2
    public PostProcessProfile m_postprocessBuiltIn;
    
#endif
}
