using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralWorlds.Setup
{
    /// <summary>
    /// Injects GAIA_INPUT_SYSTEM define into project after assets have been installed
    /// </summary>

    public class InputSystemDefineEditor : AssetPostprocessor
    {
        const string isDefine = "GAIA_INPUT_SYSTEM";

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {

            bool updateScripting = false;
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));

            if (InputSystemPackageCheck())
            {
                if (!symbols.Contains(isDefine))
                {
                    updateScripting = true;
                    symbols += ";" + isDefine;
                }
            }
            else
            {
                if (symbols.Contains(isDefine))
                {
                    updateScripting = true;
                    symbols = symbols.Replace(";" + isDefine, "");
                    symbols = symbols.Replace(isDefine, "");
                }
            }

            if (updateScripting)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), symbols);
            }
        }


        /// <summary>
        /// Checks if the Input System package is installed via reflection
        /// </summary>
        /// <returns></returns>
        public static bool InputSystemPackageCheck()
        {
            //Look for assembly
            var assemblies = CompilationPipeline.GetAssemblies();
            foreach (UnityEditor.Compilation.Assembly assembly in assemblies)
            {
                if (assembly.name.Contains("InputSystem"))
                {
                    //was found -> we are done
                    return true;
                }
            }
            return false;
        }

    }
}