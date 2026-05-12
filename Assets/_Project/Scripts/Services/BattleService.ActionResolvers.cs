using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed partial class BattleService
{
    private BattleTurnResolution ResolveAttack(
        PlayerProgress player,
        BattleEncounter encounter,
        Random random)
    {
        var language = player.Language;
        var enemyName = GetEnemyName(encounter, language);
        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = Text(language, $"{GetPlayerName(player)}の こうげき！", $"{GetPlayerName(player)} attacks!"),
                VisualCue = BattleVisualCue.PlayerAction,
                AnimationFrames = 8,
                SoundEffect = SoundEffect.Attack
            }
        };

        var damage = Math.Max(1, GetPlayerAttack(player) + random.Next(2, 6) - encounter.Enemy.Defense);
        encounter.CurrentHp = Math.Max(0, encounter.CurrentHp - damage);
        var enemyDefeated = encounter.CurrentHp == 0;
        steps.Add(new BattleSequenceStep
        {
            Message = FormatEnemyDamageMessage(language, enemyName, damage, enemyDefeated),
            VisualCue = BattleVisualCue.EnemyHit,
            AnimationFrames = 10
        });

        if (enemyDefeated)
        {
            AppendEnemyDefeatStep(encounter, steps, language, "をたおした！", " was defeated!");
            return Victory(steps);
        }

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveSpell(
        PlayerProgress player,
        BattleEncounter encounter,
        SpellDefinition? selectedSpell,
        Random random)
    {
        var language = player.Language;
        var spell = selectedSpell ?? GetKnownSpells(player).FirstOrDefault();
        if (spell is null)
        {
            return Reject(language, "まだ じゅもんを おぼえていない。", "You do not know any spells yet.");
        }

        if (player.Level < spell.MinimumLevel)
        {
            return Reject(language, "その じゅもんは まだ おぼえていない。", "You have not learned that spell yet.");
        }

        if (player.CurrentMp < spell.MpCost)
        {
            return Reject(language, "MPが たりない！", "Not enough MP!");
        }

        return spell.EffectType switch
        {
            SpellEffectType.DamageEnemy => ResolveDamageSpell(player, encounter, spell, random),
            SpellEffectType.HealPlayer => ResolveHealSpell(player, encounter, spell, random),
            SpellEffectType.PoisonEnemy => ResolveEnemyStatusSpell(player, encounter, spell, BattleStatusEffect.Poison, random),
            SpellEffectType.SleepEnemy => ResolveEnemyStatusSpell(player, encounter, spell, BattleStatusEffect.Sleep, random),
            SpellEffectType.CurePlayerStatus => ResolveCureSpell(player, encounter, spell, random),
            _ => Reject(language, "その じゅもんは まだ つかえない。", "That spell cannot be used yet.")
        };
    }

    private BattleTurnResolution ResolveDamageSpell(
        PlayerProgress player,
        BattleEncounter encounter,
        SpellDefinition spell,
        Random random)
    {
        var language = player.Language;
        var enemyName = GetEnemyName(encounter, language);
        player.CurrentMp -= spell.MpCost;
        var damage = Math.Max(1, spell.Power + (player.Level * 2) + random.Next(3, 8) - Math.Max(0, encounter.Enemy.Defense / 2));
        encounter.CurrentHp = Math.Max(0, encounter.CurrentHp - damage);

        var steps = CreateSpellCastSteps(player, spell);
        var enemyDefeated = encounter.CurrentHp == 0;
        steps.Add(new BattleSequenceStep
        {
            Message = FormatEnemyDamageMessage(language, enemyName, damage, enemyDefeated),
            VisualCue = BattleVisualCue.EnemyHit,
            AnimationFrames = 12
        });

        if (enemyDefeated)
        {
            AppendEnemyDefeatStep(encounter, steps, language, "を やきはらった！", " was burned away!");
            return Victory(steps);
        }

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveHealSpell(
        PlayerProgress player,
        BattleEncounter encounter,
        SpellDefinition spell,
        Random random)
    {
        var language = player.Language;
        if (player.CurrentHp >= player.MaxHp)
        {
            return Reject(language, "HPは もう まんたんだ。", "HP is already full.");
        }

        player.CurrentMp -= spell.MpCost;
        var healed = Math.Min(spell.Power + player.Level + random.Next(0, 5), player.MaxHp - player.CurrentHp);
        player.CurrentHp += healed;

        var steps = CreateSpellCastSteps(player, spell);
        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, $"HPが {healed}かいふくした！", $"Recovered {healed} HP!"),
            VisualCue = BattleVisualCue.PlayerHeal,
            AnimationFrames = 14
        });

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveEnemyStatusSpell(
        PlayerProgress player,
        BattleEncounter encounter,
        SpellDefinition spell,
        BattleStatusEffect statusEffect,
        Random random)
    {
        var language = player.Language;
        var enemyName = GetEnemyName(encounter, language);
        player.CurrentMp -= spell.MpCost;
        var steps = CreateSpellCastSteps(player, spell);

        var landed = random.Next(100) < spell.AccuracyPercent;
        if (!landed)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, "しかし きかなかった！", "But it had no effect!"),
                VisualCue = BattleVisualCue.EnemyStatus,
                AnimationFrames = 12
            });
        }
        else
        {
            encounter.EnemyStatusEffect = statusEffect;
            encounter.EnemyStatusTurnsRemaining = Math.Max(1, spell.DurationTurns);
            if (statusEffect == BattleStatusEffect.Poison)
            {
                encounter.EnemyPoisonPower = Math.Max(1, spell.Power + Math.Max(0, player.Level / 2));
            }

            steps.Add(new BattleSequenceStep
            {
                Message = statusEffect == BattleStatusEffect.Poison
                    ? Text(language, $"{enemyName}は どくに おかされた！", $"{enemyName} was poisoned!")
                    : Text(language, $"{enemyName}は ねむってしまった！", $"{enemyName} fell asleep!"),
                VisualCue = BattleVisualCue.EnemyStatus,
                AnimationFrames = 18
            });
        }

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveCureSpell(
        PlayerProgress player,
        BattleEncounter encounter,
        SpellDefinition spell,
        Random random)
    {
        var language = player.Language;
        if (encounter.PlayerStatusEffect == BattleStatusEffect.None)
        {
            return Reject(language, "なおす 状態異常がない。", "There is no status effect to cure.");
        }

        player.CurrentMp -= spell.MpCost;
        encounter.PlayerStatusEffect = BattleStatusEffect.None;
        encounter.PlayerStatusTurnsRemaining = 0;
        encounter.PlayerPoisonPower = 0;

        var steps = CreateSpellCastSteps(player, spell);
        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, "からだが すっきりした！", "Your body feels clear!"),
            VisualCue = BattleVisualCue.PlayerHeal,
            AnimationFrames = 14
        });

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveDefend(
        PlayerProgress player,
        BattleEncounter encounter,
        Random random)
    {
        var language = player.Language;
        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = Text(language, $"{GetPlayerName(player)}は みをまもっている！", $"{GetPlayerName(player)} guards!"),
                VisualCue = BattleVisualCue.PlayerGuard,
                AnimationFrames = 12,
                SoundEffect = SoundEffect.Defend
            }
        };

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random, isDefending: true);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveItem(
        PlayerProgress player,
        BattleEncounter encounter,
        ConsumableDefinition? selectedConsumable,
        Random random)
    {
        var language = player.Language;
        if (selectedConsumable is null)
        {
            return Reject(language, "つかえる どうぐがない。", "You have no usable items.");
        }

        if (player.GetItemCount(selectedConsumable.Id) <= 0)
        {
            return Reject(language, "その どうぐは もっていない。", "You do not have that item.");
        }

        var itemName = GameContent.GetConsumableName(selectedConsumable, language);
        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = Text(language, $"{GetPlayerName(player)}は {itemName}を つかった！", $"{GetPlayerName(player)} used {itemName}!"),
                VisualCue = BattleVisualCue.ItemUse,
                AnimationFrames = 8,
                SoundEffect = GetConsumableSoundEffect(selectedConsumable)
            }
        };

        switch (selectedConsumable.EffectType)
        {
            case ConsumableEffectType.HealHp:
            {
                if (player.CurrentHp >= player.MaxHp)
                {
                    return Reject(language, "HPは もう まんたんだ。", "HP is already full.");
                }

                player.RemoveItem(selectedConsumable.Id);
                var healed = Math.Min(selectedConsumable.Amount, player.MaxHp - player.CurrentHp);
                player.CurrentHp += healed;
                steps.Add(new BattleSequenceStep
                {
                    Message = Text(language, $"HPが {healed}かいふくした！", $"Recovered {healed} HP!"),
                    VisualCue = BattleVisualCue.PlayerHeal,
                    AnimationFrames = 12
                });

                if (TryAppendEnemyPoisonTick(encounter, steps, language))
                {
                    return Victory(steps);
                }

                AppendEnemyCounter(player, encounter, steps, random);
                return BuildResolution(player, steps);
            }
            case ConsumableEffectType.HealMp:
            {
                if (player.CurrentMp >= player.MaxMp)
                {
                    return Reject(language, "MPは もう まんたんだ。", "MP is already full.");
                }

                player.RemoveItem(selectedConsumable.Id);
                var restored = Math.Min(selectedConsumable.Amount, player.MaxMp - player.CurrentMp);
                player.CurrentMp += restored;
                steps.Add(new BattleSequenceStep
                {
                    Message = Text(language, $"MPが {restored}かいふくした！", $"Recovered {restored} MP!"),
                    VisualCue = BattleVisualCue.MpRecover,
                    AnimationFrames = 12
                });

                if (TryAppendEnemyPoisonTick(encounter, steps, language))
                {
                    return Victory(steps);
                }

                AppendEnemyCounter(player, encounter, steps, random);
                return BuildResolution(player, steps);
            }
            case ConsumableEffectType.DamageEnemy:
            {
                player.RemoveItem(selectedConsumable.Id);
                var enemyName = GetEnemyName(encounter, language);
                var damage = Math.Max(1, selectedConsumable.Amount + random.Next(-2, 4) - encounter.Enemy.Defense);
                encounter.CurrentHp = Math.Max(0, encounter.CurrentHp - damage);
                var enemyDefeated = encounter.CurrentHp == 0;
                steps.Add(new BattleSequenceStep
                {
                    Message = FormatEnemyDamageMessage(language, enemyName, damage, enemyDefeated),
                    VisualCue = BattleVisualCue.EnemyHit,
                    AnimationFrames = 12
                });

                if (enemyDefeated)
                {
                    AppendEnemyDefeatStep(encounter, steps, language, "を ふきとばした！", " was blown away!");
                    return Victory(steps);
                }

                if (TryAppendEnemyPoisonTick(encounter, steps, language))
                {
                    return Victory(steps);
                }

                AppendEnemyCounter(player, encounter, steps, random);
                return BuildResolution(player, steps);
            }
            default:
                return Reject(language, "その どうぐは まだ つかえない。", "That item cannot be used yet.");
        }
    }

    private BattleTurnResolution ResolveEquip(
        PlayerProgress player,
        BattleEncounter encounter,
        IEquipmentDefinition? selectedEquipment,
        Random random)
    {
        var language = player.Language;
        if (selectedEquipment is null)
        {
            return Reject(language, "そうびできる ものがない。", "There is no gear to equip.");
        }

        if (player.GetItemCount(selectedEquipment.Id) <= 0)
        {
            return Reject(language, "その そうびは もっていない。", "You do not have that gear.");
        }

        var equipmentName = GameContent.GetEquipmentName(selectedEquipment, language);
        if (string.Equals(player.GetEquippedItemId(selectedEquipment.Slot), selectedEquipment.Id, StringComparison.Ordinal))
        {
            return Reject(language, $"{equipmentName}は もう そうびしている。", $"{equipmentName} is already equipped.");
        }

        player.SetEquippedItemId(selectedEquipment.Slot, selectedEquipment.Id);

        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = Text(language, $"{GetPlayerName(player)}は {equipmentName}を そうびした！", $"{GetPlayerName(player)} equipped {equipmentName}!"),
                VisualCue = BattleVisualCue.PlayerGuard,
                AnimationFrames = 10,
                SoundEffect = SoundEffect.Equip
            }
        };

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveEscape(PlayerProgress player)
    {
        var language = player.Language;
        return new BattleTurnResolution
        {
            Outcome = BattleOutcome.Escaped,
            Steps =
            [
                new BattleSequenceStep
                {
                    Message = Text(language, "うまく にげきった！", "You got away safely!"),
                    VisualCue = BattleVisualCue.ItemUse,
                    AnimationFrames = 8,
                    SoundEffect = SoundEffect.Escape
                }
            ]
        };
    }

    private BattleTurnResolution ResolvePlayerSleepTurn(
        PlayerProgress player,
        BattleEncounter encounter,
        Random random)
    {
        var language = player.Language;
        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = Text(language, $"{GetPlayerName(player)}は ねむっている。", $"{GetPlayerName(player)} is asleep."),
                VisualCue = BattleVisualCue.PlayerStatus,
                AnimationFrames = 12
            }
        };

        encounter.PlayerStatusTurnsRemaining--;
        if (encounter.PlayerStatusTurnsRemaining <= 0)
        {
            encounter.PlayerStatusEffect = BattleStatusEffect.None;
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, $"{GetPlayerName(player)}は めをさました！", $"{GetPlayerName(player)} woke up!")
            });
        }

        if (TryAppendEnemyPoisonTick(encounter, steps, language))
        {
            return Victory(steps);
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private void AppendEnemyCounter(
        PlayerProgress player,
        BattleEncounter encounter,
        List<BattleSequenceStep> steps,
        Random random,
        bool isDefending = false)
    {
        var language = player.Language;
        var enemyName = GetEnemyName(encounter, language);
        if (TryAppendEnemySleepSkip(encounter, steps, language))
        {
            TryAppendPlayerPoisonTick(player, encounter, steps, language);
            return;
        }

        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, $"{enemyName}の こうげき！", $"{enemyName} attacks!"),
            VisualCue = BattleVisualCue.EnemyAction,
            AnimationFrames = 10,
            SoundEffect = SoundEffect.Attack
        });

        var enemyDamage = Math.Max(1, encounter.Enemy.Attack + random.Next(1, 5) - GetPlayerDefense(player));
        if (isDefending)
        {
            enemyDamage = Math.Max(1, (int)Math.Ceiling(enemyDamage / 2f));
        }

        player.CurrentHp = Math.Max(0, player.CurrentHp - enemyDamage);
        steps.Add(new BattleSequenceStep
        {
            Message = isDefending
                ? Text(language, $"{enemyDamage}ダメージに おさえた！", $"Reduced damage to {enemyDamage}!")
                : Text(language, $"{enemyDamage}ダメージを うけた！", $"Took {enemyDamage} damage!"),
            VisualCue = BattleVisualCue.PlayerHit,
            AnimationFrames = 10
        });

        if (player.CurrentHp == 0)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, "めのまえが まっくらになった…", "Everything went dark...")
            });
            return;
        }

        TryInflictPlayerStatus(player, encounter, steps, random, language);
        TryAppendPlayerPoisonTick(player, encounter, steps, language);
    }
}
