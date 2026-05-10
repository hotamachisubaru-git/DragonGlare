using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Security;

namespace DragonGlareAlpha.Domain.Player;

public sealed class PlayerProgress
{
    public const int MaxLevelValue = 99;
    public const int MaxVitalValue = 999;
    public const int MaxGoldValue = 99999;
    private static readonly EquipmentSlot[] EquippedSlots =
    [
        EquipmentSlot.Weapon,
        EquipmentSlot.Armor,
        EquipmentSlot.Head,
        EquipmentSlot.Arms,
        EquipmentSlot.Legs,
        EquipmentSlot.Feet
    ];
    private readonly ProtectedInt level = new(1);
    private readonly ProtectedInt experience = new();
    private readonly ProtectedInt maxHp = new(20);
    private readonly ProtectedInt currentHp = new(20);
    private readonly ProtectedInt maxMp = new(2);
    private readonly ProtectedInt currentMp = new(2);
    private readonly ProtectedInt baseAttack = new(5);
    private readonly ProtectedInt baseDefense = new(3);
    private readonly ProtectedInt gold = new(220);
    private readonly ProtectedInt bankGold = new();
    private readonly ProtectedInt loanBalance = new();
    private readonly ProtectedInt loanStepCounter = new();

    public string Name { get; set; } = string.Empty;

    public UiLanguage Language { get; set; } = UiLanguage.Japanese;

    public Point TilePosition { get; set; }

    public int Level
    {
        get => level.Value;
        set => level.Value = value;
    }

    public int Experience
    {
        get => experience.Value;
        set => experience.Value = value;
    }

    public int MaxHp
    {
        get => maxHp.Value;
        set => maxHp.Value = value;
    }

    public int CurrentHp
    {
        get => currentHp.Value;
        set => currentHp.Value = value;
    }

    public int MaxMp
    {
        get => maxMp.Value;
        set => maxMp.Value = value;
    }

    public int CurrentMp
    {
        get => currentMp.Value;
        set => currentMp.Value = value;
    }

    public int BaseAttack
    {
        get => baseAttack.Value;
        set => baseAttack.Value = value;
    }

    public int BaseDefense
    {
        get => baseDefense.Value;
        set => baseDefense.Value = value;
    }

    public int Gold
    {
        get => gold.Value;
        set => gold.Value = value;
    }

    public int BankGold
    {
        get => bankGold.Value;
        set => bankGold.Value = value;
    }

    public int LoanBalance
    {
        get => loanBalance.Value;
        set => loanBalance.Value = value;
    }

    public int LoanStepCounter
    {
        get => loanStepCounter.Value;
        set => loanStepCounter.Value = value;
    }

    public string? EquippedWeaponId { get; set; }

    public string? EquippedArmorId { get; set; }

    public string? EquippedHeadId { get; set; }

    public string? EquippedArmsId { get; set; }

    public string? EquippedLegsId { get; set; }

    public string? EquippedFeetId { get; set; }

    public List<InventoryEntry> Inventory { get; set; } = [];

