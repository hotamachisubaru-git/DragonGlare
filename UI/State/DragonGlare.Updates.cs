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
        // If a scene change is pending, process fade-out and delay other updates
        if (pendingGameState is not null)
        {
            if (sceneFadeOutFramesRemaining > 0)
            {
                sceneFadeOutFramesRemaining--;
                if (sceneFadeOutFramesRemaining == 0)
                {
                    // perform actual state switch and start fade-in
                    var previousState = gameState;
                    var nextState = pendingGameState.Value;
                    gameState = nextState;
                    pendingGameState = null;
                    startupFadeFrames = 20; // fade-in on new scene
                    if (previousState == GameState.Battle && nextState != GameState.Battle)
                    {
                        ResetBattleState();
                    }

                    UpdateBgm();
                }
            }

            // While fading out, skip most per-state updates to avoid visual/logic jumps.
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
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            modeCursor = Math.Max(0, modeCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            modeCursor = Math.Min(3, modeCursor + 1);
        }

        if (!WasPressed(Keys.Enter))
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

        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            languageCursor = 0;
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            languageCursor = 1;
        }

        if (WasPressed(Keys.Enter))
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
            ChangeGameState(GameState.ModeSelect);
        }
    }

    private void UpdateLanguageOpening()
    {
        if (WasPressed(Keys.Escape))
        {
            ChangeGameState(GameState.ModeSelect);
            return;
        }

        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            languageOpeningFinished = true;
            // If we were started from the title's "Start New Game", skip the
            // language-selection prompt and advance directly to name input.
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
            ChangeGameState(GameState.LanguageSelection);
            return;
        }

        if (WasPressed(Keys.Enter))
        {
            AddSelectedCharacter();
        }
    }

    private void UpdateSaveSlotSelection()
    {
        if (saveSlotSelectionMode != SaveSlotSelectionMode.DeleteConfirm)
        {
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                saveSlotCursor = Math.Max(0, saveSlotCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                saveSlotCursor = Math.Min(SaveService.SlotCount - 1, saveSlotCursor + 1);
            }
        }

        if (WasPressed(Keys.Escape))
        {
            CancelSaveSlotSelection();
            return;
        }

        if (!WasPressed(Keys.Enter))
        {
            return;
        }

        var selectedSlot = saveSlotCursor + 1;
        switch (saveSlotSelectionMode)
        {
            case SaveSlotSelectionMode.Load:
                LoadSelectedSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.CopySource:
                SelectCopySourceSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.CopyDestination:
                CopyToSelectedSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.DeleteSelect:
                SelectDeleteSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.DeleteConfirm:
                DeleteSelectedSlot();
                return;
            default:
                activeSaveSlot = selectedSlot;
                SaveGame();
                ChangeGameState(GameState.Field);
                return;
        }
    }

    private void LoadSelectedSlot(int selectedSlot)
    {
        if (TryLoadGame(selectedSlot))
        {
            ChangeGameState(GameState.Field);
            return;
        }

        var failureReason = saveService.LastFailureReason;
        RefreshSaveSlotSummaries();
        ShowTransientNotice(GetSaveLoadFailureMessage(failureReason));
        PlaySe(SoundEffect.Collision);
    }

    private void SelectCopySourceSlot(int selectedSlot)
    {
        if (!IsSaveSlotOccupied(selectedSlot))
        {
            ShowTransientNotice("うつすデータがありません。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        dataOperationSourceSlot = selectedSlot;
        saveSlotSelectionMode = SaveSlotSelectionMode.CopyDestination;
        saveSlotCursor = GetDefaultCopyDestinationIndex(selectedSlot);
        ShowTransientNotice($"ぼうけんのしょ {selectedSlot} を どこへうつしますか？", 240);
        PlaySe(SoundEffect.Dialog);
    }

    private void CopyToSelectedSlot(int selectedSlot)
    {
        var sourceSlot = dataOperationSourceSlot;
        if (sourceSlot is < 1 or > SaveService.SlotCount)
        {
            saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
            ShowTransientNotice("うつすデータを えらびなおしてください。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        if (selectedSlot == sourceSlot)
        {
            ShowTransientNotice("同じ枠へは うつせません。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        bool copied;
        try
        {
            copied = saveService.CopySlot(sourceSlot, selectedSlot);
        }
        catch
        {
            RefreshSaveSlotSummaries();
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
            saveSlotCursor = GetFirstOccupiedSaveSlotIndex();
            ShowTransientNotice("SAVE DATA ERROR / データを処理できません");
            PlaySe(SoundEffect.Collision);
            return;
        }

        if (copied)
        {
            RefreshSaveSlotSummaries();
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
            saveSlotCursor = selectedSlot - 1;
            ShowTransientNotice($"ぼうけんのしょ {sourceSlot} を {selectedSlot} にうつしました。", 240);
            PlaySe(SoundEffect.Dialog);
            return;
        }

        var failureReason = saveService.LastFailureReason;
        RefreshSaveSlotSummaries();
        dataOperationSourceSlot = 0;
        saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
        saveSlotCursor = GetFirstOccupiedSaveSlotIndex();
        ShowTransientNotice(GetSaveLoadFailureMessage(failureReason));
        PlaySe(SoundEffect.Collision);
    }

    private void SelectDeleteSlot(int selectedSlot)
    {
        var summary = saveSlotSummaries.ElementAtOrDefault(selectedSlot - 1);
        if (summary is null || summary.State == SaveSlotState.Empty)
        {
            ShowTransientNotice("けすデータがありません。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        dataOperationSourceSlot = selectedSlot;
        saveSlotCursor = selectedSlot - 1;
        saveSlotSelectionMode = SaveSlotSelectionMode.DeleteConfirm;
        ShowTransientNotice($"ぼうけんのしょ {selectedSlot} を けしますか？", 240);
        PlaySe(SoundEffect.Dialog);
    }

    private void DeleteSelectedSlot()
    {
        var selectedSlot = dataOperationSourceSlot;
        if (selectedSlot is < 1 or > SaveService.SlotCount)
        {
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
            ShowTransientNotice("けすデータを えらびなおしてください。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        bool deleted;
        try
        {
            deleted = saveService.DeleteSlot(selectedSlot);
        }
        catch
        {
            RefreshSaveSlotSummaries();
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
            ShowTransientNotice("SAVE DATA ERROR / データを処理できません");
            PlaySe(SoundEffect.Collision);
            return;
        }

        if (activeSaveSlot == selectedSlot)
        {
            activeSaveSlot = 0;
        }

        RefreshSaveSlotSummaries();
        dataOperationSourceSlot = 0;
        saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
        saveSlotCursor = Math.Clamp(selectedSlot - 1, 0, SaveService.SlotCount - 1);
        ShowTransientNotice(deleted
            ? $"ぼうけんのしょ {selectedSlot} を けしました。"
            : "けすデータがありません。", 240);
        PlaySe(deleted ? SoundEffect.Dialog : SoundEffect.Collision);
    }

    private void CancelSaveSlotSelection()
    {
        switch (saveSlotSelectionMode)
        {
            case SaveSlotSelectionMode.Save:
                ChangeGameState(GameState.NameInput);
                return;
            case SaveSlotSelectionMode.CopyDestination:
                dataOperationSourceSlot = 0;
                saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                saveSlotCursor = GetFirstOccupiedSaveSlotIndex();
                menuNotice = string.Empty;
                menuNoticeFrames = 0;
                return;
            case SaveSlotSelectionMode.DeleteConfirm:
                saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                saveSlotCursor = Math.Clamp(dataOperationSourceSlot - 1, 0, SaveService.SlotCount - 1);
                dataOperationSourceSlot = 0;
                menuNotice = string.Empty;
                menuNoticeFrames = 0;
                return;
            default:
                dataOperationSourceSlot = 0;
                ChangeGameState(GameState.ModeSelect);
                return;
        }
    }

    private bool IsSaveSlotOccupied(int slotNumber)
    {
        return saveSlotSummaries.ElementAtOrDefault(slotNumber - 1)?.State == SaveSlotState.Occupied;
    }

    private static string GetSaveLoadFailureMessage(SaveLoadFailureReason failureReason)
    {
        return failureReason switch
        {
            SaveLoadFailureReason.InvalidSignature => "SAVE DATA INVALID / セーブデータが改ざんされています",
            SaveLoadFailureReason.InvalidFormat => "SAVE DATA ERROR / セーブデータが壊れています",
            _ => "NO SAVE DATA / セーブデータがありません"
        };
    }

    private int GetDefaultCopyDestinationIndex(int sourceSlot)
    {
        for (var index = 0; index < saveSlotSummaries.Count; index++)
        {
            if (index + 1 != sourceSlot && saveSlotSummaries[index].State == SaveSlotState.Empty)
            {
                return index;
            }
        }

        return GetFirstDifferentSaveSlotIndex(sourceSlot);
    }

    private static int GetFirstDifferentSaveSlotIndex(int sourceSlot)
    {
        for (var slotNumber = 1; slotNumber <= SaveService.SlotCount; slotNumber++)
        {
            if (slotNumber != sourceSlot)
            {
                return slotNumber - 1;
            }
        }

        return 0;
    }

    private void UpdateField()
    {
        if (isFieldDialogOpen)
        {
            if (WasFieldInteractPressed())
            {
                AdvanceFieldDialog();
            }
            else if (WasPressed(Keys.Escape))
            {
                CloseFieldDialog();
            }

            return;
        }

        if (WasPressed(Keys.B))
        {
            EnterBattle();
            return;
        }

        if (WasPressed(Keys.V))
        {
            EnterShopBuy();
            return;
        }

        if (WasPressed(Keys.X))
        {
            isFieldStatusVisible = !isFieldStatusVisible;
            return;
        }

        if (movementCooldown > 0)
        {
            movementCooldown--;
        }

        var movement = Point.Empty;
        if (heldKeys.Contains(Keys.Up) || heldKeys.Contains(Keys.W))
        {
            movement = new Point(0, -1);
        }
        else if (heldKeys.Contains(Keys.Down) || heldKeys.Contains(Keys.S))
        {
            movement = new Point(0, 1);
        }
        else if (heldKeys.Contains(Keys.Left) || heldKeys.Contains(Keys.A))
        {
            movement = new Point(-1, 0);
        }
        else if (heldKeys.Contains(Keys.Right) || heldKeys.Contains(Keys.D))
        {
            movement = new Point(1, 0);
        }

        if (movement != Point.Empty && movementCooldown == 0)
        {
            SetPlayerFacingDirection(movement);
            var moved = TryMovePlayer(movement);
            if (!moved)
            {
                PlaySe(SoundEffect.Collision);
            }

            movementCooldown = FieldMovementAnimationDuration;
            if (gameState != GameState.Field)
            {
                return;
            }
        }

        if (WasFieldInteractPressed())
        {
            var fieldEvent = GetInteractableFieldEvent();
            if (fieldEvent is not null)
            {
                OpenFieldDialog(fieldEvent);
            }
        }
    }

    private void UpdateBattle()
    {
        if (currentEncounter is null)
        {
            ResetBattleState();
            ChangeGameState(GameState.Field);
            return;
        }

        // バトルメッセージの1行ずつ表示アニメーション
        if (battleFlowState is BattleFlowState.Victory or BattleFlowState.Defeat or BattleFlowState.Escaped)
        {
            if (battleMessageLines.Length > 0 && battleMessageVisibleLines < battleMessageLines.Length)
            {
                if (battleMessageLineTimer > 0)
                {
                    battleMessageLineTimer--;
                }
                else
                {
                    battleMessageVisibleLines++;
                    battleMessageLineTimer = BattleMessageLineDelayFrames;
                }
            }
        }

        if (battleFlowState == BattleFlowState.Intro)
        {
            if (WasConfirmPressed())
            {
                battleFlowState = BattleFlowState.CommandSelection;
                battleMessage = GetBattleCommandPromptMessage();
            }

            return;
        }

        if (battleFlowState is BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
        {
            UpdateBattleSelectionMenu();
            return;
        }

        if (battleFlowState != BattleFlowState.CommandSelection)
        {
            if (WasConfirmPressed() || WasPressed(Keys.Escape))
            {
                // メッセージ表示途中で決定ボタンが押されたら、全行を一気に表示する（スキップ）
                if (battleMessageLines.Length > 0 && battleMessageVisibleLines < battleMessageLines.Length)
                {
                    battleMessageVisibleLines = battleMessageLines.Length;
                }
                else
                {
                    FinishBattle();
                }
            }

            return;
        }

        UpdateBattleCommandCursor();

        if (WasPressed(Keys.Escape))
        {
            battleMessage = BattleEscapeMessage;
            battleFlowState = BattleFlowState.Escaped;
            PersistProgress();
            return;
        }

        if (!WasConfirmPressed())
        {
            return;
        }

        var action = GameContent.BattleCommandGrid[battleCursorRow, battleCursorColumn];
        switch (action)
        {
            case BattleActionType.Item:
                OpenBattleSelectionMenu(BattleFlowState.ItemSelection);
                return;
            case BattleActionType.Equip:
                OpenBattleSelectionMenu(BattleFlowState.EquipmentSelection);
                return;
        }

        var result = battleService.ResolveTurn(player, currentEncounter, action, null, null, random);
        ApplyBattleResolution(result);
    }

    private void UpdateBattleCommandCursor()
    {
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            battleCursorRow = Math.Max(0, battleCursorRow - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            battleCursorRow = Math.Min(GetBattleCommandRowCount() - 1, battleCursorRow + 1);
        }
        else if (WasPressed(Keys.Left) || WasPressed(Keys.A))
        {
            battleCursorColumn = Math.Max(0, battleCursorColumn - 1);
        }
        else if (WasPressed(Keys.Right) || WasPressed(Keys.D))
        {
            battleCursorColumn = Math.Min(GetBattleCommandColumnCount() - 1, battleCursorColumn + 1);
        }
    }

    private void UpdateBattleSelectionMenu()
    {
        var entries = GetActiveBattleSelectionEntries();
        if (entries.Count == 0)
        {
            CloseBattleSelectionMenu(battleFlowState == BattleFlowState.ItemSelection
                ? GetBattleNoItemsMessage()
                : GetBattleNoEquipmentMessage());
            return;
        }

        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            MoveBattleSelectionCursor(-1, entries.Count);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            MoveBattleSelectionCursor(1, entries.Count);
        }

        if (WasBattleSubmenuBackPressed())
        {
            CloseBattleSelectionMenu();
            return;
        }

        if (!WasBattleSubmenuConfirmPressed() || currentEncounter is null)
        {
            return;
        }

        var selectedEntry = entries[battleListCursor];
        var action = battleFlowState == BattleFlowState.ItemSelection
            ? BattleActionType.Item
            : BattleActionType.Equip;
        var result = battleService.ResolveTurn(
            player,
            currentEncounter,
            action,
            selectedEntry.Consumable,
            selectedEntry.Equipment,
            random);
        ApplyBattleResolution(result);
        if (result.Outcome == BattleOutcome.Ongoing)
        {
            battleFlowState = BattleFlowState.CommandSelection;
        }
    }

    private void MoveBattleSelectionCursor(int delta, int itemCount)
    {
        battleListCursor = Math.Clamp(battleListCursor + delta, 0, itemCount - 1);
        if (battleListCursor < battleListScroll)
        {
            battleListScroll = battleListCursor;
            return;
        }

        if (battleListCursor >= battleListScroll + BattleSelectionVisibleRows)
        {
            battleListScroll = battleListCursor - BattleSelectionVisibleRows + 1;
        }
    }

    private void OpenBattleSelectionMenu(BattleFlowState nextState)
    {
        battleFlowState = nextState;
        battleListCursor = 0;
        battleListScroll = 0;

        var entries = GetActiveBattleSelectionEntries();
        if (entries.Count == 0)
        {
            CloseBattleSelectionMenu(nextState == BattleFlowState.ItemSelection
                ? GetBattleNoItemsMessage()
                : GetBattleNoEquipmentMessage());
            return;
        }

        battleMessage = nextState == BattleFlowState.ItemSelection
            ? GetBattleItemPromptMessage()
            : GetBattleEquipmentPromptMessage();
    }

    private void CloseBattleSelectionMenu(string? message = null)
    {
        battleFlowState = BattleFlowState.CommandSelection;
        battleListCursor = 0;
        battleListScroll = 0;
        battleMessage = message ?? GetBattleCommandPromptMessage();
    }

    private void ApplyBattleResolution(BattleTurnResolution result)
    {
        ApplyBattleVisualEffects(result);
        var resultMessage = FormatBattleResolutionMessage(result.Steps);
        var encounterEnemy = currentEncounter?.Enemy;

        switch (result.Outcome)
        {
            case BattleOutcome.Victory:
                battleMessage = encounterEnemy is null
                    ? resultMessage
                    : $"{resultMessage}\n{progressionService.ApplyBattleRewards(player, encounterEnemy, random)}";
                AppendBattleInterest(ref battleMessage);
                battleFlowState = BattleFlowState.Victory;
                PersistProgress();
                break;
            case BattleOutcome.Defeat:
                battleMessage = $"{resultMessage}\n{progressionService.ApplyDefeatPenalty(player, PlayerStartTile)}";
                AppendBattleInterest(ref battleMessage);
                SetFieldMap(FieldMapId.Hub);
                battleFlowState = BattleFlowState.Defeat;
                PersistProgress();
                break;
            case BattleOutcome.Escaped:
                battleMessage = resultMessage;
                AppendBattleInterest(ref battleMessage);
                battleFlowState = BattleFlowState.Escaped;
                PersistProgress();
                break;
            case BattleOutcome.Invalid:
                battleMessage = resultMessage;
                break;
            default:
                battleMessage = resultMessage;
                battleFlowState = BattleFlowState.CommandSelection;
                PersistProgress();
                break;
        }
        
        // メッセージを1行ずつ表示するための初期化
        battleMessageLines = battleMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        battleMessageVisibleLines = 1; // 最初の1行だけは即座に表示
        battleMessageLineTimer = BattleMessageLineDelayFrames;
    }

    private void AppendBattleInterest(ref string message)
    {
        var addedInterest = bankService.AccrueBattleInterest(player);
        if (addedInterest <= 0)
        {
            return;
        }

        message += $"\nかしつけの りそくが {addedInterest}G ふえた。";
    }

    private void UpdateEncounterTransition()
    {
        if (encounterTransitionFrames > 0)
        {
            encounterTransitionFrames--;
        }

        if (encounterTransitionFrames > 0)
        {
            return;
        }

        if (pendingEncounter is null)
        {
            ChangeGameState(GameState.Field);
            return;
        }

        currentEncounter = pendingEncounter;
        pendingEncounter = null;
        ResetBattleSelectionState();
        battleFlowState = BattleFlowState.Intro;
        battleMessage = GetBattleEncounterMessage(currentEncounter.Enemy.Name);
        ChangeGameState(GameState.Battle);
    }

    private void UpdateShopBuy()
    {
        if (shopPhase == ShopPhase.Welcome)
        {
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                shopPromptCursor = Math.Max(0, shopPromptCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                shopPromptCursor = Math.Min(2, shopPromptCursor + 1);
            }

            if (WasShopBackPressed())
            {
                ChangeGameState(GameState.Field);
                return;
            }

            if (!WasShopConfirmPressed())
            {
                return;
            }

            if (shopPromptCursor == 0)
            {
                OpenShopBuyCatalog();
                return;
            }

            if (shopPromptCursor == 1)
            {
                OpenShopSellCatalog();
                return;
            }

            ChangeGameState(GameState.Field);
            return;
        }

        var visibleEntries = GetShopVisibleEntries();
        var maxIndex = visibleEntries.Count - 1;
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            shopItemCursor = Math.Max(0, shopItemCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            shopItemCursor = Math.Min(maxIndex, shopItemCursor + 1);
        }

        if (WasShopBackPressed())
        {
            ReturnToShopPrompt(ShopReturnMessage);
            return;
        }

        if (!WasShopConfirmPressed())
        {
            return;
        }

        var selectedEntry = visibleEntries[shopItemCursor];
        if (selectedEntry.Type == ShopMenuEntryType.PreviousPage)
        {
            ChangeShopPage(-1);
            return;
        }

        if (selectedEntry.Type == ShopMenuEntryType.NextPage)
        {
            ChangeShopPage(1);
            return;
        }

        if (selectedEntry.Type == ShopMenuEntryType.Quit)
        {
            ReturnToShopPrompt(ShopFarewellMessage);
            return;
        }

        if (shopPhase == ShopPhase.SellList)
        {
            if (selectedEntry.InventoryItem is null)
            {
                return;
            }

            var sellResult = shopService.SellItem(player, selectedEntry.InventoryItem.Value.ItemId);
            shopMessage = sellResult.Message;
            if (sellResult.Success)
            {
                ResetShopListSelection(Math.Min(shopPageIndex, Math.Max(0, GetShopPageCount() - 1)));
                PersistProgress();
            }

            return;
        }

        if (selectedEntry.Product is null)
        {
            return;
        }

        var purchaseResult = shopService.PurchaseProduct(player, selectedEntry.Product);
        shopMessage = purchaseResult.Message;
        if (purchaseResult.Success)
        {
            PersistProgress();
        }
    }

    private void EnterBattle()
    {
        StartEncounterTransition(battleService.CreateEncounter(random, currentFieldMap, player.Level));
    }

    private void EnterShopBuy()
    {
        ResetShopState();
        ChangeGameState(GameState.ShopBuy);
        PlaySe(SoundEffect.Dialog);
    }

    private void EnterBank()
    {
        ResetBankState();
        ChangeGameState(GameState.Bank);
        PlaySe(SoundEffect.Dialog);
    }

    private void UpdateBank()
    {
        if (bankPhase == BankPhase.Welcome)
        {
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                bankPromptCursor = Math.Max(0, bankPromptCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                bankPromptCursor = Math.Min(3, bankPromptCursor + 1);
            }

            if (WasShopBackPressed())
            {
                ChangeGameState(GameState.Field);
                return;
            }

            if (!WasShopConfirmPressed())
            {
                return;
            }

            switch (bankPromptCursor)
            {
                case 0:
                    OpenBankList(BankPhase.DepositList);
                    return;
                case 1:
                    OpenBankList(BankPhase.WithdrawList);
                    return;
                case 2:
                    OpenBankList(BankPhase.BorrowList);
                    return;
                default:
                    ChangeGameState(GameState.Field);
                    return;
            }
        }

        var options = GetBankAmountOptions();
        if (WasPressed(Keys.Up) || WasPressed(Keys.W))
        {
            bankItemCursor = Math.Max(0, bankItemCursor - 1);
        }
        else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
        {
            bankItemCursor = Math.Min(options.Count - 1, bankItemCursor + 1);
        }

        if (WasShopBackPressed())
        {
            ReturnToBankPrompt(BankReturnMessage);
            return;
        }

        if (!WasShopConfirmPressed())
        {
            return;
        }

        var selectedOption = options[bankItemCursor];
        if (selectedOption.Quit)
        {
            ReturnToBankPrompt(BankReturnMessage);
            return;
        }

        var amount = ResolveBankTransactionAmount(selectedOption);
        var result = bankPhase switch
        {
            BankPhase.DepositList => bankService.Deposit(player, amount),
            BankPhase.WithdrawList => bankService.Withdraw(player, amount),
            BankPhase.BorrowList => bankService.Borrow(player, amount),
            _ => new BankTransactionResult(false, 0, 0, BankReturnMessage)
        };

        bankMessage = result.Message;
        if (result.Success)
        {
            PersistProgress();
        }
    }

    private void FinishBattle()
    {
        ResetEncounterCounter();
        ChangeGameState(GameState.Field);
        PersistProgress();
    }

    private bool TryTriggerRandomEncounter()
    {
        if (currentFieldMap != FieldMapId.Field)
        {
            return false;
        }

        var tileId = map[player.TilePosition.Y, player.TilePosition.X];
        if (tileId == MapFactory.FieldGateTile)
        {
            return false;
        }

        fieldEncounterStepsRemaining -= tileId == MapFactory.GrassTile ? 2 : 1;
        if (fieldEncounterStepsRemaining > 0)
        {
            return false;
        }

        StartEncounterTransition(battleService.CreateEncounter(random, currentFieldMap, player.Level));
        return true;
    }

    private void StartEncounterTransition(BattleEncounter encounter)
    {
        pendingEncounter = encounter;
        encounterTransitionFrames = EncounterTransitionDuration;
        ResetBattleSelectionState();
        ResetEncounterCounter();
        ChangeGameState(GameState.EncounterTransition);
        PlaySe(SoundEffect.Dialog);
    }

    private void UpdateBattleVisualEffects()
    {
        if (enemyHitFlashFramesRemaining > 0)
        {
            enemyHitFlashFramesRemaining--;
        }
    }

    private void ResetBattleVisualEffects()
    {
        enemyHitFlashFramesRemaining = 0;
    }

    private void ApplyBattleVisualEffects(BattleTurnResolution result)
    {
        enemyHitFlashFramesRemaining = 0;
        if (currentEncounter is null || currentEncounter.CurrentHp <= 0)
        {
            return;
        }

        foreach (var step in result.Steps)
        {
            if (step.VisualCue == BattleVisualCue.EnemyHit)
            {
                enemyHitFlashFramesRemaining = Math.Max(enemyHitFlashFramesRemaining, step.AnimationFrames);
            }
        }
    }
}
