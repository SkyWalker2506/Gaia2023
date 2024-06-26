using UnityEngine;

namespace Gaia
{
    public class PWSkyPostProcessingManager : MonoBehaviour
    {
        public void DisableWeatherPostFX()
        {
#if GAIA_2023_PRO
            if (ProceduralWorldsGlobalWeather.Instance != null)
            {
                ProceduralWorldsGlobalWeather.Instance.m_modifyPostProcessing = false;
            }
#endif
        }
        public void EnableWeatherPostFX()
        {
#if GAIA_2023_PRO
            if (ProceduralWorldsGlobalWeather.Instance != null)
            {
                ProceduralWorldsGlobalWeather.Instance.m_modifyPostProcessing = true;
            }
#endif
        }
    }
}