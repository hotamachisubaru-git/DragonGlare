using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class BattleEffectAnimator : MonoBehaviour
    {
        [SerializeField] private Image slashImage;
        [SerializeField] private Image spellBurstImage;
        [SerializeField] private Image statusCloudImage;
        [SerializeField] private Image healImage;
        [SerializeField] private Image guardImage;
        [SerializeField] private Image itemSparkleImage;
        [SerializeField] private Image enemyDefeatImage;

        public void PlaySlash(Vector2 position, float progress)
        {
            slashImage.gameObject.SetActive(true);
            slashImage.rectTransform.anchoredPosition = position;
            var alpha = Mathf.Clamp(220 - Mathf.RoundToInt(progress * 140f), 70, 220);
            slashImage.color = new Color(246f / 255f, 244f / 255f, 228f / 255f, alpha / 255f);
        }

        public void PlaySpellBurst(Vector2 center, float progress)
        {
            spellBurstImage.gameObject.SetActive(true);
            var radius = 18 + Mathf.RoundToInt(progress * 58f);
            spellBurstImage.rectTransform.sizeDelta = new Vector2(radius * 2, radius * 2);
            spellBurstImage.rectTransform.anchoredPosition = center;
        }

        public void PlayStatusCloud(Vector2 center, BattleStatusEffect status)
        {
            statusCloudImage.gameObject.SetActive(true);
            var color = status switch
            {
                BattleStatusEffect.Poison => new Color(90f / 255f, 120f / 255f, 232f / 255f, 96f / 255f),
                BattleStatusEffect.Sleep => new Color(88f / 255f, 116f / 255f, 178f / 255f, 255f / 255f),
                _ => new Color(82f / 255f, 200f / 255f, 120f / 255f, 255f / 255f)
            };
            statusCloudImage.color = color;
            statusCloudImage.rectTransform.anchoredPosition = center;
        }

        public void PlayHeal(Vector2 position)
        {
            healImage.gameObject.SetActive(true);
            healImage.rectTransform.anchoredPosition = position;
        }

        public void PlayGuard(Vector2 position)
        {
            guardImage.gameObject.SetActive(true);
            guardImage.rectTransform.anchoredPosition = position;
        }

        public void PlayItemSparkle(Vector2 position)
        {
            itemSparkleImage.gameObject.SetActive(true);
            itemSparkleImage.rectTransform.anchoredPosition = position;
        }

        public void PlayEnemyDefeat(Vector2 position)
        {
            enemyDefeatImage.gameObject.SetActive(true);
            enemyDefeatImage.rectTransform.anchoredPosition = position;
        }

        public void ClearAll()
        {
            SetEffectActive(slashImage, false);
            SetEffectActive(spellBurstImage, false);
            SetEffectActive(statusCloudImage, false);
            SetEffectActive(healImage, false);
            SetEffectActive(guardImage, false);
            SetEffectActive(itemSparkleImage, false);
            SetEffectActive(enemyDefeatImage, false);
        }

        private static void SetEffectActive(Image image, bool active)
        {
            if (image != null)
            {
                image.gameObject.SetActive(active);
            }
        }
    }
}
