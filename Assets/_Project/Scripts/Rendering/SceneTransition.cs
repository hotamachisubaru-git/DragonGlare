using UnityEngine;

namespace DragonGlare
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private float targetAlpha;
        private float currentAlpha;
        private bool isTransitioning;

        private void Update()
        {
            if (!isTransitioning) return;

            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime / fadeDuration);
            fadeGroup.alpha = currentAlpha;

            if (Mathf.Approximately(currentAlpha, targetAlpha))
            {
                isTransitioning = false;
            }
        }

        public void FadeIn()
        {
            targetAlpha = 0f;
            currentAlpha = 1f;
            fadeGroup.alpha = 1f;
            isTransitioning = true;
        }

        public void FadeOut()
        {
            targetAlpha = 1f;
            currentAlpha = 0f;
            fadeGroup.alpha = 0f;
            isTransitioning = true;
        }

        public void SetImmediate(float alpha)
        {
            currentAlpha = alpha;
            targetAlpha = alpha;
            fadeGroup.alpha = alpha;
            isTransitioning = false;
        }
    }
}
