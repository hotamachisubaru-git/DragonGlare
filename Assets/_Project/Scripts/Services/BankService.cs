using DragonGlare.Domain;
using DragonGlare.Domain.Player;

namespace DragonGlare.Services;

public sealed class BankService
{
    private const int StepInterestInterval = 12;

    public int GetBorrowLimit(PlayerProgress player)
    {
        return Math.Min(PlayerProgress.MaxGoldValue, 240 + (player.Level * 160));
    }

    public int GetAvailableCredit(PlayerProgress player)
    {
        return Math.Max(0, GetBorrowLimit(player) - player.LoanBalance);
    }

    public BankTransactionResult Deposit(PlayerProgress player, int requestedAmount)
    {
        var language = player.Language;
        var amount = Math.Min(player.Gold, Math.Max(0, requestedAmount));
        if (amount <= 0)
        {
            return new BankTransactionResult(false, 0, 0, Text(language, "・翫後≠縺壹￠繧・縺翫°縺ｭ縺・縺ｪ縺・ｈ縺・□縲ゅ・, "* \"You have no gold to deposit.\""));
        }

        var repayment = Math.Min(amount, player.LoanBalance);
        var remaining = amount - repayment;
        var depositCapacity = PlayerProgress.MaxGoldValue - player.BankGold;
        var deposited = Math.Min(remaining, depositCapacity);
        var returned = remaining - deposited;

        player.Gold -= amount;
        player.LoanBalance -= repayment;
        player.BankGold += deposited;
        if (player.LoanBalance == 0)
        {
            player.LoanStepCounter = 0;
        }

        if (returned > 0)
        {
            player.Gold += returned;
        }

        var message = repayment > 0 && deposited > 0
            ? Text(language,
                $"・翫鶏repayment}G繧・縺ｸ繧薙＆縺・＠\n縲{deposited}G繧・縺ゅ★縺九▲縺溘ｈ縲ゅ・,
                $"* \"Repaid {repayment}G\n  and deposited {deposited}G.\"")
            : repayment > 0
                ? Text(language,
                    $"・翫鶏repayment}G繧・縺ｸ繧薙＆縺・↓ 縺ゅ※縺溘ｈ縲ゅ・,
                    $"* \"Applied {repayment}G to your loan.\"")
                : Text(language,
                    $"・翫鶏deposited}G繧・縺ゅ★縺九▲縺溘ｈ縲ゅ・,
                    $"* \"Deposited {deposited}G.\"");

        if (returned > 0)
        {
            message += language == UiLanguage.English
                ? $"\n  {returned}G stayed with you."
                : $"\n縲{returned}G縺ｯ 繧ゅ■縺阪ｌ縺・縺ｦ繧ゅ→縺ｫ 縺ｮ縺薙＠縺溘・;
        }

        return new BankTransactionResult(true, deposited, repayment, message);
    }

    public BankTransactionResult Withdraw(PlayerProgress player, int requestedAmount)
    {
        var language = player.Language;
        var amount = Math.Min(player.BankGold, Math.Max(0, requestedAmount));
        if (amount <= 0)
        {
            return new BankTransactionResult(false, 0, 0, Text(language, "・翫後・縺阪□縺帙ｋ 縺翫°縺ｭ縺・縺ｪ縺・ｈ縺・□縲ゅ・, "* \"There is no gold to withdraw.\""));
        }

        var capacity = PlayerProgress.MaxGoldValue - player.Gold;
        var withdrawn = Math.Min(amount, capacity);
        if (withdrawn <= 0)
        {
            return new BankTransactionResult(false, 0, 0, Text(language, "・翫後％繧後＞縺倥ｇ縺・・ 繧ゅ■縺阪ｌ縺ｪ縺・ｈ縲ゅ・, "* \"You cannot carry any more.\""));
        }

        player.BankGold -= withdrawn;
        player.Gold += withdrawn;

        var message = Text(language, $"・翫鶏withdrawn}G繧・縺ｲ縺阪□縺励◆繧医ゅ・, $"* \"Withdrew {withdrawn}G.\"");
        if (withdrawn < amount)
        {
            message += language == UiLanguage.English
                ? "\n  You could not carry it all."
                : "\n縲縺ｦ繧ゅ■縺・縺・▲縺ｱ縺・□縺九ｉ 縺懊ｓ縺ｶ縺ｯ 繧繧翫□縲・;
        }

        return new BankTransactionResult(true, withdrawn, 0, message);
    }

    public BankTransactionResult Borrow(PlayerProgress player, int requestedAmount)
    {
        var language = player.Language;
        var availableCredit = GetAvailableCredit(player);
        var amount = Math.Min(Math.Max(0, requestedAmount), availableCredit);
        amount = Math.Min(amount, PlayerProgress.MaxGoldValue - player.Gold);

        if (amount <= 0)
        {
            return new BankTransactionResult(false, 0, 0, Text(language, "・翫後ｂ縺・縺薙ｌ縺・§繧・≧縺ｯ 縺九○縺ｪ縺・・縲ゅ・, "* \"I cannot lend you any more.\""));
        }

        player.Gold += amount;
        player.LoanBalance += amount;

        return new BankTransactionResult(
            true,
            amount,
            0,
            Text(language,
                $"・翫鶏amount}G繧・縺九＠縺溘ｈ縲・n縲縺励ｃ縺｣縺阪ｓ縺ｯ {player.LoanBalance}G縺縲ゅ・,
                $"* \"Lent you {amount}G.\n  Your loan is {player.LoanBalance}G.\""));
    }

    public int AccrueStepInterest(PlayerProgress player, int stepCount = 1)
    {
        if (player.LoanBalance <= 0 || stepCount <= 0)
        {
            return 0;
        }

        player.LoanStepCounter += stepCount;
        var addedInterest = 0;
        while (player.LoanStepCounter >= StepInterestInterval)
        {
            player.LoanStepCounter -= StepInterestInterval;
            addedInterest += Math.Max(1, player.LoanBalance / 240);
        }

        if (addedInterest > 0)
        {
            player.LoanBalance = Math.Min(PlayerProgress.MaxGoldValue, player.LoanBalance + addedInterest);
        }

        return addedInterest;
    }

    public int AccrueBattleInterest(PlayerProgress player)
    {
        if (player.LoanBalance <= 0)
        {
            return 0;
        }

        var addedInterest = Math.Max(2, player.LoanBalance / 120);
        player.LoanBalance = Math.Min(PlayerProgress.MaxGoldValue, player.LoanBalance + addedInterest);
        return addedInterest;
    }

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
    }
}

public sealed record BankTransactionResult(bool Success, int Amount, int RepaymentAmount, string Message);
