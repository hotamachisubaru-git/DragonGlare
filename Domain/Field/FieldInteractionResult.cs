namespace DragonGlareAlpha.Domain.Field;

public sealed class FieldInteractionResult
{
    public List<string> Pages { get; init; } = [];

    public bool ShouldPersistProgress { get; init; }
}
