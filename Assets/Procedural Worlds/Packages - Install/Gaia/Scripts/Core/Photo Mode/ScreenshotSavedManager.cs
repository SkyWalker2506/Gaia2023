using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if GAIA_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Gaia
{
    public class ScreenshotSavedManager : MonoBehaviour
    {
        public static ScreenshotSavedManager Instance
        {
            get { return m_instance; }
        }
        [SerializeField]
        private static ScreenshotSavedManager m_instance;

        public bool m_useShowScreenshotSavedText = true;
        public Text m_screenshotSavedText;
        public Text m_screenshotSavePath;
        public float m_showTimeInSeconds = 1f;

        private bool m_screenshotterPresent = false;
        private ScreenShotter m_screenshotter;

        private void Awake()
        {
            m_instance = this;
            if (m_screenshotSavedText != null)
            {
                m_screenshotSavedText.gameObject.SetActive(false);
            }

            if (m_screenshotSavePath != null)
            {
                m_screenshotSavePath.gameObject.SetActive(false);
            }

            m_screenshotter = FindAnyObjectByType<ScreenShotter>();
            m_screenshotterPresent = m_screenshotter != null;
        }

        private void Update()
        {
            if (m_screenshotterPresent)
            {
#if GAIA_INPUT_SYSTEM
                if (Keyboard.current[m_screenshotter.m_screenShotKey].wasPressedThisFrame)
                {
                    ShowScreenshotSavedText();
                }
#endif
            }
        }
        private IEnumerator ShowScreenshotTakenText()
        {
            yield return new WaitForEndOfFrame();
            if (m_screenshotSavedText != null)
            {
                m_screenshotSavedText.gameObject.SetActive(true);
                if (m_screenshotSavePath != null && m_screenshotterPresent)
                {
                    m_screenshotSavePath.text = m_screenshotter.m_lastSavedPath;
                    m_screenshotSavePath.gameObject.SetActive(true);
                }
            }
            yield return new WaitForSeconds(m_showTimeInSeconds);
            if (m_screenshotSavedText != null)
            {
                m_screenshotSavedText.gameObject.SetActive(false);
                if (m_screenshotSavePath != null && m_screenshotterPresent)
                {
                    m_screenshotSavePath.gameObject.SetActive(false);
                }
            }
            StopAllCoroutines();
        }
        /// <summary>
        /// Function used to show the text for x amount of time
        /// </summary>
        public void ShowScreenshotSavedText()
        {
            if (m_useShowScreenshotSavedText)
            {
                StopAllCoroutines();
                StartCoroutine(ShowScreenshotTakenText());
            }
            else
            {
                StopAllCoroutines();
            }
        }
    }
}