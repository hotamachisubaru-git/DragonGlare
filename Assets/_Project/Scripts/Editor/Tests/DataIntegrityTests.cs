using UnityEngine;
using NUnit.Framework;

namespace DragonGlare.Tests
{
    public class DataIntegrityTests
    {
        [Test]
        public void SaveData_Serialization_ShouldPreserveAllFields()
        {
            var original = new SaveData
            {
                Name = "TestPlayer",
                Level = 50,
                Experience = 12345,
                CurrentHp = 100,
                MaxHp = 200,
                CurrentMp = 50,
                MaxMp = 100,
                Gold = 99999,
                BankGold = 50000,
                LoanBalance = 10000,
                CurrentFieldMap = FieldMapId.Castle,
                TilePosition = new Vector2Int(10, 20),
                Language = UiLanguage.English
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original);
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(json);

            Assert.AreEqual(original.Name, deserialized.Name);
            Assert.AreEqual(original.Level, deserialized.Level);
            Assert.AreEqual(original.Experience, deserialized.Experience);
            Assert.AreEqual(original.CurrentHp, deserialized.CurrentHp);
            Assert.AreEqual(original.MaxHp, deserialized.MaxHp);
            Assert.AreEqual(original.Gold, deserialized.Gold);
            Assert.AreEqual(original.BankGold, deserialized.BankGold);
            Assert.AreEqual(original.LoanBalance, deserialized.LoanBalance);
            Assert.AreEqual(original.CurrentFieldMap, deserialized.CurrentFieldMap);
            Assert.AreEqual(original.TilePosition, deserialized.TilePosition);
            Assert.AreEqual(original.Language, deserialized.Language);
        }

        [Test]
        public void PlayerProgress_Clone_ShouldCreateIndependentCopy()
        {
            var original = PlayerProgress.CreateDefault(new Vector2Int(3, 12));
            original.Name = "Original";
            original.Level = 10;
            original.Gold = 1000;

            var clone = original.Clone();

            // Modify clone
            clone.Name = "Clone";
            clone.Level = 20;
            clone.Gold = 2000;

            // Original should be unchanged
            Assert.AreEqual("Original", original.Name);
            Assert.AreEqual(10, original.Level);
            Assert.AreEqual(1000, original.Gold);
        }

        [Test]
        public void GameSession_StateIsolation_ShouldNotAffectOtherSessions()
        {
            var session1 = new GameObject().AddComponent<GameSession>();
            var session2 = new GameObject().AddComponent<GameSession>();

            session1.Initialize();
            session2.Initialize();

            session1.Player.Name = "Session1";
            session2.Player.Name = "Session2";

            Assert.AreEqual("Session1", session1.Player.Name);
            Assert.AreEqual("Session2", session2.Player.Name);
        }
    }
}
