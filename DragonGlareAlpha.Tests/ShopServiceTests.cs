using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class ShopServiceTests
{
    [Fact]
    public void PurchaseWeapon_AddsInventoryAndAutoEquipsStrongerWeapon()
    {
        var service = new ShopService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Gold = 300;
        player.EquippedWeaponId = "stick";
        player.AddItem("stick");

        var weapon = GameContent.ShopCatalog.Single(item => item.Id == "bronze_sword");
        var currentWeapon = GameContent.GetWeaponById(player.EquippedWeaponId);

        var result = service.PurchaseWeapon(player, weapon, currentWeapon);

        Assert.True(result.Success);
        Assert.True(result.Equipped);
        Assert.Equal("bronze_sword", player.EquippedWeaponId);
        Assert.Equal(1, player.GetItemCount("bronze_sword"));
        Assert.Equal(104, player.Gold);
    }
}
