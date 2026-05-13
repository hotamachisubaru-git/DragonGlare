using DragonGlare.Domain;
using DragonGlare.Domain.Battle;

namespace DragonGlare.Data;

/// <summary>
/// Spell catalogue.
/// Balance rationale:
///    EFLARE  (Lv 1): cheap opener; 2 MP for 14 dmg (7 dmg/MP)  Ereliable early option
///    EHEAL   (Lv 2): 3 MP restores 22 HP  Emeaningfully better than a Healing Herb
///    EVENOM  (Lv 3): 4 MP DoT; 4-turn poison ticks make it efficient vs high-HP foes
///    ESPARK  (Lv 4): 5 MP for 28 dmg (5.6 dmg/MP)  Eupgraded damage at mid-game cost
///    ECLEANSE(Lv 4): 3 MP utility; cost matches HEAL so there's a real trade-off
///    ESLEEP  (Lv 5): 5 MP crowd-control; 70% accuracy keeps it situational
///
/// SpellDefinition positional order:
///   Id, Name, EnglishName, Description, EnglishDescription,
///   MpCost, MinimumLevel, EffectType, Power,
///   [AccuracyPercent=100], [DurationTurns=0]
/// </summary>
public static class Spells
{
    public static readonly SpellDefinition[] SpellCatalog =
    [
        new("flare",   "メラ",     "FLARE",
            "てきに 火のダメージ",      "Fire damage to one enemy.",
            MpCost: 2, MinimumLevel: 1, SpellEffectType.DamageEnemy,    Power: 14),

        new("heal",    "ホイチE,   "HEAL",
            "HPめE22かいふぁE,         "Restore 22 HP.",
            MpCost: 3, MinimumLevel: 2, SpellEffectType.HealPlayer,     Power: 22),

        new("venom",   "ポイズン", "VENOM",
            "てきを どくにする",        "Poison one enemy.",
            MpCost: 4, MinimumLevel: 3, SpellEffectType.PoisonEnemy,    Power: 4,
            AccuracyPercent: 78, DurationTurns: 4),

        new("spark",   "ライチE��", "SPARK",
            "てきに 雷の大ダメージ",    "Heavy lightning damage.",
            MpCost: 5, MinimumLevel: 4, SpellEffectType.DamageEnemy,    Power: 28),

        new("cleanse", "キュア",   "CLEANSE",
            "状態異常めEなおす",        "Remove your status effect.",
            MpCost: 3, MinimumLevel: 4, SpellEffectType.CurePlayerStatus, Power: 0),

        new("sleep",   "ラリホ�E", "SLEEP",
            "てきを ねむらせめE,        "Put one enemy to sleep.",
            MpCost: 5, MinimumLevel: 5, SpellEffectType.SleepEnemy,     Power: 0,
            AccuracyPercent: 70, DurationTurns: 2)
    ];
}
