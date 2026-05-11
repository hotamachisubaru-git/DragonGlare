using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Field;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Persistence;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private static readonly EquipmentSlot[] ArmorSlots =
    [
        EquipmentSlot.Armor,
        EquipmentSlot.Head,
        EquipmentSlot.Arms,
        EquipmentSlot.Legs,
        EquipmentSlot.Feet
    ];

    private IEnumerable<FieldEventDefinition> GetFieldEvents(FieldMapId mapId)
    {
        return FieldContent.FieldEvents.Where(fieldEvent => fieldEvent.MapId == mapId);
    }

    private IEnumerable<FieldEventDefinition> GetCurrentFieldEvents()
    {
        return GetFieldEvents(currentFieldMap);
    }

    private IEnumerable<FieldEventDefinition> GetRenderableCurrentFieldEvents()
    {
        return GetCurrentFieldEvents().Where(fieldEvent => fieldEvent.RenderOnMap);
    }

    private bool IsBlockedByFieldEvent(Point tile)
    {
        return IsBlockedByFieldEvent(currentFieldMap, tile);
    }

    private bool IsBlockedByFieldEvent(FieldMapId mapId, Point tile)
    {
        return GetFieldEvents(mapId).Any(fieldEvent => fieldEvent.BlocksMovement && fieldEvent.TilePosition == tile);
    }

    private FieldEventDefinition? GetInteractableFieldEvent()
    {
        return GetCurrentFieldEvents()
            .FirstOrDefault(fieldEvent => fieldEvent.CanInteractFrom(player.TilePosition));
    }

    private void SetFieldMap(FieldMapId mapId)
    {
        currentFieldMap = mapId;
        map = MapFactory.CreateMap(mapId);
        ResetFieldMovementAnimation();
        ResetEncounterCounter();
        UpdateBgm();
    }

    private void ChangeGameState(GameState nextState)
    {
        if (gameState == nextState)
        {
            return;
        }

        pendingGameState = nextState;
        sceneFadeOutFramesRemaining = SceneFadeOutDuration;
    }

    private void OpenSaveSlotSelection(SaveSlotSelectionMode mode)
    {
        saveSlotSelectionMode = mode;
        dataOperationSourceSlot = 0;
        RefreshSaveSlotSummaries();
        saveSlotCursor = Math.Clamp(activeSaveSlot - 1, 0, SaveService.SlotCount - 1);
        if (mode == SaveSlotSelectionMode.Save && activeSaveSlot == 0)
        {
            saveSlotCursor = 0;
        }
        else if (mode == SaveSlotSelectionMode.CopySource)
        {
            saveSlotCursor = GetFirstOccupiedSaveSlotIndex();
        }
        else if (mode == SaveSlotSelectionMode.DeleteSelect)
        {
            saveSlotCursor = GetFirstDeletableSaveSlotIndex();
        }

        menuNotice = string.Empty;
        menuNoticeFrames = 0;
        ChangeGameState(GameState.SaveSlotSelection);
    }

    private void RefreshSaveSlotSummaries()
    {
        saveSlotSummaries = saveService.GetSlotSummaries();
    }

    private int GetFirstOccupiedSaveSlotIndex()
    {
        for (var index = 0; index < saveSlotSummaries.Count; index++)
        {
            if (saveSlotSummaries[index].State == SaveSlotState.Occupied)
            {
                return index;
            }
        }

        return 0;
    }

    private int GetFirstDeletableSaveSlotIndex()
    {
        for (var index = 0; index < saveSlotSummaries.Count; index++)
        {
            if (saveSlotSummaries[index].State != SaveSlotState.Empty)
            {
                return index;
            }
        }

        return 0;
    }

    private void ShowTransientNotice(string message, int frames = 180)
    {
        menuNotice = message;
        menuNoticeFrames = frames;
    }

    private void SwitchFieldMap(FieldMapId mapId, Point destinationTile, bool persistProgress = true)
    {
        SetFieldMap(mapId);
        player.TilePosition = destinationTile;
        CloseFieldDialog();
        movementCooldown = FieldMovementAnimationDuration;

        if (persistProgress)
        {
            PersistProgress();
        }
    }

    private void ResetEncounterCounter()
    {
        fieldEncounterStepsRemaining = random.Next(6, 12);
    }

    private void StartFieldMovementAnimation(Point movement)
    {
        fieldMovementAnimationDirection = movement;
        fieldMovementAnimationFramesRemaining = FieldMovementAnimationDuration;
    }

    private void UpdateFieldMovementAnimation()
    {
        if (fieldMovementAnimationFramesRemaining <= 0)
        {
            fieldMovementAnimationDirection = Point.Empty;
            return;
        }

        fieldMovementAnimationFramesRemaining--;
        if (fieldMovementAnimationFramesRemaining == 0)
        {
            fieldMovementAnimationDirection = Point.Empty;
        }
    }

    private void ResetFieldMovementAnimation()
    {
        fieldMovementAnimationDirection = Point.Empty;
        fieldMovementAnimationFramesRemaining = 0;
    }

    private Point GetFieldMovementAnimationOffset()
    {
        if (fieldMovementAnimationFramesRemaining <= 0)
        {
            return Point.Empty;
        }

        var progress = fieldMovementAnimationFramesRemaining / (float)FieldMovementAnimationDuration;
        return new Point(
            (int)Math.Round(fieldMovementAnimationDirection.X * TileSize * progress),
            (int)Math.Round(fieldMovementAnimationDirection.Y * TileSize * progress));
    }

    private int GetFieldViewportWidthTiles()
    {
        return isFieldStatusVisible ? CompactFieldViewportWidthTiles : ExpandedFieldViewportWidthTiles;
    }

    private int GetFieldViewportHeightTiles()
    {
        return isFieldStatusVisible ? CompactFieldViewportHeightTiles : ExpandedFieldViewportHeightTiles;
    }

    private int GetExpandedFieldViewportHorizontalPadding()
    {
        var expandedFieldViewportWidth = UiCanvas.VirtualWidth - (ExpandedFieldViewportHorizontalMargin * 2);
        return Math.Max(0, (expandedFieldViewportWidth - (ExpandedFieldViewportWidthTiles * TileSize)) / 2);
    }

    private Rectangle GetFieldViewport()
    {
        var widthTiles = GetFieldViewportWidthTiles();
        var heightTiles = GetFieldViewportHeightTiles();
        var width = widthTiles * TileSize;
        var height = heightTiles * TileSize;
        var x = isFieldStatusVisible ? 16 : ExpandedFieldViewportHorizontalMargin;
        var y = isFieldStatusVisible ? ExpandedFieldViewportVerticalTrim / 2 : 0;

        if (!isFieldStatusVisible)
        {
            width = UiCanvas.VirtualWidth - (ExpandedFieldViewportHorizontalMargin * 2);
            y += ExpandedFieldViewportVerticalTrim / 2;
            height -= ExpandedFieldViewportVerticalTrim;
        }
        else
        {
            height = Math.Min(height, UiCanvas.VirtualHeight - y - (ExpandedFieldViewportVerticalTrim / 2));
        }

        return new Rectangle(x, y, width, height);
    }

    private Point GetFieldCameraOrigin()
    {
        var viewportWidthTiles = GetFieldViewportWidthTiles();
        var viewportHeightTiles = GetFieldViewportHeightTiles();
        var maxCameraX = Math.Max(0, map.GetLength(1) - viewportWidthTiles);
        var maxCameraY = Math.Max(0, map.GetLength(0) - viewportHeightTiles);

        return new Point(
            Math.Clamp(player.TilePosition.X - (viewportWidthTiles / 2), 0, maxCameraX),
            Math.Clamp(player.TilePosition.Y - (viewportHeightTiles / 2), 0, maxCameraY));
    }

    private Point GetFieldCameraAnimationOffset(Point cameraOrigin, Point animationOffset)
    {
        var maxCameraX = Math.Max(0, map.GetLength(1) - GetFieldViewportWidthTiles());
        var maxCameraY = Math.Max(0, map.GetLength(0) - GetFieldViewportHeightTiles());

        return new Point(
            cameraOrigin.X > 0 && cameraOrigin.X < maxCameraX ? animationOffset.X : 0,
            cameraOrigin.Y > 0 && cameraOrigin.Y < maxCameraY ? animationOffset.Y : 0);
    }

    private Point GetPlayerAnimationOffset(Point cameraOrigin, Point animationOffset)
    {
        var cameraOffset = GetFieldCameraAnimationOffset(cameraOrigin, animationOffset);
        return new Point(animationOffset.X - cameraOffset.X, animationOffset.Y - cameraOffset.Y);
    }

    private Rectangle GetFieldTileRectangle(Rectangle viewport, Point cameraOrigin, Point tile, Point offset)
    {
        var horizontalPadding = isFieldStatusVisible ? 0 : GetExpandedFieldViewportHorizontalPadding();
        var x = viewport.X + horizontalPadding + ((tile.X - cameraOrigin.X) * TileSize) + offset.X;
        var y = viewport.Y + ((tile.Y - cameraOrigin.Y) * TileSize) + offset.Y;

        return new Rectangle(x, y, TileSize, TileSize);
    }

    private int GetTileIdAtWorldPosition(Point tile)
    {
        if (tile.X < 0 || tile.Y < 0 || tile.X >= map.GetLength(1) || tile.Y >= map.GetLength(0))
        {
            return MapFactory.WallTile;
        }

        return map[tile.Y, tile.X];
    }

    private void OpenFieldDialog(FieldEventDefinition fieldEvent)
    {
        if (fieldEvent.ActionType == FieldEventActionType.Bank)
        {
            EnterBank();
            return;
        }

        var result = fieldEventService.Interact(player, fieldEvent, selectedLanguage);
        activeFieldDialogPages = result.Pages
            .Where(page => !string.IsNullOrWhiteSpace(page))
            .ToArray();
        activeFieldDialogPageIndex = 0;
        activeFieldDialogPortraitAssetName = fieldEvent.PortraitAssetName;
        isFieldDialogOpen = activeFieldDialogPages.Count > 0;

        if (result.ShouldPersistProgress)
        {
            PersistProgress();
        }

        PlaySe(SoundEffect.Dialog);
    }

    private void AdvanceFieldDialog()
    {
        if (!isFieldDialogOpen)
        {
            return;
        }

        if (activeFieldDialogPageIndex < activeFieldDialogPages.Count - 1)
        {
            activeFieldDialogPageIndex++;
            PlaySe(SoundEffect.Dialog);
            return;
        }

        CloseFieldDialog();
    }

    private void CloseFieldDialog()
    {
        isFieldDialogOpen = false;
        activeFieldDialogPages = [];
        activeFieldDialogPageIndex = 0;
        activeFieldDialogPortraitAssetName = null;
    }

    private string GetCurrentFieldDialogPage()
    {
        if (!isFieldDialogOpen || activeFieldDialogPages.Count == 0)
        {
            return string.Empty;
        }

        return activeFieldDialogPages[Math.Clamp(activeFieldDialogPageIndex, 0, activeFieldDialogPages.Count - 1)];
    }

    private bool TryTransitionFromTile(Point tile)
    {
        if (!fieldTransitionService.TryGetTransition(currentFieldMap, tile, out var transition))
        {
            return false;
        }

        SwitchFieldMap(transition.ToMapId, transition.DestinationTile);
        return true;
    }

    private static string GetMapDisplayName(FieldMapId mapId, UiLanguage language)
    {
        if (language == UiLanguage.Japanese)
        {
            return mapId switch
            {
                FieldMapId.Castle => "しろ",
                FieldMapId.Dungeon => "ダンジョン",
                FieldMapId.Field => "フィールド",
                _ => "ハブ"
            };
        }

        return mapId switch
        {
            FieldMapId.Castle => "CASTLE",
            FieldMapId.Dungeon => "DUNGEON",
            FieldMapId.Field => "FIELD",
            _ => "HUB"
        };
    }

    private static string FormatBattleResolutionMessage(IEnumerable<global::DragonGlareAlpha.Domain.Battle.BattleSequenceStep> steps)
    {
        return string.Join('\n', steps.Select(step => step.Message).Where(message => !string.IsNullOrWhiteSpace(message)));
    }

    private string TrimPlayerName(string name)
    {
        var trimmed = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
        return trimmed.Length <= MaxPlayerNameLength ? trimmed : trimmed[..MaxPlayerNameLength];
    }

    private void SyncPlayerNameBuffer(string name)
    {
        playerName.Clear();
        if (!string.IsNullOrWhiteSpace(name))
        {
            playerName.Append(TrimPlayerName(name));
        }
    }
}
