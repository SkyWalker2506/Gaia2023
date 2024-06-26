using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Procedural Worlds/Gaia/Controller Settings")]
public class GRC_ControllerSettings : ScriptableObject
{
    public GameObject m_flyCamPrefab;
    public GameObject m_firstPersonControllerPrefab;
    public GameObject m_thirdPersonControllerPrefab;
}
