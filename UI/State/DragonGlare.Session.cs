using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private const string DefaultBattleMessage = "まものが あらわれた！";
    private const string BattleEscapeMessage = "うまく にげきった！";
    private const string ShopWelcomeMessage = "＊「いらっしゃい！\n　なにを するんだい？」";
    private const string ShopBrowseMessage = "＊「なにを かっていくかい？」";
    private const string ShopSellBrowseMessage = "＊「なにを うっていくんだい？」";
    private const string ShopReturnMessage = "＊「ほかに ようじは あるかい？」";
    private const string ShopFarewellMessage = "＊「また きてくれよな！」";
    private const string BankWelcomeMessage = "＊「ぎんこうへ ようこそ。\n　ごようけんは？」";
    private const string BankDepositMessage = "＊「いくら あずける？\n　しゃっきんは さきに へんさいするよ。」";
    private const string BankWithdrawMessage = "＊「いくら ひきだす？」";
    private const string BankBorrowMessage = "＊「いくら かりる？\n　りそくには きをつけな。」";
    private const string BankReturnMessage = "＊「ほかに ようじは あるかい？」";
    private const string BankFarewellMessage = "＊「またの ごりようを。」";

    private string GetDefaultBattleMessage()
    {
        return selectedLanguage == UiLanguage.English ? "A monster appeared!" : DefaultBattleMessage;
    }

    private string GetBattleEscapeMessage()
    {
        return selectedLanguage == UiLanguage.English ? "You got away safely!" : BattleEscapeMessage;
    }

    private string GetShopWelcomeMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"Welcome!\n  What do you need?\"" : ShopWelcomeMessage;
    }

    private string GetShopBrowseMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"What will you buy?\"" : ShopBrowseMessage;
    }

    private string GetShopSellBrowseMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"What will you sell?\"" : ShopSellBrowseMessage;
    }

    private string GetShopReturnMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"Anything else?\"" : ShopReturnMessage;
    }

    private string GetShopFarewellMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"Come again!\"" : ShopFarewellMessage;
    }

    private string GetBankWelcomeMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"Welcome to the bank.\n  How can I help?\"" : BankWelcomeMessage;
    }

    private string GetBankDepositMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "* \"How much will you deposit?\n  Loans are repaid first.\""
            : BankDepositMessage;
    }

    private string GetBankWithdrawMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"How much will you withdraw?\"" : BankWithdrawMessage;
    }

    private string GetBankBorrowMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "* \"How much will you borrow?\n  Watch the interest.\""
            : BankBorrowMessage;
    }

    private string GetBankReturnMessage()
    {
        return selectedLanguage == UiLanguage.English ? "* \"Anything else?\"" : BankReturnMessage;
    }

    private void ApplyExplorationSession(PlayerProgress nextPlayer, FieldMapId mapId)
    {
        player = nextPlayer;
        SetFieldMap(mapId);
        SyncPlayerNameBuffer(player.Name);
        ResetFieldUiState();
        ResetBattleState();
        ResetShopState();
        ResetBankState();
    }

    private void ResetFieldUiState()
    {
        CloseFieldDialog();
        isFieldStatusVisible = false;
        movementCooldown = 0;
        playerFacingDirection = PlayerFacingDirection.Down;
    }

    private void ResetBattleSelectionState()
    {
        battleFlowState = BattleFlowState.CommandSelection;
        battleCursorRow = 0;
        battleCursorColumn = 0;
        battleListCursor = 0;
        battleListScroll = 0;
        ResetBattleVisualEffects();
    }

    private void ResetBattleState(string? message = null)
    {
        currentEncounter = null;
        pendingEncounter = null;
        encounterTransitionFrames = 0;
        ResetBattleSelectionState();
        battleMessage = message ?? GetDefaultBattleMessage();
        battleMessageLines = [];
        battleMessageVisibleLines = 0;
        battleMessageLineTimer = 0;
        battleIntroFramesRemaining = 0;
    }

    private void ResetShopState(string? message = null)
    {
        shopPhase = ShopPhase.Welcome;
        shopPromptCursor = 0;
        ResetShopListSelection();
        shopMessage = message ?? GetShopWelcomeMessage();
    }

    private void OpenShopBuyCatalog()
    {
        shopPhase = ShopPhase.BuyList;
        ResetShopListSelection();
        shopMessage = GetShopBrowseMessage();
    }

    private void OpenShopSellCatalog()
    {
        shopPhase = ShopPhase.SellList;
        ResetShopListSelection();
        shopMessage = GetShopSellBrowseMessage();
    }

    private void ReturnToShopPrompt(string message)
    {
        shopPhase = ShopPhase.Welcome;
        shopPromptCursor = 0;
        ResetShopListSelection();
        shopMessage = message;
    }

    private void ChangeShopPage(int pageDelta)
    {
        ResetShopListSelection(shopPageIndex + pageDelta);
        shopMessage = shopPhase == ShopPhase.SellList
            ? GetShopSellBrowseMessage()
            : GetShopBrowseMessage();
    }

    private void ResetBankState(string? message = null)
    {
        bankPhase = BankPhase.Welcome;
        bankPromptCursor = 0;
        bankItemCursor = 0;
        bankMessage = message ?? GetBankWelcomeMessage();
    }

    private void OpenBankList(BankPhase nextPhase)
    {
        bankPhase = nextPhase;
        bankItemCursor = 0;
        bankMessage = nextPhase switch
        {
            BankPhase.DepositList => GetBankDepositMessage(),
            BankPhase.WithdrawList => GetBankWithdrawMessage(),
            BankPhase.BorrowList => GetBankBorrowMessage(),
            _ => GetBankWelcomeMessage()
        };
    }

    private void ReturnToBankPrompt(string message)
    {
        bankPhase = BankPhase.Welcome;
        bankPromptCursor = 0;
        bankItemCursor = 0;
        bankMessage = message;
    }
}
