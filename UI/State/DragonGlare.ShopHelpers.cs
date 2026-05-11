using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Items;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private IReadOnlyList<ShopInventoryEntry> GetSellableInventoryEntries()
    {
        return player.Inventory
            .Where(entry => entry.Quantity > 0)
            .Select(entry => TryCreateShopInventoryEntry(entry.ItemId, entry.Quantity))
            .Where(entry => entry.HasValue)
            .Select(entry => entry!.Value)
            .OrderBy(entry => entry.Price)
            .ThenBy(entry => entry.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private ShopInventoryEntry? TryCreateShopInventoryEntry(string itemId, int count)
    {
        var sellPrice = GameContent.GetSellPrice(itemId);
        if (sellPrice <= 0)
        {
            return null;
        }

        var consumable = GameContent.GetConsumableById(itemId);
        if (consumable is not null)
        {
            return new ShopInventoryEntry(
                itemId,
                GameContent.GetConsumableName(consumable, selectedLanguage),
                sellPrice,
                0,
                0,
                count,
                GameContent.GetConsumableDescription(consumable, selectedLanguage));
        }

        var weapon = GameContent.GetWeaponById(itemId);
        if (weapon is not null)
        {
            return new ShopInventoryEntry(
                itemId,
                GameContent.GetWeaponName(weapon, selectedLanguage),
                sellPrice,
                weapon.AttackBonus,
                weapon.DefenseBonus,
                count,
                $"ATK+{weapon.AttackBonus}");
        }

        var armor = GameContent.GetArmorById(itemId);
        if (armor is not null)
        {
            return new ShopInventoryEntry(
                itemId,
                GameContent.GetArmorName(armor, selectedLanguage),
                sellPrice,
                armor.AttackBonus,
                armor.DefenseBonus,
                count,
                $"{GetEquipmentSlotLabel(armor.Slot)} DEF+{armor.DefenseBonus}");
        }

        return null;
    }

    private int GetShopListItemCount()
    {
        return shopPhase switch
        {
            ShopPhase.BuyList => GameContent.ShopCatalog.Length,
            ShopPhase.SellList => GetSellableInventoryEntries().Count,
            _ => 0
        };
    }

    private int GetShopPageCount()
    {
        return Math.Max(1, (GetShopListItemCount() + ShopItemsPerPage - 1) / ShopItemsPerPage);
    }

    private IReadOnlyList<ShopMenuEntry> GetShopVisibleEntries()
    {
        var pageStartIndex = shopPageIndex * ShopItemsPerPage;
        var entries = shopPhase switch
        {
            ShopPhase.SellList => GetSellableInventoryEntries()
                .Skip(pageStartIndex)
                .Take(ShopItemsPerPage)
                .Select(item => new ShopMenuEntry(ShopMenuEntryType.InventoryItem, item.Name, InventoryItem: item))
                .ToList(),
            _ => GameContent.ShopCatalog
                .Skip(pageStartIndex)
                .Take(ShopItemsPerPage)
                .Select(item => new ShopMenuEntry(ShopMenuEntryType.Product, GameContent.GetShopProductName(item, selectedLanguage), Product: item))
                .ToList()
        };

        if (shopPageIndex > 0)
        {
            entries.Add(new ShopMenuEntry(ShopMenuEntryType.PreviousPage, selectedLanguage == UiLanguage.English ? "PREV" : "まえへ"));
        }

        if (shopPageIndex + 1 < GetShopPageCount())
        {
            entries.Add(new ShopMenuEntry(ShopMenuEntryType.NextPage, selectedLanguage == UiLanguage.English ? "NEXT" : "つぎへ"));
        }

        entries.Add(new ShopMenuEntry(ShopMenuEntryType.Quit, selectedLanguage == UiLanguage.English ? "QUIT" : "やめる"));
        return entries;
    }

    private void ResetShopListSelection(int pageIndex = 0)
    {
        shopPageIndex = Math.Clamp(pageIndex, 0, Math.Max(0, GetShopPageCount() - 1));
        shopItemCursor = 0;
    }

    private ShopMenuEntry? GetSelectedShopEntry()
    {
        if (shopPhase == ShopPhase.Welcome)
        {
            return null;
        }

        var visibleEntries = GetShopVisibleEntries();
        if (visibleEntries.Count == 0)
        {
            return null;
        }

        return visibleEntries[Math.Clamp(shopItemCursor, 0, visibleEntries.Count - 1)];
    }

    private IReadOnlyList<BankAmountOption> GetBankAmountOptions()
    {
        var options = new List<BankAmountOption>
        {
            new("10G", 10),
            new("50G", 50),
            new("100G", 100),
            new("300G", 300),
            new("500G", 500),
            new("1000G", 1000)
        };

        options.Add(bankPhase switch
        {
            BankPhase.DepositList => new BankAmountOption(selectedLanguage == UiLanguage.English ? "ALL" : "ぜんぶ", 0, UseMaximum: true),
            BankPhase.WithdrawList => new BankAmountOption(selectedLanguage == UiLanguage.English ? "MAX" : "できるだけ", 0, UseMaximum: true),
            BankPhase.BorrowList => new BankAmountOption(selectedLanguage == UiLanguage.English ? "MAX" : "かのうなだけ", 0, UseMaximum: true),
            _ => new BankAmountOption(selectedLanguage == UiLanguage.English ? "QUIT" : "やめる", 0, Quit: true)
        });
        options.Add(new BankAmountOption(selectedLanguage == UiLanguage.English ? "QUIT" : "やめる", 0, Quit: true));
        return options;
    }

    private int ResolveBankTransactionAmount(BankAmountOption option)
    {
        if (!option.UseMaximum)
        {
            return option.Amount;
        }

        return bankPhase switch
        {
            BankPhase.DepositList => player.Gold,
            BankPhase.WithdrawList => player.BankGold,
            BankPhase.BorrowList => bankService.GetAvailableCredit(player),
            _ => 0
        };
    }
}
