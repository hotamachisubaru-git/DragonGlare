using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Data;
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

        var encounter = new BattleEncounter(new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 10, 1, 0, 2, 3));

        var result = service.ResolveTurn(player, encounter, BattleActionType.Attack, null, null, null, null, random);

        Assert.Equal(BattleOutcome.Victory, result.Outcome);
        Assert.Equal(0, encounter.CurrentHp);
    }

    [Fact]
    public void GetPlayerDefense_IncludesEquippedArmorBonus()
    {
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 6;

        var armor = GameContent.GetArmorById("leather_armor");

        var defense = service.GetPlayerDefense(player, armor);

        Assert.Equal(player.BaseDefense + 3 + armor!.DefenseBonus, defense);
    }

    [Fact]
    public void ResolveTurn_WhenDefending_ReducesIncomingDamage()
    {
        var random = new FixedRandom(1);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.CurrentHp = 20;
        player.BaseDefense = 0;
        var encounter = new BattleEncounter(new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 12, 8, 0, 2, 3));

        var result = service.ResolveTurn(player, encounter, BattleActionType.Defend, null, null, null, null, random);

        Assert.Equal(BattleOutcome.Ongoing, result.Outcome);
        Assert.Equal(15, player.CurrentHp);
        Assert.Contains("みをまもっている", result.Steps[0].Message);
        Assert.Contains("ダメージに おさえた", result.Steps[2].Message);
    }

    [Fact]
    public void ResolveTurn_WhenEquippingWeapon_UpdatesEquipmentAndContinuesBattle()
    {
        var random = new FixedRandom(1);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.BaseDefense = 4;
        player.AddItem("stick");
        player.AddItem("club");
        player.EquippedWeaponId = "stick";
        var club = GameContent.GetWeaponById("club");
        var encounter = new BattleEncounter(new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 14, 6, 0, 2, 3));

        var result = service.ResolveTurn(player, encounter, BattleActionType.Equip, null, null, null, club, random);

        Assert.Equal(BattleOutcome.Ongoing, result.Outcome);
        Assert.Equal("club", player.EquippedWeaponId);
        Assert.Contains("こんぼうを そうびした", result.Steps[0].Message);
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

        var enemy = new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 10, 1, 0, 14, 12);
        var message = progression.ApplyBattleRewards(player, enemy, random);

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

    [Fact]
    public void ApplyDefeatPenalty_WhenLoanRemains_SeizesEquippedWeapon()
    {
        var progression = new ProgressionService();
        var player = PlayerProgress.CreateDefault(new Point(4, 4));
        player.AddItem("stick");
        player.EquippedWeaponId = "stick";
        player.LoanBalance = 8;

        var message = progression.ApplyDefeatPenalty(player, new Point(1, 2));

        Assert.Equal(new Point(1, 2), player.TilePosition);
        Assert.Equal(0, player.GetItemCount("stick"));
        Assert.Null(player.EquippedWeaponId);
        Assert.Equal(0, player.LoanBalance);
        Assert.Contains("さしおさえ", message);
    }

    [Fact]
    public void ApplyBattleRewards_WhenReachingLevel99_CapsLevelGoldAndVitalStats()
    {
        var random = new Random(0);
        var progression = new ProgressionService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 98;
        player.Experience = ProgressionService.MaxLevelExperience - 10;
        player.MaxHp = 120;
        player.MaxMp = 40;
        player.CurrentHp = 1;
        player.CurrentMp = 1;
        player.Gold = 99990;

        var enemy = new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 10, 1, 0, 50, 50);
        progression.ApplyBattleRewards(player, enemy, random);

        Assert.Equal(PlayerProgress.MaxLevelValue, player.Level);
        Assert.Equal(PlayerProgress.MaxVitalValue, player.MaxHp);
        Assert.Equal(PlayerProgress.MaxVitalValue, player.MaxMp);
        Assert.Equal(player.MaxHp, player.CurrentHp);
        Assert.Equal(player.MaxMp, player.CurrentMp);
        Assert.Equal(PlayerProgress.MaxGoldValue, player.Gold);
        Assert.Equal(ProgressionService.MaxLevelExperience, player.Experience);
    }

    [Fact]
    public void Normalize_ClampsLoadedValuesToConfiguredMaximums()
    {
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 140;
        player.MaxHp = 1200;
        player.CurrentHp = 1200;
        player.MaxMp = 5000;
        player.CurrentMp = 5000;
        player.Gold = 200000;

        player.Normalize();

        Assert.Equal(PlayerProgress.MaxLevelValue, player.Level);
        Assert.Equal(PlayerProgress.MaxVitalValue, player.MaxHp);
        Assert.Equal(PlayerProgress.MaxVitalValue, player.CurrentHp);
        Assert.Equal(PlayerProgress.MaxVitalValue, player.MaxMp);
        Assert.Equal(PlayerProgress.MaxVitalValue, player.CurrentMp);
        Assert.Equal(PlayerProgress.MaxGoldValue, player.Gold);
    }

    [Fact]
    public void MaxLevelExperience_IsCalculatedForLevel99()
    {
        Assert.Equal(48706, ProgressionService.MaxLevelExperience);
    }

    [Fact]
    public void CreateEncounter_UsesMapAndLevelBasedPool()
    {
        var service = new BattleService();

        var encounter = service.CreateEncounter(new FixedRandom(0), FieldMapId.Castle, 1);

        Assert.Equal(FieldMapId.Castle, encounter.Enemy.EncounterMap);
        Assert.Contains(encounter.Enemy.Id, new[] { "iron_mite" });
    }

    [Fact]
    public void ApplyBattleRewards_WhenDropRollSucceeds_AddsDroppedItem()
    {
        var progression = new ProgressionService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        var enemy = new EnemyDefinition(
            "drop_test",
            "ドロップまもの",
            FieldMapId.Field,
            1,
            99,
            1,
            12,
            2,
            1,
            4,
            6,
            new EnemyDropDefinition("fire_orb", 100));

        var message = progression.ApplyBattleRewards(player, enemy, new FixedRandom(0));

        Assert.Equal(1, player.GetItemCount("fire_orb"));
        Assert.Contains("ひのたま", message);
    }

    private sealed class FixedRandom(params int[] values) : Random
    {
        private readonly Queue<int> values = new(values);

        public override int Next(int maxValue)
        {
            return values.Count == 0 ? 0 : values.Dequeue();
        }

        public override int Next(int minValue, int maxValue)
        {
            if (values.Count == 0)
            {
                return minValue;
            }

            var value = values.Dequeue();
            return Math.Clamp(value, minValue, maxValue - 1);
        }
    }
}
