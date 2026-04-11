using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Field;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Data;

public static class GameContent
{
    public static readonly string[][] JapaneseNameTable =
    [
        ["あ", "い", "う", "え", "お", "か", "き", "く", "け", "こ"],
        ["さ", "し", "す", "せ", "そ", "た", "ち", "つ", "て", "と"],
        ["な", "に", "ぬ", "ね", "の", "は", "ひ", "ふ", "へ", "ほ"],
        ["ま", "み", "む", "め", "も", "や", "ゆ", "よ", "わ", "を"],
        ["ら", "り", "る", "れ", "ろ", "ん", "ー", "゛", "けす", "おわり"]
    ];

    public static readonly string[][] EnglishNameTable =
    [
        ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J"],
        ["K", "L", "M", "N", "O", "P", "Q", "R", "S", "T"],
        ["U", "V", "W", "X", "Y", "Z", "-", "'", "DEL", "END"]
    ];

    public static readonly string[,] BattleCommandLabels =
    {
        { "こうげき", "じゅもん" },
        { "どうぐ", "にげる" }
    };

    public static readonly BattleActionType[,] BattleCommandGrid =
    {
        { BattleActionType.Attack, BattleActionType.Spell },
        { BattleActionType.Item, BattleActionType.Run }
    };

    public static readonly WeaponDefinition[] ShopCatalog =
    [
        new("stick", "ぼう", 16, 2),
        new("club", "こんぼう", 32, 4),
        new("thorn_club", "とげのぼう", 64, 7),
        new("wood_blade", "ぼくとう", 82, 8),
        new("stone_axe", "いしのおの", 128, 11),
        new("bronze_sword", "どうのつるぎ", 196, 14)
    ];

    public static readonly EnemyDefinition[] EnemyCatalog =
    [
        new("horn_slime", "ホーンスライム", 18, 5, 1, 8, 12),
        new("night_shade", "ナイトシェイド", 28, 8, 3, 15, 24),
        new("dragon_pup", "ドラゴンパピー", 36, 11, 5, 24, 38)
    ];

    public static readonly ConsumableDefinition[] ConsumableCatalog =
    [
        new("healing_herb", "やくそう", "HPを 12かいふく", ConsumableEffectType.HealHp, 12),
        new("mana_seed", "まりょくのたね", "MPを 3かいふく", ConsumableEffectType.HealMp, 3),
        new("fire_orb", "ひのたま", "てきに 18ダメージ", ConsumableEffectType.DamageEnemy, 18)
    ];

    public static readonly FieldEventDefinition[] FieldEvents =
    [
        new(
            "guide_npc",
            FieldMapId.Hub,
            new Point(12, 7),
            Color.Cyan,
            true,
            FieldEventActionType.Dialogue,
            [
                "{player}、ようこそ。\nけんをみがき たびのしたくをしよう。".Replace("、", "、"),
                "やくそうは HPを なおし、\nひのたまは どうぐで なげられるぞ。"
            ],
            [
                "Welcome, {player}.\nSharpen your blade and prepare.",
                "Herbs heal you.\nFire orbs can be thrown from ITEMS."
            ],
            SpriteAssetName: "guide_npc.png"),
        new(
            "town_child",
            FieldMapId.Hub,
            new Point(4, 4),
            Color.FromArgb(120, 255, 180),
            true,
            FieldEventActionType.Dialogue,
            [
                "まちの こどもだ。\n「おしろの へいしって すごく まじめだよ！」",
                "「フィールドの くさむらは\n　まものが でやすいから きをつけてね。」"
            ],
            [
                "A village child grins.\n\"The castle guard is super serious!\"",
                "\"Watch the tall grass out in the field.\nMonsters jump out fast there.\""
            ],
            SpriteAssetName: "town_child.png"),
        new(
            "castle_guard",
            FieldMapId.Castle,
            new Point(12, 11),
            Color.FromArgb(255, 180, 120),
            true,
            FieldEventActionType.Dialogue,
            [
                "おしろの へいしだ。\n「りゅうの ひかりを おうものよ、あわてるな。」",
                "「レベルが あがったら そうびも みなおせ。\n　ちからだけでは かてぬぞ。」"
            ],
            [
                "A castle guard stands firm.\n\"Hunter of dragonlight, do not rush.\"",
                "\"When you grow stronger, review your gear.\nPower alone will not carry you.\""
            ],
            SpriteAssetName: "castle_guard.png"),
        new(
            "field_scout",
            FieldMapId.Field,
            new Point(11, 11),
            Color.FromArgb(255, 228, 120),
            true,
            FieldEventActionType.Dialogue,
            [
                "みはりの ぼうけんしゃだ。\n「このさきは ぬかるみが おおい。」",
                "「HPが へったら いったん もどれ。\n　むりやり すすむと いたいめを みるぞ。」"
            ],
            [
                "A field scout watches the road.\n\"The ground ahead gets rough.\"",
                "\"If your HP drops, fall back first.\nPushing through carelessly will cost you.\""
            ],
            SpriteAssetName: "field_scout.png"),
        new(
            "field_sign",
            FieldMapId.Hub,
            new Point(2, 12),
            Color.Gold,
            true,
            FieldEventActionType.Dialogue,
            [
                "たてふだだ。\nXで ステータスをひらける。",
                "Bで バトル、Vで ショップ。\nENTERで イベントを よめる。"
            ],
            [
                "A sign reads:\nPress X to open STATUS.",
                "Press B for battle, V for shop,\nand ENTER to inspect events."
            ]),
        new(
            "healing_spring",
            FieldMapId.Hub,
            new Point(16, 12),
            Color.MediumSpringGreen,
            true,
            FieldEventActionType.Recover,
            [
                "きらめく いずみだ。",
                "みずの ちからが からだに しみこんだ。"
            ],
            [
                "A shining spring bubbles here.",
                "The water restores your strength."
            ],
            RecoverHp: 999,
            RecoverMp: 999)
    ];

    public static string[][] GetNameTable(UiLanguage language)
    {
        return language == UiLanguage.Japanese ? JapaneseNameTable : EnglishNameTable;
    }

    public static WeaponDefinition? GetWeaponById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return ShopCatalog.FirstOrDefault(item => string.Equals(item.Id, itemId, StringComparison.Ordinal));
    }

    public static ConsumableDefinition? GetConsumableById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return ConsumableCatalog.FirstOrDefault(item => string.Equals(item.Id, itemId, StringComparison.Ordinal));
    }

    public static FieldEventDefinition? GetFieldEventById(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return null;
        }

        return FieldEvents.FirstOrDefault(fieldEvent => string.Equals(fieldEvent.Id, eventId, StringComparison.Ordinal));
    }
}
