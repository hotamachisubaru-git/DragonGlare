using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Text hpText;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;
        [SerializeField] private float updateSpeed = 5f;

        private float targetFillAmount;

        private void Update()
        {
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFillAmount, Time.deltaTime * updateSpeed);
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, fillImage.fillAmount);
        }

        public void SetHealth(int current, int max)
        {
            targetFillAmount = max > 0 ? (float)current / max : 0f;
            if (hpText != null)
                hpText.text = $"{current}/{max}";
        }
    }
}
