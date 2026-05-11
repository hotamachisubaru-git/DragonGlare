using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class BattleService
{
    private const int MinimumEncounterPoolSize = 2;
    private static readonly EquipmentSlot[] ArmorSlots =
    [
        EquipmentSlot.Armor,
        EquipmentSlot.Head,
        EquipmentSlot.Arms,
        EquipmentSlot.Legs,
        EquipmentSlot.Feet
    ];

    public BattleEncounter CreateEncounter(Random random, FieldMapId encounterMap, int playerLevel)
    {
        var pool = GetEncounterPool(encounterMap, playerLevel);
        var enemy = SelectEnemyFromPool(random, pool);
        return new BattleEncounter(enemy);
    }

    public IReadOnlyList<EnemyDefinition> GetEncounterPool(FieldMapId encounterMap, int playerLevel)
    {
        var enemyMap = encounterMap == FieldMapId.Dungeon ? FieldMapId.Castle : encounterMap;
        var mapPool = GameContent.EnemyCatalog
            .Where(enemy => enemy.EncounterMap == enemyMap)
            .ToArray();

        if (mapPool.Length == 0)
        {
            return GameContent.EnemyCatalog;
        }

        var levelPool = mapPool
            .Where(enemy => playerLevel >= enemy.MinRecommendedLevel && playerLevel <= enemy.MaxRecommendedLevel)
            .ToArray();

        var targetPoolSize = Math.Min(MinimumEncounterPoolSize, mapPool.Length);
        if (levelPool.Length >= targetPoolSize)
        {
            return levelPool;
        }

        return levelPool
            .Concat(mapPool
                .Except(levelPool)
                .OrderBy(enemy => GetRecommendedLevelDistance(enemy, playerLevel))
                .ThenBy(enemy => enemy.MinRecommendedLevel)
                .ThenBy(enemy => enemy.Id, StringComparer.Ordinal))
            .Take(targetPoolSize)
            .ToArray();
    }

    public IReadOnlyList<SpellDefinition> GetKnownSpells(PlayerProgress player)
    {
        return GameContent.SpellCatalog
            .Where(spell => player.Level >= spell.MinimumLevel)
            .OrderBy(spell => spell.MinimumLevel)
            .ThenBy(spell => spell.MpCost)
            .ToArray();
    }

    public BattleTurnResolution ResolveTurn(
        PlayerProgress player,
        BattleEncounter encounter,
        BattleActionType action,
        ConsumableDefinition? selectedConsumable,
        IEquipmentDefinition? selectedEquipment,
        Random random)
    {
        return ResolveTurn(player, encounter, action, null, selectedConsumable, selectedEquipment, random);
    }

    public BattleTurnResolution ResolveTurn(
        PlayerProgress player,
        BattleEncounter encounter,
        BattleActionType action,
        SpellDefinition? selectedSpell,
        ConsumableDefinition? selectedConsumable,
        IEquipmentDefinition? selectedEquipment,
        Random random)
    {
        if (encounter.PlayerStatusEffect == BattleStatusEffect.Sleep &&
            encounter.PlayerStatusTurnsRemaining > 0)
        {
            return ResolvePlayerSleepTurn(player, encounter, random);
        }

        return action switch
        {
            BattleActionType.Attack => ResolveAttack(player, encounter, random),
            BattleActionType.Spell => ResolveSpell(player, encounter, selectedSpell, random),
            BattleActionType.Defend => ResolveDefend(player, encounter, random),
            BattleActionType.Item => ResolveItem(player, encounter, selectedConsumable, random),
            BattleActionType.Equip => ResolveEquip(player, encounter, selectedEquipment, random),
            BattleActionType.Run => ResolveEscape(player),
            _ => Reject(player.Language, "こうどうできない。", "You cannot act.")
        };
    }

    public int GetPlayerAttack(PlayerProgress player, WeaponDefinition? equippedWeapon = null)
    {
        var weapon = equippedWeapon ?? GetEquippedWeapon(player);
        return player.BaseAttack + player.Level + (weapon?.AttackBonus ?? 0);
    }

    public int GetPlayerDefense(PlayerProgress player, ArmorDefinition? replacementArmor = null)
    {
        var defenseBonus = GetEquippedArmors(player)
            .Where(armor => replacementArmor is null || armor.Slot != replacementArmor.Slot)
            .Sum(armor => armor.DefenseBonus);

        defenseBonus += replacementArmor?.DefenseBonus ?? 0;
        return player.BaseDefense + Math.Max(0, player.Level / 2) + defenseBonus;
    }

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
            Message = Text(language, $"どくが {enemyName}を むしばんだ！ {damage}ダメージ！", $"Poison eats at {enemyName}! {damage} damage!"),
            VisualCue = BattleVisualCue.PoisonTick,
            AnimationFrames = 14,
            SoundEffect = SoundEffect.Poison
        });

        if (encounter.CurrentHp == 0)
        {
            AppendEnemyDefeatStep(encounter, steps, language, "は どくで たおれた！", " collapsed from poison!");
            return true;
        }

        if (encounter.EnemyStatusTurnsRemaining <= 0)
        {
            encounter.EnemyStatusEffect = BattleStatusEffect.None;
            encounter.EnemyPoisonPower = 0;
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, $"{enemyName}の どくが きえた。", $"{enemyName}'s poison faded.")
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
            Message = Text(language, $"{enemyName}は ねむっている。", $"{enemyName} is asleep."),
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
                Message = Text(language, $"{enemyName}は めをさました！", $"{enemyName} woke up!")
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
                ? Text(language, $"{GetPlayerName(player)}は どくを うけた！", $"{GetPlayerName(player)} was poisoned!")
                : Text(language, $"{GetPlayerName(player)}は ねむってしまった！", $"{GetPlayerName(player)} fell asleep!"),
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
            Message = Text(language, $"どくで {damage}ダメージを うけた！", $"Poison deals {damage} damage!"),
            VisualCue = BattleVisualCue.PoisonTick,
            AnimationFrames = 12,
            SoundEffect = SoundEffect.Poison
        });

        if (player.CurrentHp == 0)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, "めのまえが まっくらになった…", "Everything went dark...")
            });
            return;
        }

        if (encounter.PlayerStatusTurnsRemaining <= 0)
        {
            encounter.PlayerStatusEffect = BattleStatusEffect.None;
            encounter.PlayerPoisonPower = 0;
            steps.Add(new BattleSequenceStep
            {
                Message = Text(language, "どくが きえた。", "The poison faded.")
            });
        }
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

    private static EnemyDefinition SelectEnemyFromPool(Random random, IReadOnlyList<EnemyDefinition> pool)
    {
        if (pool.Count == 0)
        {
            throw new InvalidOperationException("Encounter pool must contain at least one enemy.");
        }

        var totalWeight = pool.Sum(enemy => Math.Max(1, enemy.EncounterWeight));
        var roll = random.Next(totalWeight);
        foreach (var enemy in pool)
        {
            roll -= Math.Max(1, enemy.EncounterWeight);
            if (roll < 0)
            {
                return enemy;
            }
        }

        return pool[^1];
    }

    private static int GetRecommendedLevelDistance(EnemyDefinition enemy, int playerLevel)
    {
        if (playerLevel < enemy.MinRecommendedLevel)
        {
            return enemy.MinRecommendedLevel - playerLevel;
        }

        return playerLevel > enemy.MaxRecommendedLevel
            ? playerLevel - enemy.MaxRecommendedLevel
            : 0;
    }
}
