using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    /// <summary>
    /// Simple Example Loading screen - you can build your own loading screen in a similar fashion by following this example
    /// In a nutshell there are 3 events from the Terrain Loader Manager you can subscribe to - when progress tracking has started, 
    /// when it updates and when it ends. What you do in those events is up to you, but normally you would initialize your loading screen on start
    /// update the progress bar (or a loading %, etc.) while it updates, and then close / shut down the loading screen when it ends.
    /// </summary>
    public class GaiaLoadingScreen : MonoBehaviour
    {
        public Slider m_progressBar;
        public Canvas m_canvas;
        public Image m_image;
        public Text m_text;
        public bool m_initialized;
        public float m_fadeOutSpeed;
        bool m_fadeout = false;

#if GAIA_2023_PRO
        void Start()
        {
            //Subscribe to these events to update your loading screen according to what is happening in the Load Tracking in the Terrain Loader Manager.
            TerrainLoaderManager.Instance.OnLoadProgressStarted += OnLoadProgressStarted;
            TerrainLoaderManager.Instance.OnLoadProgressUpdated += OnLoadProgressUpdated;
            TerrainLoaderManager.Instance.OnLoadProgressEnded += OnLoadProgressEnded;
            TerrainLoaderManager.Instance.OnLoadProgressTimeOut += OnLoadProgressTimeOut;
        }

        private void OnDestroy()
        {
            //Unsubscribe when the loading screen is destroyed.
            TerrainLoaderManager.Instance.OnLoadProgressStarted -= OnLoadProgressStarted;
            TerrainLoaderManager.Instance.OnLoadProgressUpdated -= OnLoadProgressUpdated;
            TerrainLoaderManager.Instance.OnLoadProgressEnded -= OnLoadProgressEnded;
            TerrainLoaderManager.Instance.OnLoadProgressTimeOut -= OnLoadProgressTimeOut;
        }
#endif

        /// <summary>
        /// Gets called when a load progress tracking starts in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressStarted()
        {
            //Load Progress Tracking started, let's reset the loading screen back into its initial state
            m_canvas.enabled = true;
            //make sure the black background image is not faded out
            m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, 1f);
            //reset progress on the loading bar
            m_progressBar.value = 0;
        }

        /// <summary>
        /// Gets called when the load progress tracking updates in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressUpdated(float progress)
        {
            //make sure the canvas is still visible
            m_canvas.enabled = true;
            //update the progress bar position with the progress from the Terrain Loader Manager
            m_progressBar.value = progress;

            //failsafe in case the load progress end process is not being called
            if (progress >= 1)
            {
                OnLoadProgressEnded();
            }
        }

        /// <summary>
        /// Gets called when the load progress tracking ends in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressEnded()
        {
            //begin fading out the screen
            m_fadeout = true;

            //deactivate the progress bar and text
            m_progressBar.gameObject.SetActive(false);
            m_text.enabled = false;
        }

        /// <summary>
        /// Gets called when the load progress tracking times out in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressTimeOut(List<TerrainScene> missingScenes)
        {

            string message = "Gaia Loading Screen Timeout, closing down loading screen.\r\n";
            message += "If you feel that there is no error but the terrain loading simply needs more time, try to increase the Timeout value in the Terrain Loader Manager.\r\n";
            message += "The following scenes are still not loaded:\r\n";

            foreach (TerrainScene terrainScene in missingScenes)
            {
                if (terrainScene == null)
                {
                    continue;
                }

                if (terrainScene.RegularReferences.Count > 0)
                {
                    message += $"\r\n {terrainScene.GetTerrainName()}";
                    message += $"\r\n Referencing Objects:\r\n";

                    foreach (GameObject regularGO in terrainScene.RegularReferences)
                    {
                        if (regularGO != null)
                        {
                            message += regularGO.name + ", ";
                        }
                    }
                }
                if (terrainScene.ImpostorReferences.Count > 0)
                {
                    message += $"\r\n {terrainScene.GetImpostorName()}";
                    message += $"\r\n Referencing Objects:\r\n";

                    foreach (GameObject regularGO in terrainScene.ImpostorReferences)
                    {
                        if (regularGO != null)
                        {
                            message += regularGO.name + ", ";
                        }
                    }
                }
                message += "\r\n";
            }
            Debug.LogWarning(message);
            OnLoadProgressEnded();
        }
            

        void Update()
        {
            //fade out the loading screen over time, then disable the canvas completely.
            if (m_fadeout)
            {
                m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, m_image.color.a - Time.deltaTime * m_fadeOutSpeed);
                if (m_image.color.a <= 0)
                {
                    //fully faded out, disable canvas and stop the fade out
                    m_canvas.enabled = false;
                    m_fadeout = false;
                }
            }
        }
    }
}