using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
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
            if (battleIntroFramesRemaining > 0)
            {
                battleIntroFramesRemaining--;
            }

            if (battleIntroFramesRemaining <= 0 || WasConfirmPressed())
            {
                battleIntroFramesRemaining = 0;
                battleFlowState = BattleFlowState.CommandSelection;
                battleMessage = GetBattleOpeningCommandMessage();
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
                battleMessage = $"{resultMessage}\n{GetBattleCommandPromptMessage()}";
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
        battleIntroFramesRemaining = BattleIntroDurationFrames;
        battleMessage = GetBattleEncounterMessage(currentEncounter.Enemy.Name);
        ChangeGameState(GameState.Battle);
    }

    private void FinishBattle()
    {
        ResetEncounterCounter();
        ChangeGameState(GameState.Field);
        PersistProgress();
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
