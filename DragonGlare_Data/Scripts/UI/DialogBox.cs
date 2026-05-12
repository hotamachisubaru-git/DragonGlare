using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField] private RetroWindow window;
        [SerializeField] private Text dialogText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text footerText;

        public void Show(string text, string footer, Sprite portrait = null)
        {
            gameObject.SetActive(true);
            dialogText.text = text;
            footerText.text = footer;

            if (portrait != null)
            {
                portraitImage.gameObject.SetActive(true);
                portraitImage.sprite = portrait;
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            dialogText.text = text;
        }
    }
}
