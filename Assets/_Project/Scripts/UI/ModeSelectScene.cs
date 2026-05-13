using UnityEngine;

namespace DragonGlare
{
    public class ModeSelectScene : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image layoutImage;
        [SerializeField] private UnityEngine.UI.Text[] menuItems;
        [SerializeField] private UnityEngine.UI.Text descriptionText;
        [SerializeField] private UnityEngine.UI.Text noticeText;
        [SerializeField] private RectTransform[] cursors;

        public void Show(int modeCursor, UiLanguage language, string menuNotice)
        {
            gameObject.SetActive(true);
            var items = language == UiLanguage.English
                ? new[] { "NEW GAME", "CONTINUE", "COPY DATA", "DELETE DATA" }
                : new[] { "はじめから", "つづきから", "データうつす", "データけす" };

            for (int i = 0; i < menuItems.Length && i < items.Length; i++)
            {
                menuItems[i].text = items[i];
            }

            for (int i = 0; i < cursors.Length; i++)
            {
                cursors[i].gameObject.SetActive(i == modeCursor);
            }

            descriptionText.text = GetDescription(modeCursor, language);
            noticeText.text = string.IsNullOrWhiteSpace(menuNotice)
                ? (language == UiLanguage.English ? "Choose a mode." : "モードを選んでください。")
                : menuNotice;
        }

        private static string GetDescription(int cursor, UiLanguage language)
        {
            if (language == UiLanguage.English)
            {
                return cursor switch
                {
                    0 => "Start a new\nadventure.",
                    1 => "Continue from\nsaved data.",
                    2 => "Copy data to\nanother slot.",
                    3 => "Delete unwanted\ndata.",
                    _ => string.Empty
                };
            }
            return cursor switch
            {
                0 => "ゲームを最初から\nはじめる。",
                1 => "前回のつづきから\nはじめる。",
                2 => "データを別の枠へ\nうつす。",
                3 => "いらないデータを\nけす。",
                _ => string.Empty
            };
        }
    }
}
