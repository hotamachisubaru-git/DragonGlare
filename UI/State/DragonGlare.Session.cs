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
        battleMessage = message ?? DefaultBattleMessage;
    }

    private void ResetShopState(string? message = null)
    {
        shopPhase = ShopPhase.Welcome;
        shopPromptCursor = 0;
        ResetShopListSelection();
        shopMessage = message ?? ShopWelcomeMessage;
    }

    private void OpenShopBuyCatalog()
    {
        shopPhase = ShopPhase.BuyList;
        ResetShopListSelection();
        shopMessage = ShopBrowseMessage;
    }

    private void OpenShopSellCatalog()
    {
        shopPhase = ShopPhase.SellList;
        ResetShopListSelection();
        shopMessage = ShopSellBrowseMessage;
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
            ? ShopSellBrowseMessage
            : ShopBrowseMessage;
    }

    private void ResetBankState(string? message = null)
    {
        bankPhase = BankPhase.Welcome;
        bankPromptCursor = 0;
        bankItemCursor = 0;
        bankMessage = message ?? BankWelcomeMessage;
    }

    private void OpenBankList(BankPhase nextPhase)
    {
        bankPhase = nextPhase;
        bankItemCursor = 0;
        bankMessage = nextPhase switch
        {
            BankPhase.DepositList => BankDepositMessage,
            BankPhase.WithdrawList => BankWithdrawMessage,
            BankPhase.BorrowList => BankBorrowMessage,
            _ => BankWelcomeMessage
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
