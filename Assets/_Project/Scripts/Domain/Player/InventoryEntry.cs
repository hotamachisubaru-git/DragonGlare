using DragonGlare.Data;

namespace DragonGlare.Domain.Player;

public sealed class InventoryEntry
{
    public string ItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Name => GameContent.GetItemName(ItemId, UiLanguage.Japanese);
    public int Count => Quantity;
    public int AttackBonus => GameContent.GetWeaponById(ItemId)?.AttackBonus ?? 0;
    public int DefenseBonus => GameContent.GetArmorById(ItemId)?.DefenseBonus ?? 0;
    public int Price => GameContent.GetSellPrice(ItemId);
    public InventoryEntry Value => this;

    public InventoryEntry Clone()
    {
        return new InventoryEntry { ItemId = ItemId, Quantity = Quantity };
    }
}
