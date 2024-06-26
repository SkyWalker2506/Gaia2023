using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine.Rendering;

namespace ProceduralWorlds.Setup
{
    /// <summary>
    /// Injects GAIA_CINEMACHINE define into project after assets have been installed
    /// </summary>
    [InitializeOnLoad]
    public class CinemachineDefineEditor : AssetPostprocessor
    {
        const string cmDefine = "GAIA_CINEMACHINE";

        static CinemachineDefineEditor()
        {

            bool updateScripting = false;
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));

            if (AddressablesPackageCheck())
            {
                if (!symbols.Contains(cmDefine))
                {
                    updateScripting = true;
                    symbols += ";" + cmDefine;
                }
            }
            else
            {
                if (symbols.Contains(cmDefine))
                {
                    updateScripting = true;
                    symbols = symbols.Replace(";" + cmDefine, "");
                    symbols = symbols.Replace(cmDefine, "");
                }
            }

            if (updateScripting)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), symbols);
            }
        }


        /// <summary>
        /// Checks if the addressables package is installed via reflection
        /// </summary>
        /// <returns></returns>
        public static bool AddressablesPackageCheck()
        {
            //Look for assembly
            var assemblies = CompilationPipeline.GetAssemblies();
            foreach (UnityEditor.Compilation.Assembly assembly in assemblies)
            {
                if (assembly.name.Contains("Cinemachine"))
                {
                    //was found -> we are done
                    return true;
                }
            }
            return false;
        }

    }
}