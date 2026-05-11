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
}
