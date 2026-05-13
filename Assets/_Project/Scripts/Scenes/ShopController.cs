using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DragonGlare.Domain;

namespace DragonGlare
{
    public class ShopController : SceneControllerBase
    {
        [SerializeField] private ShopScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.Player, Session.ShopPhase, Session.ShopPromptCursor, Session.ShopItemCursor,
                Session.ShopPageIndex, Session.ShopMessage, Session.SelectedLanguage);
        }

        public override void OnUpdate()
        {
            if (Session.ShopPhase == ShopPhase.Welcome)
            {
                UpdateWelcome();
                return;
            }

            UpdateList();
        }

        private void UpdateWelcome()
        {
            var previousCursor = Session.ShopPromptCursor;
            if (Input.WasPressed(KeyCode.Up) || Input.WasPressed(KeyCode.W))
                Session.ShopPromptCursor = Mathf.Max(0, Session.ShopPromptCursor - 1);
            else if (Input.WasPressed(KeyCode.Down) || Input.WasPressed(KeyCode.S))
                Session.ShopPromptCursor = Mathf.Min(2, Session.ShopPromptCursor + 1);

            if (previousCursor != Session.ShopPromptCursor)
                PlayCursorSe();

            if (Input.WasShopBackPressed())
            {
                PlayCancelSe();
                Session.ChangeGameState(GameState.Field);
                return;
            }

            if (!Input.WasShopConfirmPressed())
                return;

            if (Session.ShopPromptCursor == 0)
            {
                Session.ShopPhase = ShopPhase.BuyList;
                Session.ResetShopListSelection();
                Session.ShopMessage = Session.GetShopBrowseMessage();
            }
            else if (Session.ShopPromptCursor == 1)
            {
                Session.ShopPhase = ShopPhase.SellList;
                Session.ResetShopListSelection();
                Session.ShopMessage = Session.GetShopSellBrowseMessage();
            }
            else
            {
                Session.ChangeGameState(GameState.Field);
                PlayCancelSe();
            }
        }

        private void UpdateList()
        {
            var visibleEntries = Session.GetShopVisibleEntries();
            var maxIndex = visibleEntries.Count - 1;
            var previousItemCursor = Session.ShopItemCursor;
            if (Input.WasPressed(KeyCode.Up) || Input.WasPressed(KeyCode.W))
                Session.ShopItemCursor = Mathf.Max(0, Session.ShopItemCursor - 1);
            else if (Input.WasPressed(KeyCode.Down) || Input.WasPressed(KeyCode.S))
                Session.ShopItemCursor = Mathf.Min(maxIndex, Session.ShopItemCursor + 1);

            if (previousItemCursor != Session.ShopItemCursor)
                PlayCursorSe();

            if (Input.WasShopBackPressed())
            {
                PlayCancelSe();
                ReturnToShopPrompt(Session.GetShopReturnMessage());
                return;
            }

            if (!Input.WasShopConfirmPressed())
                return;

            var selectedEntry = visibleEntries[Session.ShopItemCursor];
            if (selectedEntry.Type == ShopMenuEntryType.PreviousPage)
            {
                ChangeShopPage(-1);
                return;
            }
            if (selectedEntry.Type == ShopMenuEntryType.NextPage)
            {
                ChangeShopPage(1);
                return;
            }
            if (selectedEntry.Type == ShopMenuEntryType.Quit)
            {
                PlayCancelSe();
                ReturnToShopPrompt(Session.GetShopFarewellMessage());
                return;
            }

            if (Session.ShopPhase == ShopPhase.SellList)
            {
                if (selectedEntry.InventoryItem != null)
                {
                    var sellResult = Session.ShopService.SellItem(Session.Player, selectedEntry.InventoryItem.Value.ItemId);
                    Session.ShopMessage = sellResult.Message;
                    if (sellResult.Success)
                    {
                        Session.ResetShopListSelection(Mathf.Min(Session.ShopPageIndex, Mathf.Max(0, Session.GetShopPageCount() - 1)));
                        Session.PersistProgress();
                    }
                }
                return;
            }

            if (selectedEntry.Product != null)
            {
                var purchaseResult = Session.ShopService.PurchaseProduct(Session.Player, selectedEntry.Product);
                Session.ShopMessage = purchaseResult.Message;
                if (purchaseResult.Success)
                {
                    if (purchaseResult.Equipped)
                        Audio.PlaySe(SoundEffect.Equip);
                    Session.PersistProgress();
                }
            }
        }

        private void ChangeShopPage(int pageDelta)
        {
            Session.ResetShopListSelection(Session.ShopPageIndex + pageDelta);
            Session.ShopMessage = Session.ShopPhase == ShopPhase.SellList ? Session.GetShopSellBrowseMessage() : Session.GetShopBrowseMessage();
        }

        private void ReturnToShopPrompt(string message)
        {
            Session.ShopPhase = ShopPhase.Welcome;
            Session.ShopPromptCursor = 0;
            Session.ResetShopListSelection();
            Session.ShopMessage = message;
        }
    }
}