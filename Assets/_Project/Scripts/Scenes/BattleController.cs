using UnityEngine;
using DragonGlare.Domain.Battle;

namespace DragonGlare
{
    public class BattleController : SceneControllerBase
    {
        [SerializeField] private BattleScene scene;

        public override void OnEnter()
        {
            UpdateScene();
        }

        public override void OnUpdate()
        {
            if (Session.CurrentEncounter == null)
            {
                Session.ResetBattleState();
                Session.ChangeGameState(GameState.Field);
                return;
            }

            if (Session.BattleFlowState == BattleFlowState.Intro)
            {
                UpdateIntro();
                return;
            }

            if (Session.BattleFlowState == BattleFlowState.Resolving)
            {
                UpdateResolutionSequence();
                return;
            }

            if (Session.BattleFlowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
            {
                UpdateSelectionMenu();
                return;
            }

            if (Session.BattleFlowState != BattleFlowState.CommandSelection)
            {
                if (Input.WasConfirmPressed() || Input.WasPressed(KeyCode.Escape))
                    FinishBattle();
                return;
            }

            UpdateCommandCursor();

            if (Input.WasPressed(KeyCode.Escape))
            {
                var escapeResult = Session.BattleService.ResolveTurn(Session.Player, Session.CurrentEncounter, BattleActionType.Run, null, null, Session.Random);
                ApplyBattleResolution(escapeResult);
                return;
            }

            if (!Input.WasConfirmPressed())
                return;

            var action = GameContent.BattleCommandGrid[Session.BattleCursorRow, Session.BattleCursorColumn];
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

            var result = Session.BattleService.ResolveTurn(Session.Player, Session.CurrentEncounter, action, null, null, Session.Random);
            ApplyBattleResolution(result);
        }

        private void UpdateIntro()
        {
            if (Session.BattleIntroFramesRemaining > 0)
                Session.BattleIntroFramesRemaining--;
            if (Session.BattleIntroFramesRemaining <= 0 || Input.WasConfirmPressed())
            {
                Session.BattleIntroFramesRemaining = 0;
                Session.BattleFlowState = BattleFlowState.CommandSelection;
                Session.BattleMessage = Session.GetBattleOpeningCommandMessage();
            }
        }

        private void UpdateCommandCursor()
        {
            var previousRow = Session.BattleCursorRow;
            var previousColumn = Session.BattleCursorColumn;
            if (Input.WasPressed(KeyCode.UpArrow) || Input.WasPressed(KeyCode.W))
                Session.BattleCursorRow = Mathf.Max(0, Session.BattleCursorRow - 1);
            else if (Input.WasPressed(KeyCode.DownArrow) || Input.WasPressed(KeyCode.S))
                Session.BattleCursorRow = Mathf.Min(Session.GetBattleCommandRowCount() - 1, Session.BattleCursorRow + 1);
            else if (Input.WasPressed(KeyCode.LeftArrow) || Input.WasPressed(KeyCode.A))
                Session.BattleCursorColumn = Mathf.Max(0, Session.BattleCursorColumn - 1);
            else if (Input.WasPressed(KeyCode.RightArrow) || Input.WasPressed(KeyCode.D))
                Session.BattleCursorColumn = Mathf.Min(Session.GetBattleCommandColumnCount() - 1, Session.BattleCursorColumn + 1);

            if (previousRow != Session.BattleCursorRow || previousColumn != Session.BattleCursorColumn)
                PlayCursorSe();
        }

        private void UpdateSelectionMenu()
        {
            var entries = Session.GetActiveBattleSelectionEntries();
            if (entries.Count == 0)
            {
                CloseBattleSelectionMenu(Session.GetBattleEmptySelectionMessage(Session.BattleFlowState));
                return;
            }

            if (Input.WasPressed(KeyCode.UpArrow) || Input.WasPressed(KeyCode.W))
                MoveBattleSelectionCursor(-1, entries.Count);
            else if (Input.WasPressed(KeyCode.DownArrow) || Input.WasPressed(KeyCode.S))
                MoveBattleSelectionCursor(1, entries.Count);

            if (Input.WasBattleSubmenuBackPressed())
            {
                PlayCancelSe();
                CloseBattleSelectionMenu();
                return;
            }

            if (!Input.WasBattleSubmenuConfirmPressed() || Session.CurrentEncounter == null)
                return;

            var selectedEntry = entries[Session.BattleListCursor];
            var action = Session.BattleFlowState switch
            {
                BattleFlowState.SpellSelection => BattleActionType.Spell,
                BattleFlowState.ItemSelection => BattleActionType.Item,
                _ => BattleActionType.Equip
            };
            var result = Session.BattleService.ResolveTurn(Session.Player, Session.CurrentEncounter, action, selectedEntry.Spell, selectedEntry.Consumable, selectedEntry.Equipment, Session.Random);
            ApplyBattleResolution(result);
        }

        private void MoveBattleSelectionCursor(int delta, int itemCount)
        {
            var previousCursor = Session.BattleListCursor;
            Session.BattleListCursor = Mathf.Clamp(Session.BattleListCursor + delta, 0, itemCount - 1);
            if (previousCursor != Session.BattleListCursor)
                PlayCursorSe();
            if (Session.BattleListCursor < Session.BattleListScroll)
                Session.BattleListScroll = Session.BattleListCursor;
            else if (Session.BattleListCursor >= Session.BattleListScroll + GameConstants.BattleSelectionVisibleRows)
                Session.BattleListScroll = Session.BattleListCursor - GameConstants.BattleSelectionVisibleRows + 1;
            Session.BattleMessage = Session.GetBattleSelectionMessage(Session.BattleFlowState);
        }

        private void OpenBattleSelectionMenu(BattleFlowState nextState)
        {
            Session.BattleFlowState = nextState;
            Session.BattleListCursor = 0;
            Session.BattleListScroll = 0;
            var entries = Session.GetActiveBattleSelectionEntries();
            if (entries.Count == 0)
            {
                CloseBattleSelectionMenu(Session.GetBattleEmptySelectionMessage(nextState));
                return;
            }
            Session.BattleMessage = Session.GetBattleSelectionMessage(nextState);
        }

        private void CloseBattleSelectionMenu(string message = null)
        {
            Session.BattleFlowState = BattleFlowState.CommandSelection;
            Session.BattleListCursor = 0;
            Session.BattleListScroll = 0;
            Session.BattleMessage = message ?? Session.GetBattleCommandHelpMessage();
        }

        private void UpdateResolutionSequence()
        {
            if (Session.BattleResolutionStepFramesRemaining > 0)
            {
                Session.BattleResolutionStepFramesRemaining--;
                return;
            }

            Session.BattleResolutionStepIndex++;
            if (Session.BattleResolutionStepIndex >= Session.BattleResolutionSteps.Count)
            {
                Session.BattleFlowState = Session.ActiveBattleResolution?.PlayerWon == true ? BattleFlowState.Victory :
                    Session.ActiveBattleResolution?.PlayerEscaped == true ? BattleFlowState.Escape : BattleFlowState.Defeat;
                Session.BattleMessage = Session.ActiveBattleResolution?.SummaryMessage ?? Session.BattleMessage;
                Session.ResetBattleVisualEffects();
                return;
            }

            var step = Session.BattleResolutionSteps[Session.BattleResolutionStepIndex];
            Session.BattleMessage = step.Message;
            Session.BattleResolutionStepFramesRemaining = Mathf.Max(GameConstants.BattleStepMinimumFrames, step.Message.Length / 2 + GameConstants.BattleStepMessageHoldFrames);
            ApplyBattleVisualCue(step.VisualCue);
        }

        private void ApplyBattleResolution(BattleTurnResolution resolution)
        {
            Session.ActiveBattleResolution = resolution;
            Session.BattleResolutionSteps = resolution.Steps;
            Session.BattleResolutionStepIndex = -1;
            Session.BattleResolutionStepFramesRemaining = 0;
            Session.BattleFlowState = BattleFlowState.Resolving;
            Session.BattleReturnFlowState = BattleFlowState.CommandSelection;
            UpdateResolutionSequence();
        }

        private void ApplyBattleVisualCue(BattleVisualCue cue)
        {
            switch (cue)
            {
                case BattleVisualCue.PlayerAction:
                    Session.BattlePlayerActionFramesRemaining = 8;
                    break;
                case BattleVisualCue.SpellBurst:
                    Session.BattleSpellEffectFramesRemaining = 16;
                    break;
                case BattleVisualCue.StatusCloud:
                    Session.BattleStatusEffectFramesRemaining = 16;
                    break;
                case BattleVisualCue.PlayerHeal:
                    Session.BattlePlayerHealFramesRemaining = 16;
                    break;
                case BattleVisualCue.PlayerGuard:
                    Session.BattlePlayerGuardFramesRemaining = 16;
                    break;
                case BattleVisualCue.ItemUse:
                    Session.BattleItemUseFramesRemaining = 14;
                    break;
                case BattleVisualCue.EnemyDefeat:
                    Session.BattleEnemyDefeatFramesRemaining = 16;
                    break;
                case BattleVisualCue.EnemyHitFlash:
                    Session.EnemyHitFlashFramesRemaining = 8;
                    break;
                case BattleVisualCue.PlayerHitFlash:
                    Session.PlayerHitFlashFramesRemaining = 8;
                    break;
            }
        }

        private void FinishBattle()
        {
            Session.ResetEncounterCounter();
            Session.ChangeGameState(GameState.Field);
            Session.PersistProgress();
        }

        private void UpdateScene()
        {
            scene?.Show(Session.Player, Session.CurrentEncounter, Session.BattleFlowState,
                Session.BattleCursorRow, Session.BattleCursorColumn, Session.BattleListCursor,
                Session.BattleListScroll, Session.BattleMessage, Session.BattleResolutionSteps,
                Session.BattleResolutionStepIndex, Session.SelectedLanguage, Session.FrameCounter,
                Session.BattlePlayerActionFramesRemaining, Session.BattlePlayerGuardFramesRemaining,
                Session.BattleEnemyActionFramesRemaining, Session.BattleItemUseFramesRemaining,
                Session.BattleEnemyDefeatFramesRemaining, Session.EnemyHitFlashFramesRemaining,
                Session.BattleSpellEffectFramesRemaining, Session.PlayerHitFlashFramesRemaining,
                Session.BattlePlayerHealFramesRemaining, Session.BattleStatusEffectFramesRemaining,
                Session.BattleIntroFramesRemaining);
        }
    }
}
