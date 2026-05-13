namespace DragonGlare.Domain.Player;

public sealed record WeaponDefinition(
    string Id,
    string Name,
    int Price,
    int AttackBonus,
    string EnglishName = "") : IEquipmentDefinition
{
    public EquipmentSlot Slot => EquipmentSlot.Weapon;
    public int DefenseBonus => 0;
}
