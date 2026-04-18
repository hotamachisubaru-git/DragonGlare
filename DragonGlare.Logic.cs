using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using XnaPoint = Microsoft.Xna.Framework.Point;
using DrawingPoint = System.Drawing.Point;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Persistence;
using DragonGlareAlpha.Security;
using DragonGlareAlpha.Services;
using static DragonGlareAlpha.Domain.Constants;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void StartNewGame()
    {
        player = PlayerProgress.CreateDefault(new DrawingPoint(PlayerStartTile.X, PlayerStartTile.Y));
        selectedLanguage = UiLanguage.Japanese;
        currentFieldMap = FieldMapId.Hub;
        map = MapFactory.CreateMap(currentFieldMap);
        activeSaveSlot = 0;
        fieldEncounterStepsRemaining = 7;
        playerName.Clear();
        ResetBattleState();
        ResetOpening();
        skipLanguageSelectionPrompt = false;
        ChangeGameState(GameState.LanguageSelection);
    }

    private void BeginFieldSession()
    {
        if (string.IsNullOrWhiteSpace(player.Name)) player.Name = selectedLanguage == UiLanguage.Japanese ? "ゆうしゃ" : "HERO";
        currentFieldMap = FieldMapId.Hub;
        map = MapFactory.CreateMap(currentFieldMap);
        ResetBattleState();
        ChangeGameState(GameState.Field);
    }

    private void OpenSaveSlotSelection(SaveSlotSelectionMode mode)
    {
        saveSlotSelectionMode = mode;
        RefreshSaveSlotSummaries();
        saveSlotCursor = MathHelper.Clamp(activeSaveSlot - 1, 0, SaveService.SlotCount - 1);
        menuNotice = string.Empty; menuNoticeFrames = 0;
        ChangeGameState(GameState.SaveSlotSelection);
    }

    private void ChangeGameState(GameState next)
    {
        if (gameState == next) return;
        pendingGameState = next;
        sceneFadeOutFramesRemaining = SceneFadeOutDuration;
    }

    private void UpdateSceneFadeOut()
    {
        if (sceneFadeOutFramesRemaining > 0) sceneFadeOutFramesRemaining--;
        if (sceneFadeOutFramesRemaining > 0 || pendingGameState is null) return;
        gameState = pendingGameState.Value;
        pendingGameState = null;
        startupFadeFrames = 20;
    }

    private bool TryMovePlayer(XnaPoint movement)
    {
        SetPlayerFacingDirection(movement);
        var target = new XnaPoint(player.TilePosition.X + movement.X, player.TilePosition.Y + movement.Y);
        if (!IsWalkableTile(target)) { movementCooldown = 3; return false; }

        player.TilePosition = new DrawingPoint(target.X, target.Y);
        bankService.AccrueStepInterest(player);
        StartFieldMovementAnimation(movement);
        movementCooldown = 6;

        if (TryTransitionFromTile(target)) return true;
        if (TryTriggerRandomEncounter()) { TryPersistProgress(); return true; }

        TryPersistProgress();
        return true;
    }

    private bool IsWalkableTile(XnaPoint t) => t.X >= 0 && t.Y >= 0 && t.X < map.GetLength(1) && t.Y < map.GetLength(0) && map[t.Y, t.X] != MapFactory.WallTile;

    private bool TryTransitionFromTile(XnaPoint t)
    {
        if (!fieldTransitionService.TryGetTransition(currentFieldMap, new DrawingPoint(t.X, t.Y), out var transition)) return false;
        currentFieldMap = transition.ToMapId;
        map = MapFactory.CreateMap(currentFieldMap);
        player.TilePosition = transition.DestinationTile;
        ResetEncounterCounter(); TryPersistProgress();
        return true;
    }

    private bool TryTriggerRandomEncounter()
    {
        fieldEncounterStepsRemaining--;
        if (fieldEncounterStepsRemaining > 0 || currentFieldMap == FieldMapId.Hub) return false;
        ResetEncounterCounter(); EnterBattle();
        return true;
    }

    private void EnterBattle()
    {
        currentEncounter = battleService.CreateEncounter(random, currentFieldMap, player.Level);
        encounterTransitionFrames = EncounterTransitionDuration;
        ChangeGameState(GameState.EncounterTransition);
    }

    private void ResetBattleState() { currentEncounter = null; encounterTransitionFrames = 0; }
    private void RefreshSaveSlotSummaries() => saveSlotSummaries = saveService.GetSlotSummaries();

    private bool TryLoadGame(int slot)
    {
        if (!saveService.TryLoadSlot(slot, out var data) || data is null) return false;
        var restored = SaveDataMapper.Restore(data, new DrawingPoint(PlayerStartTile.X, PlayerStartTile.Y));
        selectedLanguage = restored.Language; currentFieldMap = restored.MapId; player = restored.Player;
        map = MapFactory.CreateMap(currentFieldMap); activeSaveSlot = slot;
        ResetBattleState(); ResetEncounterCounter();
        return true;
    }

    private void TryPersistProgress()
    {
        if (activeSaveSlot > 0)
        {
            try { saveService.SaveSlot(activeSaveSlot, SaveDataMapper.Create(player, selectedLanguage, currentFieldMap, activeSaveSlot)); }
            catch { /* Ignore */ }
        }
    }

    private void RunAntiCheatChecks()
    {
        if (frameCounter % 30 == 0) { player.RekeySensitiveValues(); currentEncounter?.RekeySensitiveValues(); }
        if (frameCounter % 120 != 0) return;
        player.ValidateIntegrity(); currentEncounter?.ValidateIntegrity();
        if (antiCheatService.TryDetectViolation(out var msg)) throw new TamperDetectedException(msg);
    }

    private void HandleSecurityViolation(string message)
    {
        if (skipSaveOnClose) return;
        skipSaveOnClose = true;
        Exit();
    }

    private void SetPlayerFacingDirection(XnaPoint m)
    {
        if (m.X < 0) playerFacingDirection = PlayerFacingDirection.Left;
        else if (m.X > 0) playerFacingDirection = PlayerFacingDirection.Right;
        else if (m.Y < 0) playerFacingDirection = PlayerFacingDirection.Up;
        else if (m.Y > 0) playerFacingDirection = PlayerFacingDirection.Down;
    }

    private void ShowTransientNotice(string msg, int f = 180) { menuNotice = msg; menuNoticeFrames = f; }
    private void ResetEncounterCounter() => fieldEncounterStepsRemaining = random.Next(6, 12);
    private void ResetOpening() { languageCursor = 0; languageOpeningElapsedFrames = 0; languageOpeningLineIndex = 0; languageOpeningLineFrame = 0; languageOpeningFinished = false; }
    private void StartFieldMovementAnimation(XnaPoint m) { fieldMovementAnimationDirection = m; fieldMovementAnimationFramesRemaining = FieldMovementAnimationDuration; }
    private void UpdateFieldMovementAnimation() { if (fieldMovementAnimationFramesRemaining > 0) fieldMovementAnimationFramesRemaining--; else fieldMovementAnimationDirection = XnaPoint.Zero; }
}
