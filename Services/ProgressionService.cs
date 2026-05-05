using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class ProgressionService
{
    public static readonly int MaxLevelExperience = GetExperienceThreshold(PlayerProgress.MaxLevelValue);

    public PlayerProgress CreateNewPlayer(UiLanguage language, Point startTile)
    {
        var player = PlayerProgress.CreateDefault(startTile, language);
        GrantPrototypeStarterItems(player);
        return player;
    }

    public string ApplyBattleRewards(PlayerProgress player, EnemyDefinition enemy, Random random)
    {
        var language = player.Language;
        var previousExperience = player.Experience;
        var previousGold = player.Gold;
        player.Experience = Math.Min(MaxLevelExperience, player.Experience + enemy.ExperienceReward);
        player.Gold = Math.Min(PlayerProgress.MaxGoldValue, player.Gold + enemy.GoldReward);
        var gainedExperience = player.Experience - previousExperience;
        var gainedGold = player.Gold - previousGold;

        var messages = new List<string>
        {
            Text(language,
                $"{gainedExperience}けいけんち と {gainedGold}Gを えた！",
                $"Gained {gainedExperience} EXP and {gainedGold}G!")
        };

        if (TryAwardBattleDrop(player, enemy, random, out var dropMessage))
        {
            messages.Add(dropMessage);
        }

        while (player.Level < PlayerProgress.MaxLevelValue && player.Experience >= GetExperienceThreshold(player.Level + 1))
        {
            player.Level++;

            var previousMaxHp = player.MaxHp;
            var previousMaxMp = player.MaxMp;
            var hpGain = 4 + random.Next(0, 3);
            var mpGain = 1 + random.Next(0, 2);
            var attackGain = 1 + random.Next(0, 2);
            var defenseGain = 1 + random.Next(0, 2);

            player.MaxHp = Math.Min(PlayerProgress.MaxVitalValue, player.MaxHp + hpGain);
            player.MaxMp = Math.Min(PlayerProgress.MaxVitalValue, player.MaxMp + mpGain);
            if (player.Level == PlayerProgress.MaxLevelValue)
            {
                player.MaxHp = PlayerProgress.MaxVitalValue;
                player.MaxMp = PlayerProgress.MaxVitalValue;
            }

            hpGain = player.MaxHp - previousMaxHp;
            mpGain = player.MaxMp - previousMaxMp;
            player.BaseAttack += attackGain;
            player.BaseDefense += defenseGain;
            player.CurrentHp = player.MaxHp;
            player.CurrentMp = player.MaxMp;

            messages.Add(Text(language,
                $"{GetName(player)}は レベル{player.Level}に あがった！",
                $"{GetName(player)} reached level {player.Level}!"));
            messages.Add($"HP+{hpGain} MP+{mpGain} ATK+{attackGain} DEF+{defenseGain}");
        }

        player.Normalize();
        return string.Join("\n", messages);
    }

    public string ApplyDefeatPenalty(PlayerProgress player, Point respawnTile)
    {
        var language = player.Language;
        var goldLoss = Math.Min(player.Gold, Math.Max(0, player.Gold / 5));
        player.Gold -= goldLoss;
        player.TilePosition = respawnTile;
        player.CurrentHp = player.MaxHp;
        player.CurrentMp = player.MaxMp;
        var debtPenaltyMessage = ApplyLoanPenalty(player);
        player.Normalize();

        if (goldLoss == 0)
        {
            return string.IsNullOrWhiteSpace(debtPenaltyMessage)
                ? Text(language, "HPとMPを とりもどし\nスタートちてんに もどった。", "Recovered HP and MP\nand returned to the start.")
                : $"{Text(language, "HPとMPを とりもどし\nスタートちてんに もどった。", "Recovered HP and MP\nand returned to the start.")}\n{debtPenaltyMessage}";
        }

        return string.IsNullOrWhiteSpace(debtPenaltyMessage)
            ? Text(language, $"{goldLoss}Gを おとして\nスタートちてんに もどった。", $"Lost {goldLoss}G\nand returned to the start.")
            : $"{Text(language, $"{goldLoss}Gを おとして\nスタートちてんに もどった。", $"Lost {goldLoss}G\nand returned to the start.")}\n{debtPenaltyMessage}";
    }

    public int GetExperienceIntoCurrentLevel(PlayerProgress player)
    {
        if (player.Level >= PlayerProgress.MaxLevelValue)
        {
            return 0;
        }

        return player.Experience - GetExperienceThreshold(player.Level);
    }

    public int GetExperienceNeededForNextLevel(PlayerProgress player)
    {
        if (player.Level >= PlayerProgress.MaxLevelValue)
        {
            return 0;
        }

        return GetExperienceThreshold(player.Level + 1) - GetExperienceThreshold(player.Level);
    }

    private static int GetExperienceThreshold(int level)
    {
        if (level <= 1)
        {
            return 0;
        }

        var cappedLevel = Math.Min(level, PlayerProgress.MaxLevelValue);
        var completedLevels = cappedLevel - 1;
        return completedLevels * (24 + ((completedLevels - 1) * 10)) / 2;
    }

    private static string GetName(PlayerProgress player)
    {
        if (!string.IsNullOrWhiteSpace(player.Name))
        {
            return player.Name;
        }

        return player.Language == UiLanguage.English ? "Player" : "プレイヤー";
    }

    public void GrantPrototypeStarterItems(PlayerProgress player)
    {
        if (player.GetItemCount("healing_herb") == 0)
        {
            player.AddItem("healing_herb", 2);
        }

        if (player.GetItemCount("mana_seed") == 0)
        {
            player.AddItem("mana_seed", 1);
        }

        if (player.GetItemCount("fire_orb") == 0)
        {
            player.AddItem("fire_orb", 1);
        }
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
                $"{GameContent.GetEnemyName(enemy, player.Language)}は {itemName} x{enemy.Drop.Quantity}を おとした！",
                $"{GameContent.GetEnemyName(enemy, player.Language)} dropped {itemName} x{enemy.Drop.Quantity}!")
            : Text(player.Language,
                $"{GameContent.GetEnemyName(enemy, player.Language)}は {itemName}を おとした！",
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

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
    }
}
