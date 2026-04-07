using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Domain.Field;

public sealed record FieldEventDefinition(
    string Id,
    Point TilePosition,
    Color DisplayColor,
    bool BlocksMovement,
    FieldEventActionType ActionType,
    string[] JapanesePages,
    string[] EnglishPages,
    int RecoverHp = 0,
    int RecoverMp = 0)
{
    public IReadOnlyList<string> GetPages(UiLanguage language)
    {
        return language == UiLanguage.Japanese ? JapanesePages : EnglishPages;
    }
}
