using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class BattleAndProgressionTests
{
    [Fact]
    public void ResolveTurn_WhenAttackFinishesEnemy_ReturnsVictory()
    {
        var random = new Random(0);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 4;
        player.BaseAttack = 18;

        var encounter = new BattleEncounter(new EnemyDefinition("test", "テストまもの", 10, 1, 0, 2, 3));

        var result = service.ResolveTurn(player, encounter, BattleActionType.Attack, null, null, random);

        Assert.Equal(BattleOutcome.Victory, result.Outcome);
        Assert.Equal(0, encounter.CurrentHp);
    }

    [Fact]
    public void ApplyBattleRewards_WhenThresholdPassed_LevelsUpAndRestoresResources()
    {
        var random = new Random(0);
        var progression = new ProgressionService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Name = "テスター";
        player.Experience = 10;
        player.CurrentHp = 3;
        player.CurrentMp = 0;

        var message = progression.ApplyBattleRewards(player, 5, 12, random);

        Assert.Equal(2, player.Level);
        Assert.Equal(player.MaxHp, player.CurrentHp);
        Assert.Equal(player.MaxMp, player.CurrentMp);
        Assert.Contains("レベル2", message);
    }

    [Fact]
    public void ApplyDefeatPenalty_RespawnsPlayerAndRestoresHpMp()
    {
        var progression = new ProgressionService();
        var player = PlayerProgress.CreateDefault(new Point(4, 4));
        player.Gold = 100;
        player.CurrentHp = 0;
        player.CurrentMp = 0;

        progression.ApplyDefeatPenalty(player, new Point(1, 2));

        Assert.Equal(new Point(1, 2), player.TilePosition);
        Assert.Equal(player.MaxHp, player.CurrentHp);
        Assert.Equal(player.MaxMp, player.CurrentMp);
        Assert.Equal(80, player.Gold);
    }
}
