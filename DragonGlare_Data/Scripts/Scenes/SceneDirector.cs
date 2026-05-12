using UnityEngine;

namespace DragonGlare
{
    public class SceneDirector : MonoBehaviour
    {
        [SerializeField] private StartupOptionsController startupOptionsController;
        [SerializeField] private ModeSelectController modeSelectController;
        [SerializeField] private LanguageSelectionController languageSelectionController;
        [SerializeField] private NameInputController nameInputController;
        [SerializeField] private SaveSlotSelectionController saveSlotSelectionController;
        [SerializeField] private FieldController fieldController;
        [SerializeField] private EncounterTransitionController encounterTransitionController;
        [SerializeField] private BattleController battleController;
        [SerializeField] private ShopController shopController;
        [SerializeField] private BankController bankController;
        [SerializeField] private FadeOverlay fadeOverlay;

        private SceneControllerBase currentController;
        private GameState previousGameState;

        private void Start()
        {
            GameSession.Instance.Initialize();
            GameManager.Instance.Audio.InitializeAudio();
            GameManager.Instance.Sprites.LoadFieldSprites();
            GameSession.Instance.RefreshSaveSlotSummaries();

            GameSession.Instance.CurrentGameState = GameSession.Instance.LaunchSettings.PromptOnStartup ? GameState.StartupOptions : GameState.ModeSelect;
            TransitionToState(GameSession.Instance.CurrentGameState);
        }

        private void Update()
        {
            GameSession.Instance.FrameCounter++;
            GameSession.Instance.UpdateMenuNotice();
            UpdateProgressSaveDelay();

            if (GameSession.Instance.PendingGameState.HasValue)
            {
                if (GameSession.Instance.SceneFadeOutFramesRemaining > 0)
                {
                    GameSession.Instance.SceneFadeOutFramesRemaining--;
                    var progress = 1f - (GameSession.Instance.SceneFadeOutFramesRemaining / (float)GameConstants.SceneFadeOutDuration);
                    fadeOverlay.SetAlpha(Mathf.Clamp01(progress));
                    return;
                }

                GameSession.Instance.ApplyPendingState();
                TransitionToState(GameSession.Instance.CurrentGameState);
                fadeOverlay.SetAlpha(0f);
            }

            currentController?.OnUpdate();
        }

        private void TransitionToState(GameState state)
        {
            if (currentController != null)
            {
                currentController.OnExit();
                currentController.gameObject.SetActive(false);
            }

            currentController = GetController(state);
            if (currentController != null)
            {
                currentController.gameObject.SetActive(true);
                currentController.OnEnter();
            }

            previousGameState = state;
        }

        private SceneControllerBase GetController(GameState state)
        {
            return state switch
            {
                GameState.StartupOptions => startupOptionsController,
                GameState.ModeSelect => modeSelectController,
                GameState.LanguageSelection => languageSelectionController,
                GameState.NameInput => nameInputController,
                GameState.SaveSlotSelection => saveSlotSelectionController,
                GameState.Field => fieldController,
                GameState.EncounterTransition => encounterTransitionController,
                GameState.Battle => battleController,
                GameState.ShopBuy => shopController,
                GameState.Bank => bankController,
                _ => null
            };
        }

        private void UpdateProgressSaveDelay()
        {
            if (!GameSession.Instance.ProgressSavePending)
                return;
            if (GameSession.Instance.ProgressSaveDelayFrames > 0)
                GameSession.Instance.ProgressSaveDelayFrames--;
            if (GameSession.Instance.ProgressSaveMaxDelayFrames > 0)
                GameSession.Instance.ProgressSaveMaxDelayFrames--;
            if (GameSession.Instance.ProgressSaveDelayFrames <= 0 || GameSession.Instance.ProgressSaveMaxDelayFrames <= 0)
            {
                GameSession.Instance.FlushSave();
            }
        }

        private void OnApplicationQuit()
        {
            if (!GameSession.Instance.SkipSaveOnClose)
                GameSession.Instance.FlushSave();
        }
    }
}
