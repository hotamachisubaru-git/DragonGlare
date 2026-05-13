using System.Collections.Generic;
using System.Linq;
using DragonGlare.Data;
using DragonGlare.Domain.Battle;

namespace DragonGlare.Domain.Player;

public sealed class PlayerProgress
{
    public const int MaxLevelValue = 99;
    public const int MaxVitalValue = 999;
    public const int MaxGoldValue = 99999;

    public string Name { get; set; } = string.Empty;
    public UiLanguage Language { get; set; } = UiLanguage.Japanese;
    public Point TilePosition { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public int MaxHp { get; set; } = 20;
    public int CurrentHp { get; set; } = 20;
    public int MaxMp { get; set; } = 10;
    public int CurrentMp { get; set; } = 10;
    public int BaseAttack { get; set; } = 5;
    public int BaseDefense { get; set; } = 3;
    public int Gold { get; set; } = 220;
    public int BankGold { get; set; }
    public int LoanBalance { get; set; }
    public int LoanStepCounter { get; set; }
    public string? EquippedWeaponId { get; set; }
    public string? EquippedArmorId { get; set; }
    public string? EquippedHeadId { get; set; }
    public string? EquippedArmsId { get; set; }
    public string? EquippedLegsId { get; set; }
    public string? EquippedFeetId { get; set; }

    // -- Equipped item shortcuts --
    public WeaponDefinition? EquippedWeapon => GameContent.GetWeaponById(EquippedWeaponId);
    public ArmorDefinition? EquippedArmor => GameContent.GetArmorById(EquippedArmorId);
    public ArmorDefinition? EquippedHead => GameContent.GetArmorById(EquippedHeadId);
    public ArmorDefinition? EquippedArms => GameContent.GetArmorById(EquippedArmsId);
    public ArmorDefinition? EquippedLegs => GameContent.GetArmorById(EquippedLegsId);
    public ArmorDefinition? EquippedFeet => GameContent.GetArmorById(EquippedFeetId);
    public List<InventoryEntry> Inventory { get; set; } = [];
    public List<string> CompletedFieldEventIds { get; set; } = [];
    public List<SpellDefinition> Spells { get; set; } = [];
    public PlayerFacingDirection FacingDirection { get; set; } = PlayerFacingDirection.Down;

    public static PlayerProgress CreateDefault(Point startTile, UiLanguage language = UiLanguage.Japanese)
    {
        return new PlayerProgress
        {
            Language = language,
            TilePosition = startTile
        };
    }

    public void AddItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        var existing = Inventory.FirstOrDefault(e => e.ItemId == itemId);
        if (existing == null)
            Inventory.Add(new InventoryEntry { ItemId = itemId, Quantity = quantity });
        else
            existing.Quantity += quantity;
    }

    public int GetItemCount(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return 0;
        return Inventory.Where(e => e.ItemId == itemId).Sum(e => e.Quantity);
    }

    public bool RemoveItem(string? itemId, int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return false;
        var existing = Inventory.FirstOrDefault(e => e.ItemId == itemId);
        if (existing == null || existing.Quantity < quantity) return false;
        existing.Quantity -= quantity;
        if (existing.Quantity == 0) Inventory.Remove(existing);
        return true;
    }

    public string? GetEquippedItemId(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => EquippedWeaponId,
        EquipmentSlot.Armor => EquippedArmorId,
        EquipmentSlot.Head => EquippedHeadId,
        EquipmentSlot.Arms => EquippedArmsId,
        EquipmentSlot.Legs => EquippedLegsId,
        EquipmentSlot.Feet => EquippedFeetId,
        _ => null
    };

    public void SetEquippedItemId(EquipmentSlot slot, string? itemId)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon: EquippedWeaponId = itemId; break;
            case EquipmentSlot.Armor: EquippedArmorId = itemId; break;
            case EquipmentSlot.Head: EquippedHeadId = itemId; break;
            case EquipmentSlot.Arms: EquippedArmsId = itemId; break;
            case EquipmentSlot.Legs: EquippedLegsId = itemId; break;
            case EquipmentSlot.Feet: EquippedFeetId = itemId; break;
        }
    }

    public string? GetEquippedItemName(EquipmentSlot slot)
    {
        var id = GetEquippedItemId(slot);
        return string.IsNullOrWhiteSpace(id) ? null : id;
    }

    public bool HasCompletedFieldEvent(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId)) return false;
        return CompletedFieldEventIds.Contains(eventId);
    }

    public void CompleteFieldEvent(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId) || HasCompletedFieldEvent(eventId)) return;
        CompletedFieldEventIds.Add(eventId);
    }

    public void TakeDamage(int damage)
    {
        CurrentHp = System.Math.Max(0, CurrentHp - damage);
    }

    public void Heal(int amount)
    {
        CurrentHp = System.Math.Min(MaxHp, CurrentHp + amount);
    }

    public PlayerProgress Clone()
    {
        return new PlayerProgress
        {
            Name = Name,
            Language = Language,
            TilePosition = TilePosition,
            Level = Level,
            Experience = Experience,
            MaxHp = MaxHp,
            CurrentHp = CurrentHp,
            MaxMp = MaxMp,
            CurrentMp = CurrentMp,
            BaseAttack = BaseAttack,
            BaseDefense = BaseDefense,
            Gold = Gold,
            BankGold = BankGold,
            LoanBalance = LoanBalance,
            LoanStepCounter = LoanStepCounter,
            EquippedWeaponId = EquippedWeaponId,
            EquippedArmorId = EquippedArmorId,
            EquippedHeadId = EquippedHeadId,
            EquippedArmsId = EquippedArmsId,
            EquippedLegsId = EquippedLegsId,
            EquippedFeetId = EquippedFeetId,
            Inventory = Inventory.Select(i => i.Clone()).ToList(),
            CompletedFieldEventIds = new List<string>(CompletedFieldEventIds),
            Spells = new List<SpellDefinition>(Spells),
            FacingDirection = FacingDirection
        };
    }
}