    public List<string> CompletedFieldEventIds { get; set; } = [];

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
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return;
        }

        var existing = Inventory.FirstOrDefault(entry => string.Equals(entry.ItemId, itemId, StringComparison.Ordinal));
        if (existing is null)
        {
            Inventory.Add(new InventoryEntry
            {
                ItemId = itemId,
                Quantity = quantity
            });
            return;
        }

        existing.Quantity += quantity;
    }

    public int GetItemCount(string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        return Inventory
            .Where(entry => string.Equals(entry.ItemId, itemId, StringComparison.Ordinal))
            .Sum(entry => entry.Quantity);
    }

    public bool RemoveItem(string? itemId, int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return false;
        }

        var existing = Inventory.FirstOrDefault(entry => string.Equals(entry.ItemId, itemId, StringComparison.Ordinal));
        if (existing is null || existing.Quantity < quantity)
        {
            return false;
        }

        existing.Quantity -= quantity;
        if (existing.Quantity == 0)
        {
            Inventory.Remove(existing);
        }

        if (GetItemCount(itemId) == 0)
        {
            foreach (var slot in EquippedSlots)
            {
                if (string.Equals(GetEquippedItemId(slot), itemId, StringComparison.Ordinal))
                {
                    SetEquippedItemId(slot, null);
                }
            }
        }

        return true;
    }

    public bool HasCompletedFieldEvent(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return false;
        }

        return CompletedFieldEventIds.Any(completedEventId => string.Equals(completedEventId, eventId, StringComparison.Ordinal));
    }

    public void CompleteFieldEvent(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId) || HasCompletedFieldEvent(eventId))
        {
            return;
        }

        CompletedFieldEventIds.Add(eventId);
    }

    public string? GetEquippedItemId(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => EquippedWeaponId,
            EquipmentSlot.Armor => EquippedArmorId,
            EquipmentSlot.Head => EquippedHeadId,
            EquipmentSlot.Arms => EquippedArmsId,
            EquipmentSlot.Legs => EquippedLegsId,
            EquipmentSlot.Feet => EquippedFeetId,
            _ => null
        };
    }

    public void SetEquippedItemId(EquipmentSlot slot, string? itemId)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                EquippedWeaponId = itemId;
                break;
            case EquipmentSlot.Armor:
                EquippedArmorId = itemId;
                break;
            case EquipmentSlot.Head:
                EquippedHeadId = itemId;
                break;
            case EquipmentSlot.Arms:
                EquippedArmsId = itemId;
                break;
            case EquipmentSlot.Legs:
                EquippedLegsId = itemId;
                break;
            case EquipmentSlot.Feet:
                EquippedFeetId = itemId;
                break;
        }
    }

    public IEnumerable<string> GetEquippedItemIds()
    {
        foreach (var slot in EquippedSlots)
        {
            var itemId = GetEquippedItemId(slot);
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                yield return itemId;
            }
        }
    }

    public void Normalize()
    {
        Level = Math.Clamp(Level, 1, MaxLevelValue);
        Experience = Math.Max(0, Experience);
        MaxHp = MaxHp <= 0 ? 20 : Math.Min(MaxHp, MaxVitalValue);
        CurrentHp = CurrentHp <= 0 ? MaxHp : Math.Min(CurrentHp, MaxHp);
        MaxMp = MaxMp <= 0 ? 2 : Math.Min(MaxMp, MaxVitalValue);
        CurrentMp = Math.Clamp(CurrentMp, 0, MaxMp);
        BaseAttack = BaseAttack <= 0 ? 5 : BaseAttack;
        BaseDefense = BaseDefense <= 0 ? 3 : BaseDefense;
        Gold = Math.Clamp(Gold, 0, MaxGoldValue);
        BankGold = Math.Clamp(BankGold, 0, MaxGoldValue);
        LoanBalance = Math.Clamp(LoanBalance, 0, MaxGoldValue);
        LoanStepCounter = Math.Clamp(LoanStepCounter, 0, 100000);
        if (LoanBalance == 0)
        {
            LoanStepCounter = 0;
        }

        if (Level == MaxLevelValue)
        {
            MaxHp = MaxVitalValue;
            MaxMp = MaxVitalValue;
            CurrentHp = Math.Min(CurrentHp, MaxHp);
            CurrentMp = Math.Min(CurrentMp, MaxMp);
        }

        Inventory = Inventory
            .Where(entry => !string.IsNullOrWhiteSpace(entry.ItemId) && entry.Quantity > 0)
            .GroupBy(entry => entry.ItemId, StringComparer.Ordinal)
            .Select(group => new InventoryEntry
            {
                ItemId = group.Key,
                Quantity = group.Sum(entry => entry.Quantity)
            })
            .ToList();

        CompletedFieldEventIds = CompletedFieldEventIds
            .Where(eventId => !string.IsNullOrWhiteSpace(eventId))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(eventId => eventId, StringComparer.Ordinal)
            .ToList();

        foreach (var slot in EquippedSlots)
        {
            var equippedItemId = GetEquippedItemId(slot);
            if (!string.IsNullOrWhiteSpace(equippedItemId) && GetItemCount(equippedItemId) == 0)
            {
                AddItem(equippedItemId, 1);
            }
        }
    }

    public void ValidateIntegrity()
    {
        level.Validate();
        experience.Validate();
        maxHp.Validate();
        currentHp.Validate();
        maxMp.Validate();
        currentMp.Validate();
        baseAttack.Validate();
        baseDefense.Validate();
        gold.Validate();
        bankGold.Validate();
        loanBalance.Validate();
        loanStepCounter.Validate();

        foreach (var entry in Inventory)
        {
            entry.ValidateIntegrity();
        }
    }

    public void RekeySensitiveValues()
    {
        level.Rekey();
        experience.Rekey();
        maxHp.Rekey();
        currentHp.Rekey();
        maxMp.Rekey();
        currentMp.Rekey();
        baseAttack.Rekey();
        baseDefense.Rekey();
        gold.Rekey();
        bankGold.Rekey();
        loanBalance.Rekey();
        loanStepCounter.Rekey();

        foreach (var entry in Inventory)
        {
            entry.RekeySensitiveValues();
        }
    }
}
