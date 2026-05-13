using UnityEngine;
using DragonGlare.Domain.Battle;

namespace DragonGlare
{
    public class EncounterTransitionController : SceneControllerBase
    {
        [SerializeField] private EncounterTransitionScene scene;

        public override void OnEnter()
        {
            scene?.Show(Session.EncounterTransitionFrames, Session.PendingEncounter, Session.SelectedLanguage);
        }

        public override void OnUpdate()
        {
            if (Session.EncounterTransitionFrames > 0)
                Session.EncounterTransitionFrames--;

            if (Session.EncounterTransitionFrames > 0)
                return;

            if (Session.PendingEncounter == null)
            {
                Session.ChangeGameState(GameState.Field);
                return;
            }

            Session.CurrentEncounter = Session.PendingEncounter;
            Session.PendingEncounter = null;
            Session.ResetBattleSelectionState();
            Session.BattleFlowState = BattleFlowState.Intro;
            Session.BattleIntroFramesRemaining = GameConstants.BattleIntroDurationFrames;
            Session.BattleMessage = Session.GetBattleEncounterMessage(GameContent.GetEnemyName(Session.CurrentEncounter.Enemy, Session.SelectedLanguage));
            Session.ChangeGameState(GameState.Battle);
        }
    }
}
