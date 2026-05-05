using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Domain.Player;

public sealed record ArmorDefinition(
    string Id,
    string Name,
    int Price,
    int DefenseBonus,
    EquipmentSlot Slot = EquipmentSlot.Armor,
    string EnglishName = "") : IEquipmentDefinition
{
    public int AttackBonus => 0;
}
