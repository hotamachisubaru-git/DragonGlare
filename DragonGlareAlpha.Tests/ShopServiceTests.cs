using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class ShopServiceTests
{
    [Fact]
    public void PurchaseEquipment_AddsInventoryAndAutoEquipsStrongerWeapon()
    {
        var service = new ShopService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Gold = 300;
        player.EquippedWeaponId = "stick";
        player.AddItem("stick");

        var weapon = GameContent.ShopCatalog.Single(item => item.Id == "bronze_sword");
        var currentWeapon = GameContent.GetWeaponById(player.EquippedWeaponId);

        var result = service.PurchaseProduct(player, weapon, currentWeapon, null);

        Assert.True(result.Success);
        Assert.True(result.Equipped);
        Assert.Equal("bronze_sword", player.EquippedWeaponId);
        Assert.Equal(1, player.GetItemCount("bronze_sword"));
        Assert.Equal(104, player.Gold);
    }

    [Fact]
    public void PurchaseEquipment_AutoEquipsStrongerArmorAndTracksInventory()
    {
        var service = new ShopService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Gold = 100;
        player.EquippedArmorId = "cloth_tunic";
        player.AddItem("cloth_tunic");

        var armor = GameContent.ShopCatalog.Single(item => item.Id == "leather_armor");
        var currentArmor = GameContent.GetArmorById(player.EquippedArmorId);

        var result = service.PurchaseProduct(player, armor, null, currentArmor);

        Assert.True(result.Success);
        Assert.True(result.Equipped);
        Assert.Equal("leather_armor", player.EquippedArmorId);
        Assert.Equal(1, player.GetItemCount("leather_armor"));
        Assert.Equal(52, player.Gold);
    }

    [Fact]
    public void PurchaseProduct_AddsConsumableToStackedInventory()
    {
        var service = new ShopService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Gold = 50;
        player.AddItem("healing_herb", 2);
        var herb = GameContent.ShopCatalog.Single(item => item.Id == "healing_herb");

        var result = service.PurchaseProduct(player, herb, null, null);

        Assert.True(result.Success);
        Assert.False(result.Equipped);
        Assert.Equal(3, player.GetItemCount("healing_herb"));
        Assert.Equal(40, player.Gold);
    }

    [Fact]
    public void SellItem_RemovesOneFromStackAndAddsGold()
    {
        var service = new ShopService();
        var player = PlayerProgress.CreateDefault(new Point(0, 0));
        player.Gold = 10;
        player.AddItem("healing_herb", 2);

        var result = service.SellItem(player, "healing_herb");

        Assert.True(result.Success);
        Assert.Equal(1, player.GetItemCount("healing_herb"));
        Assert.Equal(15, player.Gold);
    }

    [Fact]
    public void ShopCatalog_IsSortedByPriceAcrossMultiplePages()
    {
        var prices = GameContent.ShopCatalog.Select(item => item.Price).ToArray();

        Assert.True(GameContent.ShopCatalog.Length > 12);
        Assert.Equal(prices.OrderBy(price => price), prices);
    }
}
