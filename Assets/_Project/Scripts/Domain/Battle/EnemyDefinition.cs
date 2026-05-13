using DragonGlare.Domain;

namespace DragonGlare.Domain.Battle;

public sealed record EnemyDefinition(
    string Id,
    string Name,
    FieldMapId EncounterMap,
    int MinRecommendedLevel,
    int MaxRecommendedLevel,
    int EncounterWeight,
    int MaxHp,
    int Attack,
    int Defense,
    int ExperienceReward,
    int GoldReward,
    EnemyDropDefinition? Drop = null,
    string SpriteAssetName = "",
    string EnglishName = "",
    BattleStatusEffect AttackStatusEffect = BattleStatusEffect.None,
    int AttackStatusChancePercent = 0,
    int AttackStatusTurns = 0);
