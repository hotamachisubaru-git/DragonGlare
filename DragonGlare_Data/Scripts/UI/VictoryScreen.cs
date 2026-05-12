using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text victoryText;
        [SerializeField] private Text rewardsText;
        [SerializeField] private Button continueButton;
        [SerializeField] private float fadeInDuration = 1f;

        private float timer;
        private bool isFadingIn;

        private void Awake()
        {
            continueButton.onClick.AddListener(OnContinue);
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

        public void Show(string rewards)
        {
            gameObject.SetActive(true);
            timer = 0f;
            isFadingIn = true;
            canvasGroup.alpha = 0f;
            rewardsText.text = rewards;
        }

        private void OnContinue()
        {
            GameManager.Instance.SceneUI.ChangeGameState(GameState.Field);
        }
    }
}
