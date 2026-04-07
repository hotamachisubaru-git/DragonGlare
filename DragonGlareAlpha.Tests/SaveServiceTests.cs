using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Persistence;

namespace DragonGlareAlpha.Tests;

public sealed class SaveServiceTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsExpandedProgress()
    {
        var service = new SaveService();
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        try
        {
            var save = new SaveData
            {
                Language = "ja",
                Name = "テスター",
                PlayerX = 7,
                PlayerY = 8,
                Level = 3,
                Experience = 29,
                MaxHp = 30,
                CurrentHp = 24,
                MaxMp = 6,
                CurrentMp = 5,
                BaseAttack = 10,
                BaseDefense = 7,
                Gold = 123,
                EquippedWeaponId = "bronze_sword",
                Inventory =
                [
                    new InventoryEntry
                    {
                        ItemId = "bronze_sword",
                        Quantity = 1
                    }
                ]
            };

            service.Save(tempPath, save);
            var loaded = service.TryLoad(tempPath, out var roundTripped);

            Assert.True(loaded);
            Assert.NotNull(roundTripped);
            Assert.Equal(3, roundTripped!.Level);
            Assert.Equal("bronze_sword", roundTripped.EquippedWeaponId);
            Assert.Single(roundTripped.Inventory);
            Assert.Equal(123, roundTripped.Gold);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
