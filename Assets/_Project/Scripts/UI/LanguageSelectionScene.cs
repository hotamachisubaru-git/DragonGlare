using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class LanguageSelectionScene : MonoBehaviour
    {
        [SerializeField] private Image backdropImage;
        [SerializeField] private Text narrationText;
        [SerializeField] private Text japaneseOption;
        [SerializeField] private Text englishOption;
        [SerializeField] private Text promptText;
        [SerializeField] private Text helpText;
        [SerializeField] private RectTransform cursor;
        [SerializeField] private CanvasGroup narrationGroup;

        public void Show(int languageCursor, bool openingFinished, int elapsedFrames, OpeningNarrationLine[] script)
        {
            gameObject.SetActive(true);
            if (!openingFinished)
            {
                ShowOpeningNarration(elapsedFrames, script);
                return;
            }

            narrationGroup.alpha = 0f;
            japaneseOption.gameObject.SetActive(true);
            englishOption.gameObject.SetActive(true);
            promptText.gameObject.SetActive(true);
            helpText.gameObject.SetActive(true);

            cursor.position = languageCursor == 0 ? japaneseOption.rectTransform.position : englishOption.rectTransform.position;
        }

        private void ShowOpeningNarration(int elapsedFrames, OpeningNarrationLine[] script)
        {
            narrationGroup.alpha = 1f;
            japaneseOption.gameObject.SetActive(false);
            englishOption.gameObject.SetActive(false);
            promptText.gameObject.SetActive(false);
            helpText.gameObject.SetActive(false);

            int accumulated = 0;
            OpeningNarrationLine currentLine = default;
            bool found = false;
            foreach (var line in script)
            {
                if (elapsedFrames < accumulated + line.DisplayFrames)
                {
                    currentLine = line;
                    found = true;
                    break;
                }
                accumulated += line.DisplayFrames;
            }

            if (!found)
            {
                narrationText.text = string.Empty;
                return;
            }

            var localFrame = elapsedFrames - accumulated;
            var fadeFrames = Mathf.Min(24, Mathf.Max(12, currentLine.DisplayFrames / 4));
            float alpha;
            if (localFrame < fadeFrames)
                alpha = localFrame / (float)fadeFrames;
            else if (localFrame > currentLine.DisplayFrames - fadeFrames)
                alpha = (currentLine.DisplayFrames - localFrame) / (float)fadeFrames;
            else
                alpha = 1f;

            narrationText.text = currentLine.Text;
            narrationGroup.alpha = alpha;
        }
    }
}
