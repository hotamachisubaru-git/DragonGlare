namespace DragonGlare.Domain.Field;

public sealed class FieldInteractionResult
{
    public List<string> Pages { get; init; } = [];

    public bool ShouldPersistProgress { get; init; }

    public bool TransitionToBattle { get; init; }

    public bool TransitionToShop { get; init; }

    public bool TransitionToBank { get; init; }
}
