using DragonGlare.Data;
using DragonGlare.Domain;
using DragonGlare.Domain.Battle;
using DragonGlare.Domain.Items;
using DragonGlare.Domain.Player;

namespace DragonGlare.Services;

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
                Message = Text(language, $"{GetPlayerName(player)}縺ｮ 縺薙≧縺偵″・・, $"{GetPlayerName(player)} attacks!"),
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
            AppendEnemyDefeatStep(encounter, steps, language, "繧偵◆縺翫＠縺滂ｼ・, " was defeated!");
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
            return Reject(language, "縺ｾ縺 縺倥ｅ繧ゅｓ繧・縺翫⊂縺医※縺・↑縺・・, "You do not know any spells yet.");
        }

        if (player.Level < spell.MinimumLevel)
        {
            return Reject(language, "縺昴・ 縺倥ｅ繧ゅｓ縺ｯ 縺ｾ縺 縺翫⊂縺医※縺・↑縺・・, "You have not learned that spell yet.");
        }

        if (player.CurrentMp < spell.MpCost)
        {
            return Reject(language, "MP縺・縺溘ｊ縺ｪ縺・ｼ・, "Not enough MP!");
        }

        return spell.EffectType switch
        {
            SpellEffectType.DamageEnemy => ResolveDamageSpell(player, encounter, spell, random),
            SpellEffectType.HealPlayer => ResolveHealSpell(player, encounter, spell, random),
            SpellEffectType.PoisonEnemy => ResolveEnemyStatusSpell(player, encounter, spell, BattleStatusEffect.Poison, random),
            SpellEffectType.SleepEnemy => ResolveEnemyStatusSpell(player, encounter, spell, BattleStatusEffect.Sleep, random),
            SpellEffectType.CurePlayerStatus => ResolveCureSpell(player, encounter, spell, random),
            _ => Reject(language, "縺昴・ 縺倥ｅ繧ゅｓ縺ｯ 縺ｾ縺 縺､縺九∴縺ｪ縺・・, "That spell cannot be used yet.")
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
            AppendEnemyDefeatStep(encounter, steps, language, "繧・繧・″縺ｯ繧峨▲縺滂ｼ・, " was burned away!");
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
            return Reject(language, "HP縺ｯ 繧ゅ≧ 縺ｾ繧薙◆繧薙□縲・, "HP is already full.");
        }

        player.CurrentMp -= spell.MpCost;
        var healed = Math.Min(spell.Power + player.Level + random.Next(0, 5), player.MaxHp - player.CurrentHp);
        player.CurrentHp += healed;

        var steps = CreateSpellCastSteps(player, spell);
        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, $"HP縺・{healed}縺九＞縺ｵ縺上＠縺滂ｼ・, $"Recovered {healed} HP!"),
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
                Message = Text(language, "縺励°縺・縺阪°縺ｪ縺九▲縺滂ｼ・, "But it had no effect!"),
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
                    ? Text(language, $"{enemyName}縺ｯ 縺ｩ縺上↓ 縺翫°縺輔ｌ縺滂ｼ・, $"{enemyName} was poisoned!")
                    : Text(language, $"{enemyName}縺ｯ 縺ｭ繧縺｣縺ｦ縺励∪縺｣縺滂ｼ・, $"{enemyName} fell asleep!"),
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
            return Reject(language, "縺ｪ縺翫☆ 迥ｶ諷狗焚蟶ｸ縺後↑縺・・, "There is no status effect to cure.");
        }

        player.CurrentMp -= spell.MpCost;
        encounter.PlayerStatusEffect = BattleStatusEffect.None;
        encounter.PlayerStatusTurnsRemaining = 0;
        encounter.PlayerPoisonPower = 0;

        var steps = CreateSpellCastSteps(player, spell);
        steps.Add(new BattleSequenceStep
        {
            Message = Text(language, "縺九ｉ縺縺・縺吶▲縺阪ｊ縺励◆・・, "Your body feels clear!"),
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
                Message = Text(language, $"{GetPlayerName(player)}縺ｯ 縺ｿ繧偵∪繧ゅ▲縺ｦ縺・ｋ・・, $"{GetPlayerName(player)} guards!"),
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
            return Reject(language, "縺､縺九∴繧・縺ｩ縺・＄縺後↑縺・・, "You have no usable items.");
        }

        if (player.GetItemCount(selectedConsumable.Id) <= 0)
        {
            return Reject(language, "縺昴・ 縺ｩ縺・＄縺ｯ 繧ゅ▲縺ｦ縺・↑縺・・, "You do not have that item.");
        }

        var itemName = GameContent.GetConsumableName(selectedConsumable, language);
        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = Text(language, $"{GetPlayerName(player)}縺ｯ {itemName}繧・縺､縺九▲縺滂ｼ・, $"{GetPlayerName(player)} used {itemName}!"),
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
                    return Reject(language, "HP縺ｯ 繧ゅ≧ 縺ｾ繧薙◆繧薙□縲・, "HP is already full.");
                }

                player.RemoveItem(selectedConsumable.Id);
                var healed = Math.Min(selectedConsumable.Amount, player.MaxHp - player.CurrentHp);
                player.CurrentHp += healed;
                steps.Add(new BattleSequenceStep
                {
                    Message = Text(language, $"HP縺・{healed}縺九＞縺ｵ縺上＠縺滂ｼ・, $"Recovered {healed} HP!"),
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
                    return Reject(language, "MP縺ｯ 繧ゅ≧ 縺ｾ繧薙◆繧薙□縲・, "MP is already full.");
                }

                player.RemoveItem(selectedConsumable.Id);
                var restored = Math.Min(selectedConsumable.Amount, player.MaxMp - player.CurrentMp);
                player.CurrentMp += restored;
                steps.Add(new BattleSequenceStep
                {
                    Message = Text(language, $"MP縺・{restored}縺九＞縺ｵ縺上＠縺滂ｼ・, $"Recovered {restored} MP!"),
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
                    AppendEnemyDefeatStep(encounter, steps, language, "繧・縺ｵ縺阪→縺ｰ縺励◆・・, " was blown away!");
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
                return Reject(language, "縺昴・ 縺ｩ縺・＄縺ｯ 縺ｾ縺 縺､縺九∴縺ｪ縺・・, "That item cannot be used yet.");
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
            return Reject(language, "縺昴≧縺ｳ縺ｧ縺阪ｋ 繧ゅ・縺後↑縺・・, "There is no gear to equip.");
        }

        if (player.GetItemCount(selectedEquipment.Id) <= 0)
        {
            return Reject(language, "縺昴・ 縺昴≧縺ｳ縺ｯ 繧ゅ▲縺ｦ縺・↑縺・・, "You do not have that gear.");
        }

        var equipmentName = GameContent.GetEquipmentName(selectedEquipment, language);
        if (string.Equals(player.GetEquippedItemId(selectedEquipment.Slot), selectedEquipment.Id, StringComparison.Ordinal))
        {
            return Reject(language, $"{equipmentName}縺ｯ 繧ゅ≧ 縺昴≧縺ｳ縺励※縺・ｋ縲・, $"{equipmentName} is already equipped.");
        }

        player.SetEquippedItemId(selectedEquipment.Slot, selectedEquipment.Id);

        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = Text(language, $"{GetPlayerName(player)}縺ｯ {equipmentName}繧・縺昴≧縺ｳ縺励◆・・, $"{GetPlayerName(player)} equipped {equipmentName}!"),
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
                    Message = Text(language, "縺・∪縺・縺ｫ縺偵″縺｣縺滂ｼ・, "You got away safely!"),
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
                Message = Text(language, $"{GetPlayerName(player)}縺ｯ 縺ｭ繧縺｣縺ｦ縺・ｋ縲・, $"{GetPlayerName(player)} is asleep."),
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
                Message = Text(language, $"{GetPlayerName(player)}縺ｯ 繧√ｒ縺輔∪縺励◆・・, $"{GetPlayerName(player)} woke up!")
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
            Message = Text(language, $"{enemyName}縺ｮ 縺薙≧縺偵″・・, $"{enemyName} attacks!"),
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
                ? Text(language, $"{enemyDamage}繝繝｡繝ｼ繧ｸ縺ｫ 縺翫＆縺医◆・・, $"Reduced damage to {enemyDamage}!")
                : Text(language, $"{enemyDamage}繝繝｡繝ｼ繧ｸ繧・縺・￠縺滂ｼ・, $"Took {enemyDamage} damage!"),
            VisualCue = BattleVisualCue.PlayerHit,
            AnimationFrames = 10
        });

        if (player.CurrentHp == 0)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, "繧√・縺ｾ縺医′ 縺ｾ縺｣縺上ｉ縺ｫ縺ｪ縺｣縺溪ｦ", "Everything went dark...")
            });
            return;
        }

        TryInflictPlayerStatus(player, encounter, steps, random, language);
        TryAppendPlayerPoisonTick(player, encounter, steps, language);
    }
}
