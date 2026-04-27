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
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                shopPromptCursor = Math.Max(0, shopPromptCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                shopPromptCursor = Math.Min(2, shopPromptCursor + 1);
            }

            if (WasShopBackPressed())
            {
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
            return;
        }

        var visibleEntries = GetShopVisibleEntries();
        var maxIndex = visibleEntries.Count - 1;
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            shopItemCursor = Math.Max(0, shopItemCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            shopItemCursor = Math.Min(maxIndex, shopItemCursor + 1);
        }

        if (WasShopBackPressed())
        {
            ReturnToShopPrompt(ShopReturnMessage);
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
            ReturnToShopPrompt(ShopFarewellMessage);
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
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                bankPromptCursor = Math.Max(0, bankPromptCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                bankPromptCursor = Math.Min(3, bankPromptCursor + 1);
            }

            if (WasShopBackPressed())
            {
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
                    ChangeGameState(GameState.Field);
                    return;
            }
        }

        var options = GetBankAmountOptions();
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            bankItemCursor = Math.Max(0, bankItemCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            bankItemCursor = Math.Min(options.Count - 1, bankItemCursor + 1);
        }

        if (WasShopBackPressed())
        {
            ReturnToBankPrompt(BankReturnMessage);
            return;
        }

        if (!WasShopConfirmPressed())
        {
            return;
        }

        var selectedOption = options[bankItemCursor];
        if (selectedOption.Quit)
        {
            ReturnToBankPrompt(BankReturnMessage);
            return;
        }

        var amount = ResolveBankTransactionAmount(selectedOption);
        var result = bankPhase switch
        {
            BankPhase.DepositList => bankService.Deposit(player, amount),
            BankPhase.WithdrawList => bankService.Withdraw(player, amount),
            BankPhase.BorrowList => bankService.Borrow(player, amount),
            _ => new BankTransactionResult(false, 0, 0, BankReturnMessage)
        };

        bankMessage = result.Message;
        if (result.Success)
        {
            PersistProgress();
        }
    }
}
