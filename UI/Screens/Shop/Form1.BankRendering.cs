using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha;

public partial class Form1
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
            DrawOption(g, bankPromptCursor == 0, 84, 44, "あずける");
            DrawOption(g, bankPromptCursor == 1, 84, 72, "ひきだす");
            DrawOption(g, bankPromptCursor == 2, 84, 100, "かりる");
            DrawOption(g, bankPromptCursor == 3, 84, 128, "やめる");
        }
        else
        {
            DrawText(g, "↑↓: せんたく", new Rectangle(54, 50, 188, 24), smallFont);
            DrawText(g, "Z: けってい", new Rectangle(54, 78, 188, 24), smallFont);
            DrawText(g, "ESC(X): もどる", new Rectangle(54, 106, 188, 24), smallFont);
        }

        DrawWindow(g, listRect);
        DrawText(g, bankPhase switch
        {
            BankPhase.DepositList => "あずける",
            BankPhase.WithdrawList => "ひきだす",
            BankPhase.BorrowList => "かりる",
            _ => "ぎんこう"
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
        DrawText(g, $"てもち: {player.Gold}G", new Rectangle(infoRect.X + 20, infoRect.Y + 14, 196, 20), smallFont);
        DrawText(g, $"よきん: {player.BankGold}G", new Rectangle(infoRect.X + 20, infoRect.Y + 38, 196, 20), smallFont);
        DrawText(g, $"しゃっきん: {player.LoanBalance}G", new Rectangle(infoRect.X + 20, infoRect.Y + 62, 196, 20), smallFont);
        DrawText(g, $"しんよう: {bankService.GetAvailableCredit(player)}G", new Rectangle(infoRect.X + 20, infoRect.Y + 86, 196, 20), smallFont);

        DrawWindow(g, messageRect);
        DrawText(g, bankMessage, Rectangle.Inflate(messageRect, -24, -24), smallFont, wrap: true);
    }
}
