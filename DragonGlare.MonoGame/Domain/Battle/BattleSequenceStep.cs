using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Domain.Battle;

public sealed class BattleSequenceStep
{
    public string Message { get; init; } = string.Empty;

    public BattleVisualCue VisualCue { get; init; }

    public int AnimationFrames { get; init; } = 12;
}
