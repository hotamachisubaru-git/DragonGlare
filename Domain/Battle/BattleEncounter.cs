using DragonGlareAlpha.Security;

namespace DragonGlareAlpha.Domain.Battle;

public sealed class BattleEncounter
{
    private readonly ProtectedInt currentHp = new();

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

    public int CurrentHp
    {
        get => currentHp.Value;
        set => currentHp.Value = value;
    }

    public void ValidateIntegrity()
    {
        currentHp.Validate();
    }

    public void RekeySensitiveValues()
    {
        currentHp.Rekey();
    }
}
