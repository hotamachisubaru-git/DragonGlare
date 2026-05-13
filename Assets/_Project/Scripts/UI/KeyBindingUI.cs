using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class KeyBindingUI : MonoBehaviour
    {
        [SerializeField] private Transform bindingRoot;
        [SerializeField] private GameObject bindingItemPrefab;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button resetButton;

        private void Awake()
        {
            saveButton.onClick.AddListener(SaveBindings);
            resetButton.onClick.AddListener(ResetBindings);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            LoadBindings();
        }

        private void LoadBindings()
        {
            // Load key bindings from InputManager or PlayerPrefs
        }

        private void SaveBindings()
        {
            // Save key bindings to PlayerPrefs
        }

        private void ResetBindings()
        {
            // Reset to default bindings
        }
    }
}
