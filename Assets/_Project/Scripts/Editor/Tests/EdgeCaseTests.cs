using UnityEngine;
using NUnit.Framework;
using DragonGlare.Domain;

namespace DragonGlare.Tests
{
    public class EdgeCaseTests
    {
        [Test]
        public void PlayerProgress_TakeDamage_WhenHpIsZero()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.CurrentHp = 0;
            player.TakeDamage(10);
            Assert.AreEqual(0, player.CurrentHp);
        }

        [Test]
        public void PlayerProgress_Heal_WhenHpIsMax()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.CurrentHp = player.MaxHp;
            player.Heal(10);
            Assert.AreEqual(player.MaxHp, player.CurrentHp);
        }

        [Test]
        public void BattleService_CreateEncounter_HighLevelPlayer()
        {
            var battleService = new BattleService();
            var random = new System.Random();
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.Level = 99;

            var encounter = battleService.CreateEncounter(random, FieldMapId.Field, player.Level);

            Assert.IsNotNull(encounter);
            Assert.IsNotNull(encounter.Enemy);
        }

        [Test]
        public void SaveManager_TryLoadSlot_EmptySlot()
        {
            var saveManager = new GameObject().AddComponent<SaveManager>();
            var success = saveManager.TryLoadSlot(1, out var saveData);

            Assert.IsFalse(success);
            Assert.IsNull(saveData);
        }

        [Test]
        public void GameSession_ChangeGameState_SameState()
        {
            var session = new GameObject().AddComponent<GameSession>();
            session.Initialize();

            session.ChangeGameState(GameState.ModeSelect);
            Assert.IsFalse(session.PendingGameState.HasValue);
        }

        [Test]
        public void InputManager_WasPressed_InvalidKey()
        {
            var inputManager = new GameObject().AddComponent<InputManager>();
            inputManager.PollInput();

            Assert.IsFalse(inputManager.WasPressed((KeyCode)9999));
        }

        [Test]
        public void ProtectedInt_DivisionByZero()
        {
            var a = new ProtectedInt(10);
            var b = new ProtectedInt(0);

            Assert.Throws<System.DivideByZeroException>(() =>
            {
                var result = a / b;
                var value = result.Value; // Force evaluation
            });
        }

        [Test]
        public void ShopService_PurchaseProduct_InsufficientGold()
        {
            var shopService = new ShopService();
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.Gold = 0;

            var product = new DragonGlare.Domain.Commerce.ShopProductDefinition
            {
                Price = 100,
                Name = "TestItem"
            };

            var result = shopService.PurchaseProduct(player, product);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void BankService_Borrow_MaxCreditReached()
        {
            var bankService = new BankService();
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.LoanBalance = 999999;

            var result = bankService.Borrow(player, 1000);

            Assert.IsFalse(result.Success);
        }
    }
}