using System;
using Object = UnityEngine.Object;
namespace ProceduralWorlds.GTS
{
    public static class GTSEvents
    {
        private static Action<Object> m_onDestroy;
        public static Action<Object> Destroy
        {
            set => m_onDestroy = value;
            get
            {
                if (m_onDestroy == null)
                    m_onDestroy = DefaultDestroy;
                return m_onDestroy;
            }
        }
        private static void DefaultDestroy(Object @object) => Object.Destroy(@object);
    }
}