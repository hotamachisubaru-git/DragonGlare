using UnityEngine;

namespace DragonGlare
{
    public class NameInputController : SceneControllerBase
    {
        [SerializeField] private NameInputScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.SelectedLanguage, Session.NameCursorRow, Session.NameCursorColumn, Session.PlayerName.ToString());
        }

        public override void OnUpdate()
        {
            var table = GameContent.GetNameTable(Session.SelectedLanguage);
            if (Input.WasPressed(KeyCode.Up) || Input.WasPressed(KeyCode.W))
                MoveNameCursor(0, -1, table);
            else if (Input.WasPressed(KeyCode.Down) || Input.WasPressed(KeyCode.S))
                MoveNameCursor(0, 1, table);
            else if (Input.WasPressed(KeyCode.Left) || Input.WasPressed(KeyCode.A))
                MoveNameCursor(-1, 0, table);
            else if (Input.WasPressed(KeyCode.Right) || Input.WasPressed(KeyCode.D))
                MoveNameCursor(1, 0, table);

            if (Input.WasPrimaryConfirmPressed())
                AddSelectedCharacter(table);
            else if (Input.WasPressed(KeyCode.X))
                RemoveLastCharacter();
            else if (Input.WasShopBackPressed())
            {
                PlayCancelSe();
                Session.ChangeGameState(GameState.LanguageSelection);
            }
        }

        private void MoveNameCursor(int deltaX, int deltaY, string[][] table)
        {
            var previousRow = Session.NameCursorRow;
            var previousColumn = Session.NameCursorColumn;
            Session.NameCursorRow = Mathf.Clamp(Session.NameCursorRow + deltaY, 0, table.Length - 1);
            var maxColumn = table[Session.NameCursorRow].Length - 1;
            Session.NameCursorColumn = Mathf.Clamp(Session.NameCursorColumn + deltaX, 0, maxColumn);
            if (previousRow != Session.NameCursorRow || previousColumn != Session.NameCursorColumn)
                PlayCursorSe();
        }

        private void AddSelectedCharacter(string[][] table)
        {
            var selected = table[Session.NameCursorRow][Session.NameCursorColumn];
            var deleteToken = Session.SelectedLanguage == UiLanguage.Japanese ? "けす" : "DEL";
            var endToken = Session.SelectedLanguage == UiLanguage.Japanese ? "おわり" : "END";

            if (selected == deleteToken)
            {
                RemoveLastCharacter();
                return;
            }

            if (selected == endToken)
            {
                if (Session.PlayerName.Length > 0)
                {
                    Session.Player.Name = Session.PlayerName.ToString().Trim();
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.Save;
                    Session.ChangeGameState(GameState.SaveSlotSelection);
                }
                return;
            }

            if (Session.PlayerName.Length < GameConstants.MaxPlayerNameLength)
                Session.PlayerName.Append(selected);
        }

        private void RemoveLastCharacter()
        {
            if (Session.PlayerName.Length > 0)
            {
                Session.PlayerName.Remove(Session.PlayerName.Length - 1, 1);
                PlayCancelSe();
            }
        }
    }
}
