namespace DragonGlare.Domain.Battle;

public sealed class BattleTurnResolution
{
    public BattleOutcome Outcome { get; init; }

    public bool ActionAccepted { get; init; } = true;

    public List<BattleSequenceStep> Steps { get; init; } = [];

    public bool PlayerWon => Outcome == BattleOutcome.Victory;

    public bool PlayerEscaped => Outcome == BattleOutcome.Escaped;

    public string SummaryMessage { get; init; } = string.Empty;
}
