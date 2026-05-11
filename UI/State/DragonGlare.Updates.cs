using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Persistence;
using DragonGlareAlpha.Security;
using DragonGlareAlpha.Services;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void UpdateGame()
    {
        frameCounter++;
        UpdateFieldMovementAnimation();
        UpdateBattleVisualEffects();
        RunAntiCheatChecks();

        if (AdvancePendingStateChange())
        {
            UpdateBgm();
            return;
        }

        UpdateStartupFade();
        UpdateTransientNotice();

        switch (gameState)
        {
            case GameState.StartupOptions:
                UpdateStartupOptions();
                break;
            case GameState.ModeSelect:
                UpdateModeSelect();
                break;
            case GameState.LanguageSelection:
                UpdateLanguageSelection();
                break;
            case GameState.NameInput:
                UpdateNameInput();
                break;
            case GameState.SaveSlotSelection:
                UpdateSaveSlotSelection();
                break;
            case GameState.Field:
                UpdateField();
                break;
            case GameState.EncounterTransition:
                UpdateEncounterTransition();
                break;
            case GameState.Battle:
                UpdateBattle();
                break;
            case GameState.ShopBuy:
                UpdateShopBuy();
                break;
            case GameState.Bank:
                UpdateBank();
                break;
        }

        UpdateBgm();
    }

    private bool AdvancePendingStateChange()
    {
        if (pendingGameState is null)
        {
            return false;
        }

        if (sceneFadeOutFramesRemaining > 0)
        {
            sceneFadeOutFramesRemaining--;
            if (sceneFadeOutFramesRemaining == 0)
            {
                CompletePendingStateChange();
            }
        }

        return true;
    }

    private void CompletePendingStateChange()
    {
        var previousState = gameState;
        var nextState = pendingGameState!.Value;
        gameState = nextState;
        pendingGameState = null;
        startupFadeFrames = 20;

        if (previousState == GameState.Battle && nextState != GameState.Battle)
        {
            ResetBattleState();
        }

        UpdateBgm();
        UpdateQueuedProgressSave();
    }

    private void UpdateStartupFade()
    {
        if (startupFadeFrames > 0)
        {
            startupFadeFrames--;
        }
    }

    private void UpdateTransientNotice()
    {
        if (menuNoticeFrames <= 0)
        {
            return;
        }

        menuNoticeFrames--;
        if (menuNoticeFrames == 0)
        {
            menuNotice = string.Empty;
        }
    }

    private void RunAntiCheatChecks()
    {
        if (frameCounter % 30 == 0)
        {
            player.RekeySensitiveValues();
            currentEncounter?.RekeySensitiveValues();
            pendingEncounter?.RekeySensitiveValues();
        }

        if (frameCounter % 120 != 0)
        {
            return;
        }

        player.ValidateIntegrity();
        currentEncounter?.ValidateIntegrity();
        pendingEncounter?.ValidateIntegrity();

        if (antiCheatService.TryDetectViolation(out var message))
        {
            throw new TamperDetectedException(message);
        }
    }

    private void UpdateModeSelect()
    {
        var previousCursor = modeCursor;
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            modeCursor = Math.Max(0, modeCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            modeCursor = Math.Min(3, modeCursor + 1);
        }
        PlayCursorSeIfChanged(previousCursor, modeCursor);

        if (!WasPrimaryConfirmPressed())
        {
            return;
        }

        if (modeCursor == 0)
        {
            StartNewGame();
            return;
        }

        if (modeCursor == 1)
        {
            OpenSaveSlotSelection(SaveSlotSelectionMode.Load);
            return;
        }

        OpenSaveSlotSelection(modeCursor == 2
            ? SaveSlotSelectionMode.CopySource
            : SaveSlotSelectionMode.DeleteSelect);
    }

    private void UpdateLanguageSelection()
    {
        if (!languageOpeningFinished)
        {
            UpdateLanguageOpening();
            return;
        }

        var previousCursor = languageCursor;
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            languageCursor = 0;
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            languageCursor = 1;
        }
        PlayCursorSeIfChanged(previousCursor, languageCursor);

        if (WasPrimaryConfirmPressed())
        {
            selectedLanguage = languageCursor == 0 ? UiLanguage.Japanese : UiLanguage.English;
            player.Language = selectedLanguage;
            ChangeGameState(GameState.NameInput);
            playerName.Clear();
            nameCursorRow = 0;
            nameCursorColumn = 0;
        }

        if (WasPressed(Keys.Escape))
        {
            PlayCancelSe();
            ChangeGameState(GameState.ModeSelect);
        }
    }

    private void UpdateLanguageOpening()
    {
        if (WasPressed(Keys.Escape))
        {
            PlayCancelSe();
            ChangeGameState(GameState.ModeSelect);
            return;
        }

        if (WasPrimaryConfirmPressed())
        {
            languageOpeningElapsedFrames = LanguageOpeningTotalFrames;
            languageOpeningLineIndex = LanguageOpeningScript.Length;
            languageOpeningLineFrame = 0;
            languageOpeningFinished = true;
        }

        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            languageOpeningFinished = true;
            if (skipLanguageSelectionPrompt)
            {
                skipLanguageSelectionPrompt = false;
                selectedLanguage = UiLanguage.Japanese;
                ChangeGameState(GameState.NameInput);
                return;
            }

            return;
        }

        languageOpeningElapsedFrames = Math.Min(LanguageOpeningTotalFrames, languageOpeningElapsedFrames + 1);
        languageOpeningLineFrame++;

        var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
        if (languageOpeningLineFrame < currentLine.DisplayFrames + currentLine.GapFrames)
        {
            return;
        }

        languageOpeningLineIndex++;
        languageOpeningLineFrame = 0;
        if (languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            languageOpeningFinished = true;
        }
    }

    private void UpdateNameInput()
    {
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            MoveNameCursor(0, -1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            MoveNameCursor(0, 1);
        }
        else if (WasPressed(Keys.Left) || WasPressed(Keys.A))
        {
            MoveNameCursor(-1, 0);
        }
        else if (WasPressed(Keys.Right) || WasPressed(Keys.D))
        {
            MoveNameCursor(1, 0);
        }

        if (WasPressed(Keys.Back))
        {
            RemoveLastCharacter();
        }

        if (WasPressed(Keys.Escape))
        {
            PlayCancelSe();
            ChangeGameState(GameState.LanguageSelection);
            return;
        }

        if (WasPrimaryConfirmPressed())
        {
            AddSelectedCharacter();
        }
    }

}
