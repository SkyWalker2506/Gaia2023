using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Setup
{
    [CreateAssetMenu(menuName = "Procedural Worlds/Setup/Setup Settings")]
    public class SetupSettings : ScriptableObject
    {
        public Texture2D m_IconWarning;
        public Texture2D m_IconOK;
        public Texture2D m_IconNoUnityPackage;
        public Texture2D m_IconNotInstalled;
        public Texture2D m_IconUpdateAvailable;
    }
}