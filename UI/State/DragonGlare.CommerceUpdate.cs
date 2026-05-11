using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Services;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void UpdateShopBuy()
    {
        if (shopPhase == ShopPhase.Welcome)
        {
            var previousCursor = shopPromptCursor;
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                shopPromptCursor = Math.Max(0, shopPromptCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                shopPromptCursor = Math.Min(2, shopPromptCursor + 1);
            }
            PlayCursorSeIfChanged(previousCursor, shopPromptCursor);

            if (WasShopBackPressed())
            {
                PlayCancelSe();
                ChangeGameState(GameState.Field);
                return;
            }

            if (!WasShopConfirmPressed())
            {
                return;
            }

            if (shopPromptCursor == 0)
            {
                OpenShopBuyCatalog();
                return;
            }

            if (shopPromptCursor == 1)
            {
                OpenShopSellCatalog();
                return;
            }

            ChangeGameState(GameState.Field);
            PlayCancelSe();
            return;
        }

        var visibleEntries = GetShopVisibleEntries();
        var maxIndex = visibleEntries.Count - 1;
        var previousItemCursor = shopItemCursor;
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            shopItemCursor = Math.Max(0, shopItemCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            shopItemCursor = Math.Min(maxIndex, shopItemCursor + 1);
        }
        PlayCursorSeIfChanged(previousItemCursor, shopItemCursor);

        if (WasShopBackPressed())
        {
            PlayCancelSe();
            ReturnToShopPrompt(GetShopReturnMessage());
            return;
        }

        if (!WasShopConfirmPressed())
        {
            return;
        }

        var selectedEntry = visibleEntries[shopItemCursor];
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
            ReturnToShopPrompt(GetShopFarewellMessage());
            return;
        }

        if (shopPhase == ShopPhase.SellList)
        {
            if (selectedEntry.InventoryItem is null)
            {
                return;
            }

            var sellResult = shopService.SellItem(player, selectedEntry.InventoryItem.Value.ItemId);
            shopMessage = sellResult.Message;
            if (sellResult.Success)
            {
                ResetShopListSelection(Math.Min(shopPageIndex, Math.Max(0, GetShopPageCount() - 1)));
                PersistProgress();
            }

            return;
        }

        if (selectedEntry.Product is null)
        {
            return;
        }

        var purchaseResult = shopService.PurchaseProduct(player, selectedEntry.Product);
        shopMessage = purchaseResult.Message;
        if (purchaseResult.Success)
        {
            if (purchaseResult.Equipped)
            {
                PlaySe(SoundEffect.Equip);
            }

            PersistProgress();
        }
    }

    private void EnterShopBuy()
    {
        ResetShopState();
        ChangeGameState(GameState.ShopBuy);
        PlaySe(SoundEffect.Dialog);
    }

    private void EnterBank()
    {
        ResetBankState();
        ChangeGameState(GameState.Bank);
        PlaySe(SoundEffect.Dialog);
    }

    private void UpdateBank()
    {
        if (bankPhase == BankPhase.Welcome)
        {
            var previousCursor = bankPromptCursor;
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                bankPromptCursor = Math.Max(0, bankPromptCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                bankPromptCursor = Math.Min(3, bankPromptCursor + 1);
            }
            PlayCursorSeIfChanged(previousCursor, bankPromptCursor);

            if (WasShopBackPressed())
            {
                PlayCancelSe();
                ChangeGameState(GameState.Field);
                return;
            }

            if (!WasShopConfirmPressed())
            {
                return;
            }

            switch (bankPromptCursor)
            {
                case 0:
                    OpenBankList(BankPhase.DepositList);
                    return;
                case 1:
                    OpenBankList(BankPhase.WithdrawList);
                    return;
                case 2:
                    OpenBankList(BankPhase.BorrowList);
                    return;
                default:
                    PlayCancelSe();
                    ChangeGameState(GameState.Field);
                    return;
            }
        }

        var options = GetBankAmountOptions();
        var previousItemCursor = bankItemCursor;
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            bankItemCursor = Math.Max(0, bankItemCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            bankItemCursor = Math.Min(options.Count - 1, bankItemCursor + 1);
        }
        PlayCursorSeIfChanged(previousItemCursor, bankItemCursor);

        if (WasShopBackPressed())
        {
            PlayCancelSe();
            ReturnToBankPrompt(GetBankReturnMessage());
            return;
        }

        if (!WasShopConfirmPressed())
        {
            return;
        }

        var selectedOption = options[bankItemCursor];
        if (selectedOption.Quit)
        {
            PlayCancelSe();
            ReturnToBankPrompt(GetBankReturnMessage());
            return;
        }

        var amount = ResolveBankTransactionAmount(selectedOption);
        var result = bankPhase switch
        {
            BankPhase.DepositList => bankService.Deposit(player, amount),
            BankPhase.WithdrawList => bankService.Withdraw(player, amount),
            BankPhase.BorrowList => bankService.Borrow(player, amount),
            _ => new BankTransactionResult(false, 0, 0, GetBankReturnMessage())
        };

        bankMessage = result.Message;
        if (result.Success)
        {
            PersistProgress();
        }
    }
}
