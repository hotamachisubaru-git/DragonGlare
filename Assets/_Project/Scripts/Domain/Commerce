using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Domain.Commerce;

public sealed record ShopProductDefinition(
    IEquipmentDefinition? Equipment = null,
    ConsumableDefinition? Consumable = null)
{
    public string Id => Equipment?.Id ?? Consumable?.Id ?? string.Empty;

    public string Name => Equipment?.Name ?? Consumable?.Name ?? string.Empty;

    public int Price => Equipment?.Price ?? Consumable?.Price ?? 0;

    public int AttackBonus => Equipment?.AttackBonus ?? 0;

    public int DefenseBonus => Equipment?.DefenseBonus ?? 0;

    public bool IsEquipment => Equipment is not null;

    public bool IsConsumable => Consumable is not null;
}
