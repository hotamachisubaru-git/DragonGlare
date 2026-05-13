using UnityEngine;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class StartupOptionsController : SceneControllerBase
    {
        [SerializeField] private StartupOptionsScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.OptionsCursor, Session.ActiveDisplayMode, Session.LaunchSettings.PromptOnStartup);
        }

        public override void OnUpdate()
        {
            var previousCursor = Session.OptionsCursor;
            if (Input.WasPressed(KeyCode.Up) || Input.WasPressed(KeyCode.W))
                Session.OptionsCursor = Mathf.Max(0, Session.OptionsCursor - 1);
            else if (Input.WasPressed(KeyCode.Down) || Input.WasPressed(KeyCode.S))
                Session.OptionsCursor = Mathf.Min(5, Session.OptionsCursor + 1);

            if (previousCursor != Session.OptionsCursor)
                PlayCursorSe();

            if (!Input.WasPrimaryConfirmPressed())
                return;

            if (Session.OptionsCursor < 4)
            {
                SetStartupDisplayMode((LaunchDisplayMode)Session.OptionsCursor);
                return;
            }

            if (Session.OptionsCursor == 4)
            {
                Session.LaunchSettings = new LaunchSettings
                {
                    DisplayMode = Session.ActiveDisplayMode,
                    PromptOnStartup = !Session.LaunchSettings.PromptOnStartup
                };
                return;
            }

            SaveLaunchSettings();
            Session.ChangeGameState(GameState.ModeSelect);
        }

        private void SetStartupDisplayMode(LaunchDisplayMode displayMode)
        {
            Session.ActiveDisplayMode = displayMode;
            Session.LaunchSettings = new LaunchSettings
            {
                DisplayMode = displayMode,
                PromptOnStartup = Session.LaunchSettings.PromptOnStartup
            };
            ApplyDisplayMode();
        }

        private void ApplyDisplayMode()
        {
            if (Session.ActiveDisplayMode == LaunchDisplayMode.Fullscreen)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            }
            else
            {
                var size = Session.ActiveDisplayMode switch
                {
                    LaunchDisplayMode.Window720p => new Vector2Int(1280, 720),
                    LaunchDisplayMode.Window1080p => new Vector2Int(1920, 1080),
                    _ => new Vector2Int(640, 480)
                };
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                Session.LastWindowedDisplayMode = Session.ActiveDisplayMode;
            }
        }

        private void SaveLaunchSettings()
        {
            PlayerPrefs.SetInt("DisplayMode", (int)Session.ActiveDisplayMode);
            PlayerPrefs.SetInt("PromptOnStartup", Session.LaunchSettings.PromptOnStartup ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}