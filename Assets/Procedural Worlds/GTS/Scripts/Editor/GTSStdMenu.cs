using PWCommon5;
using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.GTS
{
    public static class GTSStdMenu
    {
        /// <summary>
        /// Show tutorials
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/GTS/Show GTS Tutorials...", false, 60)]
        public static void ShowTutorial() => Application.OpenURL(PWApp.CONF.TutorialsLink);
        /// <summary>
        /// Show support page
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/GTS/Show GTS Support, Lodge a Ticket...", false, 61)]
        public static void ShowSupport() => Application.OpenURL(PWApp.CONF.SupportLink);
        /// <summary>
        /// Show review option
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/GTS/Please Review GTS...", false, 62)]
        public static void ShowProductAssetStore() => Application.OpenURL(PWApp.CONF.ASLink);
    }
}