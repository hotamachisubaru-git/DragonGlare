using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class BuffIcon : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text durationText;
        [SerializeField] private Image cooldownOverlay;

        public void SetBuff(Sprite icon, float duration, float maxDuration)
        {
            iconImage.sprite = icon;
            durationText.text = Mathf.CeilToInt(duration).ToString();
            cooldownOverlay.fillAmount = duration / maxDuration;
        }
    }
}
