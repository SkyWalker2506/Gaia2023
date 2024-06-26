using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    public class PWSkyStandalone : MonoBehaviour
    {
        public GaiaTimeOfDay m_gaiaTimeOfDay = new GaiaTimeOfDay();
        public GaiaLightingProfileValues m_profileValues = new GaiaLightingProfileValues();

        private static PWSkyStandalone m_pwSkyStandalone;
        public static PWSkyStandalone Instance
        {
            get 
            {
                if (m_pwSkyStandalone == null)
                {
                    m_pwSkyStandalone = FindAnyObjectByType<PWSkyStandalone>();
                }
                return m_pwSkyStandalone;
            }
        }

        Light m_sunLight, m_moonLight;
        bool m_sunLightExists, m_moonLightExists;
        public void UpdateGaiaTimeOfDay(bool revertDefault)
        {
#if GAIA_2023_PRO
            if (ProceduralWorldsGlobalWeather.Instance !=null)
            {
                bool applicationUpdate = !ProceduralWorldsGlobalWeather.Instance.IsRaining;
                if (ProceduralWorldsGlobalWeather.Instance.IsSnowing)
                {
                    applicationUpdate = false;
                }
                if (Application.isPlaying && applicationUpdate)
                {
                    PW_VFX_Atmosphere.Instance.UpdateSystem();
                }

                UpdateNightMode();
            }
#endif
        }

        public static float GetTimeOfDayMainValue()
        {
            if (Instance == null)
            {
                return 0f;
            }
            float value = 0;
            value = ((Instance.m_gaiaTimeOfDay.m_todHour * 60f) + Instance.m_gaiaTimeOfDay.m_todMinutes) / 1440f;
            return value;
        }

        public void UpdateGaiaWeather()
        {
#if GAIA_2023_PRO
            if (ProceduralWorldsGlobalWeather.Instance != null)
            {
                //ProceduralWorldsGlobalWeather.Instance.Season = SceneProfile.m_gaiaWeather.m_season;
                //ProceduralWorldsGlobalWeather.Instance.WindDirection = SceneProfile.m_gaiaWeather.m_windDirection;
            }
#endif
        }

        public void Update()
        {
#if GAIA_2023_PRO
            //if (!Application.isPlaying)
            //{
            //    WeatherPresent = CheckWeatherPresent();
            //    WeatherSystem = ProceduralWorldsGlobalWeather.Instance;
            //}

            ProceduralWorldsGlobalWeather weatherSystem = ProceduralWorldsGlobalWeather.Instance;

            if (weatherSystem != null)
            {
                if (weatherSystem.CheckIsNight())
                {
                    Shader.SetGlobalVector(GaiaShaderID.m_globalLightDirection, -weatherSystem.m_moonLight.transform.forward);
                    Shader.SetGlobalColor(GaiaShaderID.m_globalLightColor, new Vector4(weatherSystem.m_moonLight.color.r * weatherSystem.m_moonLight.intensity, weatherSystem.m_moonLight.color.g * weatherSystem.m_moonLight.intensity, weatherSystem.m_moonLight.color.b * weatherSystem.m_moonLight.intensity, weatherSystem.m_moonLight.color.a * weatherSystem.m_moonLight.intensity));
                }
                else
                {
                    Shader.SetGlobalVector(GaiaShaderID.m_globalLightDirection, -weatherSystem.m_sunLight.transform.forward);
                    Shader.SetGlobalColor(GaiaShaderID.m_globalLightColor, new Vector4(weatherSystem.m_sunLight.color.r * weatherSystem.m_sunLight.intensity, weatherSystem.m_sunLight.color.g * weatherSystem.m_sunLight.intensity, weatherSystem.m_sunLight.color.b * weatherSystem.m_sunLight.intensity, weatherSystem.m_sunLight.color.a * weatherSystem.m_sunLight.intensity));
                }
            }
            else
            {
                if (m_sunLightExists)
                {
                    if (m_sunLight != null)
                    {
                        Shader.SetGlobalVector(GaiaShaderID.m_globalLightDirection, -m_sunLight.transform.forward);
                        Shader.SetGlobalColor(GaiaShaderID.m_globalLightColor, m_sunLight.color * m_sunLight.intensity);
                    }
                }
            }
#else
            if (m_sunLightExists)
            {
                Shader.SetGlobalVector(GaiaShaderID.m_globalLightDirection, -m_sunLight.transform.forward);
                Shader.SetGlobalVector(GaiaShaderID.m_globalLightColor, m_sunLight.color * m_sunLight.intensity);
            }
#endif

#if GAIA_2023_PRO
            if (ProceduralWorldsGlobalWeather.Instance)
            {
                if (Application.isPlaying)
                {
                    if (m_gaiaTimeOfDay.m_todEnabled)
                    {
                        m_gaiaTimeOfDay.m_todMinutes += Time.deltaTime * m_gaiaTimeOfDay.m_todDayTimeScale;
                    }
                }
                else
                {

                    if (weatherSystem!=null && weatherSystem.RunInEditor)
                    {
                        m_gaiaTimeOfDay.m_todMinutes += Time.deltaTime * m_gaiaTimeOfDay.m_todDayTimeScale;
                    }
                }

                if (m_gaiaTimeOfDay.m_todMinutes > 59.1f)
                {
                    m_gaiaTimeOfDay.m_todMinutes = 0f;
                    m_gaiaTimeOfDay.m_todHour++;
                }

                if (m_gaiaTimeOfDay.m_todHour > 23)
                {
                    m_gaiaTimeOfDay.m_todHour = 0;
                }

                UpdateGaiaTimeOfDay(false);
            }
#endif
        }

        /// <summary>
        /// Update the night mode stuff
        /// </summary>
        public void UpdateNightMode()
        {
#if GAIA_2023_PRO
            if (ProceduralWorldsGlobalWeather.Instance!=null)
            {
                if (m_sunLight == null)
                {
                    m_sunLight = GaiaUtils.GetMainDirectionalLight(false);
                    if (m_sunLight != null)
                    {
                        m_sunLightExists = true;
                    }
                }
                else
                {
                    m_sunLightExists = true;
                }

                if (m_moonLight == null)
                {
                    m_moonLight = GaiaUtils.GetMainMoonLight(false);

                    if (m_moonLight != null)
                    {
                        m_moonLightExists = true;
                    }
                }
                else
                {
                    m_moonLightExists = true;
                }

                if (ProceduralWorldsGlobalWeather.Instance.CheckIsNight())
                {
                    if (m_moonLightExists)
                    {
                        RenderSettings.sun = m_moonLight;
                    }

                    if (m_sunLightExists)
                    {
//#if HDPipeline
//                        if (SunHDLightData == null)
//                        {
//                            SunHDLightData = GaiaHDRPRuntimeUtils.GetHDLightData(m_sunLight);
//                        }

//                        SunHDLightData.intensity = 0;
//                        SunHDLightData.lightUnit = LightUnit.Lux;
//#endif
                        m_sunLight.intensity = 0f;
                    }
                }
                else
                {
                    if (m_moonLightExists)
                    {
//#if HDPipeline
//                        if (MoonHDLightData == null)
//                        {
//                            MoonHDLightData = GaiaHDRPRuntimeUtils.GetHDLightData(m_moonLight);
//                        }

//                        MoonHDLightData.intensity = 0;
//                        MoonHDLightData.lightUnit = LightUnit.Lux;
//#endif
                        m_moonLight.intensity = 0f;
                    }

                    if (m_sunLightExists)
                    {
                        RenderSettings.sun = m_sunLight;
                    }
                }
            }
#endif
        }



    }
}
