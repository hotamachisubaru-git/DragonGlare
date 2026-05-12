using UnityEngine;

namespace DragonGlare
{
    public class SaveSlotSelectionController : SceneControllerBase
    {
        [SerializeField] private SaveSlotSelectionScene scene;

        public override void OnEnter()
        {
            Session.RefreshSaveSlotSummaries();
            scene?.Show(Session.SelectedLanguage, Session.SaveSlotSelectionMode, Session.SaveSlotCursor, Session.SaveSlotSummaries, Session.DataOperationSourceSlot, Session.MenuNotice);
        }

        public override void OnUpdate()
        {
            var previousCursor = Session.SaveSlotCursor;
            if (Input.WasPressed(KeyCode.Up) || Input.WasPressed(KeyCode.W))
                Session.SaveSlotCursor = Mathf.Max(0, Session.SaveSlotCursor - 1);
            else if (Input.WasPressed(KeyCode.Down) || Input.WasPressed(KeyCode.S))
                Session.SaveSlotCursor = Mathf.Min(SaveManager.SlotCount - 1, Session.SaveSlotCursor + 1);

            if (previousCursor != Session.SaveSlotCursor)
                PlayCursorSe();

            if (Input.WasShopBackPressed())
            {
                HandleBack();
                return;
            }

            if (!Input.WasPrimaryConfirmPressed())
                return;

            var slotNumber = Session.SaveSlotCursor + 1;
            switch (Session.SaveSlotSelectionMode)
            {
                case SaveSlotSelectionMode.Save:
                    Session.SaveGame(slotNumber);
                    Session.ApplyExplorationSession(Session.Player, Session.CurrentFieldMap);
                    Session.ChangeGameState(GameState.Field);
                    break;
                case SaveSlotSelectionMode.Load:
                    if (GameManager.Instance.Save.TryLoadSlot(slotNumber, out var saveData) && saveData != null)
                    {
                        Session.ApplyExplorationSession(SaveDataMapper.ToPlayerProgress(saveData), saveData.CurrentFieldMap);
                        Session.ChangeGameState(GameState.Field);
                    }
                    else
                    {
                        Session.ShowMenuNotice(Session.SelectedLanguage == UiLanguage.English ? "Could not load data." : "よみこめませんでした。");
                    }
                    break;
                case SaveSlotSelectionMode.CopySource:
                    Session.DataOperationSourceSlot = slotNumber;
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.CopyDestination;
                    break;
                case SaveSlotSelectionMode.CopyDestination:
                    if (Session.DataOperationSourceSlot != slotNumber)
                    {
                        if (GameManager.Instance.Save.CopySlot(Session.DataOperationSourceSlot, slotNumber))
                        {
                            Session.RefreshSaveSlotSummaries();
                            Session.ShowMenuNotice(Session.SelectedLanguage == UiLanguage.English ? "Copy complete." : "コピーしました。");
                        }
                        else
                        {
                            Session.ShowMenuNotice(Session.SelectedLanguage == UiLanguage.English ? "Copy failed." : "コピーできませんでした。");
                        }
                    }
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                    Session.DataOperationSourceSlot = 0;
                    break;
                case SaveSlotSelectionMode.DeleteSelect:
                    Session.DataOperationSourceSlot = slotNumber;
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.DeleteConfirm;
                    break;
                case SaveSlotSelectionMode.DeleteConfirm:
                    GameManager.Instance.Save.DeleteSlot(slotNumber);
                    Session.RefreshSaveSlotSummaries();
                    Session.ShowMenuNotice(Session.SelectedLanguage == UiLanguage.English ? "Deleted." : "けしました。");
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                    Session.DataOperationSourceSlot = 0;
                    break;
            }
        }

        private void HandleBack()
        {
            PlayCancelSe();
            switch (Session.SaveSlotSelectionMode)
            {
                case SaveSlotSelectionMode.Save:
                    Session.ChangeGameState(GameState.NameInput);
                    break;
                case SaveSlotSelectionMode.Load:
                case SaveSlotSelectionMode.CopySource:
                case SaveSlotSelectionMode.DeleteSelect:
                    Session.ChangeGameState(GameState.ModeSelect);
                    break;
                case SaveSlotSelectionMode.CopyDestination:
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                    Session.DataOperationSourceSlot = 0;
                    break;
                case SaveSlotSelectionMode.DeleteConfirm:
                    Session.SaveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                    break;
            }
        }
    }
}
