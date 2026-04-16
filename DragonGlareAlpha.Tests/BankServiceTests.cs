using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class BankServiceTests
{
    [Fact]
    public void Deposit_RepaysLoanBeforeIncreasingBankBalance()
    {
        var service = new BankService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Gold = 150;
        player.LoanBalance = 90;

        var result = service.Deposit(player, 120);

        Assert.True(result.Success);
        Assert.Equal(30, player.Gold);
        Assert.Equal(30, player.BankGold);
        Assert.Equal(0, player.LoanBalance);
        Assert.Equal(90, result.RepaymentAmount);
    }

    [Fact]
    public void Withdraw_MovesMoneyFromBankToHand()
    {
        var service = new BankService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Gold = 20;
        player.BankGold = 100;

        var result = service.Withdraw(player, 60);

        Assert.True(result.Success);
        Assert.Equal(80, player.Gold);
        Assert.Equal(40, player.BankGold);
        Assert.Equal(60, result.Amount);
    }

    [Fact]
    public void Borrow_AddsGoldAndLoanBalanceWithinCreditLimit()
    {
        var service = new BankService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 1;
        player.Gold = 50;

        var result = service.Borrow(player, 200);

        Assert.True(result.Success);
        Assert.Equal(250, player.Gold);
        Assert.Equal(200, player.LoanBalance);
    }

    [Fact]
    public void AccrueStepInterest_AddsInterestAfterConfiguredInterval()
    {
        var service = new BankService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.LoanBalance = 240;

        var addedInterest = service.AccrueStepInterest(player, 12);

        Assert.Equal(1, addedInterest);
        Assert.Equal(241, player.LoanBalance);
        Assert.Equal(0, player.LoanStepCounter);
    }
}
