using UnityEngine;
using TMPro;

namespace Akila.MasterFPSCounter
{
    [RequireComponent(typeof(TextMeshProUGUI)), DisallowMultipleComponent]
    public class BasicFPSCounter : MonoBehaviour
    {
        private TextMeshProUGUI textMeshProUGUI;

        private float fps = 60;
        private int fpsAccumulator = 0;
        private float fpsPeriod = 0;
        private float nextUpdate;

        private void Start()
        {
            //Get defaults
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            fpsPeriod = Time.realtimeSinceStartup + 0.5f;
        }

        private void Update()
        {
            CalculateFramerate();

            //Update UI
            if (Time.time >= nextUpdate)
            {
                nextUpdate = Time.time + 1 / 10;
                textMeshProUGUI.text = $"<color=#808080>FPS: <color=#FFFFFF>{fps}";
            }
        }

        private void CalculateFramerate()
        {
            fpsAccumulator++;
            if (Time.realtimeSinceStartup > fpsPeriod)
            {
                fps = (int)(fpsAccumulator / 0.5f);
                fpsAccumulator = 0;
                fpsPeriod += 0.5f;
            }
        }
    }
}