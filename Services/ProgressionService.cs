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

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
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
