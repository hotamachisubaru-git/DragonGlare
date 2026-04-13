using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

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
        var amount = Math.Min(player.Gold, Math.Max(0, requestedAmount));
        if (amount <= 0)
        {
            return new BankTransactionResult(false, 0, 0, "＊「あずける おかねが ないようだ。」");
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
            ? $"＊「{repayment}Gを へんさいし\n　{deposited}Gを あずかったよ。」"
            : repayment > 0
                ? $"＊「{repayment}Gを へんさいに あてたよ。」"
                : $"＊「{deposited}Gを あずかったよ。」";

        if (returned > 0)
        {
            message += $"\n　{returned}Gは もちきれず てもとに のこした。";
        }

        return new BankTransactionResult(true, deposited, repayment, message);
    }

    public BankTransactionResult Withdraw(PlayerProgress player, int requestedAmount)
    {
        var amount = Math.Min(player.BankGold, Math.Max(0, requestedAmount));
        if (amount <= 0)
        {
            return new BankTransactionResult(false, 0, 0, "＊「ひきだせる おかねが ないようだ。」");
        }

        var capacity = PlayerProgress.MaxGoldValue - player.Gold;
        var withdrawn = Math.Min(amount, capacity);
        if (withdrawn <= 0)
        {
            return new BankTransactionResult(false, 0, 0, "＊「これいじょうは もちきれないよ。」");
        }

        player.BankGold -= withdrawn;
        player.Gold += withdrawn;

        var message = $"＊「{withdrawn}Gを ひきだしたよ。」";
        if (withdrawn < amount)
        {
            message += "\n　てもちが いっぱいだから ぜんぶは むりだ。";
        }

        return new BankTransactionResult(true, withdrawn, 0, message);
    }

    public BankTransactionResult Borrow(PlayerProgress player, int requestedAmount)
    {
        var availableCredit = GetAvailableCredit(player);
        var amount = Math.Min(Math.Max(0, requestedAmount), availableCredit);
        amount = Math.Min(amount, PlayerProgress.MaxGoldValue - player.Gold);

        if (amount <= 0)
        {
            return new BankTransactionResult(false, 0, 0, "＊「もう これいじょうは かせないね。」");
        }

        player.Gold += amount;
        player.LoanBalance += amount;

        return new BankTransactionResult(
            true,
            amount,
            0,
            $"＊「{amount}Gを かしたよ。\n　しゃっきんは {player.LoanBalance}Gだ。」");
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
}

public sealed record BankTransactionResult(bool Success, int Amount, int RepaymentAmount, string Message);
