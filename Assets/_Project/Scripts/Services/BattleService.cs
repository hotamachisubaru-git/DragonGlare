using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed partial class BattleService
{
    private const int MinimumEncounterPoolSize = 2;
    private static readonly EquipmentSlot[] ArmorSlots =
    [
        EquipmentSlot.Armor,
        EquipmentSlot.Head,
        EquipmentSlot.Arms,
        EquipmentSlot.Legs,
        EquipmentSlot.Feet
    ];

    public BattleEncounter CreateEncounter(Random random, FieldMapId encounterMap, int playerLevel)
    {
        var pool = GetEncounterPool(encounterMap, playerLevel);
        var enemy = SelectEnemyFromPool(random, pool);
        return new BattleEncounter(enemy);
    }

    public IReadOnlyList<EnemyDefinition> GetEncounterPool(FieldMapId encounterMap, int playerLevel)
    {
        var enemyMap = encounterMap == FieldMapId.Dungeon ? FieldMapId.Castle : encounterMap;
        var mapPool = GameContent.EnemyCatalog
            .Where(enemy => enemy.EncounterMap == enemyMap)
            .ToArray();

        if (mapPool.Length == 0)
        {
            return GameContent.EnemyCatalog;
        }

        var levelPool = mapPool
            .Where(enemy => playerLevel >= enemy.MinRecommendedLevel && playerLevel <= enemy.MaxRecommendedLevel)
            .ToArray();

        var targetPoolSize = Math.Min(MinimumEncounterPoolSize, mapPool.Length);
        if (levelPool.Length >= targetPoolSize)
        {
            return levelPool;
        }

        return levelPool
            .Concat(mapPool
                .Except(levelPool)
                .OrderBy(enemy => GetRecommendedLevelDistance(enemy, playerLevel))
                .ThenBy(enemy => enemy.MinRecommendedLevel)
                .ThenBy(enemy => enemy.Id, StringComparer.Ordinal))
            .Take(targetPoolSize)
            .ToArray();
    }

    public IReadOnlyList<SpellDefinition> GetKnownSpells(PlayerProgress player)
    {
        return GameContent.SpellCatalog
            .Where(spell => player.Level >= spell.MinimumLevel)
            .OrderBy(spell => spell.MinimumLevel)
            .ThenBy(spell => spell.MpCost)
            .ToArray();
    }

    public BattleTurnResolution ResolveTurn(
        PlayerProgress player,
        BattleEncounter encounter,
        BattleActionType action,
        ConsumableDefinition? selectedConsumable,
        IEquipmentDefinition? selectedEquipment,
        Random random)
    {
        return ResolveTurn(player, encounter, action, null, selectedConsumable, selectedEquipment, random);
    }

    public BattleTurnResolution ResolveTurn(
        PlayerProgress player,
        BattleEncounter encounter,
        BattleActionType action,
        SpellDefinition? selectedSpell,
        ConsumableDefinition? selectedConsumable,
        IEquipmentDefinition? selectedEquipment,
        Random random)
    {
        if (encounter.PlayerStatusEffect == BattleStatusEffect.Sleep &&
            encounter.PlayerStatusTurnsRemaining > 0)
        {
            return ResolvePlayerSleepTurn(player, encounter, random);
        }

        return action switch
        {
            BattleActionType.Attack => ResolveAttack(player, encounter, random),
            BattleActionType.Spell => ResolveSpell(player, encounter, selectedSpell, random),
            BattleActionType.Defend => ResolveDefend(player, encounter, random),
            BattleActionType.Item => ResolveItem(player, encounter, selectedConsumable, random),
            BattleActionType.Equip => ResolveEquip(player, encounter, selectedEquipment, random),
            BattleActionType.Run => ResolveEscape(player),
            _ => Reject(player.Language, "こうどうできない。", "You cannot act.")
        };
    }

    public int GetPlayerAttack(PlayerProgress player, WeaponDefinition? equippedWeapon = null)
    {
        var weapon = equippedWeapon ?? GetEquippedWeapon(player);
        return player.BaseAttack + player.Level + (weapon?.AttackBonus ?? 0);
    }

    public int GetPlayerDefense(PlayerProgress player, ArmorDefinition? replacementArmor = null)
    {
        var defenseBonus = GetEquippedArmors(player)
            .Where(armor => replacementArmor is null || armor.Slot != replacementArmor.Slot)
            .Sum(armor => armor.DefenseBonus);

        defenseBonus += replacementArmor?.DefenseBonus ?? 0;
        return player.BaseDefense + Math.Max(0, player.Level / 2) + defenseBonus;
    }

    private static EnemyDefinition SelectEnemyFromPool(Random random, IReadOnlyList<EnemyDefinition> pool)
    {
        if (pool.Count == 0)
        {
            throw new InvalidOperationException("Encounter pool must contain at least one enemy.");
        }

        var totalWeight = pool.Sum(enemy => Math.Max(1, enemy.EncounterWeight));
        var roll = random.Next(totalWeight);
        foreach (var enemy in pool)
        {
            roll -= Math.Max(1, enemy.EncounterWeight);
            if (roll < 0)
            {
                return enemy;
            }
        }

        return pool[^1];
    }

    private static int GetRecommendedLevelDistance(EnemyDefinition enemy, int playerLevel)
    {
        if (playerLevel < enemy.MinRecommendedLevel)
        {
            return enemy.MinRecommendedLevel - playerLevel;
        }

        return playerLevel > enemy.MaxRecommendedLevel
            ? playerLevel - enemy.MaxRecommendedLevel
            : 0;
    }
}
