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

        if (battleFlowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
        {
            UpdateBattleSelectionMenu();
            return;
        }

        if (battleFlowState != BattleFlowState.CommandSelection)
        {
            if (WasConfirmPressed() || WasPressed(Keys.Escape))
            {
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
            battleMessage = GetBattleEscapeMessage();
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
            case BattleActionType.Spell:
                OpenBattleSelectionMenu(BattleFlowState.SpellSelection);
                return;
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
            CloseBattleSelectionMenu(GetBattleEmptySelectionMessage(battleFlowState));
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
        var action = battleFlowState switch
        {
            BattleFlowState.SpellSelection => BattleActionType.Spell,
            BattleFlowState.ItemSelection => BattleActionType.Item,
            _ => BattleActionType.Equip
        };
        var result = battleService.ResolveTurn(
            player,
            currentEncounter,
            action,
            selectedEntry.Spell,
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
            CloseBattleSelectionMenu(GetBattleEmptySelectionMessage(nextState));
            return;
        }

        battleMessage = GetBattleSelectionPromptMessage(nextState);
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

        battleMessageLines = battleMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        battleMessageVisibleLines = 1;
        battleMessageLineTimer = BattleMessageLineDelayFrames;
    }

    private void AppendBattleInterest(ref string message)
    {
        var addedInterest = bankService.AccrueBattleInterest(player);
        if (addedInterest <= 0)
        {
            return;
        }

        message += selectedLanguage == UiLanguage.English
            ? $"\nLoan interest increased by {addedInterest}G."
            : $"\nかしつけの りそくが {addedInterest}G ふえた。";
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
        battleMessage = GetBattleEncounterMessage(GameContent.GetEnemyName(currentEncounter.Enemy, selectedLanguage));
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

        if (battleSpellEffectFramesRemaining > 0)
        {
            battleSpellEffectFramesRemaining--;
        }

        if (playerHitFlashFramesRemaining > 0)
        {
            playerHitFlashFramesRemaining--;
        }

        if (battlePlayerHealFramesRemaining > 0)
        {
            battlePlayerHealFramesRemaining--;
        }

        if (battleStatusEffectFramesRemaining > 0)
        {
            battleStatusEffectFramesRemaining--;
        }
    }

    private void ResetBattleVisualEffects()
    {
        enemyHitFlashFramesRemaining = 0;
        battleSpellEffectFramesRemaining = 0;
        playerHitFlashFramesRemaining = 0;
        battlePlayerHealFramesRemaining = 0;
        battleStatusEffectFramesRemaining = 0;
    }

    private void ApplyBattleVisualEffects(BattleTurnResolution result)
    {
        enemyHitFlashFramesRemaining = 0;
        battleSpellEffectFramesRemaining = 0;
        playerHitFlashFramesRemaining = 0;
        battlePlayerHealFramesRemaining = 0;
        battleStatusEffectFramesRemaining = 0;

        foreach (var step in result.Steps)
        {
            if (step.VisualCue == BattleVisualCue.EnemyHit)
            {
                enemyHitFlashFramesRemaining = Math.Max(enemyHitFlashFramesRemaining, step.AnimationFrames);
            }

            if (step.VisualCue == BattleVisualCue.SpellCast)
            {
                battleSpellEffectFramesRemaining = Math.Max(battleSpellEffectFramesRemaining, step.AnimationFrames);
            }

            if (step.VisualCue == BattleVisualCue.PlayerHit)
            {
                playerHitFlashFramesRemaining = Math.Max(playerHitFlashFramesRemaining, step.AnimationFrames);
            }

            if (step.VisualCue is BattleVisualCue.PlayerHeal or BattleVisualCue.MpRecover)
            {
                battlePlayerHealFramesRemaining = Math.Max(battlePlayerHealFramesRemaining, step.AnimationFrames);
            }

            if (step.VisualCue is BattleVisualCue.EnemyStatus or BattleVisualCue.PlayerStatus or BattleVisualCue.PoisonTick)
            {
                battleStatusEffectFramesRemaining = Math.Max(battleStatusEffectFramesRemaining, step.AnimationFrames);
            }
        }
    }
}
