using DragonGlare.Domain;
using DragonGlare.Domain.Player;

namespace DragonGlare.Data;

/// <summary>
/// Weapon and armour catalogues.
///
/// Pricing philosophy:
///    EEach tier costs roughly 1.6-1.8ÁEthe previous, so upgrades feel
///     meaningful but reachable after ~8-12 battles in the appropriate zone.
///    EStarter gold (220 G) covers a Club or two Herbs with change to spare.
///    EThe field-tier weapons (Iron Sword ↁEDragon Lance) require castle/field
///     farming, naturally gating progression.
///
/// Stat philosophy:
///    EWeapon ATK bonus doubles roughly every two tiers (2ↁEↁEↁEↁE1ↁE4ↁE7ↁE0ↁE4ↁE8).
///    EBody-armour DEF scales 1ↁEↁEↁEↁEↁE1ↁE3ↁE6 across eight tiers.
///    ESlot armours (head/arms/legs/feet) stay lighter (1-4) so the body slot
///     remains the dominant defensive choice.
///
/// PINNED values (must not change  Etests assert exact prices / bonuses):
///   bronze_sword   Price = 196
///   leather_armor  Price = 48,  DefenseBonus = 3
///   bronze_helm    Price = 112
///   leather_cap    DefenseBonus = 1
///   travel_boots   DefenseBonus = 1
///
/// WeaponDefinition: Id, Name, Price, AttackBonus, [EnglishName]
/// ArmorDefinition:  Id, Name, Price, DefenseBonus, [Slot=Armor], [EnglishName]
/// </summary>
public static class Equipment
{
    // ── Weapons ──────────────────────────────────────────────────────────────
    public static readonly WeaponDefinition[] WeaponCatalog =
    [
        new("stick",        "ぼぁE,         16,   2, "Stick"),
        new("club",         "こんぼぁE,     36,   4, "Club"),          // was 32
        new("bamboo_spear", "たけめE��",     58,   6, "Bamboo Spear"),  // was 52
        new("thorn_club",   "とげ�EぼぁE,   78,   8, "Thorn Club"),    // was 64, atk was 7
        new("wood_blade",   "ぼくとぁE,    108,  11, "Wood Blade"),    // was 82, atk was 8
        new("stone_axe",    "ぁE��のお�E",  148,  14, "Stone Axe"),     // was 128, atk was 11
        new("bronze_sword", "どぁE�Eつるぎ",196,  17, "Bronze Sword"),  // PINNED price=196, atk was 14
        new("iron_sword",   "てつのけん",  310,  20, "Iron Sword"),    // was 288, atk was 17
        new("steel_blade",  "はが�Eけん",  460,  24, "Steel Blade"),   // was 416, atk was 20
        new("dragon_lance", "りゅぁE�EめE��",650,  28, "Dragon Lance")   // was 580, atk was 24
    ];

    // ── Body Armour ───────────────────────────────────────────────────────────
    public static readonly ArmorDefinition[] ArmorCatalog =
    [
        // Body slot  (EquipmentSlot.Armor is default)
        new("cloth_tunic",     "ぬののふぁE,    18,  1,  EnglishName: "Cloth Tunic"),
        new("leather_armor",   "かわのよろぁE,  48,  3,  EnglishName: "Leather Armor"),  // PINNED price=48, def=3
        new("scale_vest",      "ぁE��こ�EぁE,    82,  5,  EnglishName: "Scale Vest"),      // was 72 def 4
        new("bronze_mail",     "どぁE��ろい",   128,  7,  EnglishName: "Bronze Mail"),     // was 108 def 6
        new("iron_armor",      "てつよろぁE,   188,  9,  EnglishName: "Iron Armor"),      // was 152 def 8
        new("steel_armor",     "はが�EよろぁE, 270, 11,  EnglishName: "Steel Armor"),     // was 224 def 10
        new("silver_mail",     "ぎんむねあて", 380, 13,  EnglishName: "Silver Mail"),     // was 336
        new("dragon_mail",     "りゅぁE��ろい", 540, 16,  EnglishName: "Dragon Mail"),     // was 492

        // Head slot
        new("leather_cap",     "かわぼぁE��",    38,  1, EquipmentSlot.Head, "Leather Cap"),     // PINNED def=1
        new("bronze_helm",     "どぁE�Eか�Eと", 112,  2, EquipmentSlot.Head, "Bronze Helm"),     // PINNED price=112
        new("steel_helm",      "はが�Eか�Eと", 260,  4, EquipmentSlot.Head, "Steel Helm"),      // was 238

        // Arms slot
        new("leather_bracers", "かわのこて",    34,  1, EquipmentSlot.Arms, "Leather Bracers"),
        new("bronze_gauntlets","どぁE�Eこて",   108,  2, EquipmentSlot.Arms, "Bronze Gauntlets"), // was 104
        new("steel_gauntlets", "はが�Eのこて", 248,  4, EquipmentSlot.Arms, "Steel Gauntlets"),  // was 226

        // Legs slot
        new("leather_leggings","かわレギンス",  42,  1, EquipmentSlot.Legs, "Leather Leggings"),
        new("bronze_greaves",  "どぁE��ギンス", 118,  2, EquipmentSlot.Legs, "Bronze Greaves"),  // was 116
        new("steel_greaves",   "はが�Eレギンス",252, 4, EquipmentSlot.Legs, "Steel Greaves"),   // was 232

        // Feet slot
        new("travel_boots",    "た�Eのブ�EチE,  28,  1, EquipmentSlot.Feet, "Travel Boots"),    // PINNED def=1
        new("bronze_boots",    "どぁE�Eブ�EチE,  98,  2, EquipmentSlot.Feet, "Bronze Boots"),    // was 96
        new("steel_boots",     "はが�Eブ�EチE, 218,  3, EquipmentSlot.Feet, "Steel Boots")      // was 212
    ];
}
