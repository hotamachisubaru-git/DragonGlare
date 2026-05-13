using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class CreditsRoll : MonoBehaviour
    {
        [SerializeField] private RectTransform creditsPanel;
        [SerializeField] private float scrollSpeed = 50f;
        [SerializeField] private Button skipButton;

        private bool isScrolling;
        private float startPosition;
        private float endPosition;

        private void Awake()
        {
            skipButton.onClick.AddListener(SkipCredits);
        }

        private void Update()
        {
            if (!isScrolling) return;

            creditsPanel.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            if (creditsPanel.anchoredPosition.y >= endPosition)
            {
                isScrolling = false;
                OnCreditsFinished();
            }
        }

        public void StartCredits()
        {
            gameObject.SetActive(true);
            startPosition = -Screen.height;
            endPosition = creditsPanel.sizeDelta.y;
            creditsPanel.anchoredPosition = new Vector2(0f, startPosition);
            isScrolling = true;
        }

        private void SkipCredits()
        {
            isScrolling = false;
            OnCreditsFinished();
        }

        private void OnCreditsFinished()
        {
            gameObject.SetActive(false);
            // Return to title screen or main menu
        }
    }
}
