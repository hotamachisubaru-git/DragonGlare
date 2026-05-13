using UnityEngine;

namespace DragonGlare
{
    public class StartupOptionsScene : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Text titleText;
        [SerializeField] private UnityEngine.UI.Text[] modeTexts;
        [SerializeField] private UnityEngine.UI.Text promptText;
        [SerializeField] private UnityEngine.UI.Text startText;
        [SerializeField] private UnityEngine.UI.Text helpText;
        [SerializeField] private RectTransform[] cursors;

        public void Show(int optionsCursor, LaunchDisplayMode activeDisplayMode, bool promptOnStartup)
        {
            gameObject.SetActive(true);
            var modes = new[] { "ウィンドウ 640x480（標準）", "ウィンドウ 1280x720（720p）", "ウィンドウ 1920x1080（1080p）", "フルスクリーン" };
            for (int i = 0; i < modeTexts.Length && i < modes.Length; i++)
            {
                var activeMarker = activeDisplayMode == (LaunchDisplayMode)i ? "[x]" : "[ ]";
                modeTexts[i].text = $"{activeMarker} {modes[i]}";
            }

            promptText.text = $"次回もこの画面を表示: [{(promptOnStartup ? "はい" : "いいえ")}]";

            for (int i = 0; i < cursors.Length; i++)
            {
                cursors[i].gameObject.SetActive(i == optionsCursor);
            }
        }
    }
}
