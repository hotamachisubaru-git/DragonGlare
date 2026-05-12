using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class TimerDisplay : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private bool countDown;
        [SerializeField] private float startTime = 300f;

        private float currentTime;
        private bool isRunning;

        private void Update()
        {
            if (!isRunning) return;

            if (countDown)
            {
                currentTime -= Time.deltaTime;
                if (currentTime <= 0f)
                {
                    currentTime = 0f;
                    isRunning = false;
                    OnTimerFinished();
                }
            }
            else
            {
                currentTime += Time.deltaTime;
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        public void StartTimer()
        {
            isRunning = true;
            if (countDown)
                currentTime = startTime;
            else
                currentTime = 0f;
        }

        public void StopTimer()
        {
            isRunning = false;
        }

        public void ResetTimer()
        {
            currentTime = countDown ? startTime : 0f;
            UpdateDisplay();
        }

        private void OnTimerFinished()
        {
            // Handle timer finished event
        }
    }
}
