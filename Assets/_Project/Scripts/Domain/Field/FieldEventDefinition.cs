using DragonGlare.Domain;

namespace DragonGlare.Domain.Field;

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
    public bool IsBlocking => BlocksMovement;

    public bool IsInteractable => ActionType != FieldEventActionType.Treasure || true;

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
            System.Math.Abs(TilePosition.X - playerTile.X) + System.Math.Abs(TilePosition.Y - playerTile.Y) == 1;
    }

    public IReadOnlyList<string> DialogPages => JapanesePages;
}
