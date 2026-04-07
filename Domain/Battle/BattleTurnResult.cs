using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Domain.Battle;

public sealed class BattleTurnResult
{
    public BattleOutcome Outcome { get; init; }

    public string Message { get; init; } = string.Empty;
}
