using DragonGlare.Domain;
using DragonGlare.Domain.Battle;
using DragonGlare.Domain.Player;

namespace DragonGlare.Services;

public sealed partial class BattleService
{
    private bool TryAppendEnemyPoisonTick(
        BattleEncounter encounter,
        List<BattleSequenceStep> steps,
        UiLanguage language)
    {
        if (encounter.EnemyStatusEffect != BattleStatusEffect.Poison ||
            encounter.EnemyStatusTurnsRemaining <= 0)
        {
            return false;
        }

        var enemyName = GetEnemyName(encounter, language);
        var damage = Math.Max(1, encounter.EnemyPoisonPower);
        encounter.CurrentHp = Math.Max(0, encounter.CurrentHp - damage);
        encounter.EnemyStatusTurnsRemaining--;
        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, $"縺ｩ縺上′ {enemyName}繧・繧縺励・繧薙□・・{damage}繝繝｡繝ｼ繧ｸ・・, $"Poison eats at {enemyName}! {damage} damage!"),
            VisualCue = BattleVisualCue.PoisonTick,
            AnimationFrames = 14,
            SoundEffect = SoundEffect.Poison
        });

        if (encounter.CurrentHp == 0)
        {
            AppendEnemyDefeatStep(encounter, steps, language, "縺ｯ 縺ｩ縺上〒 縺溘♀繧後◆・・, " collapsed from poison!");
            return true;
        }

        if (encounter.EnemyStatusTurnsRemaining <= 0)
        {
            encounter.EnemyStatusEffect = BattleStatusEffect.None;
            encounter.EnemyPoisonPower = 0;
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, $"{enemyName}縺ｮ 縺ｩ縺上′ 縺阪∴縺溘・, $"{enemyName}'s poison faded.")
            });
        }

        return false;
    }

    private static bool TryAppendEnemySleepSkip(
        BattleEncounter encounter,
        List<BattleSequenceStep> steps,
        UiLanguage language)
    {
        if (encounter.EnemyStatusEffect != BattleStatusEffect.Sleep ||
            encounter.EnemyStatusTurnsRemaining <= 0)
        {
            return false;
        }

        var enemyName = GetEnemyName(encounter, language);
        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, $"{enemyName}縺ｯ 縺ｭ繧縺｣縺ｦ縺・ｋ縲・, $"{enemyName} is asleep."),
            VisualCue = BattleVisualCue.EnemyStatus,
            AnimationFrames = 14,
            SoundEffect = SoundEffect.Magic
        });

        encounter.EnemyStatusTurnsRemaining--;
        if (encounter.EnemyStatusTurnsRemaining <= 0)
        {
            encounter.EnemyStatusEffect = BattleStatusEffect.None;
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, $"{enemyName}縺ｯ 繧√ｒ縺輔∪縺励◆・・, $"{enemyName} woke up!")
            });
        }

        return true;
    }

    private static void TryInflictPlayerStatus(
        PlayerProgress player,
        BattleEncounter encounter,
        List<BattleSequenceStep> steps,
        Random random,
        UiLanguage language)
    {
        if (encounter.Enemy.AttackStatusEffect == BattleStatusEffect.None ||
            encounter.Enemy.AttackStatusChancePercent <= 0 ||
            encounter.PlayerStatusEffect != BattleStatusEffect.None ||
            random.Next(100) >= encounter.Enemy.AttackStatusChancePercent)
        {
            return;
        }

        encounter.PlayerStatusEffect = encounter.Enemy.AttackStatusEffect;
        encounter.PlayerStatusTurnsRemaining = Math.Max(1, encounter.Enemy.AttackStatusTurns);
        if (encounter.PlayerStatusEffect == BattleStatusEffect.Poison)
        {
            encounter.PlayerPoisonPower = Math.Max(1, encounter.Enemy.Attack / 4);
        }

        steps.Add(new BattleSequenceStep
        {
            Message = encounter.PlayerStatusEffect == BattleStatusEffect.Poison
                ? Text(language, $"{GetPlayerName(player)}縺ｯ 縺ｩ縺上ｒ 縺・￠縺滂ｼ・, $"{GetPlayerName(player)} was poisoned!")
                : Text(language, $"{GetPlayerName(player)}縺ｯ 縺ｭ繧縺｣縺ｦ縺励∪縺｣縺滂ｼ・, $"{GetPlayerName(player)} fell asleep!"),
            VisualCue = BattleVisualCue.PlayerStatus,
            AnimationFrames = 14,
            SoundEffect = encounter.PlayerStatusEffect == BattleStatusEffect.Poison
                ? SoundEffect.Poison
                : SoundEffect.Magic
        });
    }

    private static void TryAppendPlayerPoisonTick(
        PlayerProgress player,
        BattleEncounter encounter,
        List<BattleSequenceStep> steps,
        UiLanguage language)
    {
        if (encounter.PlayerStatusEffect != BattleStatusEffect.Poison ||
            encounter.PlayerStatusTurnsRemaining <= 0 ||
            player.CurrentHp <= 0)
        {
            return;
        }

        var damage = Math.Max(1, encounter.PlayerPoisonPower);
        player.CurrentHp = Math.Max(0, player.CurrentHp - damage);
        encounter.PlayerStatusTurnsRemaining--;
        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, $"縺ｩ縺上〒 {damage}繝繝｡繝ｼ繧ｸ繧・縺・￠縺滂ｼ・, $"Poison deals {damage} damage!"),
            VisualCue = BattleVisualCue.PoisonTick,
            AnimationFrames = 12,
            SoundEffect = SoundEffect.Poison
        });

        if (player.CurrentHp == 0)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, "繧√・縺ｾ縺医′ 縺ｾ縺｣縺上ｉ縺ｫ縺ｪ縺｣縺溪ｦ", "Everything went dark...")
            });
            return;
        }

        if (encounter.PlayerStatusTurnsRemaining <= 0)
        {
            encounter.PlayerStatusEffect = BattleStatusEffect.None;
            encounter.PlayerPoisonPower = 0;
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, "縺ｩ縺上′ 縺阪∴縺溘・, "The poison faded.")
            });
        }
    }
}
