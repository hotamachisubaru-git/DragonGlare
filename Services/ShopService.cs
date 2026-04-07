using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class ShopService
{
    public ShopPurchaseResult PurchaseWeapon(PlayerProgress player, WeaponDefinition weapon, WeaponDefinition? currentWeapon)
    {
        if (player.Gold < weapon.Price)
        {
            return new ShopPurchaseResult(false, false, "＊「おかねが たりないね。」");
        }

        player.Gold -= weapon.Price;
        player.AddItem(weapon.Id);

        var shouldEquip = currentWeapon is null || weapon.AttackBonus > currentWeapon.AttackBonus;
        if (shouldEquip)
        {
            player.EquippedWeaponId = weapon.Id;
        }

        var message = shouldEquip
            ? $"＊「{weapon.Name}を かった！\n　さっそく そうびしたぜ。」"
            : $"＊「{weapon.Name}を かった！\n　もちものに いれておくよ。」";

        return new ShopPurchaseResult(true, shouldEquip, message);
    }
}

public sealed record ShopPurchaseResult(bool Success, bool Equipped, string Message);
