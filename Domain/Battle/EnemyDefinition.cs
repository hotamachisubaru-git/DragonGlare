namespace DragonGlareAlpha.Domain.Battle;

public sealed record EnemyDefinition(
    string Id,
    string Name,
    int MaxHp,
    int Attack,
    int Defense,
    int ExperienceReward,
    int GoldReward);
