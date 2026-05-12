using UnityEngine;

namespace DragonGlare
{
    public class LanguageSelectionController : SceneControllerBase
    {
        [SerializeField] private LanguageSelectionScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.LanguageCursor, Session.LanguageOpeningFinished, Session.LanguageOpeningElapsedFrames, GameConstants.LanguageOpeningScript);
        }

        public override void OnUpdate()
        {
            if (!Session.LanguageOpeningFinished)
            {
                UpdateOpening();
                return;
            }

            var previousCursor = Session.LanguageCursor;
            if (Input.WasPressed(KeyCode.Up) || Input.WasPressed(KeyCode.W))
                Session.LanguageCursor = Mathf.Max(0, Session.LanguageCursor - 1);
            else if (Input.WasPressed(KeyCode.Down) || Input.WasPressed(KeyCode.S))
                Session.LanguageCursor = Mathf.Min(1, Session.LanguageCursor + 1);

            if (previousCursor != Session.LanguageCursor)
                PlayCursorSe();

            if (Input.WasShopBackPressed())
            {
                PlayCancelSe();
                Session.ChangeGameState(GameState.ModeSelect);
                return;
            }

            if (!Input.WasPrimaryConfirmPressed())
                return;

            Session.SelectedLanguage = Session.LanguageCursor == 0 ? UiLanguage.Japanese : UiLanguage.English;
            Session.ChangeGameState(GameState.NameInput);
        }

        private void UpdateOpening()
        {
            Session.LanguageOpeningElapsedFrames++;
            var currentLine = GameConstants.LanguageOpeningScript[Session.LanguageOpeningLineIndex];
            Session.LanguageOpeningLineFrame++;
            if (Session.LanguageOpeningLineFrame >= currentLine.DisplayFrames)
            {
                Session.LanguageOpeningLineFrame = 0;
                Session.LanguageOpeningLineIndex++;
                if (Session.LanguageOpeningLineIndex >= GameConstants.LanguageOpeningScript.Length)
                {
                    Session.LanguageOpeningFinished = true;
                }
            }

            if (Input.WasPrimaryConfirmPressed() || Input.WasShopBackPressed())
            {
                Session.LanguageOpeningFinished = true;
            }
        }
    }
}
