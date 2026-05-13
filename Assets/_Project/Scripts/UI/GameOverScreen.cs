using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text gameOverText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private float fadeInDuration = 2f;

        private float timer;
        private bool isFadingIn;

        private void Awake()
        {
            continueButton.onClick.AddListener(OnContinue);
            quitButton.onClick.AddListener(OnQuit);
        }

        private void Update()
        {
            if (!isFadingIn) return;

            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeInDuration);
            canvasGroup.alpha = alpha;

            if (timer >= fadeInDuration)
            {
                isFadingIn = false;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            timer = 0f;
            isFadingIn = true;
            canvasGroup.alpha = 0f;
        }

        private void OnContinue()
        {
            // Load last save or return to checkpoint
            GameManager.Instance.SceneUI.ChangeGameState(GameState.Field);
        }

        private void OnQuit()
        {
            GameManager.Instance.SceneUI.ChangeGameState(GameState.ModeSelect);
        }
    }
}