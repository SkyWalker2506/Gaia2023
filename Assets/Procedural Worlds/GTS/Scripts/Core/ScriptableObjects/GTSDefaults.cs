using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralWorlds.GTS
{
    [CreateAssetMenu(fileName = "GTS Defaults", menuName = "Procedural Worlds/GTS/Defaults", order = 0)]
    public class GTSDefaults : ScriptableObject
    {
        #region Singleton

        private static GTSDefaults m_instance = null;

        public static GTSDefaults Instance
        {
            get
            {
#if UNITY_EDITOR
                if (m_instance == null)
                    m_instance = AssetDatabase.LoadAssetAtPath<GTSDefaults>(GTSConstants.GTSDefaultsPath);
#endif
                return m_instance;
            }
        }

        #endregion

        #region Variables

        [SerializeField] public bool debugEnabled = true;
        [SerializeField] public Material builtInMaterial;
        [SerializeField] public Material urpMaterial;
        [SerializeField] public Material hdrpMaterial;
        [SerializeField] public Object urpPackage;
        [SerializeField] public Object hdrpPackage;
        [SerializeField] public Object hdrp2022Package;
        [SerializeField] public Object hdrp2022_3_Package;
        [SerializeField] public Object hdrp2023_1_Package;
        [SerializeField] public Object documentationPDF;
        [SerializeField] public Object shaderFolder;

        #endregion

        #region Properties

        public bool packagesImported
        {
            get
            {
                GTSUtility.GetPipelineShaders(out Shader gtsShader, out Shader unityShader);
                if (gtsShader == null)
                    return false;
                return true;
            }
        }

        #endregion

        #region Methods

#if UNITY_EDITOR
        public static Material GetDefaultTerrainMaterial()
        {
            GTSDefaults defaults = Instance;
            if (defaults == null)
                return null;
            GTSPipeline currentPipeline = GTSUtility.GetCurrentPipeline();
            switch (currentPipeline)
            {
                case GTSPipeline.BuiltIn:
                    return defaults.builtInMaterial;
                case GTSPipeline.URP:
                    return defaults.urpMaterial;
                case GTSPipeline.HDRP:
                    return defaults.hdrpMaterial;
            }

            return null;
        }
#endif

        #endregion
    }
}