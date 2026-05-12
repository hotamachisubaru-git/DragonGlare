using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private Image healthFill;
        [SerializeField] private Image shieldFill;
        [SerializeField] private Text bossNameText;
        [SerializeField] private Text healthPercentText;

        public void SetBossName(string name)
        {
            bossNameText.text = name;
        }

        public void SetHealth(float healthPercent)
        {
            healthFill.fillAmount = healthPercent;
            healthPercentText.text = $"{Mathf.RoundToInt(healthPercent * 100f)}%";
        }

        public void SetShield(float shieldPercent)
        {
            shieldFill.fillAmount = shieldPercent;
        }
    }
}
