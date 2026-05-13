using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain.Battle;

namespace DragonGlare
{
    public class EncounterTransitionScene : MonoBehaviour
    {
        [SerializeField] private Image stripePrefab;
        [SerializeField] private RectTransform stripeRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Image flashOverlay;

        private Image[] stripes;
        private const int StripeCount = 12;

        public void Show(int remainingFrames, BattleEncounter pendingEncounter, UiLanguage language)
        {
            gameObject.SetActive(true);
            if (stripes == null)
                InitializeStripes();

            float progress = 1f - (remainingFrames / (float)GameConstants.EncounterTransitionDuration);
            float stripeProgress = Mathf.Clamp01((progress - 0.08f) / 0.68f);
            float finalFlash = Mathf.Clamp01((progress - 0.72f) / 0.28f);
            float flashPulse = progress <= 0.36f
                ? 1f - Mathf.Abs((progress / 0.36f * 2f) - 1f)
                : 0f;

            flashOverlay.color = new Color(1f, 1f, 1f, flashPulse * 170f / 255f);

            var stripeHeight = Mathf.CeilToInt(UiCanvas.VirtualHeight / (float)StripeCount);
            var filledHeight = Mathf.Max(1, Mathf.CeilToInt(stripeHeight * stripeProgress));
            for (int i = 0; i < StripeCount; i++)
            {
                var y = i * stripeHeight;
                var inset = (int)((i % 2 == 0 ? 26f : 12f) * (1f - stripeProgress));
                var rect = stripes[i].rectTransform;
                rect.anchoredPosition = new Vector2(inset, -y);
                rect.sizeDelta = new Vector2(UiCanvas.VirtualWidth - (inset * 2), filledHeight);
                stripes[i].color = new Color(240f / 255f, 252f / 255f, 255f / 255f);
            }

            if (pendingEncounter != null && progress >= 0.36f)
            {
                messageText.gameObject.SetActive(true);
                var enemyName = GameContent.GetEnemyName(pendingEncounter.Enemy, language);
                messageText.text = language == UiLanguage.English
                    ? $"{enemyName} is near..."
                    : $"{enemyName}の けはいがする…";
            }
            else
            {
                messageText.gameObject.SetActive(false);
            }

            if (finalFlash > 0f)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, finalFlash);
            }
        }

        private void InitializeStripes()
        {
            stripes = new Image[StripeCount];
            for (int i = 0; i < StripeCount; i++)
            {
                var go = Instantiate(stripePrefab.gameObject, stripeRoot);
                stripes[i] = go.GetComponent<Image>();
            }
        }
    }
}
