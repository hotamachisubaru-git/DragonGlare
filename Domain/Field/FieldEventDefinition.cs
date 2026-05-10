using DragonGlareAlpha.Domain;

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
    int RecoverMp = 0,
    FieldEventReward? Reward = null,
    string[]? JapaneseCompletedPages = null,
    string[]? EnglishCompletedPages = null,
    bool RenderOnMap = true,
    Rectangle? InteractionArea = null)
{
    public IReadOnlyList<string> GetPages(UiLanguage language, bool completed = false)
    {
        if (completed)
        {
            var completedPages = language == UiLanguage.Japanese ? JapaneseCompletedPages : EnglishCompletedPages;
            if (completedPages is { Length: > 0 })
            {
                return completedPages;
            }
        }

        return language == UiLanguage.Japanese ? JapanesePages : EnglishPages;
    }

    public bool CanInteractFrom(Point playerTile)
    {
        if (InteractionArea is { } interactionArea)
        {
            return interactionArea.Contains(playerTile);
        }

        return TilePosition == playerTile ||
            Math.Abs(TilePosition.X - playerTile.X) + Math.Abs(TilePosition.Y - playerTile.Y) == 1;
    }
}
