using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SaveIcon : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private float rotateSpeed = 360f;
        [SerializeField] private float fadeSpeed = 2f;

        private bool isSaving;
        private float targetAlpha;

        private void Update()
        {
            if (isSaving)
            {
                iconImage.rectTransform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
            }

            var alpha = Mathf.MoveTowards(iconImage.color.a, targetAlpha, Time.deltaTime * fadeSpeed);
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, alpha);
        }

        public void Show()
        {
            isSaving = true;
            targetAlpha = 1f;
        }

        public void Hide()
        {
            isSaving = false;
            targetAlpha = 0f;
        }
    }
}
