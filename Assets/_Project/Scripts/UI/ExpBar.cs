using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class ExpBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Text levelText;
        [SerializeField] private float updateSpeed = 5f;

        private float targetFillAmount;

        private void Update()
        {
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFillAmount, Time.deltaTime * updateSpeed);
        }

        public void SetExperience(int current, int required, int level)
        {
            targetFillAmount = required > 0 ? (float)current / required : 0f;
            if (levelText != null)
                levelText.text = $"Lv.{level}";
        }
    }
}
