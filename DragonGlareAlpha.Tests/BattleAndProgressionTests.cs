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

        var result = service.ResolveTurn(player, encounter, BattleActionType.Attack, null, null, random);

        Assert.Equal(BattleOutcome.Victory, result.Outcome);
        Assert.Equal(0, encounter.CurrentHp);
    }

    [Fact]
    public void GetPlayerDefense_IncludesEquippedArmorBonusesAcrossSlots()
    {
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 6;
        player.EquippedArmorId = "leather_armor";
        player.EquippedHeadId = "leather_cap";
        player.EquippedFeetId = "travel_boots";

        var defense = service.GetPlayerDefense(player);

        Assert.Equal(player.BaseDefense + 3 + 3 + 1 + 1, defense);
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

        var result = service.ResolveTurn(player, encounter, BattleActionType.Defend, null, null, random);

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

        var result = service.ResolveTurn(player, encounter, BattleActionType.Equip, null, club, random);

        Assert.Equal(BattleOutcome.Ongoing, result.Outcome);
        Assert.Equal("club", player.EquippedWeaponId);
        Assert.Contains("こんぼうを そうびした", result.Steps[0].Message);
    }

    [Fact]
    public void ResolveTurn_WhenEquippingHeadGear_UpdatesHeadSlotAndContinuesBattle()
    {
        var random = new FixedRandom(1);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.BaseDefense = 4;
        player.AddItem("leather_cap");
        var cap = GameContent.GetArmorById("leather_cap");
        var encounter = new BattleEncounter(new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 14, 6, 0, 2, 3));

        var result = service.ResolveTurn(player, encounter, BattleActionType.Equip, null, cap, random);

        Assert.Equal(BattleOutcome.Ongoing, result.Outcome);
        Assert.Equal("leather_cap", player.EquippedHeadId);
        Assert.Contains("かわぼうしを そうびした", result.Steps[0].Message);
    }

    [Fact]
    public void ResolveTurn_WhenCastingKnownDamageSpell_ConsumesMpAndDamagesEnemy()
    {
        var random = new FixedRandom(3, 1);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 4;
        player.CurrentMp = 8;
        player.BaseDefense = 99;
        var spell = GameContent.SpellCatalog.Single(spell => spell.Id == "spark");
        var encounter = new BattleEncounter(new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 80, 1, 2, 2, 3));

        var result = service.ResolveTurn(player, encounter, BattleActionType.Spell, spell, null, null, random);

        Assert.Equal(BattleOutcome.Ongoing, result.Outcome);
        Assert.Equal(3, player.CurrentMp);
        Assert.True(encounter.CurrentHp < encounter.Enemy.MaxHp);
        Assert.Contains("ライデン", result.Steps[0].Message);
    }

    [Fact]
    public void ResolveTurn_WhenCastingPoisonSpell_AppliesEnemyStatusAndTicksDamage()
    {
        var random = new FixedRandom(0, 1);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Level = 3;
        player.CurrentMp = 8;
        player.BaseDefense = 99;
        var spell = GameContent.SpellCatalog.Single(spell => spell.Id == "venom");
        var encounter = new BattleEncounter(new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 80, 1, 1, 2, 3));

        var result = service.ResolveTurn(player, encounter, BattleActionType.Spell, spell, null, null, random);

        Assert.Equal(BattleOutcome.Ongoing, result.Outcome);
        Assert.Equal(BattleStatusEffect.Poison, encounter.EnemyStatusEffect);
        Assert.True(encounter.CurrentHp < encounter.Enemy.MaxHp);
        Assert.Contains(result.Steps, step => step.Message.Contains("どく", StringComparison.Ordinal));
    }

    [Fact]
    public void ResolveTurn_WhenEnemyInflictsSleep_PlayerLosesNextAction()
    {
        var random = new FixedRandom(1, 0, 1, 99);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.CurrentHp = 20;
        player.BaseDefense = 99;
        var enemy = new EnemyDefinition(
            "sleep_test",
            "ねむりまもの",
            FieldMapId.Field,
            1,
            99,
            1,
            40,
            1,
            1,
            2,
            3,
            AttackStatusEffect: BattleStatusEffect.Sleep,
            AttackStatusChancePercent: 50,
            AttackStatusTurns: 1);
        var encounter = new BattleEncounter(enemy);

        var first = service.ResolveTurn(player, encounter, BattleActionType.Defend, null, null, random);
        var second = service.ResolveTurn(player, encounter, BattleActionType.Attack, null, null, random);

        Assert.Equal(BattleStatusEffect.Sleep, first.Steps.Any(step => step.Message.Contains("ねむってしまった", StringComparison.Ordinal))
            ? BattleStatusEffect.Sleep
            : BattleStatusEffect.None);
        Assert.Contains("ねむっている", second.Steps[0].Message);
        Assert.Equal(BattleStatusEffect.None, encounter.PlayerStatusEffect);
    }

    [Fact]
    public void ResolveTurn_WhenLanguageIsEnglish_UsesEnglishBattleText()
    {
        var random = new FixedRandom(2, 1);
        var service = new BattleService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0), UiLanguage.English);
        player.BaseDefense = 99;
        var enemy = new EnemyDefinition("test", "テストまもの", FieldMapId.Field, 1, 99, 1, 40, 1, 1, 2, 3, EnglishName: "Test Beast");
        var encounter = new BattleEncounter(enemy);

        var result = service.ResolveTurn(player, encounter, BattleActionType.Attack, null, null, random);

        Assert.Contains("Adventurer attacks!", result.Steps[0].Message);
        Assert.Contains("Test Beast takes", result.Steps[1].Message);
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
    public void GetEncounterPool_ForDungeon_UsesCastleEnemies()
    {
        var service = new BattleService();

        var pool = service.GetEncounterPool(FieldMapId.Dungeon, 1);

        Assert.Contains(pool, enemy => enemy.EncounterMap == FieldMapId.Castle);
        Assert.DoesNotContain(pool, enemy => enemy.EncounterMap == FieldMapId.Field);
    }

    [Fact]
    public void GetEncounterPool_WhenOnlyOneEnemyMatchesLevel_IncludesNearbyEnemy()
    {
        var service = new BattleService();

        var pool = service.GetEncounterPool(FieldMapId.Field, 2);

        Assert.Contains(pool, enemy => enemy.Id == "bog_lizard");
        Assert.Contains(pool, enemy => enemy.Id == "stone_wolf");
        Assert.True(pool.Count >= 2);
    }

    [Fact]
    public void GetEncounterPool_WhenNoEnemyMatchesLevel_UsesNearestMapEnemies()
    {
        var service = new BattleService();

        var pool = service.GetEncounterPool(FieldMapId.Field, 1);

        Assert.Contains(pool, enemy => enemy.Id == "bog_lizard");
        Assert.Contains(pool, enemy => enemy.Id == "stone_wolf");
        Assert.DoesNotContain(pool, enemy => enemy.Id == "ancient_wyrm");
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
