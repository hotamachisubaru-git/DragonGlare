using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed partial class ProgressionService
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
        return ApplyBattleRewardsDetailed(player, enemy, random).ToMessageText();
    }

    public BattleRewardResult ApplyBattleRewardsDetailed(PlayerProgress player, EnemyDefinition enemy, Random random)
    {
        return ApplyPartyBattleRewardsDetailed(player, [player], enemy, random);
    }

    public BattleRewardResult ApplyPartyBattleRewardsDetailed(
        PlayerProgress partyInventoryOwner,
        IEnumerable<PlayerProgress> partyMembers,
        EnemyDefinition enemy,
        Random random)
    {
        var language = partyInventoryOwner.Language;
        var members = NormalizeRewardPartyMembers(partyInventoryOwner, partyMembers);
        var previousGold = partyInventoryOwner.Gold;
        partyInventoryOwner.Gold = Math.Min(PlayerProgress.MaxGoldValue, partyInventoryOwner.Gold + enemy.GoldReward);
        var gainedGold = partyInventoryOwner.Gold - previousGold;
        var memberRewardResults = members
            .Select(member => ApplyExperienceReward(member, enemy.ExperienceReward, random))
            .ToArray();
        var gainedExperience = memberRewardResults.Length == 0
            ? 0
            : memberRewardResults.Max(result => result.GainedExperience);

        var rewardMessage = Text(language,
            $"{gainedExperience}けいけんちと{gainedGold}Gをえた！",
            $"Gained {gainedExperience} EXP and {gainedGold}G!");

        TryAwardBattleDrop(partyInventoryOwner, enemy, random, out var dropMessage);
        return new BattleRewardResult(
            rewardMessage,
            memberRewardResults.SelectMany(result => result.LevelUps).ToArray(),
            dropMessage);
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

    private static IReadOnlyList<PlayerProgress> NormalizeRewardPartyMembers(
        PlayerProgress fallbackMember,
        IEnumerable<PlayerProgress> partyMembers)
    {
        var members = new List<PlayerProgress>();
        foreach (var member in partyMembers)
        {
            if (member is null || ContainsReference(members, member))
            {
                continue;
            }

            members.Add(member);
        }

        if (members.Count == 0)
        {
            members.Add(fallbackMember);
        }

        return members;
    }

    private static bool ContainsReference(IEnumerable<PlayerProgress> members, PlayerProgress candidate)
    {
        foreach (var member in members)
        {
            if (ReferenceEquals(member, candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
    }

    private static string GetName(PlayerProgress player)
    {
        if (!string.IsNullOrWhiteSpace(player.Name))
        {
            return player.Name;
        }

        return player.Language == UiLanguage.English ? "Player" : "プレイヤー";
    }
}

public sealed record BattleRewardResult(
    string RewardMessage,
    IReadOnlyList<BattleLevelUpResult> LevelUps,
    string DropMessage)
{
    public IReadOnlyList<string> LevelUpMessages => LevelUps
        .Select(levelUp => levelUp.ToMessageText())
        .ToArray();

    public IEnumerable<string> Messages
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(RewardMessage))
            {
                yield return RewardMessage;
            }

            foreach (var message in LevelUps
                .Select(levelUp => levelUp.ToMessageText())
                .Where(message => !string.IsNullOrWhiteSpace(message)))
            {
                yield return message;
            }

            if (!string.IsNullOrWhiteSpace(DropMessage))
            {
                yield return DropMessage;
            }
        }
    }

    public string ToMessageText()
    {
        return string.Join('\n', Messages);
    }
}

public sealed record BattleLevelUpResult(
    string MemberName,
    UiLanguage Language,
    int NewLevel,
    int HpGain,
    int MpGain,
    int AttackGain,
    int DefenseGain)
{
    public string ToMessageText()
    {
        return Language == UiLanguage.English
            ? $"{MemberName} reached Lv {NewLevel}!\nHP+{HpGain} MP+{MpGain} ATK+{AttackGain} DEF+{DefenseGain}"
            : $"{MemberName}は Lv {NewLevel} にあがった！\nHPが{HpGain} MPが{MpGain} ATKが{AttackGain} DEFが{DefenseGain} あがった！";
    }
}

internal sealed record BattleMemberRewardResult(
    int GainedExperience,
    IReadOnlyList<BattleLevelUpResult> LevelUps);
