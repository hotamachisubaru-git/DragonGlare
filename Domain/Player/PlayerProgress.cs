using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Domain.Player;

public sealed class PlayerProgress
{
    public string Name { get; set; } = string.Empty;

    public UiLanguage Language { get; set; } = UiLanguage.Japanese;

    public Point TilePosition { get; set; }

    public int Level { get; set; } = 1;

    public int Experience { get; set; }

    public int MaxHp { get; set; } = 20;

    public int CurrentHp { get; set; } = 20;

    public int MaxMp { get; set; } = 2;

    public int CurrentMp { get; set; } = 2;

    public int BaseAttack { get; set; } = 5;

    public int BaseDefense { get; set; } = 3;

    public int Gold { get; set; } = 220;

    public string? EquippedWeaponId { get; set; }

    public List<InventoryEntry> Inventory { get; set; } = [];

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

        if (string.Equals(EquippedWeaponId, itemId, StringComparison.Ordinal) && GetItemCount(itemId) == 0)
        {
            EquippedWeaponId = null;
        }

        return true;
    }

    public void Normalize()
    {
        Level = Math.Max(1, Level);
        Experience = Math.Max(0, Experience);
        MaxHp = MaxHp <= 0 ? 20 : MaxHp;
        CurrentHp = CurrentHp <= 0 ? MaxHp : Math.Min(CurrentHp, MaxHp);
        MaxMp = MaxMp <= 0 ? 2 : MaxMp;
        CurrentMp = Math.Clamp(CurrentMp, 0, MaxMp);
        BaseAttack = BaseAttack <= 0 ? 5 : BaseAttack;
        BaseDefense = BaseDefense <= 0 ? 3 : BaseDefense;
        Gold = Math.Max(0, Gold);

        Inventory = Inventory
            .Where(entry => !string.IsNullOrWhiteSpace(entry.ItemId) && entry.Quantity > 0)
            .GroupBy(entry => entry.ItemId, StringComparer.Ordinal)
            .Select(group => new InventoryEntry
            {
                ItemId = group.Key,
                Quantity = group.Sum(entry => entry.Quantity)
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(EquippedWeaponId) && GetItemCount(EquippedWeaponId) == 0)
        {
            AddItem(EquippedWeaponId, 1);
        }
    }
}
