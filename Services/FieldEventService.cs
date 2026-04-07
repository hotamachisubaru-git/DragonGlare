using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Field;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class FieldEventService
{
    public FieldInteractionResult Interact(PlayerProgress player, FieldEventDefinition fieldEvent, UiLanguage language)
    {
        var pages = fieldEvent
            .GetPages(language)
            .Select(page => page.Replace("{player}", GetPlayerName(player), StringComparison.Ordinal))
            .ToList();

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
        }

        return new FieldInteractionResult
        {
            Pages = pages
        };
    }

    private static string GetPlayerName(PlayerProgress player)
    {
        if (!string.IsNullOrWhiteSpace(player.Name))
        {
            return player.Name;
        }

        return player.Language == UiLanguage.English ? "adventurer" : "ぼうけんしゃ";
    }
}
