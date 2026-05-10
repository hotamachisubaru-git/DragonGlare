using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Commerce;
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

    public static readonly string[,] JapaneseBattleCommandLabels =
    {
        { "たたかう", "どうぐ" },
        { "まもる", "そうび" },
        { "じゅもん", "にげる" }
    };

    public static readonly string[,] EnglishBattleCommandLabels =
    {
        { "ATTACK", "ITEM" },
        { "GUARD", "EQUIP" },
        { "SPELL", "RUN" }
    };

    public static readonly BattleActionType[,] BattleCommandGrid =
    {
        { BattleActionType.Attack, BattleActionType.Item },
        { BattleActionType.Defend, BattleActionType.Equip },
        { BattleActionType.Spell, BattleActionType.Run }
    };

    public static readonly SpellDefinition[] SpellCatalog =
    [
        new("flare", "メラ", "FLARE", "てきに 火のダメージ", "Fire damage to one enemy.", 2, 1, SpellEffectType.DamageEnemy, 12),
        new("heal", "ホイミ", "HEAL", "HPを 18かいふく", "Restore 18 HP.", 3, 2, SpellEffectType.HealPlayer, 18),
        new("venom", "ポイズン", "VENOM", "てきを どくにする", "Poison one enemy.", 4, 3, SpellEffectType.PoisonEnemy, 4, 78, 4),
        new("spark", "ライデン", "SPARK", "てきに 雷の大ダメージ", "Heavy lightning damage.", 5, 4, SpellEffectType.DamageEnemy, 24),
        new("cleanse", "キュア", "CLEANSE", "状態異常を なおす", "Remove your status effect.", 3, 4, SpellEffectType.CurePlayerStatus, 0),
        new("sleep", "ラリホー", "SLEEP", "てきを ねむらせる", "Put one enemy to sleep.", 5, 5, SpellEffectType.SleepEnemy, 0, 70, 2)
    ];

    public static readonly WeaponDefinition[] WeaponCatalog =
    [
        new("stick", "ぼう", 16, 2, "Stick"),
        new("club", "こんぼう", 32, 4, "Club"),
        new("bamboo_spear", "たけやり", 52, 6, "Bamboo Spear"),
        new("thorn_club", "とげのぼう", 64, 7, "Thorn Club"),
        new("wood_blade", "ぼくとう", 82, 8, "Wood Blade"),
        new("stone_axe", "いしのおの", 128, 11, "Stone Axe"),
        new("bronze_sword", "どうのつるぎ", 196, 14, "Bronze Sword"),
        new("iron_sword", "てつのけん", 288, 17, "Iron Sword"),
        new("steel_blade", "はがねけん", 416, 20, "Steel Blade"),
        new("dragon_lance", "りゅうのやり", 580, 24, "Dragon Lance")
    ];

    public static readonly ArmorDefinition[] ArmorCatalog =
    [
        new("cloth_tunic", "ぬののふく", 18, 1, EnglishName: "Cloth Tunic"),
        new("leather_armor", "かわのよろい", 48, 3, EnglishName: "Leather Armor"),
        new("scale_vest", "うろこふく", 72, 4, EnglishName: "Scale Vest"),
        new("bronze_mail", "どうよろい", 108, 6, EnglishName: "Bronze Mail"),
        new("iron_armor", "てつよろい", 152, 8, EnglishName: "Iron Armor"),
        new("steel_armor", "はがねよろい", 224, 10, EnglishName: "Steel Armor"),
        new("silver_mail", "ぎんむねあて", 336, 13, EnglishName: "Silver Mail"),
        new("dragon_mail", "りゅうよろい", 492, 16, EnglishName: "Dragon Mail"),
        new("leather_cap", "かわぼうし", 38, 1, EquipmentSlot.Head, "Leather Cap"),
        new("bronze_helm", "どうのかぶと", 112, 2, EquipmentSlot.Head, "Bronze Helm"),
        new("steel_helm", "はがねかぶと", 238, 4, EquipmentSlot.Head, "Steel Helm"),
        new("leather_bracers", "かわのこて", 34, 1, EquipmentSlot.Arms, "Leather Bracers"),
        new("bronze_gauntlets", "どうのこて", 104, 2, EquipmentSlot.Arms, "Bronze Gauntlets"),
        new("steel_gauntlets", "はがねのこて", 226, 4, EquipmentSlot.Arms, "Steel Gauntlets"),
        new("leather_leggings", "かわレギンス", 42, 1, EquipmentSlot.Legs, "Leather Leggings"),
        new("bronze_greaves", "どうレギンス", 116, 2, EquipmentSlot.Legs, "Bronze Greaves"),
        new("steel_greaves", "はがねレギンス", 232, 4, EquipmentSlot.Legs, "Steel Greaves"),
        new("travel_boots", "たびのブーツ", 28, 1, EquipmentSlot.Feet, "Travel Boots"),
        new("bronze_boots", "どうのブーツ", 96, 2, EquipmentSlot.Feet, "Bronze Boots"),
        new("steel_boots", "はがねブーツ", 212, 3, EquipmentSlot.Feet, "Steel Boots")
    ];

    public static readonly ConsumableDefinition[] ConsumableCatalog =
    [
        new("healing_herb", "やくそう", "HPを 12かいふく", ConsumableEffectType.HealHp, 12, 10, "Healing Herb", "Restores 12 HP"),
        new("mana_seed", "まりょくのたね", "MPを 3かいふく", ConsumableEffectType.HealMp, 3, 24, "Mana Seed", "Restores 3 MP"),
        new("fire_orb", "ひのたま", "てきに 18ダメージ", ConsumableEffectType.DamageEnemy, 18, 36, "Fire Orb", "Deals 18 damage"),
        new("healing_bloom", "いやしぐさ", "HPを 28かいふく", ConsumableEffectType.HealHp, 28, 26, "Healing Bloom", "Restores 28 HP"),
        new("ether_drop", "まりょくのみず", "MPを 7かいふく", ConsumableEffectType.HealMp, 7, 44, "Ether Drop", "Restores 7 MP"),
        new("thunder_orb", "いかずちだま", "てきに 32ダメージ", ConsumableEffectType.DamageEnemy, 32, 64, "Thunder Orb", "Deals 32 damage"),
        new("royal_jelly", "おうじょのミツ", "HPを 60かいふく", ConsumableEffectType.HealHp, 60, 88, "Royal Jelly", "Restores 60 HP")
    ];

    public static readonly ShopProductDefinition[] ShopCatalog =
        ConsumableCatalog
            .Select(item => new ShopProductDefinition(Consumable: item))
            .Concat(WeaponCatalog.Select(item => new ShopProductDefinition(Equipment: item)))
            .Concat(ArmorCatalog.Select(item => new ShopProductDefinition(Equipment: item)))
            .OrderBy(item => item.Price)
            .ThenBy(item => item.Name, StringComparer.Ordinal)
            .ToArray();

    public static readonly EnemyDefinition[] EnemyCatalog =
    [
        new("horn_slime", "ホーンスライム", FieldMapId.Hub, 1, 2, 6, 18, 5, 1, 8, 12, new EnemyDropDefinition("healing_herb", 24), SpriteAssetName: "horn_slime.png", EnglishName: "Horn Slime"),
        new("moss_toad", "モストード", FieldMapId.Hub, 1, 4, 4, 24, 7, 2, 12, 18, new EnemyDropDefinition("healing_herb", 18), SpriteAssetName: "moss_toad.png", EnglishName: "Moss Toad", AttackStatusEffect: BattleStatusEffect.Poison, AttackStatusChancePercent: 22, AttackStatusTurns: 4),
        new("ember_bat", "エンバーバット", FieldMapId.Hub, 3, 6, 2, 30, 9, 3, 16, 24, new EnemyDropDefinition("mana_seed", 14), SpriteAssetName: "ember_bat.png", EnglishName: "Ember Bat"),
        new("iron_mite", "アイアンマイト", FieldMapId.Castle, 1, 4, 5, 26, 8, 3, 13, 20, new EnemyDropDefinition("healing_herb", 18), SpriteAssetName: "iron_mite.png", EnglishName: "Iron Mite"),
        new("night_shade", "ナイトシェイド", FieldMapId.Castle, 3, 7, 3, 38, 11, 5, 24, 34, new EnemyDropDefinition("mana_seed", 15), SpriteAssetName: "night_shade.png", EnglishName: "Night Shade", AttackStatusEffect: BattleStatusEffect.Sleep, AttackStatusChancePercent: 24, AttackStatusTurns: 1),
        new("bell_armor", "ベルアーマー", FieldMapId.Castle, 5, 10, 2, 50, 14, 7, 38, 54, new EnemyDropDefinition("fire_orb", 12), SpriteAssetName: "bell_armor.png", EnglishName: "Bell Armor"),
        new("bog_lizard", "ボグリザード", FieldMapId.Field, 2, 5, 5, 34, 10, 4, 20, 28, new EnemyDropDefinition("healing_herb", 18), SpriteAssetName: "enemy_slime.png", EnglishName: "Bog Lizard", AttackStatusEffect: BattleStatusEffect.Poison, AttackStatusChancePercent: 18, AttackStatusTurns: 4),
        new("stone_wolf", "ストーンウルフ", FieldMapId.Field, 4, 8, 4, 46, 14, 7, 34, 46, new EnemyDropDefinition("mana_seed", 15), SpriteAssetName: "stone_wolf.png", EnglishName: "Stone Wolf"),
        new("dragon_pup", "ドラゴンパピー", FieldMapId.Field, 6, 11, 3, 58, 18, 9, 48, 68, new EnemyDropDefinition("fire_orb", 12), SpriteAssetName: "dragon_pup.png", EnglishName: "Dragon Pup"),
        new("wyvern_scout", "ワイバーンスカウト", FieldMapId.Field, 9, 15, 3, 72, 21, 11, 66, 96, new EnemyDropDefinition("mana_seed", 10), SpriteAssetName: "wyvern_scout.png", EnglishName: "Wyvern Scout", AttackStatusEffect: BattleStatusEffect.Sleep, AttackStatusChancePercent: 18, AttackStatusTurns: 1),
        new("lava_drake", "ラヴァドレイク", FieldMapId.Field, 13, 99, 2, 90, 25, 13, 88, 132, new EnemyDropDefinition("fire_orb", 15), SpriteAssetName: "lava_drake.png", EnglishName: "Lava Drake"),
        new("ancient_wyrm", "エンシェントワーム", FieldMapId.Field, 18, 99, 1, 112, 29, 15, 120, 180, new EnemyDropDefinition("mana_seed", 12), SpriteAssetName: "ancient_wyrm.png", EnglishName: "Ancient Wyrm", AttackStatusEffect: BattleStatusEffect.Poison, AttackStatusChancePercent: 30, AttackStatusTurns: 5)
    ];

    public static FieldTransitionDefinition[] FieldTransitions => FieldContent.FieldTransitions;

    public static FieldEventDefinition[] FieldEvents => FieldContent.FieldEvents;

    public static string[][] GetNameTable(UiLanguage language)
    {
        return language == UiLanguage.Japanese ? JapaneseNameTable : EnglishNameTable;
    }

    public static string GetBattleCommandLabel(UiLanguage language, int row, int column)
    {
        var labels = language == UiLanguage.English ? EnglishBattleCommandLabels : JapaneseBattleCommandLabels;
        return labels[row, column];
    }

    public static string GetSpellName(SpellDefinition spell, UiLanguage language)
    {
        return language == UiLanguage.English ? spell.EnglishName : spell.Name;
    }

    public static string GetSpellDescription(SpellDefinition spell, UiLanguage language)
    {
        return language == UiLanguage.English ? spell.EnglishDescription : spell.Description;
    }

    public static string GetConsumableName(ConsumableDefinition item, UiLanguage language)
    {
        return language == UiLanguage.English && !string.IsNullOrWhiteSpace(item.EnglishName)
            ? item.EnglishName
            : item.Name;
    }

    public static string GetConsumableDescription(ConsumableDefinition item, UiLanguage language)
    {
        return language == UiLanguage.English && !string.IsNullOrWhiteSpace(item.EnglishDescription)
            ? item.EnglishDescription
            : item.Description;
    }

    public static string GetWeaponName(WeaponDefinition item, UiLanguage language)
    {
        return language == UiLanguage.English && !string.IsNullOrWhiteSpace(item.EnglishName)
            ? item.EnglishName
            : item.Name;
    }

    public static string GetArmorName(ArmorDefinition item, UiLanguage language)
    {
        return language == UiLanguage.English && !string.IsNullOrWhiteSpace(item.EnglishName)
            ? item.EnglishName
            : item.Name;
    }

    public static string GetEquipmentName(IEquipmentDefinition item, UiLanguage language)
    {
        return item switch
        {
            WeaponDefinition weapon => GetWeaponName(weapon, language),
            ArmorDefinition armor => GetArmorName(armor, language),
            _ => item.Name
        };
    }

    public static string GetEnemyName(EnemyDefinition enemy, UiLanguage language)
    {
        return language == UiLanguage.English && !string.IsNullOrWhiteSpace(enemy.EnglishName)
            ? enemy.EnglishName
            : enemy.Name;
    }

    public static string GetShopProductName(ShopProductDefinition product, UiLanguage language)
    {
        if (product.Consumable is not null)
        {
            return GetConsumableName(product.Consumable, language);
        }

        return product.Equipment is not null
            ? GetEquipmentName(product.Equipment, language)
            : string.Empty;
    }

    public static WeaponDefinition? GetWeaponById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return WeaponCatalog.FirstOrDefault(item => string.Equals(item.Id, itemId, StringComparison.Ordinal));
    }

    public static ArmorDefinition? GetArmorById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return ArmorCatalog.FirstOrDefault(item => string.Equals(item.Id, itemId, StringComparison.Ordinal));
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

        return FieldContent.GetFieldEventById(eventId);
    }

    public static ShopProductDefinition? GetShopProductById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return ShopCatalog.FirstOrDefault(item => string.Equals(item.Id, itemId, StringComparison.Ordinal));
    }

    public static string GetItemName(string? itemId)
    {
        return GetConsumableById(itemId)?.Name
            ?? GetWeaponById(itemId)?.Name
            ?? GetArmorById(itemId)?.Name
            ?? string.Empty;
    }

    public static string GetItemName(string? itemId, UiLanguage language)
    {
        var consumable = GetConsumableById(itemId);
        if (consumable is not null)
        {
            return GetConsumableName(consumable, language);
        }

        var weapon = GetWeaponById(itemId);
        if (weapon is not null)
        {
            return GetWeaponName(weapon, language);
        }

        var armor = GetArmorById(itemId);
        if (armor is not null)
        {
            return GetArmorName(armor, language);
        }

        return string.Empty;
    }

    public static int GetItemPrice(string? itemId)
    {
        return GetConsumableById(itemId)?.Price
            ?? GetWeaponById(itemId)?.Price
            ?? GetArmorById(itemId)?.Price
            ?? 0;
    }

    public static int GetSellPrice(string? itemId)
    {
        var itemPrice = GetItemPrice(itemId);
        return itemPrice <= 0 ? 0 : Math.Max(1, itemPrice / 2);
    }
}
