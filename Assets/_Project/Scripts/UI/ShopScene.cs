using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain.Player;
using System.Collections.Generic;
using System.Linq;

namespace DragonGlare
{
    public class ShopScene : MonoBehaviour
    {
        [SerializeField] private Text helpText;
        [SerializeField] private Transform listRoot;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private Text listTitle;
        [SerializeField] private Text pageText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text messageText;
        [SerializeField] private RectTransform cursor;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private Text[] equipmentSlots;
        [SerializeField] private Text atkDefText;
        [SerializeField] private Text detailLine1;
        [SerializeField] private Text detailLine2;

        private List<ShopListItem> listItems = new();

        public void Show(PlayerProgress player, ShopPhase phase, int promptCursor, int itemCursor, int pageIndex, string message, UiLanguage language)
        {
            gameObject.SetActive(true);
            messageText.text = message;
            goldText.text = $"G {player.Gold}";

            if (phase == ShopPhase.Welcome)
            {
                ShowWelcome(language, promptCursor);
                return;
            }

            ShowList(player, phase, itemCursor, pageIndex, language);
            UpdateInfoPanel(player, phase, itemCursor, pageIndex, language);
        }

        private void ShowWelcome(UiLanguage language, int promptCursor)
        {
            listRoot.gameObject.SetActive(false);
            infoPanel.SetActive(false);
            helpText.gameObject.SetActive(true);

            var options = language == UiLanguage.English
                ? new[] { "BUY", "SELL", "LEAVE" }
                : new[] { "かう", "うる", "やめる" };

            helpText.text = string.Join("\n", options.Select((o, i) => i == promptCursor ? $"> {o}" : $"  {o}"));
        }

        private void ShowList(PlayerProgress player, ShopPhase phase, int itemCursor, int pageIndex, UiLanguage language)
        {
            listRoot.gameObject.SetActive(true);
            infoPanel.SetActive(true);
            helpText.gameObject.SetActive(false);

            listTitle.text = phase == ShopPhase.SellList
                ? (language == UiLanguage.English ? "SELL" : "うるもの")
                : (language == UiLanguage.English ? "CATALOG" : "いちらん");

            var entries = GetVisibleEntries(player, phase, pageIndex);
            EnsureListItems(entries.Count);

            for (int i = 0; i < entries.Count; i++)
            {
                listItems[i].Show(entries[i], language, i == itemCursor);
            }

            if (itemCursor >= 0 && itemCursor < listItems.Count)
            {
                cursor.position = listItems[itemCursor].transform.position;
            }

            var totalPages = Mathf.Max(1, Mathf.CeilToInt(GetProducts().Count / (float)GameConstants.ShopItemsPerPage));
            pageText.text = $"{pageIndex + 1}/{totalPages}";
        }

        private void UpdateInfoPanel(PlayerProgress player, ShopPhase phase, int itemCursor, int pageIndex, UiLanguage language)
        {
            var entries = GetVisibleEntries(player, phase, pageIndex);
            var selected = itemCursor >= 0 && itemCursor < entries.Count ? entries[itemCursor] : null;

            var labels = language == UiLanguage.English
                ? new[] { "WPN", "HD", "ARM", "ARM", "LEG", "FT" }
                : new[] { "ぶき", "あたま", "よろい", "うで", "あし", "くつ" };
            var slots = new[] { EquipmentSlot.Weapon, EquipmentSlot.Head, EquipmentSlot.Armor, EquipmentSlot.Arms, EquipmentSlot.Legs, EquipmentSlot.Feet };
            for (int i = 0; i < equipmentSlots.Length && i < slots.Length; i++)
            {
                var name = player.GetEquippedItemName(slots[i]) ?? (language == UiLanguage.English ? "None" : "なし");
                equipmentSlots[i].text = $"{labels[i]}: {name}";
            }

            var battleService = new DragonGlare.Services.BattleService();
            atkDefText.text = $"ATK {battleService.GetPlayerAttack(player, player.EquippedWeapon)}  DEF {battleService.GetPlayerDefense(player, player.EquippedArmor)}";

            if (selected?.Product != null)
            {
                detailLine1.text = selected.Product.IsEquipment && selected.Product.Equipment != null
                    ? $"{GetSlotLabel(selected.Product.Equipment.Slot, language)}: {player.GetEquippedItemName(selected.Product.Equipment.Slot) ?? (language == UiLanguage.English ? "None" : "なし")}"
                    : $"LV {player.Level}";
                detailLine2.text = selected.Product.Equipment is DragonGlare.Domain.Player.WeaponDefinition weapon
                    ? (language == UiLanguage.English ? $"EQUIP ATK {battleService.GetPlayerAttack(player, weapon)}" : $"そうびで ATK {battleService.GetPlayerAttack(player, weapon)}")
                    : selected.Product.Equipment is DragonGlare.Domain.Player.ArmorDefinition armor
                    ? (language == UiLanguage.English ? $"EQUIP DEF {battleService.GetPlayerDefense(player, armor)}" : $"そうびで DEF {battleService.GetPlayerDefense(player, armor)}")
                    : $"EXP {player.Experience}";
            }
            else
            {
                detailLine1.text = $"LV {player.Level}";
                detailLine2.text = $"EXP {player.Experience}";
            }
        }

        private void EnsureListItems(int count)
        {
            while (listItems.Count < count)
            {
                var go = Instantiate(listItemPrefab, listRoot);
                listItems.Add(go.GetComponent<ShopListItem>());
            }
            for (int i = 0; i < listItems.Count; i++)
            {
                listItems[i].gameObject.SetActive(i < count);
            }
        }

        private IReadOnlyList<DragonGlare.Domain.Commerce.ShopProductDefinition> GetProducts()
        {
            var shopService = new DragonGlare.Services.ShopService();
            return shopService.GetProductsForField(GameManager.Instance.SceneUI.CurrentFieldMap);
        }

        private List<ShopMenuEntry> GetVisibleEntries(PlayerProgress player, ShopPhase phase, int pageIndex)
        {
            var entries = new List<ShopMenuEntry>();
            var products = GetProducts();
            var inventory = player.Inventory;

            if (phase == ShopPhase.SellList)
            {
                foreach (var item in inventory)
                {
                    entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.InventoryItem, InventoryItem = item });
                }
            }
            else
            {
                var pageProducts = products.Skip(pageIndex * GameConstants.ShopItemsPerPage).Take(GameConstants.ShopItemsPerPage);
                foreach (var product in pageProducts)
                {
                    entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.Product, Product = product });
                }
            }

            if (pageIndex > 0)
                entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.PreviousPage });
            if (pageIndex < Mathf.Max(1, Mathf.CeilToInt(products.Count / (float)GameConstants.ShopItemsPerPage)) - 1)
                entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.NextPage });
            entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.Quit });

            return entries;
        }

        private static string GetSlotLabel(EquipmentSlot slot, UiLanguage language)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => language == UiLanguage.English ? "WPN" : "ぶき",
                EquipmentSlot.Head => language == UiLanguage.English ? "HD" : "あたま",
                EquipmentSlot.Armor => language == UiLanguage.English ? "ARM" : "よろい",
                EquipmentSlot.Arms => language == UiLanguage.English ? "ARM" : "うで",
                EquipmentSlot.Legs => language == UiLanguage.English ? "LEG" : "あし",
                EquipmentSlot.Feet => language == UiLanguage.English ? "FT" : "くつ",
                _ => string.Empty
            };
        }
    }
}
