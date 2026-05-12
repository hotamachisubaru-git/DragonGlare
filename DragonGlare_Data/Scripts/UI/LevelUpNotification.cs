using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class LevelUpNotification : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text levelText;
        [SerializeField] private Text statsText;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeSpeed = 2f;

        private float timer;
        private bool isShowing;

        private void Update()
        {
            if (!isShowing) return;

            timer += Time.deltaTime;
            if (timer >= displayDuration)
            {
                float alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                canvasGroup.alpha = alpha;
                if (alpha <= 0f)
                {
                    isShowing = false;
                    gameObject.SetActive(false);
                }
            }
        }

        public void Show(int newLevel, string statChanges)
        {
            gameObject.SetActive(true);
            levelText.text = $"Level Up! Lv.{newLevel}";
            statsText.text = statChanges;
            canvasGroup.alpha = 1f;
            timer = 0f;
            isShowing = true;
        }
    }
}
