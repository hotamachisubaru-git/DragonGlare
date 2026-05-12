using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class MessageWindow : MonoBehaviour
    {
        [SerializeField] private RetroWindow window;
        [SerializeField] private Text messageText;
        [SerializeField] private Text footerText;

        public void Show(string message, string footer = "")
        {
            gameObject.SetActive(true);
            messageText.text = message;
            footerText.text = footer;
            footerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(footer));
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        public void SetFooter(string footer)
        {
            footerText.text = footer;
            footerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(footer));
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
