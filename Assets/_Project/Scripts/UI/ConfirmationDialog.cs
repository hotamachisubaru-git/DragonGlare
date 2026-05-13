using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class ConfirmationDialog : MonoBehaviour
    {
        [SerializeField] private Text messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private RetroWindow window;

        private System.Action onYes;
        private System.Action onNo;

        private void Awake()
        {
            yesButton.onClick.AddListener(() => { onYes?.Invoke(); Hide(); });
            noButton.onClick.AddListener(() => { onNo?.Invoke(); Hide(); });
        }

        public void Show(string message, System.Action yesAction, System.Action noAction = null)
        {
            gameObject.SetActive(true);
            messageText.text = message;
            onYes = yesAction;
            onNo = noAction;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
