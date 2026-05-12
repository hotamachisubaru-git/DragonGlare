using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DragonGlare.Domain.Player;

namespace DragonGlare.Persistence
{
    public static class SaveDataMapper
    {
        public static SaveData ToSaveData(PlayerProgress player, UiLanguage language, FieldMapId currentFieldMap)
        {
            return new SaveData
            {
                Name = player.Name,
                Level = player.Level,
                Experience = player.Experience,
                CurrentHp = player.CurrentHp,
                MaxHp = player.MaxHp,
                CurrentMp = player.CurrentMp,
                MaxMp = player.MaxMp,
                Gold = player.Gold,
                BankGold = player.BankGold,
                LoanBalance = player.LoanBalance,
                CurrentFieldMap = currentFieldMap,
                TilePosition = player.TilePosition,
                FacingDirection = player.FacingDirection,
                EquippedWeaponId = player.EquippedWeapon?.ItemId,
                EquippedArmorId = player.EquippedArmor?.ItemId,
                EquippedHeadId = player.EquippedHead?.ItemId,
                EquippedArmsId = player.EquippedArms?.ItemId,
                EquippedLegsId = player.EquippedLegs?.ItemId,
                EquippedFeetId = player.EquippedFeet?.ItemId,
                Spells = new List<DragonGlare.Domain.Battle.SpellDefinition>(player.Spells),
                Inventory = new List<InventoryEntry>(player.Inventory),
                Language = language,
                SavedAtUtc = System.DateTime.UtcNow
            };
        }

        public static PlayerProgress ToPlayerProgress(SaveData saveData)
        {
            var player = new PlayerProgress
            {
                Name = saveData.Name,
                Level = saveData.Level,
                Experience = saveData.Experience,
                CurrentHp = saveData.CurrentHp,
                MaxHp = saveData.MaxHp,
                CurrentMp = saveData.CurrentMp,
                MaxMp = saveData.MaxMp,
                Gold = saveData.Gold,
                BankGold = saveData.BankGold,
                LoanBalance = saveData.LoanBalance,
                TilePosition = saveData.TilePosition,
                FacingDirection = saveData.FacingDirection,
                Spells = new List<DragonGlare.Domain.Battle.SpellDefinition>(saveData.Spells),
                Inventory = new List<InventoryEntry>(saveData.Inventory)
            };

            // Restore equipment references from inventory
            foreach (var item in player.Inventory)
            {
                if (item.Item is DragonGlare.Domain.Player.WeaponDefinition weapon && weapon.ItemId == saveData.EquippedWeaponId)
                    player.EquippedWeapon = weapon;
                else if (item.Item is DragonGlare.Domain.Player.ArmorDefinition armor)
                {
                    if (armor.ItemId == saveData.EquippedArmorId)
                        player.EquippedArmor = armor;
                    else if (armor.ItemId == saveData.EquippedHeadId)
                        player.EquippedHead = armor;
                    else if (armor.ItemId == saveData.EquippedArmsId)
                        player.EquippedArms = armor;
                    else if (armor.ItemId == saveData.EquippedLegsId)
                        player.EquippedLegs = armor;
                    else if (armor.ItemId == saveData.EquippedFeetId)
                        player.EquippedFeet = armor;
                }
            }

            return player;
        }
    }
}
