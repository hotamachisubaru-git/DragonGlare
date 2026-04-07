using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Domain.Items;

public sealed record ConsumableDefinition(
    string Id,
    string Name,
    string Description,
    ConsumableEffectType EffectType,
    int Amount);
