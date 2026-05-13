namespace DragonGlare.Domain.Player;

public sealed class InventoryEntry
{
    public string ItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Name => ItemId;

    public InventoryEntry Clone()
    {
        return new InventoryEntry { ItemId = ItemId, Quantity = Quantity };
    }
}
