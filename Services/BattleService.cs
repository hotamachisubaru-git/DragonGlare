using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Services;

public sealed class BattleService
{
    private const int SpellCost = 2;

    public BattleEncounter CreateEncounter(Random random)
    {
        var enemy = GameContent.EnemyCatalog[random.Next(GameContent.EnemyCatalog.Length)];
        return new BattleEncounter(enemy);
    }

    public BattleTurnResolution ResolveTurn(
        PlayerProgress player,
        BattleEncounter encounter,
        BattleActionType action,
        WeaponDefinition? equippedWeapon,
        ConsumableDefinition? selectedConsumable,
        Random random)
    {
        return action switch
        {
            BattleActionType.Attack => ResolveAttack(player, encounter, equippedWeapon, random),
            BattleActionType.Spell => ResolveSpell(player, encounter, random),
            BattleActionType.Item => ResolveItem(player, encounter, selectedConsumable, equippedWeapon, random),
            BattleActionType.Run => ResolveEscape(),
            _ => Reject("こうどうできない。")
        };
    }

    public int GetPlayerAttack(PlayerProgress player, WeaponDefinition? equippedWeapon)
    {
        return player.BaseAttack + player.Level + (equippedWeapon?.AttackBonus ?? 0);
    }

    public int GetPlayerDefense(PlayerProgress player)
    {
        return player.BaseDefense + Math.Max(0, player.Level / 2);
    }

