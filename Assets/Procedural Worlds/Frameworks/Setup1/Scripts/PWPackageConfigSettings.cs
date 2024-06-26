using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralWorlds.Setup
{
    [System.Serializable]
    public class PWPackageConfig
    {
        public string m_packageID;
        public PWPackageAction m_lastAction;
        public int m_majorVersion;
        public int m_minorVersion;
        public int m_patchVersion;
        public double m_installTimeStamp = 0;
    }

    /// <summary>
    /// Holds the user-made configuration about which package is installed, and which is not.
    /// This scriptable object will then "survive" updates in the User data folder to keep the configuration alive across updates.
    /// </summary>
    public class PWPackageConfigSettings : ScriptableObject
    {
        public List<PWPackageConfig> m_pWPackageConfigs = new List<PWPackageConfig>();
    }
}
