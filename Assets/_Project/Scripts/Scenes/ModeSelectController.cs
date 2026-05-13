using UnityEngine;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class ModeSelectController : SceneControllerBase
    {
        [SerializeField] private ModeSelectScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.ModeCursor, Session.SelectedLanguage, Session.MenuNotice);
        }

        public override void OnUpdate()
        {
            var previousCursor = Session.ModeCursor;
            if (Input.WasPressed(KeyCode.Up) || Input.WasPressed(KeyCode.W))
                Session.ModeCursor = Mathf.Max(0, Session.ModeCursor - 1);
            else if (Input.WasPressed(KeyCode.Down) || Input.WasPressed(KeyCode.S))
                Session.ModeCursor = Mathf.Min(3, Session.ModeCursor + 1);

            if (previousCursor != Session.ModeCursor)
                PlayCursorSe();

            if (Input.WasShopBackPressed())
            {
                PlayCancelSe();
                return;
            }

            if (!Input.WasPrimaryConfirmPressed())
                return;

            switch (Session.ModeCursor)
            {
                case 0:
                    Session.ChangeGameState(GameState.LanguageSelection);
                    break;
                case 1:
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.Load;
                    Session.ChangeGameState(GameState.SaveSlotSelection);
                    break;
                case 2:
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                    Session.ChangeGameState(GameState.SaveSlotSelection);
                    break;
                case 3:
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                    Session.ChangeGameState(GameState.SaveSlotSelection);
                    break;
            }
        }
    }
}