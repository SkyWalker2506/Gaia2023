using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Akila.MasterFPSCounter
{
    public class InfoContainer : MonoBehaviour
    {
        public TextMeshProUGUI fpsText;
        public TextMeshProUGUI averageFPSText;
        public TextMeshProUGUI maxFPSText;
        public TextMeshProUGUI minFPSText;

        private float fps = 60;
        private float averageFPS = 60;
        private float maxFPS = 60;
        private float minFPS = 60;

        private int fpsAccumulator = 0;
        private float fpsPeriod = 0;
        private float nextUpdate;

        private List<float> fpsHistory = new List<float>();
        
        private void Start()
        {
            //Get defaults
            fpsPeriod = Time.realtimeSinceStartup + 0.5f;
        }

        private void Update()
        {
            CalculateFramerate();

            //Update UI
            if (Time.time >= nextUpdate)
            {
                nextUpdate = Time.time + 1 / 10;
                UpdateUI();
            }
        }

        private void CalculateFramerate()
        {
            fpsAccumulator++;
            if (Time.realtimeSinceStartup > fpsPeriod)
            {
                fps = (int)(fpsAccumulator / 0.5f);
                fpsHistory.Add(fps);
                
                fpsAccumulator = 0;
                fpsPeriod += 0.5f;
            }

            float totalFPS = 0;
            foreach(float frame in fpsHistory)
            {
                totalFPS += frame;
            }

            float numberOfFrames = fpsHistory.Count + 1;
            averageFPS = (int)totalFPS / Mathf.Clamp(numberOfFrames, 1, 10e10f);

            CalculateMinAndMaxFPS();

            //Clear FPS History each 100 frame
            if (fpsHistory.ToArray().Length > 100) fpsHistory.RemoveRange(1, 99);
        }

        private void CalculateMinAndMaxFPS()
        {
            float max = fpsHistory.Count > 0 ? fpsHistory[0] : 60;
            float min = fpsHistory.Count > 0 ? fpsHistory[0] : 60;

            int index = 0;

            for (int i = 1; i < fpsHistory.Count; i++)
            {
                if (fpsHistory[i] < min)
                {
                    min = fpsHistory[i];

                    index = i;
                }
            }

            for (int i = 1; i < fpsHistory.Count; i++)
            {
                if (fpsHistory[i] > max)
                {
                    max = fpsHistory[i];

                    index = i;
                }
            }

            maxFPS = max;
            minFPS = min;
        }

        private void UpdateUI()
        {
            if (fpsText)
                fpsText.text = $"<color=#808080>FPS: <color=#FFFFFF>{(int)fps}";
            else 
                Debug.LogWarning("Couldn't find a text mesh pro assgined in 'FPS Text', Info Container will ignore it.", gameObject);

            if (averageFPSText)
                averageFPSText.text = $"<color=#808080>Average FPS: <color=#FFFFFF>{(int)averageFPS}";
            else
                Debug.LogWarning("Couldn't find a text mesh pro assgined in 'Average FPS Text', Info Container will ignore it.", gameObject);

            if (maxFPSText)
                maxFPSText.text = $"<color=#808080>Max FPS: <color=#FFFFFF>{(int)maxFPS}";
            else
                Debug.LogWarning("Couldn't find a text mesh pro assgined in 'Max FPS', Info Container will ignore it.", gameObject);

            if (minFPSText)
                minFPSText.text = $"<color=#808080>Min FPS: <color=#FFFFFF>{(int)minFPS}";
            else
                Debug.LogWarning("Couldn't find a text mesh pro assgined in 'Min FPS', Info Container will ignore it.", gameObject);
        }
    }
}