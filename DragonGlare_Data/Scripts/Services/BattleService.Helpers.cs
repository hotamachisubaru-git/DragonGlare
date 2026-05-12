using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed partial class BattleService
{
    private static List<BattleSequenceStep> CreateSpellCastSteps(PlayerProgress player, SpellDefinition spell)
    {
        var language = player.Language;
        var spellName = GameContent.GetSpellName(spell, language);
        return
        [
            new BattleSequenceStep
            {
                Message = Text(language, $"{GetPlayerName(player)}は {spellName}を となえた！", $"{GetPlayerName(player)} casts {spellName}!"),
                VisualCue = BattleVisualCue.SpellCast,
                AnimationFrames = 16,
                SoundEffect = GetSpellSoundEffect(spell)
            }
        ];
    }

    private static SoundEffect GetSpellSoundEffect(SpellDefinition spell)
    {
        return spell.Id switch
        {
            "flare" => SoundEffect.Fire,
            "spark" => SoundEffect.Raiden,
            "heal" or "cleanse" => SoundEffect.Cure,
            "venom" => SoundEffect.Poison,
            "sleep" => SoundEffect.Magic,
            _ => spell.EffectType switch
            {
                SpellEffectType.HealPlayer or SpellEffectType.CurePlayerStatus => SoundEffect.Cure,
                SpellEffectType.PoisonEnemy => SoundEffect.Poison,
                _ => SoundEffect.Magic
            }
        };
    }

    private static SoundEffect GetConsumableSoundEffect(ConsumableDefinition item)
    {
        return item.Id switch
        {
            "fire_orb" => SoundEffect.Fire,
            "thunder_orb" => SoundEffect.Raiden,
            _ => item.EffectType switch
            {
                ConsumableEffectType.HealHp or ConsumableEffectType.HealMp => SoundEffect.Cure,
                ConsumableEffectType.DamageEnemy => SoundEffect.Fire,
                _ => SoundEffect.Magic
            }
        };
    }

    private static void AppendEnemyDefeatStep(
        BattleEncounter encounter,
        List<BattleSequenceStep> steps,
        UiLanguage language,
        string japaneseSuffix,
        string englishSuffix)
    {
        var enemyName = GetEnemyName(encounter, language);
        steps.Add(new BattleSequenceStep
        {
            Message = language == UiLanguage.English
                ? $"{enemyName}{englishSuffix}"
                : $"{enemyName}{japaneseSuffix}",
            VisualCue = BattleVisualCue.EnemyDefeat,
            AnimationFrames = 16
        });
    }

    private static string FormatEnemyDamageMessage(UiLanguage language, string enemyName, int damage, bool enemyDefeated)
    {
        if (enemyDefeated)
        {
            return Text(language, $"{damage}ダメージ！", $"{damage} damage!");
        }

        return Text(language, $"{enemyName}に{damage}ダメージ！", $"{enemyName} takes {damage} damage!");
    }

    private static BattleTurnResolution Victory(List<BattleSequenceStep> steps)
    {
        return new BattleTurnResolution
        {
            Outcome = BattleOutcome.Victory,
            Steps = steps
        };
    }

    private static BattleTurnResolution BuildResolution(PlayerProgress player, List<BattleSequenceStep> steps)
    {
        return new BattleTurnResolution
        {
            Outcome = player.CurrentHp == 0 ? BattleOutcome.Defeat : BattleOutcome.Ongoing,
            Steps = steps
        };
    }

    private static BattleTurnResolution Reject(UiLanguage language, string japaneseMessage, string englishMessage)
    {
        return new BattleTurnResolution
        {
            Outcome = BattleOutcome.Invalid,
            ActionAccepted = false,
            Steps =
            [
                new BattleSequenceStep
                {
                    Message = Text(language, japaneseMessage, englishMessage)
                }
            ]
        };
    }

    private static string GetPlayerName(PlayerProgress player)
    {
        if (!string.IsNullOrWhiteSpace(player.Name))
        {
            return player.Name;
        }

        return player.Language == UiLanguage.English ? "Adventurer" : "ぼうけんしゃ";
    }

    private static string GetEnemyName(BattleEncounter encounter, UiLanguage language)
    {
        return GetEnemyName(encounter.Enemy, language);
    }

    private static string GetEnemyName(EnemyDefinition enemy, UiLanguage language)
    {
        return GameContent.GetEnemyName(enemy, language);
    }

    private static string Text(UiLanguage language, string japanese, string english)
    {
        return language == UiLanguage.English ? english : japanese;
    }

    private static WeaponDefinition? GetEquippedWeapon(PlayerProgress player)
    {
        return GameContent.GetWeaponById(player.EquippedWeaponId);
    }

    private static IEnumerable<ArmorDefinition> GetEquippedArmors(PlayerProgress player)
    {
        foreach (var slot in ArmorSlots)
        {
            var armor = GameContent.GetArmorById(player.GetEquippedItemId(slot));
            if (armor is not null)
            {
                yield return armor;
            }
        }
    }
}
