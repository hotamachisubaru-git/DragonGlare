using DragonGlare.Domain.Commerce;
using DragonGlare.Domain.Items;
using DragonGlare.Domain.Player;

namespace DragonGlare
{
    public sealed class ShopMenuEntry
    {
        public ShopMenuEntryType Type;
        public ShopProductDefinition Product;
        public InventoryEntry InventoryItem;
        public string Label => Type switch
        {
            ShopMenuEntryType.PreviousPage => "←",
            ShopMenuEntryType.NextPage => "→",
            ShopMenuEntryType.Quit => "Quit",
            _ => Product?.Name ?? InventoryItem?.Name ?? string.Empty
        };
    }

    public struct BattleSelectionEntry
    {
        public string Label;
        public string Detail;
        public string Badge;
        public DragonGlare.Domain.Battle.SpellDefinition Spell;
        public ConsumableDefinition Consumable;
        public IEquipmentDefinition Equipment;
    }

    public struct BankOption
    {
        public string Label;
        public int Amount;
        public bool Quit;
    }
}
