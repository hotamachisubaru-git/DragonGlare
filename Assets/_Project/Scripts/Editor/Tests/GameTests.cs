using UnityEngine;
using NUnit.Framework;
using DragonGlare.Domain.Player;
using DragonGlare.Domain.Battle;
using DragonGlare.Persistence;

namespace DragonGlare.Tests
{
    public class PlayerProgressTests
    {
        [Test]
        public void CreateDefault_ShouldInitializeWithCorrectValues()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            Assert.AreEqual(1, player.Level);
            Assert.AreEqual(20, player.MaxHp);
            Assert.AreEqual(20, player.CurrentHp);
            Assert.AreEqual(10, player.MaxMp);
            Assert.AreEqual(10, player.CurrentMp);
            Assert.AreEqual(0, player.Gold);
        }

        [Test]
        public void TakeDamage_ShouldReduceHp()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.CurrentHp = 20;
            player.TakeDamage(5);
            Assert.AreEqual(15, player.CurrentHp);
        }

        [Test]
        public void Heal_ShouldIncreaseHp()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.CurrentHp = 10;
            player.Heal(5);
            Assert.AreEqual(15, player.CurrentHp);
        }

        [Test]
        public void Heal_ShouldNotExceedMaxHp()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.CurrentHp = 18;
            player.Heal(5);
            Assert.AreEqual(20, player.CurrentHp);
        }
    }

    public class BattleServiceTests
    {
        private BattleService battleService;
        private System.Random random;

        [SetUp]
        public void Setup()
        {
            battleService = new BattleService();
            random = new System.Random(42);
        }

        [Test]
        public void GetPlayerAttack_ShouldIncludeWeaponBonus()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            var weapon = new WeaponDefinition { AttackBonus = 5 };
            var attack = battleService.GetPlayerAttack(player, weapon);
            Assert.AreEqual(player.BaseAttack + 5, attack);
        }

        [Test]
        public void GetPlayerDefense_ShouldIncludeArmorBonus()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            var armor = new ArmorDefinition { DefenseBonus = 3 };
            var defense = battleService.GetPlayerDefense(player, armor);
            Assert.AreEqual(player.BaseDefense + 3, defense);
        }
    }

    public class SaveDataMapperTests
    {
        [Test]
        public void ToSaveData_ShouldPreserveAllValues()
        {
            var player = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            player.Name = "Test";
            player.Level = 5;
            player.Gold = 100;

            var saveData = SaveDataMapper.ToSaveData(player, UiLanguage.Japanese, FieldMapId.Hub);

            Assert.AreEqual("Test", saveData.Name);
            Assert.AreEqual(5, saveData.Level);
            Assert.AreEqual(100, saveData.Gold);
            Assert.AreEqual(FieldMapId.Hub, saveData.CurrentFieldMap);
        }

        [Test]
        public void ToPlayerProgress_ShouldRestoreAllValues()
        {
            var original = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            original.Name = "Test";
            original.Level = 5;
            original.Gold = 100;

            var saveData = SaveDataMapper.ToSaveData(original, UiLanguage.Japanese, FieldMapId.Hub);
            var restored = SaveDataMapper.ToPlayerProgress(saveData);

            Assert.AreEqual(original.Name, restored.Name);
            Assert.AreEqual(original.Level, restored.Level);
            Assert.AreEqual(original.Gold, restored.Gold);
        }
    }

    public class GameSessionTests
    {
        [Test]
        public void ChangeGameState_ShouldSetPendingState()
        {
            var session = new GameObject().AddComponent<GameSession>();
            session.Initialize();

            session.ChangeGameState(GameState.Battle);

            Assert.IsTrue(session.PendingGameState.HasValue);
            Assert.AreEqual(GameState.Battle, session.PendingGameState.Value);
        }

        [Test]
        public void ApplyPendingState_ShouldUpdateCurrentState()
        {
            var session = new GameObject().AddComponent<GameSession>();
            session.Initialize();

            session.ChangeGameState(GameState.Battle);
            session.ApplyPendingState();

            Assert.AreEqual(GameState.Battle, session.CurrentGameState);
            Assert.IsFalse(session.PendingGameState.HasValue);
        }
    }
}
