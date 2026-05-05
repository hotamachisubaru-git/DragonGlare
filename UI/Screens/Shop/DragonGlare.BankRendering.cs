using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void DrawBank(Graphics g)
    {
        DrawFieldScene(g);
        const int rowHeight = 24;
        const int listStartY = 64;

        var helpRect = new Rectangle(32, 20, 242, 136);
        var listRect = new Rectangle(304, 20, 316, 274);
        var infoRect = new Rectangle(32, 176, 242, 112);
        var messageRect = new Rectangle(70, 304, 498, 140);

        DrawWindow(g, helpRect);
        if (bankPhase == BankPhase.Welcome)
        {
            DrawOption(g, bankPromptCursor == 0, 84, 44, selectedLanguage == UiLanguage.English ? "DEPOSIT" : "あずける");
            DrawOption(g, bankPromptCursor == 1, 84, 72, selectedLanguage == UiLanguage.English ? "WITHDRAW" : "ひきだす");
            DrawOption(g, bankPromptCursor == 2, 84, 100, selectedLanguage == UiLanguage.English ? "BORROW" : "かりる");
            DrawOption(g, bankPromptCursor == 3, 84, 128, selectedLanguage == UiLanguage.English ? "LEAVE" : "やめる");
        }
        else
        {
            DrawText(g, selectedLanguage == UiLanguage.English ? "D-PAD/LS: CHOOSE" : "十字/LS: せんたく", new Rectangle(54, 50, 188, 24), smallFont);
            DrawText(g, selectedLanguage == UiLanguage.English ? "A/Y/Z: OK" : "A/Y/Z: けってい", new Rectangle(54, 78, 188, 24), smallFont);
            DrawText(g, selectedLanguage == UiLanguage.English ? "B/X/ESC: BACK" : "B/X/ESC: もどる", new Rectangle(54, 106, 188, 24), smallFont);
        }

        DrawWindow(g, listRect);
        DrawText(g, bankPhase switch
        {
            BankPhase.DepositList => selectedLanguage == UiLanguage.English ? "DEPOSIT" : "あずける",
            BankPhase.WithdrawList => selectedLanguage == UiLanguage.English ? "WITHDRAW" : "ひきだす",
            BankPhase.BorrowList => selectedLanguage == UiLanguage.English ? "BORROW" : "かりる",
            _ => selectedLanguage == UiLanguage.English ? "BANK" : "ぎんこう"
        }, new Rectangle(listRect.X + 20, 34, 140, 24), smallFont);

        var options = bankPhase == BankPhase.Welcome
            ? []
            : GetBankAmountOptions();

        for (var i = 0; i < options.Count; i++)
        {
            var option = options[i];
            var rowY = listStartY + (i * rowHeight);
            if (bankItemCursor == i)
            {
                DrawSelectionMarker(g, listRect.X + 12, rowY + 7);
            }

            DrawText(g, option.Label, new Rectangle(listRect.X + 36, rowY, 150, 20), smallFont);
            if (!option.Quit)
            {
                var resolvedAmount = ResolveBankTransactionAmount(option);
                DrawText(g, $"{resolvedAmount}G", new Rectangle(listRect.X + 200, rowY, 92, 20), smallFont, StringAlignment.Far);
            }
        }

        DrawWindow(g, infoRect);
        DrawText(g, selectedLanguage == UiLanguage.English ? $"CASH: {player.Gold}G" : $"てもち: {player.Gold}G", new Rectangle(infoRect.X + 20, infoRect.Y + 14, 196, 20), smallFont);
        DrawText(g, selectedLanguage == UiLanguage.English ? $"BANK: {player.BankGold}G" : $"よきん: {player.BankGold}G", new Rectangle(infoRect.X + 20, infoRect.Y + 38, 196, 20), smallFont);
        DrawText(g, selectedLanguage == UiLanguage.English ? $"LOAN: {player.LoanBalance}G" : $"しゃっきん: {player.LoanBalance}G", new Rectangle(infoRect.X + 20, infoRect.Y + 62, 196, 20), smallFont);
        DrawText(g, selectedLanguage == UiLanguage.English ? $"CREDIT: {bankService.GetAvailableCredit(player)}G" : $"しんよう: {bankService.GetAvailableCredit(player)}G", new Rectangle(infoRect.X + 20, infoRect.Y + 86, 196, 20), smallFont);

        DrawWindow(g, messageRect);
        DrawText(g, bankMessage, Rectangle.Inflate(messageRect, -24, -24), smallFont, wrap: true);
    }
}
