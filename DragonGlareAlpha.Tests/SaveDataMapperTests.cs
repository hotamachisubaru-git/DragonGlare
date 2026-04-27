using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Persistence;

namespace DragonGlareAlpha.Tests;

public sealed class SaveDataMapperTests
{
    [Fact]
    public void Restore_RehydratesPlayerLanguageMapAndInventory()
    {
        var saveData = new SaveData
        {
            Language = "en",
            CurrentFieldMap = FieldMapId.Field,
            Name = "Explorer",
            PlayerX = 8,
            PlayerY = 6,
            Level = 7,
            Experience = 321,
            MaxHp = 44,
            CurrentHp = 39,
            MaxMp = 15,
            CurrentMp = 11,
            BaseAttack = 13,
            BaseDefense = 9,
            Gold = 456,
            BankGold = 321,
            LoanBalance = 120,
            LoanStepCounter = 5,
            EquippedWeaponId = "bronze_sword",
            EquippedArmorId = "leather_armor",
            EquippedHeadId = "leather_cap",
            EquippedFeetId = "travel_boots",
            Inventory =
            [
                new InventoryEntry { ItemId = "bronze_sword", Quantity = 1 },
                new InventoryEntry { ItemId = "healing_herb", Quantity = 2 }
            ]
        };

        var restored = SaveDataMapper.Restore(saveData, new Point(3, 12));

        Assert.Equal(UiLanguage.English, restored.Language);
        Assert.Equal(FieldMapId.Field, restored.MapId);
        Assert.Equal("Explorer", restored.Player.Name);
        Assert.Equal(new Point(8, 6), restored.Player.TilePosition);
        Assert.Equal(7, restored.Player.Level);
        Assert.Equal("bronze_sword", restored.Player.EquippedWeaponId);
        Assert.Equal("leather_armor", restored.Player.EquippedArmorId);
        Assert.Equal("leather_cap", restored.Player.EquippedHeadId);
        Assert.Equal("travel_boots", restored.Player.EquippedFeetId);
        Assert.Equal(321, restored.Player.BankGold);
        Assert.Equal(120, restored.Player.LoanBalance);
        Assert.Equal(5, restored.Player.LoanStepCounter);
        Assert.Equal(2, restored.Player.GetItemCount("healing_herb"));
    }

    [Fact]
    public void Create_CopiesPlayerStateIntoSaveData()
    {
        var player = PlayerProgress.CreateDefault(new Point(4, 5), UiLanguage.Japanese);
        player.Name = "テスター";
        player.Level = 6;
        player.Experience = 180;
        player.MaxHp = 38;
        player.CurrentHp = 32;
        player.MaxMp = 12;
        player.CurrentMp = 10;
        player.BaseAttack = 11;
        player.BaseDefense = 8;
        player.Gold = 900;
        player.BankGold = 777;
        player.LoanBalance = 222;
        player.LoanStepCounter = 9;
        player.EquippedWeaponId = "club";
        player.EquippedHeadId = "leather_cap";
        player.EquippedFeetId = "travel_boots";
        player.Inventory =
        [
            new InventoryEntry { ItemId = "club", Quantity = 1 },
            new InventoryEntry { ItemId = "leather_cap", Quantity = 1 },
            new InventoryEntry { ItemId = "travel_boots", Quantity = 1 }
        ];

        var saveData = SaveDataMapper.Create(player, UiLanguage.Japanese, FieldMapId.Castle, 2);

        Assert.Equal("ja", saveData.Language);
        Assert.Equal("テスター", saveData.Name);
        Assert.Equal(FieldMapId.Castle, saveData.CurrentFieldMap);
        Assert.Equal(2, saveData.SlotNumber);
        Assert.Equal(4, saveData.PlayerX);
        Assert.Equal(5, saveData.PlayerY);
        Assert.Equal("club", saveData.EquippedWeaponId);
        Assert.Equal("leather_cap", saveData.EquippedHeadId);
        Assert.Equal("travel_boots", saveData.EquippedFeetId);
        Assert.Equal(777, saveData.BankGold);
        Assert.Equal(222, saveData.LoanBalance);
        Assert.Equal(9, saveData.LoanStepCounter);
        Assert.Equal(3, saveData.Inventory.Count);
    }
}
