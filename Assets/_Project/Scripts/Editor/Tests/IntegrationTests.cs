using UnityEngine;
using NUnit.Framework;

namespace DragonGlare.Tests
{
    public class IntegrationTests
    {
        [Test]
        public void FullGameFlow_NewGameToField()
        {
            // Setup
            var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            var session = new GameObject("GameSession").AddComponent<GameSession>();
            session.Initialize();

            // Simulate new game flow
            session.SelectedLanguage = UiLanguage.Japanese;
            session.Player.Name = "Test";
            session.ApplyExplorationSession(session.Player, FieldMapId.Hub);

            // Verify
            Assert.AreEqual(GameState.Field, session.CurrentGameState);
            Assert.AreEqual(FieldMapId.Hub, session.CurrentFieldMap);
            Assert.AreEqual("Test", session.Player.Name);
        }

        [Test]
        public void FullGameFlow_SaveAndLoad()
        {
            var session = new GameObject("GameSession").AddComponent<GameSession>();
            var saveManager = new GameObject("SaveManager").AddComponent<SaveManager>();
            session.Initialize();

            // Setup player
            session.Player.Name = "TestSave";
            session.Player.Level = 10;
            session.Player.Gold = 5000;

            // Save
            session.SaveGame(1);

            // Load
            var success = saveManager.TryLoadSlot(1, out var saveData);

            Assert.IsTrue(success);
            Assert.AreEqual("TestSave", saveData.Name);
            Assert.AreEqual(10, saveData.Level);
            Assert.AreEqual(5000, saveData.Gold);
        }

        [Test]
        public void BattleFlow_EncounterToVictory()
        {
            var session = new GameObject("GameSession").AddComponent<GameSession>();
            session.Initialize();

            // Create encounter
            var encounter = session.BattleService.CreateEncounter(session.Random, FieldMapId.Field, 1);
            session.CurrentEncounter = encounter;
            session.BattleFlowState = BattleFlowState.Intro;

            // Simulate battle resolution
            var result = session.BattleService.ResolveTurn(
                session.Player, encounter, BattleActionType.Attack, null, null, session.Random);

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result.Steps);
        }
    }
}
