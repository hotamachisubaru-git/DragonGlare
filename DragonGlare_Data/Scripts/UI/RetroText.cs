using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class RetroText : MonoBehaviour
    {
        [SerializeField] private Text targetText;
        [SerializeField] private Font retroFont;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color textColor = Color.white;

        private void Awake()
        {
            if (targetText == null)
                targetText = GetComponent<Text>();

            if (retroFont != null)
                targetText.font = retroFont;

            targetText.fontSize = fontSize;
            targetText.color = textColor;
            targetText.alignment = TextAnchor.UpperLeft;
        }
    }
}
