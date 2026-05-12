using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class ScrollableText : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Text contentText;
        [SerializeField] private float scrollSpeed = 50f;

        public void AppendText(string text)
        {
            contentText.text += text + "\n";
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public void SetText(string text)
        {
            contentText.text = text;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public void Clear()
        {
            contentText.text = string.Empty;
        }
    }
}
