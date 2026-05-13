namespace DragonGlare.Domain.Battle;

public sealed class BattleEncounter
{
    public BattleEncounter(EnemyDefinition enemy)
    {
        Enemy = enemy;
        CurrentHp = enemy.MaxHp;
    }

    public EnemyDefinition Enemy { get; }

    public BattleStatusEffect EnemyStatusEffect { get; set; }

    public int EnemyStatusTurnsRemaining { get; set; }

    public int EnemyPoisonPower { get; set; }

    public BattleStatusEffect PlayerStatusEffect { get; set; }

    public int PlayerStatusTurnsRemaining { get; set; }

    public int PlayerPoisonPower { get; set; }

    public int CurrentHp { get; set; }
}
