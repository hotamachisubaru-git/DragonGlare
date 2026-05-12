using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private Text fpsText;
        [SerializeField] private float updateInterval = 0.5f;

        private float accum;
        private int frames;
        private float timeleft;

        private void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;

            if (timeleft <= 0f)
            {
                float fps = accum / frames;
                fpsText.text = $"{fps:F1} FPS";
                timeleft = updateInterval;
                accum = 0f;
                frames = 0;
            }
        }
    }
}
