using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    [ExecuteAlways]
    public class TerrainDetailOverwrite : MonoBehaviour
    {
        #region Public Variables

        public GaiaConstants.TerrainDetailQuality m_detailQuality;
        public float m_gaiaDetailDistance;
        public float m_gaiaDetailFadeoutDistance;
        public int m_unityDetailDistance;
        public float m_unityDetailDensity;
        public Terrain m_terrain;

#endregion

#region Private Variables

        private bool m_applyTerrainChanges = false;

#endregion

#region Startup

        /// <summary>
        /// Apply on enable
        /// </summary>
        private void OnEnable()
        {
            if (m_terrain == null)
            {
                m_terrain = gameObject.GetComponent<Terrain>();
            }
            m_unityDetailDistance = (int)m_terrain.detailObjectDistance;
            m_unityDetailDensity = m_terrain.detailObjectDensity;
            GaiaUtils.SetTerrainDetailShaderDistances(m_gaiaDetailDistance, m_gaiaDetailFadeoutDistance);
            GetResolutionPatches();
        }
#endregion

#region Updates

        /// <summary>
        /// Applies detail distance settings if global then it'll apply to all instances
        /// </summary>
        /// <param name="isGlobal"></param>
        public void ApplySettings(bool isGlobal)
        {
            GaiaUtils.SetTerrainDetailShaderDistances(m_gaiaDetailDistance, m_gaiaDetailFadeoutDistance);
            if (isGlobal)
            {
                TerrainDetailOverwrite[] detailOverwrites = FindObjectsByType<TerrainDetailOverwrite>(FindObjectsSortMode.None);
                List<Terrain> terains = new List<Terrain>();
                if (detailOverwrites.Length > 0)
                {
                    foreach (TerrainDetailOverwrite detailOverwrite in detailOverwrites)
                    {
                        detailOverwrite.m_unityDetailDistance = m_unityDetailDistance;
                        detailOverwrite.m_unityDetailDensity = m_unityDetailDensity;
                        terains.Add(detailOverwrite.GetComponent<Terrain>());
                    }

                    foreach (Terrain terrain in terains)
                    {
                        if (terrain.detailObjectDistance != m_unityDetailDistance)
                        {
                            m_applyTerrainChanges = true;
                            terrain.detailObjectDistance = m_unityDetailDistance;
                        }

                        if (terrain.detailObjectDensity != m_unityDetailDensity)
                        {
                            m_applyTerrainChanges = true;
                            terrain.detailObjectDensity = m_unityDetailDensity;
                        }

                        if (m_applyTerrainChanges)
                        {
                            terrain.Flush();
                            m_applyTerrainChanges = false;
                        }
                    }
                }
            }
            else
            {
                if (m_terrain == null)
                {
                    m_terrain = gameObject.GetComponent<Terrain>();
                }

                if (m_terrain != null)
                {
                    if (m_terrain.detailObjectDistance != m_unityDetailDistance)
                    {
                        m_applyTerrainChanges = true;
                        m_terrain.detailObjectDistance = m_unityDetailDistance;
                    }

                    if (m_terrain.detailObjectDensity != m_unityDetailDensity)
                    {
                        m_applyTerrainChanges = true;
                        m_terrain.detailObjectDensity = m_unityDetailDensity;
                    }

                    if (m_applyTerrainChanges)
                    {
                        m_terrain.Flush();
                        m_applyTerrainChanges = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the detail patch resolution
        /// </summary>
        public void GetResolutionPatches()
        {
            if (m_terrain == null)
            {
                return;
            }

            if (m_terrain.terrainData == null)
            {
                return;
            }

            if (m_terrain.terrainData.detailResolutionPerPatch == 2)
            {
                m_detailQuality = GaiaConstants.TerrainDetailQuality.Ultra2;
            }
            else if (m_terrain.terrainData.detailResolutionPerPatch == 4)
            {
                m_detailQuality = GaiaConstants.TerrainDetailQuality.VeryHigh4;
            }
            else if (m_terrain.terrainData.detailResolutionPerPatch == 8)
            {
                m_detailQuality = GaiaConstants.TerrainDetailQuality.High8;
            }
            else if (m_terrain.terrainData.detailResolutionPerPatch == 16)
            {
                m_detailQuality = GaiaConstants.TerrainDetailQuality.Medium16;
            }
            else if (m_terrain.terrainData.detailResolutionPerPatch == 32)
            {
                m_detailQuality = GaiaConstants.TerrainDetailQuality.Low32;
            }
            else
            {
                m_detailQuality = GaiaConstants.TerrainDetailQuality.VeryLow64;
            }
        }

#endregion
    }
}