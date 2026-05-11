using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private WeaponDefinition? GetEquippedWeapon()
    {
        return GameContent.GetWeaponById(player.EquippedWeaponId);
    }

    private ArmorDefinition? GetEquippedArmor()
    {
        return GetEquippedArmor(EquipmentSlot.Armor);
    }

    private ArmorDefinition? GetEquippedArmor(EquipmentSlot slot)
    {
        return GameContent.GetArmorById(player.GetEquippedItemId(slot));
    }

    private string GetDisplayPlayerName()
    {
        if (!string.IsNullOrWhiteSpace(player.Name))
        {
            return player.Name;
        }

        if (playerName.Length > 0)
        {
            return playerName.ToString();
        }

        return selectedLanguage == UiLanguage.English ? "Hero" : "のりたま";
    }

    private string GetEquippedWeaponName()
    {
        var weapon = GetEquippedWeapon();
        return weapon is null
            ? GetNoneLabel()
            : GameContent.GetWeaponName(weapon, selectedLanguage);
    }

    private string GetEquippedArmorName()
    {
        var armor = GetEquippedArmor();
        return armor is null
            ? GetNoneLabel()
            : GameContent.GetArmorName(armor, selectedLanguage);
    }

    private string GetCurrentEquipmentNameForSlot(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => GetEquippedWeaponName(),
            _ => GetEquippedArmor(slot) is { } armor
                ? GameContent.GetArmorName(armor, selectedLanguage)
                : GetNoneLabel()
        };
    }

    private string GetNoneLabel()
    {
        return selectedLanguage == UiLanguage.English ? "NONE" : "なし";
    }

    private string GetEquipmentSlotLabel(EquipmentSlot slot)
    {
        if (selectedLanguage == UiLanguage.English)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => "WEAPON",
                EquipmentSlot.Armor => "CHEST",
                EquipmentSlot.Head => "HEAD",
                EquipmentSlot.Arms => "ARMS",
                EquipmentSlot.Legs => "LEGS",
                EquipmentSlot.Feet => "FEET",
                _ => "GEAR"
            };
        }

        return slot switch
        {
            EquipmentSlot.Weapon => "ぶき",
            EquipmentSlot.Armor => "むねあて",
            EquipmentSlot.Head => "あたま",
            EquipmentSlot.Arms => "こて",
            EquipmentSlot.Legs => "レギンス",
            EquipmentSlot.Feet => "ブーツ",
            _ => "そうび"
        };
    }

    private string GetEquippedArmorSummary()
    {
        var equippedCount = ArmorSlots.Count(slot => !string.IsNullOrWhiteSpace(player.GetEquippedItemId(slot)));
        return $"{equippedCount}/{ArmorSlots.Length}";
    }

    private int GetTotalAttack()
    {
        return battleService.GetPlayerAttack(player);
    }

    private int GetTotalDefense()
    {
        return battleService.GetPlayerDefense(player);
    }

    private string GetExperienceSummary()
    {
        if (player.Level >= PlayerProgress.MaxLevelValue)
        {
            return "MAX";
        }

        var current = progressionService.GetExperienceIntoCurrentLevel(player);
        var needed = progressionService.GetExperienceNeededForNextLevel(player);
        return $"{current}/{needed}";
    }

    private static string FormatSignedStat(int value)
    {
        return value switch
        {
            > 0 => $" (+{value})",
            < 0 => $" ({value})",
            _ => string.Empty
        };
    }
}
