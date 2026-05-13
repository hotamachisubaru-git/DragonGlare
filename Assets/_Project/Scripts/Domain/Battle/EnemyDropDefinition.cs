namespace DragonGlare.Domain.Battle;

public sealed record EnemyDropDefinition(
    string ItemId,
    int ChancePercent,
    int Quantity = 1);
