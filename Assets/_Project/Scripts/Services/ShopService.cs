using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Commerce;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class ShopService
{
    public ShopTransactionResult PurchaseProduct(PlayerProgress player, ShopProductDefinition product)
    {
        var language = player.Language;
        if (string.IsNullOrWhiteSpace(product.Id) || (product.Equipment is null && product.Consumable is null))
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "＊「その しょうひんは まだ あつかえない。」", "* \"That item is not for sale yet.\""));
        }

        if (player.Gold < product.Price)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "＊「おかねが たりないね。」", "* \"You do not have enough gold.\""));
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
                    $"＊「{productName}を かった！\n　もちものに しまっておくよ。」",
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
                $"＊「{productName}を かった！\n　さっそく そうびしたぜ。」",
                $"* \"Bought {productName}!\n  You equipped it right away.\"")
            : Text(language,
                $"＊「{productName}を かった！\n　もちものに いれておくよ。」",
                $"* \"Bought {productName}!\n  I put it in your bag.\"");

        return new ShopTransactionResult(true, shouldEquip, product.Price, message);
    }

    public ShopTransactionResult SellItem(PlayerProgress player, string? itemId)
    {
        var language = player.Language;
        if (string.IsNullOrWhiteSpace(itemId) || player.GetItemCount(itemId) <= 0)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "＊「それは うれないみたいだ。」", "* \"I cannot buy that.\""));
        }

        var sellPrice = GameContent.GetSellPrice(itemId);
        if (sellPrice <= 0)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "＊「それは うれないみたいだ。」", "* \"I cannot buy that.\""));
        }

        var availableCapacity = PlayerProgress.MaxGoldValue - player.Gold;
        if (availableCapacity <= 0)
        {
            return new ShopTransactionResult(false, false, 0, Text(language, "＊「これいじょう おかねは もてないね。」", "* \"You cannot carry any more gold.\""));
        }

        var gainedGold = Math.Min(sellPrice, availableCapacity);
        var itemName = GameContent.GetItemName(itemId, language);
        player.RemoveItem(itemId);
        player.Gold += gainedGold;

        var message = gainedGold == sellPrice
            ? Text(language,
                $"＊「{itemName}を うった！\n　{gainedGold}Gを てにいれた。」",
                $"* \"Sold {itemName}!\n  You received {gainedGold}G.\"")
            : Text(language,
                $"＊「{itemName}を うった！\n　{gainedGold}Gだけ うけとった。」",
                $"* \"Sold {itemName}!\n  You could only take {gainedGold}G.\"");

        return new ShopTransactionResult(true, false, gainedGold, message);
    }

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
    }
}

public sealed record ShopTransactionResult(bool Success, bool Equipped, int GoldDelta, string Message);
