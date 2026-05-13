using DragonGlare.Data;
using DragonGlare.Domain;
using DragonGlare.Domain.Commerce;
using DragonGlare.Domain.Player;

namespace DragonGlare.Services;

public sealed class ShopService
{
    public ShopTransactionResult PurchaseProduct(PlayerProgress player, ShopProductDefinition product)
    {
        var language = player.Language;
        if (string.IsNullOrWhiteSpace(product.Id) || (product.Equipment is null && product.Consumable is null))
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "・翫後◎縺ｮ 縺励ｇ縺・・繧薙・ 縺ｾ縺 縺ゅ▽縺九∴縺ｪ縺・ゅ・, "* \"That item is not for sale yet.\""));
        }

        if (player.Gold < product.Price)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "・翫後♀縺九・縺・縺溘ｊ縺ｪ縺・・縲ゅ・, "* \"You do not have enough gold.\""));
        }

        player.Gold -= product.Price;
        player.AddItem(product.Id);
        var productName = GameContent.GetShopProductName(product, language);

        if (product.Consumable is not null)
        {
            return new ShopTransactionResult(
                true,
                false,
                product.Price,
                Text(language,
                    $"・翫鶏productName}繧・縺九▲縺滂ｼ―n縲繧ゅ■繧ゅ・縺ｫ 縺励∪縺｣縺ｦ縺翫￥繧医ゅ・,
                    $"* \"Bought {productName}!\n  I put it in your bag.\""));
        }

        var equipment = product.Equipment!;
        var shouldEquip = false;
        switch (equipment.Slot)
        {
            case EquipmentSlot.Weapon:
                var currentWeapon = GameContent.GetWeaponById(player.EquippedWeaponId);
                shouldEquip = currentWeapon is null || equipment.AttackBonus > currentWeapon.AttackBonus;
                if (shouldEquip)
                {
                    player.EquippedWeaponId = equipment.Id;
                }

                break;
            default:
                var currentArmor = GameContent.GetArmorById(player.GetEquippedItemId(equipment.Slot));
                shouldEquip = currentArmor is null || equipment.DefenseBonus > currentArmor.DefenseBonus;
                if (shouldEquip)
                {
                    player.SetEquippedItemId(equipment.Slot, equipment.Id);
                }

                break;
        }

        var message = shouldEquip
            ? Text(language,
                $"・翫鶏productName}繧・縺九▲縺滂ｼ―n縲縺輔▲縺昴￥ 縺昴≧縺ｳ縺励◆縺懊ゅ・,
                $"* \"Bought {productName}!\n  You equipped it right away.\"")
            : Text(language,
                $"・翫鶏productName}繧・縺九▲縺滂ｼ―n縲繧ゅ■繧ゅ・縺ｫ 縺・ｌ縺ｦ縺翫￥繧医ゅ・,
                $"* \"Bought {productName}!\n  I put it in your bag.\"");

        return new ShopTransactionResult(true, shouldEquip, product.Price, message);
    }

    public ShopTransactionResult SellItem(PlayerProgress player, string? itemId)
    {
        var language = player.Language;
        if (string.IsNullOrWhiteSpace(itemId) || player.GetItemCount(itemId) <= 0)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "・翫後◎繧後・ 縺・ｌ縺ｪ縺・∩縺溘＞縺縲ゅ・, "* \"I cannot buy that.\""));
        }

        var sellPrice = GameContent.GetSellPrice(itemId);
        if (sellPrice <= 0)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "・翫後◎繧後・ 縺・ｌ縺ｪ縺・∩縺溘＞縺縲ゅ・, "* \"I cannot buy that.\""));
        }

        var availableCapacity = PlayerProgress.MaxGoldValue - player.Gold;
        if (availableCapacity <= 0)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "・翫後％繧後＞縺倥ｇ縺・縺翫°縺ｭ縺ｯ 繧ゅ※縺ｪ縺・・縲ゅ・, "* \"You cannot carry any more gold.\""));
        }

        var gainedGold = Math.Min(sellPrice, availableCapacity);
        var itemName = GameContent.GetItemName(itemId, language);
        player.RemoveItem(itemId);
        player.Gold += gainedGold;

        var message = gainedGold == sellPrice
            ? Text(language,
                $"・翫鶏itemName}繧・縺・▲縺滂ｼ―n縲{gainedGold}G繧・縺ｦ縺ｫ縺・ｌ縺溘ゅ・,
                $"* \"Sold {itemName}!\n  You received {gainedGold}G.\"")
            : Text(language,
                $"・翫鶏itemName}繧・縺・▲縺滂ｼ―n縲{gainedGold}G縺縺・縺・￠縺ｨ縺｣縺溘ゅ・,
                $"* \"Sold {itemName}!\n  You could only take {gainedGold}G.\"");

        return new ShopTransactionResult(true, false, gainedGold, message);
    }

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
    }
}

public sealed record ShopTransactionResult(bool Success, bool Equipped, int GoldDelta, string Message);
