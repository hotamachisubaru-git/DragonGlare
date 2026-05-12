using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class AchievementNotification : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float slideOutDuration = 0.5f;

        private RectTransform rectTransform;
        private float timer;
        private bool isShowing;
        private Vector2 hiddenPosition;
        private Vector2 shownPosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            hiddenPosition = new Vector2(Screen.width + rectTransform.sizeDelta.x, rectTransform.anchoredPosition.y);
            shownPosition = rectTransform.anchoredPosition;
        }

        private void Update()
        {
            if (!isShowing) return;

            timer += Time.deltaTime;
            if (timer < slideInDuration)
            {
                float t = timer / slideInDuration;
                rectTransform.anchoredPosition = Vector2.Lerp(hiddenPosition, shownPosition, t);
            }
            else if (timer < slideInDuration + displayDuration)
            {
                rectTransform.anchoredPosition = shownPosition;
            }
            else if (timer < slideInDuration + displayDuration + slideOutDuration)
            {
                float t = (timer - slideInDuration - displayDuration) / slideOutDuration;
                rectTransform.anchoredPosition = Vector2.Lerp(shownPosition, hiddenPosition, t);
            }
            else
            {
                isShowing = false;
                gameObject.SetActive(false);
            }
        }

        public void Show(string title, string description, Sprite icon)
        {
            gameObject.SetActive(true);
            titleText.text = title;
            descriptionText.text = description;
            iconImage.sprite = icon;
            timer = 0f;
            isShowing = true;
            rectTransform.anchoredPosition = hiddenPosition;
        }
    }
}
