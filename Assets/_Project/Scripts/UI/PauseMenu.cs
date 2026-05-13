using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private OptionsMenu optionsMenu;

        private bool isPaused;

        private void Awake()
        {
            resumeButton.onClick.AddListener(Resume);
            optionsButton.onClick.AddListener(ShowOptions);
            quitButton.onClick.AddListener(QuitToTitle);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        public void Pause()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            optionsMenu.Hide();
            Time.timeScale = 1f;
        }

        private void ShowOptions()
        {
            optionsMenu.Show();
        }

        private void QuitToTitle()
        {
            Resume();
            GameManager.Instance.SceneUI.ChangeGameState(GameState.ModeSelect);
        }
    }
}