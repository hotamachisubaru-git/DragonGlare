using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
using XnaPoint = Microsoft.Xna.Framework.Point;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Persistence;
using static DragonGlareAlpha.Domain.Constants;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void UpdateGame()
    {
        frameCounter++;
        UpdateFieldMovementAnimation();
        RunAntiCheatChecks();

        if (WasPressed(XnaKeys.F11))
        {
            ToggleFullscreen();
        }

        if (WasPressed(XnaKeys.Escape) && gameState == GameState.ModeSelect)
        {
            Exit();
            return;
        }

        if (pendingGameState is not null)
        {
            UpdateSceneFadeOut();
            return;
        }

        if (startupFadeFrames > 0)
        {
            startupFadeFrames--;
        }

        if (menuNoticeFrames > 0)
        {
            menuNoticeFrames--;
            if (menuNoticeFrames == 0)
            {
                menuNotice = string.Empty;
            }
        }

        switch (gameState)
        {
            case GameState.ModeSelect: UpdateModeSelect(); break;
            case GameState.LanguageSelection: UpdateLanguageSelection(); break;
            case GameState.NameInput: UpdateNameInput(); break;
            case GameState.SaveSlotSelection: UpdateSaveSlotSelection(); break;
            case GameState.Field: UpdateField(); break;
            case GameState.EncounterTransition: UpdateEncounterTransition(); break;
            case GameState.Battle: UpdateBattle(); break;
            case GameState.ShopBuy: UpdateShopBuy(); break;
            case GameState.Bank: UpdateBank(); break;
        }
    }

    private void UpdateModeSelect()
    {
        if (WasPressed(XnaKeys.Up) || WasPressed(XnaKeys.W)) modeCursor = Math.Max(0, modeCursor - 1);
        else if (WasPressed(XnaKeys.Down) || WasPressed(XnaKeys.S)) modeCursor = Math.Min(3, modeCursor + 1);

        if (!WasConfirmPressed()) return;

        if (modeCursor == 0) { StartNewGame(); return; }
        if (modeCursor == 1) { OpenSaveSlotSelection(SaveSlotSelectionMode.Load); return; }

        ShowTransientNotice(modeCursor == 2 ? "データうつしは まだ未実装です。" : "データけしは まだ未実装です。");
    }

    private void UpdateLanguageSelection()
    {
        if (!languageOpeningFinished) { UpdateLanguageOpening(); return; }

        if (WasPressed(XnaKeys.Up) || WasPressed(XnaKeys.W)) languageCursor = 0;
        else if (WasPressed(XnaKeys.Down) || WasPressed(XnaKeys.S)) languageCursor = 1;

        if (WasPressed(XnaKeys.Escape)) { ChangeGameState(GameState.ModeSelect); return; }
        if (!WasConfirmPressed()) return;

        selectedLanguage = languageCursor == 0 ? UiLanguage.Japanese : UiLanguage.English;
        player.Language = selectedLanguage;

        if (skipLanguageSelectionPrompt) { skipLanguageSelectionPrompt = false; BeginFieldSession(); return; }
        ChangeGameState(GameState.NameInput);
    }

    private void UpdateLanguageOpening()
    {
        StartPrologueBgm();
        if (WasPressed(XnaKeys.Escape)) { StopPrologueBgm(); ChangeGameState(GameState.ModeSelect); return; }
        if (WasConfirmPressed()) { languageOpeningFinished = true; StopPrologueBgm(); return; }

        languageOpeningElapsedFrames = Math.Min(LanguageOpeningTotalFrames, languageOpeningElapsedFrames + 1);
        languageOpeningLineFrame++;

        var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
        if (languageOpeningLineFrame < currentLine.DisplayFrames + currentLine.GapFrames) return;

        languageOpeningLineIndex++;
        languageOpeningLineFrame = 0;
        if (languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            languageOpeningFinished = true;
            StopPrologueBgm();
        }
    }

    private void UpdateNameInput()
    {
        if (WasPressed(XnaKeys.Escape)) { ChangeGameState(GameState.LanguageSelection); return; }
        if (WasConfirmPressed())
        {
            playerName.Clear();
            playerName.Append(selectedLanguage == UiLanguage.Japanese ? "ゆうしゃ" : "HERO");
            player.Name = playerName.ToString();
            OpenSaveSlotSelection(SaveSlotSelectionMode.Save);
        }
    }

    private void UpdateSaveSlotSelection()
    {
        if (WasPressed(XnaKeys.Up) || WasPressed(XnaKeys.W)) saveSlotCursor = Math.Max(0, saveSlotCursor - 1);
        else if (WasPressed(XnaKeys.Down) || WasPressed(XnaKeys.S)) saveSlotCursor = Math.Min(SaveService.SlotCount - 1, saveSlotCursor + 1);

        if (WasPressed(XnaKeys.Escape)) { ChangeGameState(saveSlotSelectionMode == SaveSlotSelectionMode.Save ? GameState.NameInput : GameState.ModeSelect); return; }
        if (!WasConfirmPressed()) return;

        var selectedSlot = saveSlotCursor + 1;
        if (saveSlotSelectionMode == SaveSlotSelectionMode.Load)
        {
            if (TryLoadGame(selectedSlot)) { ChangeGameState(GameState.Field); return; }
            RefreshSaveSlotSummaries();
            ShowTransientNotice(saveService.LastFailureReason switch {
                SaveLoadFailureReason.InvalidSignature => "SAVE DATA INVALID / セーブデータが改ざんされています",
                SaveLoadFailureReason.InvalidFormat => "SAVE DATA ERROR / セーブデータが壊れています",
                _ => "NO SAVE DATA / セーブデータがありません"
            });
            return;
        }
        activeSaveSlot = selectedSlot;
        TryPersistProgress();
        ChangeGameState(GameState.Field);
    }

    private void UpdateField()
    {
        if (WasPressed(XnaKeys.Escape)) { ChangeGameState(GameState.ModeSelect); return; }
        if (WasPressed(XnaKeys.B)) { EnterBattle(); return; }
        if (WasPressed(XnaKeys.V)) { ChangeGameState(GameState.ShopBuy); return; }
        if (WasPressed(XnaKeys.N)) { ChangeGameState(GameState.Bank); return; }

        if (movementCooldown > 0) { movementCooldown--; return; }

        var movement = XnaPoint.Zero;
        if (IsDown(XnaKeys.Left) || IsDown(XnaKeys.A)) movement = new XnaPoint(-1, 0);
        else if (IsDown(XnaKeys.Right) || IsDown(XnaKeys.D)) movement = new XnaPoint(1, 0);
        else if (IsDown(XnaKeys.Up) || IsDown(XnaKeys.W)) movement = new XnaPoint(0, -1);
        else if (IsDown(XnaKeys.Down) || IsDown(XnaKeys.S)) movement = new XnaPoint(0, 1);

        if (movement != XnaPoint.Zero) TryMovePlayer(movement);
    }

    private void UpdateEncounterTransition()
    {
        encounterTransitionFrames--;
        if (encounterTransitionFrames <= 0) ChangeGameState(GameState.Battle);
    }

    private void UpdateBattle()
    {
        if (WasPressed(XnaKeys.Escape)) { ResetBattleState(); ChangeGameState(GameState.Field); return; }
        if (WasConfirmPressed())
        {
            if (currentEncounter is not null) currentEncounter.CurrentHp = 0;
            ResetBattleState();
            ChangeGameState(GameState.Field);
        }
    }

    private void UpdateShopBuy() { if (WasPressed(XnaKeys.Escape) || WasConfirmPressed()) ChangeGameState(GameState.Field); }

    private void UpdateBank()
    {
        if (WasPressed(XnaKeys.Escape) || WasConfirmPressed()) { bankService.AccrueStepInterest(player); ChangeGameState(GameState.Field); }
    }
}
