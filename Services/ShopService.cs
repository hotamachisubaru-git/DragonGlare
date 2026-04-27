using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Commerce;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class ShopService
{
    public ShopTransactionResult PurchaseProduct(PlayerProgress player, ShopProductDefinition product)
    {
        if (string.IsNullOrWhiteSpace(product.Id) || (product.Equipment is null && product.Consumable is null))
        {
            return new ShopTransactionResult(false, false, 0, "＊「その しょうひんは まだ あつかえない。」");
        }

        if (player.Gold < product.Price)
        {
            return new ShopTransactionResult(false, false, 0, "＊「おかねが たりないね。」");
        }

        player.Gold -= product.Price;
        player.AddItem(product.Id);

        if (product.Consumable is not null)
        {
            return new ShopTransactionResult(
                true,
                false,
                product.Price,
                $"＊「{product.Name}を かった！\n　もちものに しまっておくよ。」");
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
            ? $"＊「{equipment.Name}を かった！\n　さっそく そうびしたぜ。」"
            : $"＊「{equipment.Name}を かった！\n　もちものに いれておくよ。」";

        return new ShopTransactionResult(true, shouldEquip, product.Price, message);
    }

    public ShopTransactionResult SellItem(PlayerProgress player, string? itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || player.GetItemCount(itemId) <= 0)
        {
            return new ShopTransactionResult(false, false, 0, "＊「それは うれないみたいだ。」");
        }

        var sellPrice = GameContent.GetSellPrice(itemId);
        if (sellPrice <= 0)
        {
            return new ShopTransactionResult(false, false, 0, "＊「それは うれないみたいだ。」");
        }

        var availableCapacity = PlayerProgress.MaxGoldValue - player.Gold;
        if (availableCapacity <= 0)
        {
            return new ShopTransactionResult(false, false, 0, "＊「これいじょう おかねは もてないね。」");
        }

        var gainedGold = Math.Min(sellPrice, availableCapacity);
        var itemName = GameContent.GetItemName(itemId);
        player.RemoveItem(itemId);
        player.Gold += gainedGold;

        var message = gainedGold == sellPrice
            ? $"＊「{itemName}を うった！\n　{gainedGold}Gを てにいれた。」"
            : $"＊「{itemName}を うった！\n　{gainedGold}Gだけ うけとった。」";

        return new ShopTransactionResult(true, false, gainedGold, message);
    }
}

public sealed record ShopTransactionResult(bool Success, bool Equipped, int GoldDelta, string Message);
