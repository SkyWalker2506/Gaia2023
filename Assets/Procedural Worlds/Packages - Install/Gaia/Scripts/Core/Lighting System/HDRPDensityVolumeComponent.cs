#if HDPipeline
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Gaia
{
    public class HDRPDensityVolumeComponent : MonoBehaviour
    {
        public LocalVolumetricFog m_densityVolume;
        public bool m_isVolumeActive = true;
        [SerializeField, HideInInspector]
        private Bounds m_volumeBounds;

        private void Start()
        {
           Setup();
        }
        /// <summary>
        /// Sets up this component
        /// </summary>
        public void Setup()
        {
            if (m_densityVolume == null)
            {
                m_densityVolume = GetComponent<LocalVolumetricFog>();
            }

            if (m_densityVolume != null)
            {
                m_volumeBounds = new Bounds(transform.position, m_densityVolume.parameters.size);
            }
        }
        /// <summary>
        /// Checks to see if a camera is within the bounds
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public bool IsInBounds(Camera camera)
        {
            if (camera != null && m_isVolumeActive)
            {
                if (m_volumeBounds.Contains(camera.transform.position))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif