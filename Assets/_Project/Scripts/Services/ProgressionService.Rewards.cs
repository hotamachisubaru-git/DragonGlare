using DragonGlare.Data;
using DragonGlare.Domain;
using DragonGlare.Domain.Battle;
using DragonGlare.Domain.Player;

namespace DragonGlare.Services;

public sealed partial class ProgressionService
{
    private static BattleMemberRewardResult ApplyExperienceReward(PlayerProgress member, int experienceReward, Random random)
    {
        var previousExperience = member.Experience;
        member.Experience = Math.Min(MaxLevelExperience, member.Experience + experienceReward);
        var gainedExperience = member.Experience - previousExperience;
        var levelUps = new List<BattleLevelUpResult>();

        while (member.Level < PlayerProgress.MaxLevelValue && member.Experience >= GetExperienceThreshold(member.Level + 1))
        {
            member.Level++;

            var previousMaxHp = member.MaxHp;
            var previousMaxMp = member.MaxMp;
            var hpGain = 4 + random.Next(0, 3);
            var mpGain = 1 + random.Next(0, 2);
            var attackGain = 1 + random.Next(0, 2);
            var defenseGain = 1 + random.Next(0, 2);

            member.MaxHp = Math.Min(PlayerProgress.MaxVitalValue, member.MaxHp + hpGain);
            member.MaxMp = Math.Min(PlayerProgress.MaxVitalValue, member.MaxMp + mpGain);
            if (member.Level == PlayerProgress.MaxLevelValue)
            {
                member.MaxHp = PlayerProgress.MaxVitalValue;
                member.MaxMp = PlayerProgress.MaxVitalValue;
            }

            hpGain = member.MaxHp - previousMaxHp;
            mpGain = member.MaxMp - previousMaxMp;
            member.BaseAttack += attackGain;
            member.BaseDefense += defenseGain;
            member.CurrentHp = member.MaxHp;
            member.CurrentMp = member.MaxMp;

            levelUps.Add(new BattleLevelUpResult(
                GetName(member),
                member.Language,
                member.Level,
                hpGain,
                mpGain,
                attackGain,
                defenseGain));
        }

        member.Normalize();
        return new BattleMemberRewardResult(gainedExperience, levelUps);
    }

    private static bool TryAwardBattleDrop(PlayerProgress player, EnemyDefinition enemy, Random random, out string dropMessage)
    {
        dropMessage = string.Empty;
        if (enemy.Drop is null ||
            string.IsNullOrWhiteSpace(enemy.Drop.ItemId) ||
            enemy.Drop.Quantity <= 0 ||
            enemy.Drop.ChancePercent <= 0)
        {
            return false;
        }

        if (random.Next(100) >= enemy.Drop.ChancePercent)
        {
            return false;
        }

        player.AddItem(enemy.Drop.ItemId, enemy.Drop.Quantity);
        var itemName = GameContent.GetItemName(enemy.Drop.ItemId, player.Language);
        if (string.IsNullOrWhiteSpace(itemName))
        {
            itemName = enemy.Drop.ItemId;
        }

        dropMessage = enemy.Drop.Quantity > 1
            ? Text(player.Language,
                $"{GameContent.GetEnemyName(enemy, player.Language)}は {itemName} x{enemy.Drop.Quantity}をおとした！",
                $"{GameContent.GetEnemyName(enemy, player.Language)} dropped {itemName} x{enemy.Drop.Quantity}!")
            : Text(player.Language,
                $"{GameContent.GetEnemyName(enemy, player.Language)}は {itemName}をおとした！",
                $"{GameContent.GetEnemyName(enemy, player.Language)} dropped {itemName}!");
        return true;
    }

    private static string ApplyLoanPenalty(PlayerProgress player)
    {
        var language = player.Language;
        if (player.LoanBalance <= 0)
        {
            return string.Empty;
        }

        var seizedItems = new List<string>();
        foreach (var itemId in GetSeizableEquipmentIds(player))
        {
            var sellPrice = GameContent.GetSellPrice(itemId);
            if (sellPrice <= 0 || !player.RemoveItem(itemId))
            {
                continue;
            }

            player.LoanBalance = Math.Max(0, player.LoanBalance - sellPrice);
            var itemName = GameContent.GetItemName(itemId, language);
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                seizedItems.Add(itemName);
            }

            if (player.LoanBalance == 0)
            {
                player.LoanStepCounter = 0;
                break;
            }
        }

        if (seizedItems.Count == 0)
        {
            return Text(language,
                $"しゃっきん {player.LoanBalance}Gが のこったままだ。",
                $"Your {player.LoanBalance}G loan remains.");
        }

        var seizedList = string.Join(language == UiLanguage.English ? " and " : " と ", seizedItems);
        return Text(language,
            $"しゃっきんの かたに\n{seizedList}を さしおさえられた。",
            $"{seizedList} was seized\nto repay your loan.");
    }

    private static IEnumerable<string> GetSeizableEquipmentIds(PlayerProgress player)
    {
        foreach (var itemId in player.GetEquippedItemIds())
        {
            yield return itemId;
        }
    }
}
