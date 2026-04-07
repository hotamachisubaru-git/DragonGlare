using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Domain.Battle;

public sealed class BattleTurnResolution
{
    public BattleOutcome Outcome { get; init; }

    public bool ActionAccepted { get; init; } = true;

    public List<BattleSequenceStep> Steps { get; init; } = [];
}
