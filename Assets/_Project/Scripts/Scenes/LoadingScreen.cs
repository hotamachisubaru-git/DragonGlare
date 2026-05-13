using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text loadingText;
        [SerializeField] private float fadeSpeed = 2f;

        private bool isShowing;
        private float targetAlpha;

        private void Update()
        {
            var alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = isShowing;
            canvasGroup.blocksRaycasts = isShowing;
        }

        public void Show(string message = "Loading...")
        {
            isShowing = true;
            targetAlpha = 1f;
            loadingText.text = message;
        }

        public void Hide()
        {
            isShowing = false;
            targetAlpha = 0f;
        }
    }
}
