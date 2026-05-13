using DragonGlare.Domain;

namespace DragonGlare.Domain.Battle;

public sealed record SpellDefinition(
    string Id,
    string Name,
    string EnglishName,
    string Description,
    string EnglishDescription,
    int MpCost,
    int MinimumLevel,
    SpellEffectType EffectType,
    int Power,
    int AccuracyPercent = 100,
    int DurationTurns = 0);
