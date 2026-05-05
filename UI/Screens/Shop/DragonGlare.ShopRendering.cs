using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void DrawShopBuy(Graphics g)
    {
        DrawFieldScene(g);
        const int itemRowHeight = 22;
        const int listStartY = 62;

        var shopHelpRect = new Rectangle(32, 20, 242, 120);
        var shopListRect = new Rectangle(304, 20, 316, 274);
        var shopInfoRect = new Rectangle(32, 146, 242, 154);
        var shopMessageRect = new Rectangle(70, 304, 498, 140);
        var visibleEntries = GetShopVisibleEntries();

        DrawWindow(g, shopHelpRect);
        if (shopPhase == ShopPhase.Welcome)
        {
            DrawOption(g, shopPromptCursor == 0, 84, 44, selectedLanguage == UiLanguage.English ? "BUY" : "かう");
            DrawOption(g, shopPromptCursor == 1, 84, 72, selectedLanguage == UiLanguage.English ? "SELL" : "うる");
            DrawOption(g, shopPromptCursor == 2, 84, 100, selectedLanguage == UiLanguage.English ? "LEAVE" : "やめる");
        }
        else
        {
            DrawText(g, selectedLanguage == UiLanguage.English ? "D-PAD/LS: CHOOSE" : "十字/LS: せんたく", new Rectangle(54, 50, 188, 24), smallFont);
            DrawText(g, selectedLanguage == UiLanguage.English ? (shopPhase == ShopPhase.BuyList ? "A/Y/Z: BUY" : "A/Y/Z: SELL") : (shopPhase == ShopPhase.BuyList ? "A/Y/Z: こうにゅう" : "A/Y/Z: ばいきゃく"), new Rectangle(54, 78, 188, 24), smallFont);
            DrawText(g, selectedLanguage == UiLanguage.English ? "B/X/ESC: BACK" : "B/X/ESC: もどる", new Rectangle(54, 106, 188, 24), smallFont);
        }

        DrawWindow(g, shopListRect);
        DrawText(g, selectedLanguage == UiLanguage.English ? (shopPhase == ShopPhase.SellList ? "SELL" : "CATALOG") : (shopPhase == ShopPhase.SellList ? "うるもの" : "いちらん"), new Rectangle(shopListRect.X + 20, 34, 120, 24), smallFont);
        DrawText(g, "ATK", new Rectangle(shopListRect.X + 142, 34, 38, 24), smallFont, StringAlignment.Center);
        DrawText(g, "DEF", new Rectangle(shopListRect.X + 180, 34, 38, 24), smallFont, StringAlignment.Center);
        DrawText(g, "G", new Rectangle(shopListRect.X + 220, 34, 44, 24), smallFont, StringAlignment.Center);
        DrawText(g, "OWN", new Rectangle(shopListRect.X + 266, 34, 34, 24), smallFont, StringAlignment.Center);

        for (var i = 0; i < visibleEntries.Count; i++)
        {
            var entry = visibleEntries[i];
            var rowY = listStartY + (i * itemRowHeight);
            if (shopPhase != ShopPhase.Welcome && shopItemCursor == i)
            {
                DrawSelectionMarker(g, shopListRect.X + 12, rowY + 7);
            }

            if (entry.Type == ShopMenuEntryType.Product && entry.Product is not null)
            {
                var item = entry.Product;
                DrawText(g, item.Name, new Rectangle(shopListRect.X + 36, rowY, 106, 20), smallFont);
                DrawText(g, item.AttackBonus > 0 ? $"+{item.AttackBonus}" : "-", new Rectangle(shopListRect.X + 142, rowY, 38, 20), smallFont, StringAlignment.Center);
                DrawText(g, item.DefenseBonus > 0 ? $"+{item.DefenseBonus}" : "-", new Rectangle(shopListRect.X + 180, rowY, 38, 20), smallFont, StringAlignment.Center);
                DrawText(g, item.Price.ToString(), new Rectangle(shopListRect.X + 220, rowY, 44, 20), smallFont, StringAlignment.Center);
                DrawText(g, player.GetItemCount(item.Id).ToString(), new Rectangle(shopListRect.X + 266, rowY, 34, 20), smallFont, StringAlignment.Center);
                continue;
            }

            if (entry.Type == ShopMenuEntryType.InventoryItem && entry.InventoryItem is not null)
            {
                var item = entry.InventoryItem.Value;
                DrawText(g, item.Name, new Rectangle(shopListRect.X + 36, rowY, 106, 20), smallFont);
                DrawText(g, item.AttackBonus > 0 ? $"+{item.AttackBonus}" : "-", new Rectangle(shopListRect.X + 142, rowY, 38, 20), smallFont, StringAlignment.Center);
                DrawText(g, item.DefenseBonus > 0 ? $"+{item.DefenseBonus}" : "-", new Rectangle(shopListRect.X + 180, rowY, 38, 20), smallFont, StringAlignment.Center);
                DrawText(g, item.Price.ToString(), new Rectangle(shopListRect.X + 220, rowY, 44, 20), smallFont, StringAlignment.Center);
                DrawText(g, item.Count.ToString(), new Rectangle(shopListRect.X + 266, rowY, 34, 20), smallFont, StringAlignment.Center);
                continue;
            }

            DrawText(g, entry.Label, new Rectangle(shopListRect.X + 36, rowY, 118, 20), smallFont);
        }

        DrawText(g, $"{shopPageIndex + 1}/{GetShopPageCount()}", new Rectangle(shopListRect.X + 20, shopListRect.Bottom - 28, 60, 24), smallFont);
        DrawText(g, $"G {player.Gold}", new Rectangle(shopListRect.X + 176, shopListRect.Bottom - 28, 122, 24), smallFont, StringAlignment.Far);

        var selectedEntry = GetSelectedShopEntry();
        var detailLine1 = selectedEntry is null
            ? $"LV {player.Level}"
            : selectedEntry.Value.Type == ShopMenuEntryType.InventoryItem && selectedEntry.Value.InventoryItem is not null
                ? selectedEntry.Value.InventoryItem.Value.Detail
                : selectedEntry.Value.Product?.IsEquipment == true && selectedEntry.Value.Product.Equipment is not null
                    ? $"{GetEquipmentSlotLabel(selectedEntry.Value.Product.Equipment.Slot)}: {GetCurrentEquipmentNameForSlot(selectedEntry.Value.Product.Equipment.Slot)}"
                    : selectedEntry.Value.Product?.Consumable is not null
                        ? GameContent.GetConsumableDescription(selectedEntry.Value.Product.Consumable, selectedLanguage)
                        : $"LV {player.Level}";
        var detailLine2 = $"EXP {GetExperienceSummary()}";
        if (selectedEntry is not null &&
            selectedEntry.Value.Type == ShopMenuEntryType.Product &&
            selectedEntry.Value.Product is not null &&
            selectedEntry.Value.Product.IsEquipment)
        {
            detailLine2 = selectedEntry.Value.Product.Equipment switch
            {
                WeaponDefinition weapon => selectedLanguage == UiLanguage.English
                    ? $"EQUIP ATK {battleService.GetPlayerAttack(player, weapon)}"
                    : $"そうびで ATK {battleService.GetPlayerAttack(player, weapon)}",
                ArmorDefinition armor => selectedLanguage == UiLanguage.English
                    ? $"EQUIP DEF {battleService.GetPlayerDefense(player, armor)}"
                    : $"そうびで DEF {battleService.GetPlayerDefense(player, armor)}",
                _ => detailLine2
            };
        }

        DrawWindow(g, shopInfoRect);
        var equipmentLineY = shopInfoRect.Y + 5;
        DrawShopEquipmentSlot(g, shopInfoRect, EquipmentSlot.Weapon, equipmentLineY);
        DrawShopEquipmentSlot(g, shopInfoRect, EquipmentSlot.Head, equipmentLineY + 16);
        DrawShopEquipmentSlot(g, shopInfoRect, EquipmentSlot.Armor, equipmentLineY + 32);
        DrawShopEquipmentSlot(g, shopInfoRect, EquipmentSlot.Arms, equipmentLineY + 48);
        DrawShopEquipmentSlot(g, shopInfoRect, EquipmentSlot.Legs, equipmentLineY + 64);
        DrawShopEquipmentSlot(g, shopInfoRect, EquipmentSlot.Feet, equipmentLineY + 80);
        DrawText(g, $"ATK {GetTotalAttack()}  DEF {GetTotalDefense()}", new Rectangle(shopInfoRect.X + 20, equipmentLineY + 98, 202, 16), smallFont);
        DrawText(g, detailLine1, new Rectangle(shopInfoRect.X + 20, equipmentLineY + 114, 202, 16), smallFont);
        DrawText(g, detailLine2, new Rectangle(shopInfoRect.X + 20, equipmentLineY + 130, 202, 16), smallFont);

        DrawWindow(g, shopMessageRect);
        DrawText(g, shopMessage, Rectangle.Inflate(shopMessageRect, -24, -24), smallFont, wrap: true);
    }

    private void DrawShopEquipmentSlot(Graphics g, Rectangle panelRect, EquipmentSlot slot, int y)
    {
        DrawText(g, $"{GetEquipmentSlotLabel(slot)}:", new Rectangle(panelRect.X + 16, y, 84, 16), smallFont);
        DrawText(g, GetCurrentEquipmentNameForSlot(slot), new Rectangle(panelRect.X + 100, y, 122, 16), smallFont, StringAlignment.Far);
    }
}
