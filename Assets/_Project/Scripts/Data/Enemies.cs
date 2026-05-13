using DragonGlare.Domain;
using DragonGlare.Domain.Battle;

namespace DragonGlare.Data;

/// <summary>
/// Enemy roster.  Level ranges are calibrated so each map tier matches the
/// equipment tier sold in the shop.  Gold rewards are tuned so ~8-12 kills
/// funds the next equipment upgrade.
///
/// EnemyDefinition positional order:
///   Id, Name, EncounterMap,
///   MinRecommendedLevel, MaxRecommendedLevel, EncounterWeight,
///   MaxHp, Attack, Defense, ExperienceReward, GoldReward,
///   [Drop], [SpriteAssetName], [EnglishName],
///   [AttackStatusEffect], [AttackStatusChancePercent], [AttackStatusTurns]
/// </summary>
public static class Enemies
{
    public static readonly EnemyDefinition[] EnemyCatalog =
    [
        // ── Hub (Lv 1-4) ────────────────────────────────────────────────────
        // Horn Slime   Egentle intro; ~12 kills to Lv 2; drops herbs
        new("horn_slime",  "ホ�Eンスライム", FieldMapId.Hub,
            1, 2, 6,
            22, 5, 1, 6, 14,
            new EnemyDropDefinition("healing_herb", 22),
            SpriteAssetName: "horn_slime.png",
            EnglishName: "Horn Slime"),

        // Moss Toad    Epoisons; teaches early resource management
        new("moss_toad",   "モスト�EチE,     FieldMapId.Hub,
            1, 3, 4,
            28, 6, 1, 8, 18,
            new EnemyDropDefinition("healing_herb", 16),
            SpriteAssetName: "moss_toad.png",
            EnglishName: "Moss Toad",
            AttackStatusEffect: BattleStatusEffect.Poison,
            AttackStatusChancePercent: 20, AttackStatusTurns: 3),

        // Ember Bat    Ehub elite; rewards mana seeds; rare
        new("ember_bat",   "エンバ�EバッチE, FieldMapId.Hub,
            2, 4, 2,
            34, 7, 2, 11, 22,
            new EnemyDropDefinition("mana_seed", 14),
            SpriteAssetName: "ember_bat.png",
            EnglishName: "Ember Bat"),

        // ── Castle (Lv 3-8) ──────────────────────────────────────────────────
        // Iron Mite    Earmoured crawler; common; solid XP
        new("iron_mite",   "アイアンマイチE, FieldMapId.Castle,
            3, 5, 5,
            40, 9, 3, 14, 26,
            new EnemyDropDefinition("healing_herb", 16),
            SpriteAssetName: "iron_mite.png",
            EnglishName: "Iron Mite"),

        // Night Shade  Esleeps player; teaches status counter-play
        new("night_shade", "ナイトシェイチE, FieldMapId.Castle,
            4, 7, 3,
            50, 12, 4, 20, 36,
            new EnemyDropDefinition("mana_seed", 14),
            SpriteAssetName: "night_shade.png",
            EnglishName: "Night Shade",
            AttackStatusEffect: BattleStatusEffect.Sleep,
            AttackStatusChancePercent: 22, AttackStatusTurns: 1),

        // Bell Armor   Eheavy-hitter; rare; gate to iron-tier gear
        new("bell_armor",  "ベルアーマ�E",   FieldMapId.Castle,
            5, 8, 2,
            64, 15, 6, 30, 54,
            new EnemyDropDefinition("fire_orb", 10),
            SpriteAssetName: "bell_armor.png",
            EnglishName: "Bell Armor"),

        // ── Field (Lv 6+) ────────────────────────────────────────────────────
        // Bog Lizard    Efirst field mob; poisons; iron-tier content
        new("bog_lizard",  "ボグリザーチE,   FieldMapId.Field,
            6, 8, 5,
            58, 14, 5, 28, 44,
            new EnemyDropDefinition("healing_herb", 16),
            SpriteAssetName: "enemy_slime.png",
            EnglishName: "Bog Lizard",
            AttackStatusEffect: BattleStatusEffect.Poison,
            AttackStatusChancePercent: 18, AttackStatusTurns: 3),

        // Stone Wolf    Estrong melee; funds steel-tier shopping
        new("stone_wolf",  "スト�EンウルチE, FieldMapId.Field,
            7, 10, 4,
            72, 17, 6, 38, 58,
            new EnemyDropDefinition("mana_seed", 14),
            SpriteAssetName: "stone_wolf.png",
            EnglishName: "Stone Wolf"),

        // Dragon Pup    Efire-type; drops fire orbs; needs spells/items
        new("dragon_pup",  "ドラゴンパピー", FieldMapId.Field,
            9, 12, 3,
            88, 20, 8, 52, 74,
            new EnemyDropDefinition("fire_orb", 12),
            SpriteAssetName: "dragon_pup.png",
            EnglishName: "Dragon Pup"),

        // Wyvern Scout  Esleeps player; mid-field elite
        new("wyvern_scout", "ワイバ�EンスカウチE, FieldMapId.Field,
            11, 14, 3,
            108, 24, 10, 70, 96,
            new EnemyDropDefinition("mana_seed", 10),
            SpriteAssetName: "wyvern_scout.png",
            EnglishName: "Wyvern Scout",
            AttackStatusEffect: BattleStatusEffect.Sleep,
            AttackStatusChancePercent: 18, AttackStatusTurns: 1),

        // Lava Drake    Eelite fire drake; lucrative drops
        new("lava_drake",  "ラヴァドレイク",     FieldMapId.Field,
            13, 99, 2,
            130, 28, 12, 96, 130,
            new EnemyDropDefinition("fire_orb", 14),
            SpriteAssetName: "lava_drake.png",
            EnglishName: "Lava Drake"),

        // Ancient Wyrm  Etop-tier mob; heavy poison; best gold
        new("ancient_wyrm", "エンシェントワーム", FieldMapId.Field,
            16, 99, 1,
            160, 32, 14, 130, 170,
            new EnemyDropDefinition("mana_seed", 12),
            SpriteAssetName: "ancient_wyrm.png",
            EnglishName: "Ancient Wyrm",
            AttackStatusEffect: BattleStatusEffect.Poison,
            AttackStatusChancePercent: 28, AttackStatusTurns: 4)
    ];
}
