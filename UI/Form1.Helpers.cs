using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha;

public partial class Form1
{
    private static bool IsInsideCastleZone(Point tile)
    {
        return tile.X >= 1 && tile.X <= 4 && tile.Y >= 1 && tile.Y <= 4;
    }

    private WeaponDefinition? GetEquippedWeapon()
    {
        return GameContent.GetWeaponById(player.EquippedWeaponId);
    }

    private string GetDisplayPlayerName()
    {
        if (!string.IsNullOrWhiteSpace(player.Name))
        {
            return player.Name;
        }

        return playerName.Length == 0 ? "のりたま" : playerName.ToString();
    }

    private string GetEquippedWeaponName()
    {
        return GetEquippedWeapon()?.Name ?? "なし";
    }

    private int GetTotalAttack()
    {
        return battleService.GetPlayerAttack(player, GetEquippedWeapon());
    }

    private int GetTotalDefense()
    {
        return battleService.GetPlayerDefense(player);
    }

    private string GetExperienceSummary()
    {
        var current = progressionService.GetExperienceIntoCurrentLevel(player);
        var needed = progressionService.GetExperienceNeededForNextLevel(player);
        return $"{current}/{needed}";
    }

    private string TrimPlayerName(string name)
    {
        var trimmed = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
        return trimmed.Length <= 10 ? trimmed : trimmed[..10];
    }

    private void SyncPlayerNameBuffer(string name)
    {
        playerName.Clear();
        if (!string.IsNullOrWhiteSpace(name))
        {
            playerName.Append(TrimPlayerName(name));
        }
    }

    private static string FormatBattleResolutionMessage(IEnumerable<DragonGlareAlpha.Domain.Battle.BattleSequenceStep> steps)
    {
        return string.Join('\n', steps.Select(step => step.Message).Where(message => !string.IsNullOrWhiteSpace(message)));
    }
}
