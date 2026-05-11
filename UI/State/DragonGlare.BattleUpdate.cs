using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Services;
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

        if (battleFlowState == BattleFlowState.Resolving)
        {
            UpdateBattleResolutionSequence();
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
                FinishBattle();
            }

            return;
        }

        UpdateBattleCommandCursor();

        if (WasPressed(Keys.Escape))
        {
            var escapeResult = battleService.ResolveTurn(player, currentEncounter, BattleActionType.Run, null, null, random);
            ApplyBattleResolution(escapeResult);
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
        var previousRow = battleCursorRow;
        var previousColumn = battleCursorColumn;
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

        if (previousRow != battleCursorRow || previousColumn != battleCursorColumn)
        {
            PlaySe(SoundEffect.Cursor);
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
            PlayCancelSe();
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
    }

    private void MoveBattleSelectionCursor(int delta, int itemCount)
    {
        var previousCursor = battleListCursor;
        battleListCursor = Math.Clamp(battleListCursor + delta, 0, itemCount - 1);
        PlayCursorSeIfChanged(previousCursor, battleListCursor);
        if (battleListCursor < battleListScroll)
        {
            battleListScroll = battleListCursor;
            return;
        }

        if (battleListCursor >= battleListScroll + BattleSelectionVisibleRows)
        {
            battleListScroll = battleListCursor - BattleSelectionVisibleRows + 1;
        }

        battleMessage = GetBattleSelectionMessage(battleFlowState);
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

        battleMessage = GetBattleSelectionMessage(nextState);
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
        battleReturnFlowState = battleFlowState;
        activeBattleResolution = result;
        var steps = result.Steps
            .SelectMany(SplitBattleStepLines)
            .ToList();
        var encounterEnemy = currentEncounter?.Enemy;

        switch (result.Outcome)
        {
            case BattleOutcome.Victory:
                if (encounterEnemy is not null)
                {
                    AppendBattleRewardSteps(
                        steps,
                        progressionService.ApplyBattleRewardsDetailed(player, encounterEnemy, random));
                }

                AppendBattleInterest(steps);
                PersistProgress();
                break;
            case BattleOutcome.Defeat:
                AppendBattleMessageSteps(
                    steps,
                    progressionService.ApplyDefeatPenalty(player, PlayerStartTile),
                    BattleVisualCue.PlayerHeal,
                    12);
                AppendBattleInterest(steps);
                SetFieldMap(FieldMapId.Hub);
                PersistProgress();
                break;
            case BattleOutcome.Escaped:
                AppendBattleInterest(steps);
                PersistProgress();
                break;
            case BattleOutcome.Invalid:
                break;
            default:
                PersistProgress();
                break;
        }

        StartBattleResolutionSequence(steps);
    }

    private void AppendBattleInterest(List<BattleSequenceStep> steps)
    {
        var addedInterest = bankService.AccrueBattleInterest(player);
        if (addedInterest <= 0)
        {
            return;
        }

        steps.Add(new BattleSequenceStep
        {
            Message = selectedLanguage == UiLanguage.English
                ? $"Loan interest increased by {addedInterest}G."
                : $"かしつけの りそくが {addedInterest}G ふえた。",
            VisualCue = BattleVisualCue.ItemUse,
            AnimationFrames = 8
        });
    }

    private static void AppendBattleRewardSteps(List<BattleSequenceStep> steps, BattleRewardResult reward)
    {
        if (!string.IsNullOrWhiteSpace(reward.RewardMessage))
        {
            AppendBattleMessageSteps(steps, reward.RewardMessage, BattleVisualCue.ItemUse, 14);
        }

        foreach (var levelUpMessage in reward.LevelUpMessages.Where(message => !string.IsNullOrWhiteSpace(message)))
        {
            AppendBattleMessageSteps(steps, levelUpMessage, BattleVisualCue.PlayerHeal, 18);
        }

        if (!string.IsNullOrWhiteSpace(reward.DropMessage))
        {
            AppendBattleMessageSteps(steps, reward.DropMessage, BattleVisualCue.ItemUse, 14);
        }
    }

    private static IEnumerable<BattleSequenceStep> SplitBattleStepLines(BattleSequenceStep step)
    {
        foreach (var line in step.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            yield return new BattleSequenceStep
            {
                Message = line,
                VisualCue = step.VisualCue,
                AnimationFrames = step.AnimationFrames,
                SoundEffect = step.SoundEffect
            };
        }
    }

    private static void AppendBattleMessageSteps(
        List<BattleSequenceStep> steps,
        string message,
        BattleVisualCue visualCue = BattleVisualCue.None,
        int animationFrames = BattleStepMinimumFrames)
    {
        foreach (var line in message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            steps.Add(new BattleSequenceStep
            {
                Message = line,
                VisualCue = visualCue,
                AnimationFrames = animationFrames
            });
        }
    }

    private void StartBattleResolutionSequence(IReadOnlyList<BattleSequenceStep> steps)
    {
        battleFlowState = BattleFlowState.Resolving;
        battleResolutionSteps = steps;
        battleResolutionStepIndex = -1;
        battleResolutionStepFramesRemaining = 0;

        if (battleResolutionSteps.Count == 0)
        {
            CompleteBattleResolutionSequence();
            return;
        }

        AdvanceBattleResolutionStep();
    }

    private void UpdateBattleResolutionSequence()
    {
        if (battleResolutionStepFramesRemaining > 0)
        {
            if (WasConfirmPressed())
            {
                battleResolutionStepFramesRemaining = 0;
                ResetBattleVisualEffects();
                AdvanceBattleResolutionStep();
                return;
            }

            battleResolutionStepFramesRemaining--;
            if (battleResolutionStepFramesRemaining <= 0)
            {
                AdvanceBattleResolutionStep();
            }

            return;
        }

        AdvanceBattleResolutionStep();
    }

    private void AdvanceBattleResolutionStep()
    {
        var nextIndex = battleResolutionStepIndex + 1;
        if (nextIndex >= battleResolutionSteps.Count)
        {
            CompleteBattleResolutionSequence();
            return;
        }

        battleResolutionStepIndex = nextIndex;
        var step = battleResolutionSteps[battleResolutionStepIndex];
        battleMessage = GetVisibleBattleResolutionMessage();
        battleResolutionStepFramesRemaining = Math.Max(BattleStepMessageHoldFrames, step.AnimationFrames);
        ResetBattleVisualEffects();
        ApplyBattleStepVisualEffect(step);
    }

    private string GetVisibleBattleResolutionMessage()
    {
        if (battleResolutionStepIndex < 0 || battleResolutionSteps.Count == 0)
        {
            return string.Empty;
        }

        var visibleStepCount = Math.Min(BattleResolutionVisibleLines, battleResolutionStepIndex + 1);
        var startIndex = Math.Max(0, battleResolutionStepIndex - visibleStepCount + 1);
        return string.Join(
            '\n',
            battleResolutionSteps
                .Skip(startIndex)
                .Take(visibleStepCount)
                .Select(step => step.Message));
    }

    private void CompleteBattleResolutionSequence()
    {
        var result = activeBattleResolution;
        ResetBattleResolutionState();
        if (result is null)
        {
            battleFlowState = BattleFlowState.CommandSelection;
            battleMessage = GetBattleCommandPromptMessage();
            return;
        }

        switch (result.Outcome)
        {
            case BattleOutcome.Victory:
            case BattleOutcome.Defeat:
            case BattleOutcome.Escaped:
                FinishBattle();
                break;
            case BattleOutcome.Invalid:
                battleFlowState = result.ActionAccepted
                    ? BattleFlowState.CommandSelection
                    : battleReturnFlowState;
                battleMessage = battleFlowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection
                    ? GetBattleSelectionMessage(battleFlowState)
                    : GetBattleCommandPromptMessage();
                break;
            default:
                battleFlowState = BattleFlowState.CommandSelection;
                battleMessage = GetBattleCommandPromptMessage();
                break;
        }
    }

    private void ResetBattleResolutionState()
    {
        battleResolutionSteps = [];
        activeBattleResolution = null;
        battleResolutionStepIndex = -1;
        battleResolutionStepFramesRemaining = 0;
        ResetBattleVisualEffects();
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
        if (battlePlayerActionFramesRemaining > 0)
        {
            battlePlayerActionFramesRemaining--;
        }

        if (battlePlayerGuardFramesRemaining > 0)
        {
            battlePlayerGuardFramesRemaining--;
        }

        if (battleEnemyActionFramesRemaining > 0)
        {
            battleEnemyActionFramesRemaining--;
        }

        if (battleItemUseFramesRemaining > 0)
        {
            battleItemUseFramesRemaining--;
        }

        if (battleEnemyDefeatFramesRemaining > 0)
        {
            battleEnemyDefeatFramesRemaining--;
        }

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
        battlePlayerActionFramesRemaining = 0;
        battlePlayerGuardFramesRemaining = 0;
        battleEnemyActionFramesRemaining = 0;
        battleItemUseFramesRemaining = 0;
        battleEnemyDefeatFramesRemaining = 0;
        enemyHitFlashFramesRemaining = 0;
        battleSpellEffectFramesRemaining = 0;
        playerHitFlashFramesRemaining = 0;
        battlePlayerHealFramesRemaining = 0;
        battleStatusEffectFramesRemaining = 0;
    }

    private void ApplyBattleStepVisualEffect(BattleSequenceStep step)
    {
        if (step.SoundEffect is { } soundEffect)
        {
            PlaySe(soundEffect);
        }

        switch (step.VisualCue)
        {
            case BattleVisualCue.PlayerAction:
                battlePlayerActionFramesRemaining = step.AnimationFrames;
                break;
            case BattleVisualCue.PlayerGuard:
                battlePlayerGuardFramesRemaining = step.AnimationFrames;
                break;
            case BattleVisualCue.EnemyAction:
                battleEnemyActionFramesRemaining = step.AnimationFrames;
                break;
            case BattleVisualCue.EnemyHit:
                enemyHitFlashFramesRemaining = Math.Max(enemyHitFlashFramesRemaining, step.AnimationFrames);
                break;
            case BattleVisualCue.SpellCast:
                battleSpellEffectFramesRemaining = Math.Max(battleSpellEffectFramesRemaining, step.AnimationFrames);
                break;
            case BattleVisualCue.PlayerHit:
                playerHitFlashFramesRemaining = Math.Max(playerHitFlashFramesRemaining, step.AnimationFrames);
                break;
            case BattleVisualCue.PlayerHeal:
            case BattleVisualCue.MpRecover:
                battlePlayerHealFramesRemaining = Math.Max(battlePlayerHealFramesRemaining, step.AnimationFrames);
                break;
            case BattleVisualCue.ItemUse:
                battleItemUseFramesRemaining = Math.Max(battleItemUseFramesRemaining, step.AnimationFrames);
                break;
            case BattleVisualCue.EnemyDefeat:
                battleEnemyDefeatFramesRemaining = Math.Max(battleEnemyDefeatFramesRemaining, step.AnimationFrames);
                break;
            case BattleVisualCue.EnemyStatus:
            case BattleVisualCue.PlayerStatus:
            case BattleVisualCue.PoisonTick:
                battleStatusEffectFramesRemaining = Math.Max(battleStatusEffectFramesRemaining, step.AnimationFrames);
                break;
        }
    }
}
