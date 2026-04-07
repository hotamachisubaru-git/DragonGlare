namespace DragonGlareAlpha.Domain.Player;

public sealed record WeaponDefinition(
    string Id,
    string Name,
    int Price,
    int AttackBonus);
