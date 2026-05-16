using DragonGlare.Data;
using DragonGlare.Domain;
using DragonGlare.Domain.Field;
using DragonGlare.Domain.Player;

namespace DragonGlare.Services;

public sealed class FieldEventService
{
    public FieldInteractionResult Interact(PlayerProgress player, FieldEventDefinition fieldEvent, UiLanguage language)
    {
        var isCompletedTreasure = fieldEvent.ActionType == FieldEventActionType.Treasure &&
            player.HasCompletedFieldEvent(fieldEvent.Id);
        var pages = fieldEvent
            .GetPages(language, isCompletedTreasure)
            .Select(page => page.Replace("{player}", GetPlayerName(player), StringComparison.Ordinal))
            .ToList();
        var shouldPersistProgress = false;

        if (fieldEvent.ActionType == FieldEventActionType.Recover)
        {
            var recoveredHp = Math.Min(fieldEvent.RecoverHp, player.MaxHp - player.CurrentHp);
            var recoveredMp = Math.Min(fieldEvent.RecoverMp, player.MaxMp - player.CurrentMp);
            player.CurrentHp += recoveredHp;
            player.CurrentMp += recoveredMp;

            var recoveryPage = language == UiLanguage.Japanese
                ? $"HP+{recoveredHp}  MP+{recoveredMp}\nからだが かるくなった。"
                : $"HP+{recoveredHp}  MP+{recoveredMp}\nYou feel refreshed.";

            pages.Add(recoveryPage);
            shouldPersistProgress = true;
        }

        if (fieldEvent.ActionType == FieldEventActionType.Treasure && !isCompletedTreasure)
        {
            pages.AddRange(ApplyTreasureReward(player, fieldEvent, language));
            player.CompleteFieldEvent(fieldEvent.Id);
            shouldPersistProgress = true;
        }

        return new FieldInteractionResult
        {
            Pages = pages,
            ShouldPersistProgress = shouldPersistProgress
        };
    }

    private static IEnumerable<string> ApplyTreasureReward(PlayerProgress player, FieldEventDefinition fieldEvent, UiLanguage language)
    {
        var reward = fieldEvent.Reward;
        if (reward is null || !reward.HasAnyReward)
        {
            yield return Text(language, "しかし なにも はいっていなかった。", "But it was empty.");
            yield break;
        }

        if (reward.HasItem)
        {
            var itemId = reward.ItemId ?? string.Empty;
            player.AddItem(itemId, reward.ItemQuantity);
            var itemName = GameContent.GetItemName(itemId, language);
            if (string.IsNullOrWhiteSpace(itemName))
            {
                itemName = itemId;
            }

            var quantitySuffix = reward.ItemQuantity > 1
                ? Text(language, $"×{reward.ItemQuantity}", $" x{reward.ItemQuantity}")
                : string.Empty;
            yield return Text(language, $"{itemName}{quantitySuffix}を てにいれた！", $"Received {itemName}{quantitySuffix}!");
        }

        if (reward.HasGold)
        {
            var previousGold = player.Gold;
            player.Gold = Math.Min(PlayerProgress.MaxGoldValue, player.Gold + reward.Gold);
            var gainedGold = player.Gold - previousGold;
            yield return gainedGold > 0
                ? Text(language, $"{gainedGold}Gを てにいれた！", $"Received {gainedGold}G!")
                : Text(language, "これいじょう おかねを もてない。", "You cannot carry any more gold.");
        }
    }

    private static string GetPlayerName(PlayerProgress player)
    {
        if (!string.IsNullOrWhiteSpace(player.Name))
        {
            return player.Name;
        }

        return player.Language == UiLanguage.English ? "adventurer" : "ぼうけんしゃ";
    }

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
    }
}
