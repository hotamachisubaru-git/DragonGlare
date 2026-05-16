using DragonGlare.Domain;
using DragonGlare.Domain.Items;

namespace DragonGlare.Data;

/// <summary>
/// Consumable item catalogue.
///
/// Balance philosophy:
///   – Healing Herb (10 G, 16 HP): cheaper than HEAL (3 MP) for big HP pools
///     but costs a turn and gold; both options are valid at different stages.
///   – Mana Seed (28 G, 4 MP): lets you cast an extra FLARE or top off HEAL;
///     priced so gold cost is meaningful relative to field gold drops.
///   – Fire Orb (40 G, 22 dmg): better single-hit than FLARE (14 dmg) but
///     consumes gold; trades resource types.
///   – Healing Bloom (30 G, 38 HP): mid-game heal; covers more HP than two
///     Herbs for less gold, rewarding inventory planning.
///   – Ether Drop (52 G, 8 MP): nearly two full FLARE casts; strong utility
///     in extended dungeon runs.
///   – Thunder Orb (72 G, 38 dmg): matches SPARK (28 dmg @ 5 MP) but costs
///     gold; situationally better when MP is depleted.
///   – Royal Jelly (100 G, 72 HP): late-game panic button; nearly full-heal
///     at high levels before MaxHp inflation kicks in.
///
/// PINNED values (tests assert exact prices):
///   healing_herb  Price = 10
///
/// ConsumableDefinition: Id, Name, Description, EffectType, Amount, Price,
///                        [EnglishName], [EnglishDescription]
/// </summary>
public static class Consumables
{
    public static readonly ConsumableDefinition[] ConsumableCatalog = new ConsumableDefinition[]
    {
        new("healing_herb",  "やくそう",
            "HPを 16かいふく",     ConsumableEffectType.HealHp,     16,  10,   // PINNED price=10; was 12 HP
            "Healing Herb",    "Restores 16 HP"),

        new("mana_seed",     "まりょくのたね",
            "MPを 4かいふく",      ConsumableEffectType.HealMp,      4,  28,   // was 3 MP / 24 G
            "Mana Seed",       "Restores 4 MP"),

        new("fire_orb",      "ひのたま",
            "てきに 22ダメージ",   ConsumableEffectType.DamageEnemy, 22,  40,  // was 18 dmg / 36 G
            "Fire Orb",        "Deals 22 damage"),

        new("healing_bloom", "いやしぐさ",
            "HPを 38かいふく",     ConsumableEffectType.HealHp,     38,  30,   // was 28 HP / 26 G
            "Healing Bloom",   "Restores 38 HP"),

        new("ether_drop",    "まりょくのみず",
            "MPを 8かいふく",      ConsumableEffectType.HealMp,      8,  52,   // was 7 MP / 44 G
            "Ether Drop",      "Restores 8 MP"),

        new("thunder_orb",   "いかずちだま",
            "てきに 38ダメージ",   ConsumableEffectType.DamageEnemy, 38,  72,   // was 32 dmg / 64 G
            "Thunder Orb",     "Deals 38 damage"),

        new("royal_jelly",   "おうじょのミツ",
            "HPを 72かいふく",     ConsumableEffectType.HealHp,     72, 100,   // was 60 HP / 88 G
            "Royal Jelly",     "Restores 72 HP")
    };
}
