using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DragonGlare.Data;
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
                EquippedWeaponId = player.EquippedWeaponId,
                EquippedArmorId = player.EquippedArmorId,
                EquippedHeadId = player.EquippedHeadId,
                EquippedArmsId = player.EquippedArmsId,
                EquippedLegsId = player.EquippedLegsId,
                EquippedFeetId = player.EquippedFeetId,
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
                EquippedWeaponId = saveData.EquippedWeaponId,
                EquippedArmorId = saveData.EquippedArmorId,
                EquippedHeadId = saveData.EquippedHeadId,
                EquippedArmsId = saveData.EquippedArmsId,
                EquippedLegsId = saveData.EquippedLegsId,
                EquippedFeetId = saveData.EquippedFeetId,
                Spells = new List<DragonGlare.Domain.Battle.SpellDefinition>(saveData.Spells ?? []),
                Inventory = new List<InventoryEntry>(saveData.Inventory ?? [])
            };

            // Ensure equipped items exist in inventory
            EnsureHasItem(player, saveData.EquippedWeaponId);
            EnsureHasItem(player, saveData.EquippedArmorId);
            EnsureHasItem(player, saveData.EquippedHeadId);
            EnsureHasItem(player, saveData.EquippedArmsId);
            EnsureHasItem(player, saveData.EquippedLegsId);
            EnsureHasItem(player, saveData.EquippedFeetId);

            return player;
        }

        private static void EnsureHasItem(PlayerProgress player, string? itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || player.GetItemCount(itemId) > 0) return;
            player.AddItem(itemId, 1);
        }
    }
}
