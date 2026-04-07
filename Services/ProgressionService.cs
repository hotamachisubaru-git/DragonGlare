using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class ProgressionService
{
    public PlayerProgress CreateNewPlayer(UiLanguage language, Point startTile)
    {
        var player = PlayerProgress.CreateDefault(startTile, language);
        GrantPrototypeStarterItems(player);
        return player;
    }

    public string ApplyBattleRewards(PlayerProgress player, int experienceReward, int goldReward, Random random)
    {
        player.Experience += experienceReward;
        player.Gold += goldReward;

        var messages = new List<string>
        {
            $"{experienceReward}けいけんち と {goldReward}Gを えた！"
        };

        while (player.Experience >= GetExperienceThreshold(player.Level + 1))
        {
            player.Level++;

            var hpGain = 4 + random.Next(0, 3);
            var mpGain = 1 + random.Next(0, 2);
            var attackGain = 1 + random.Next(0, 2);
            var defenseGain = 1 + random.Next(0, 2);

            player.MaxHp += hpGain;
            player.MaxMp += mpGain;
            player.BaseAttack += attackGain;
            player.BaseDefense += defenseGain;
            player.CurrentHp = player.MaxHp;
            player.CurrentMp = player.MaxMp;

            messages.Add($"{GetName(player)}は レベル{player.Level}に あがった！");
            messages.Add($"HP+{hpGain} MP+{mpGain} ATK+{attackGain} DEF+{defenseGain}");
        }

        return string.Join("\n", messages);
    }

    public string ApplyDefeatPenalty(PlayerProgress player, Point respawnTile)
    {
        var goldLoss = Math.Min(player.Gold, Math.Max(0, player.Gold / 5));
        player.Gold -= goldLoss;
        player.TilePosition = respawnTile;
        player.CurrentHp = player.MaxHp;
        player.CurrentMp = player.MaxMp;

        if (goldLoss == 0)
        {
            return "HPとMPを とりもどし\nスタートちてんに もどった。";
        }

        return $"{goldLoss}Gを おとして\nスタートちてんに もどった。";
    }

    public int GetExperienceIntoCurrentLevel(PlayerProgress player)
    {
        return player.Experience - GetExperienceThreshold(player.Level);
    }

    public int GetExperienceNeededForNextLevel(PlayerProgress player)
    {
        return GetExperienceThreshold(player.Level + 1) - GetExperienceThreshold(player.Level);
    }

    private static int GetExperienceThreshold(int level)
    {
        if (level <= 1)
        {
            return 0;
        }

        var total = 0;
        for (var currentLevel = 1; currentLevel < level; currentLevel++)
        {
            total += 12 + ((currentLevel - 1) * 10);
        }

        return total;
    }

    private static string GetName(PlayerProgress player)
    {
        return string.IsNullOrWhiteSpace(player.Name) ? "プレイヤー" : player.Name;
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
}
