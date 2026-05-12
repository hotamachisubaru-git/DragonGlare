using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class SkillIcon : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text cooldownText;
        [SerializeField] private KeyCode shortcutKey;
        [SerializeField] private Text shortcutText;

        public void SetSkill(Sprite icon, float cooldown, float maxCooldown)
        {
            iconImage.sprite = icon;
            cooldownOverlay.fillAmount = cooldown / maxCooldown;
            cooldownText.text = cooldown > 0 ? Mathf.CeilToInt(cooldown).ToString() : string.Empty;
            shortcutText.text = shortcutKey.ToString();
        }

        public void UpdateCooldown(float cooldown, float maxCooldown)
        {
            cooldownOverlay.fillAmount = cooldown / maxCooldown;
            cooldownText.text = cooldown > 0 ? Mathf.CeilToInt(cooldown).ToString() : string.Empty;
        }
    }
}