    private BattleTurnResolution ResolveAttack(
        PlayerProgress player,
        BattleEncounter encounter,
        WeaponDefinition? equippedWeapon,
        Random random)
    {
        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = $"{GetPlayerName(player)}の こうげき！"
            }
        };

        var damage = Math.Max(1, GetPlayerAttack(player, equippedWeapon) + random.Next(2, 6) - encounter.Enemy.Defense);
        encounter.CurrentHp = Math.Max(0, encounter.CurrentHp - damage);
        steps.Add(new BattleSequenceStep
        {
            Message = $"{encounter.Enemy.Name}に {damage}ダメージ！",
            VisualCue = BattleVisualCue.EnemyHit,
            AnimationFrames = 10
        });

        if (encounter.CurrentHp == 0)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = $"{encounter.Enemy.Name}を たおした！",
                VisualCue = BattleVisualCue.EnemyDefeat,
                AnimationFrames = 16
            });

            return new BattleTurnResolution
            {
                Outcome = BattleOutcome.Victory,
                Steps = steps
            };
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveSpell(PlayerProgress player, BattleEncounter encounter, Random random)
    {
        if (player.CurrentMp < SpellCost)
        {
            return Reject("MPが たりない！");
        }

        player.CurrentMp -= SpellCost;
        var damage = 10 + (player.Level * 2) + random.Next(3, 8);
        encounter.CurrentHp = Math.Max(0, encounter.CurrentHp - damage);

        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = $"{GetPlayerName(player)}は メラを となえた！",
                VisualCue = BattleVisualCue.SpellCast,
                AnimationFrames = 12
            },
            new()
            {
                Message = $"{encounter.Enemy.Name}に {damage}ダメージ！",
                VisualCue = BattleVisualCue.EnemyHit,
                AnimationFrames = 10
            }
        };

        if (encounter.CurrentHp == 0)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = $"{encounter.Enemy.Name}を やきはらった！",
                VisualCue = BattleVisualCue.EnemyDefeat,
                AnimationFrames = 16
            });

            return new BattleTurnResolution
            {
                Outcome = BattleOutcome.Victory,
                Steps = steps
            };
        }

        AppendEnemyCounter(player, encounter, steps, random);
        return BuildResolution(player, steps);
    }

    private BattleTurnResolution ResolveItem(
        PlayerProgress player,
        BattleEncounter encounter,
        ConsumableDefinition? selectedConsumable,
        WeaponDefinition? equippedWeapon,
        Random random)
    {
        if (selectedConsumable is null)
        {
            return Reject("つかえる どうぐがない。");
        }

        if (player.GetItemCount(selectedConsumable.Id) <= 0)
        {
            return Reject("その どうぐは もっていない。");
        }

        var steps = new List<BattleSequenceStep>
        {
            new()
            {
                Message = $"{GetPlayerName(player)}は {selectedConsumable.Name}を つかった！",
                VisualCue = BattleVisualCue.ItemUse,
                AnimationFrames = 8
            }
        };

        switch (selectedConsumable.EffectType)
        {
            case ConsumableEffectType.HealHp:
            {
                if (player.CurrentHp >= player.MaxHp)
                {
                    return Reject("HPは もう まんたんだ。");
                }

                player.RemoveItem(selectedConsumable.Id);
                var healed = Math.Min(selectedConsumable.Amount, player.MaxHp - player.CurrentHp);
                player.CurrentHp += healed;
                steps.Add(new BattleSequenceStep
                {
                    Message = $"HPが {healed}かいふくした！",
                    VisualCue = BattleVisualCue.PlayerHeal,
                    AnimationFrames = 12
                });
                AppendEnemyCounter(player, encounter, steps, random);
                return BuildResolution(player, steps);
            }
            case ConsumableEffectType.HealMp:
            {
                if (player.CurrentMp >= player.MaxMp)
                {
                    return Reject("MPは もう まんたんだ。");
                }

                player.RemoveItem(selectedConsumable.Id);
                var restored = Math.Min(selectedConsumable.Amount, player.MaxMp - player.CurrentMp);
                player.CurrentMp += restored;
                steps.Add(new BattleSequenceStep
                {
                    Message = $"MPが {restored}かいふくした！",
                    VisualCue = BattleVisualCue.MpRecover,
                    AnimationFrames = 12
                });
                AppendEnemyCounter(player, encounter, steps, random);
                return BuildResolution(player, steps);
            }
            case ConsumableEffectType.DamageEnemy:
            {
                player.RemoveItem(selectedConsumable.Id);
                var damage = Math.Max(1, selectedConsumable.Amount + random.Next(-2, 4) - encounter.Enemy.Defense);
                encounter.CurrentHp = Math.Max(0, encounter.CurrentHp - damage);
                steps.Add(new BattleSequenceStep
                {
                    Message = $"{encounter.Enemy.Name}に {damage}ダメージ！",
                    VisualCue = BattleVisualCue.EnemyHit,
                    AnimationFrames = 12
                });

                if (encounter.CurrentHp == 0)
                {
                    steps.Add(new BattleSequenceStep
                    {
                        Message = $"{encounter.Enemy.Name}を ふきとばした！",
                        VisualCue = BattleVisualCue.EnemyDefeat,
                        AnimationFrames = 16
                    });

                    return new BattleTurnResolution
                    {
                        Outcome = BattleOutcome.Victory,
                        Steps = steps
                    };
                }

                AppendEnemyCounter(player, encounter, steps, random);
                return BuildResolution(player, steps);
            }
            default:
                return Reject("その どうぐは まだ つかえない。");
        }
    }

    private static BattleTurnResolution ResolveEscape()
    {
        return new BattleTurnResolution
        {
            Outcome = BattleOutcome.Escaped,
            Steps =
            [
                new BattleSequenceStep
                {
                    Message = "うまく にげきった！",
                    VisualCue = BattleVisualCue.ItemUse,
                    AnimationFrames = 8
                }
            ]
        };
    }

    private void AppendEnemyCounter(
        PlayerProgress player,
        BattleEncounter encounter,
        List<BattleSequenceStep> steps,
        Random random)
    {
        steps.Add(new BattleSequenceStep
        {
            Message = $"{encounter.Enemy.Name}の こうげき！"
        });

        var enemyDamage = Math.Max(1, encounter.Enemy.Attack + random.Next(1, 5) - GetPlayerDefense(player));
        player.CurrentHp = Math.Max(0, player.CurrentHp - enemyDamage);
        steps.Add(new BattleSequenceStep
        {
            Message = $"{enemyDamage}ダメージを うけた！",
            VisualCue = BattleVisualCue.PlayerHit,
            AnimationFrames = 10
        });

        if (player.CurrentHp == 0)
        {
            steps.Add(new BattleSequenceStep
            {
                Message = "めのまえが まっくらになった…"
            });
        }
    }

    private static BattleTurnResolution BuildResolution(PlayerProgress player, List<BattleSequenceStep> steps)
    {
        return new BattleTurnResolution
        {
            Outcome = player.CurrentHp == 0 ? BattleOutcome.Defeat : BattleOutcome.Ongoing,
            Steps = steps
        };
    }

    private static BattleTurnResolution Reject(string message)
    {
        return new BattleTurnResolution
        {
            Outcome = BattleOutcome.Invalid,
            ActionAccepted = false,
            Steps =
            [
                new BattleSequenceStep
                {
                    Message = message
                }
            ]
        };
    }

    private static string GetPlayerName(PlayerProgress player)
    {
        return string.IsNullOrWhiteSpace(player.Name) ? "ぼうけんしゃ" : player.Name;
    }
}
