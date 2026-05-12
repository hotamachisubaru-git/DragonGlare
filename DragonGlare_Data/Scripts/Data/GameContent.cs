using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Commerce;
using DragonGlareAlpha.Domain.Field;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Data;

/// <summary>
/// Central access point for all game content.
/// Catalogue data lives in the dedicated files:
///   Data/Enemies.cs       – EnemyDefinition[]  (Enemies.EnemyCatalog)
///   Data/Spells.cs        – SpellDefinition[]  (Spells.SpellCatalog)
///   Data/Equipment.cs     – WeaponDefinition[] / ArmorDefinition[]
///   Data/Consumables.cs   – ConsumableDefinition[]
///   Data/FieldContent.cs  – FieldTransitionDefinition[] / FieldEventDefinition[]
/// </summary>
public static class GameContent
{
    public static readonly string[][] JapaneseNameTable =
    [
        ["あ", "い", "う", "え", "お", "か", "き", "く", "け", "こ"],
        ["さ", "し", "す", "せ", "そ", "た", "ち", "つ", "て", "と"],
        ["な", "に", "ぬ", "ね", "の", "は", "ひ", "ふ", "へ", "ほ"],
        ["ま", "み", "む", "め", "も", "や", "ゆ", "よ", "わ", "を"],
        ["ら", "り", "る", "れ", "ろ", "ん", "ー", "゛", "゜", "っ"],
        ["ぁ", "ぃ", "ぅ", "ぇ", "ぉ", "ゃ", "ゅ", "ょ", "ゎ", "けす", "おわり"]
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

    // ── Catalogue references (data lives in dedicated files) ─────────────────
    public static SpellDefinition[]    SpellCatalog    => Spells.SpellCatalog;
    public static WeaponDefinition[]   WeaponCatalog   => Equipment.WeaponCatalog;
    public static ArmorDefinition[]    ArmorCatalog    => Equipment.ArmorCatalog;
    public static ConsumableDefinition[] ConsumableCatalog => Consumables.ConsumableCatalog;
    public static EnemyDefinition[]    EnemyCatalog    => Enemies.EnemyCatalog;

    private static readonly Lazy<ShopProductDefinition[]> _shopCatalog = new(() =>
        Consumables.ConsumableCatalog
            .Select(item => new ShopProductDefinition(Consumable: item))
            .Concat(Equipment.WeaponCatalog.Select(item => new ShopProductDefinition(Equipment: item)))
            .Concat(Equipment.ArmorCatalog.Select(item => new ShopProductDefinition(Equipment: item)))
            .OrderBy(item => item.Price)
            .ThenBy(item => item.Name, StringComparer.Ordinal)
            .ToArray()
    );

    public static ShopProductDefinition[] ShopCatalog => _shopCatalog.Value;

    public static FieldTransitionDefinition[] FieldTransitions => FieldContent.FieldTransitions;

    public static FieldEventDefinition[] FieldEvents => FieldContent.FieldEvents;

    // ── Lookup dictionaries ──────────────────────────────────────────────────
    private static readonly Dictionary<string, WeaponDefinition> WeaponById =
        Equipment.WeaponCatalog.ToDictionary(item => item.Id, StringComparer.Ordinal);

    private static readonly Dictionary<string, ArmorDefinition> ArmorById =
        Equipment.ArmorCatalog.ToDictionary(item => item.Id, StringComparer.Ordinal);

    private static readonly Dictionary<string, ConsumableDefinition> ConsumableById =
        Consumables.ConsumableCatalog.ToDictionary(item => item.Id, StringComparer.Ordinal);

    private static readonly Lazy<Dictionary<string, ShopProductDefinition>> ShopProductById =
        new(() => ShopCatalog.ToDictionary(item => item.Id, StringComparer.Ordinal));

    // ── Name-table helpers ───────────────────────────────────────────────────
    public static string[][] GetNameTable(UiLanguage language)
    {
        return language == UiLanguage.Japanese ? JapaneseNameTable : EnglishNameTable;
    }

    public static string GetBattleCommandLabel(UiLanguage language, int row, int column)
    {
        var labels = language == UiLanguage.English ? EnglishBattleCommandLabels : JapaneseBattleCommandLabels;
        return labels[row, column];
    }

    // ── Localised name / description helpers ─────────────────────────────────
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

    // ── By-ID lookups ─────────────────────────────────────────────────────────
    public static WeaponDefinition? GetWeaponById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return WeaponById.TryGetValue(itemId, out var item) ? item : null;
    }

    public static ArmorDefinition? GetArmorById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return ArmorById.TryGetValue(itemId, out var item) ? item : null;
    }

    public static ConsumableDefinition? GetConsumableById(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return ConsumableById.TryGetValue(itemId, out var item) ? item : null;
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

        return ShopProductById.Value.TryGetValue(itemId, out var item) ? item : null;
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
