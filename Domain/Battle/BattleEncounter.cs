namespace DragonGlareAlpha.Domain.Battle;

public sealed class BattleEncounter
{
    public BattleEncounter(EnemyDefinition enemy)
    {
        Enemy = enemy;
        CurrentHp = enemy.MaxHp;
    }

    public EnemyDefinition Enemy { get; }

    public int CurrentHp { get; set; }
}
