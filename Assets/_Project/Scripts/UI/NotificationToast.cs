using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class NotificationToast : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text messageText;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeSpeed = 2f;

        private float timer;
        private bool isShowing;

        private void Update()
        {
            if (!isShowing) return;

            timer += Time.deltaTime;
            if (timer >= displayDuration)
            {
                var alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                canvasGroup.alpha = alpha;
                if (alpha <= 0f)
                {
                    isShowing = false;
                    gameObject.SetActive(false);
                }
            }
        }

        public void Show(string message)
        {
            gameObject.SetActive(true);
            messageText.text = message;
            canvasGroup.alpha = 1f;
            timer = 0f;
            isShowing = true;
        }
    }
}
