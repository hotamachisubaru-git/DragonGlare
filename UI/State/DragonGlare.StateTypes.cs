using DragonGlareAlpha.Domain.Commerce;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private enum ShopMenuEntryType
    {
        Product,
        InventoryItem,
        PreviousPage,
        NextPage,
        Quit
    }

    private enum PlayerFacingDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private readonly record struct BattleSelectionEntry(
        string Label,
        string Detail,
        string Badge,
        SpellDefinition? Spell = null,
        ConsumableDefinition? Consumable = null,
        IEquipmentDefinition? Equipment = null);

    private readonly record struct ShopInventoryEntry(
        string ItemId,
        string Name,
        int Price,
        int AttackBonus,
        int DefenseBonus,
        int Count,
        string Detail);

    private readonly record struct ShopMenuEntry(
        ShopMenuEntryType Type,
        string Label,
        ShopProductDefinition? Product = null,
        ShopInventoryEntry? InventoryItem = null);

    private readonly record struct BankAmountOption(
        string Label,
        int Amount,
        bool UseMaximum = false,
        bool Quit = false);

    private readonly record struct OpeningNarrationLine(
        string Text,
        int DisplayFrames,
        int GapFrames);
}
