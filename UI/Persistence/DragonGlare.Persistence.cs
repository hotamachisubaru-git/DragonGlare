using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Persistence;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void StartNewGame()
    {
        selectedLanguage = UiLanguage.Japanese;
        languageCursor = 0;
        languageOpeningElapsedFrames = 0;
        languageOpeningLineIndex = 0;
        languageOpeningLineFrame = 0;
        languageOpeningFinished = false;
        languageOpeningLastSourceX = -1;
        languageOpeningLastSourceY = -1;
        prologueBgmCompleted = false;
        skipLanguageSelectionPrompt = false;
        nameCursorRow = 0;
        nameCursorColumn = 0;
        activeSaveSlot = 0;
        saveSlotCursor = 0;
        menuNotice = string.Empty;
        menuNoticeFrames = 0;
        playerName.Clear();
        ApplyExplorationSession(progressionService.CreateNewPlayer(UiLanguage.Japanese, PlayerStartTile), FieldMapId.Hub);
        ChangeGameState(GameState.LanguageSelection);
    }

    private bool TryLoadGame(int slotNumber)
    {
        if (!saveService.TryLoadSlot(slotNumber, out var save) || save is null)
        {
            return false;
        }

        activeSaveSlot = slotNumber;
        var restored = SaveDataMapper.Restore(save, PlayerStartTile);
        var loadedPlayer = restored.Player;
        loadedPlayer.Name = TrimPlayerName(loadedPlayer.Name);

        var loadedMapId = restored.MapId;
        if (!IsWalkableTile(MapFactory.CreateMap(loadedMapId), loadedPlayer.TilePosition) ||
            IsBlockedByFieldEvent(loadedMapId, loadedPlayer.TilePosition))
        {
            loadedMapId = FieldMapId.Hub;
            loadedPlayer.TilePosition = PlayerStartTile;
        }

        selectedLanguage = restored.Language;
        ApplyExplorationSession(loadedPlayer, loadedMapId);
        return true;
    }

    private void PersistProgress()
    {
        if (activeSaveSlot is < 1 or > SaveService.SlotCount)
        {
            return;
        }

        progressSavePending = true;
        progressSaveDelayFrames = ProgressSaveDelayFrames;
        if (progressSaveMaxDelayFrames <= 0)
        {
            progressSaveMaxDelayFrames = ProgressSaveMaxDelayFrames;
        }
    }

    private void UpdateQueuedProgressSave()
    {
        if (!progressSavePending)
        {
            return;
        }

        if (pendingGameState is not null || sceneFadeOutFramesRemaining > 0)
        {
            return;
        }

        progressSaveDelayFrames = Math.Max(0, progressSaveDelayFrames - 1);
        progressSaveMaxDelayFrames = Math.Max(0, progressSaveMaxDelayFrames - 1);

        var canWaitForInputToSettle = progressSaveMaxDelayFrames > 0;
        if (canWaitForInputToSettle &&
            gameState == GameState.Field &&
            (movementCooldown > 0 || fieldMovementAnimationFramesRemaining > 0))
        {
            return;
        }

        if (progressSaveDelayFrames > 0 && progressSaveMaxDelayFrames > 0)
        {
            return;
        }

        FlushQueuedProgressSave(refreshSlotSummaries: false);
    }

    private void FlushQueuedProgressSave(bool refreshSlotSummaries)
    {
        if (!progressSavePending)
        {
            return;
        }

        progressSavePending = false;
        progressSaveDelayFrames = 0;
        progressSaveMaxDelayFrames = 0;
        SaveGame(refreshSlotSummaries);
    }

    private void SaveGame(bool refreshSlotSummaries = true)
    {
        if (gameState == GameState.ModeSelect || gameState == GameState.LanguageSelection)
        {
            return;
        }

        if (gameState == GameState.NameInput)
        {
            if (playerName.Length == 0)
            {
                return;
            }

            player.Name = TrimPlayerName(playerName.ToString());
        }

        if (string.IsNullOrWhiteSpace(player.Name))
        {
            return;
        }

        if (activeSaveSlot is < 1 or > SaveService.SlotCount)
        {
            return;
        }

        player.Language = selectedLanguage;

        var save = SaveDataMapper.Create(player, selectedLanguage, currentFieldMap, activeSaveSlot);

        try
        {
            saveService.SaveSlot(activeSaveSlot, save);
            if (refreshSlotSummaries)
            {
                RefreshSaveSlotSummaries();
            }
        }
        catch
        {
        }
    }
}
