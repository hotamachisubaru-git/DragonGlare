using DragonGlareAlpha.Domain;
using Microsoft.Xna.Framework;

namespace DragonGlareAlpha.Domain.Field;

public sealed record FieldEventDefinition(
    string Id,
    FieldMapId MapId,
    Point TilePosition,
    Color DisplayColor,
    bool BlocksMovement,
    FieldEventActionType ActionType,
    string[] JapanesePages,
    string[] EnglishPages,
    string? SpriteAssetName = null,
    string? PortraitAssetName = null,
    int RecoverHp = 0,
    int RecoverMp = 0)
{
    public IReadOnlyList<string> GetPages(UiLanguage language)
    {
        return language == UiLanguage.Japanese ? JapanesePages : EnglishPages;
    }
}
