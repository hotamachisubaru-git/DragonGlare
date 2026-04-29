namespace DragonGlareAlpha.Domain;

public enum GameState
{
    ModeSelect,
    LanguageSelection,
    NameInput,
    SaveSlotSelection,
    Field,
    EncounterTransition,
    Battle,
    ShopBuy,
    Bank
}

public enum SaveSlotSelectionMode
{
    Save,
    Load,
    CopySource,
    CopyDestination,
    DeleteSelect,
    DeleteConfirm
}

public enum ShopPhase
{
    Welcome,
    BuyList,
    SellList
}

public enum BankPhase
{
    Welcome,
    DepositList,
    WithdrawList,
    BorrowList
}

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Head,
    Arms,
    Legs,
    Feet
}

public enum UiLanguage
{
    Japanese,
    English
}

public enum FieldMapId
{
    Hub,
    Castle,
    Field
}

public enum BgmTrack
{
    MainMenu,
    Prologue,
    Field,
    Castle,
    Battle,
    Shop
}

public enum SoundEffect
{
    Dialog,
    Collision
}

public enum BattleFlowState
{
    Intro,
    CommandSelection,
    ItemSelection,
    EquipmentSelection,
    Resolving,
    Victory,
    Defeat,
    Escaped
}

public enum BattleActionType
{
    Attack,
    Spell,
    Defend,
    Item,
    Equip,
    Run
}

public enum BattleOutcome
{
    Ongoing,
    Victory,
    Defeat,
    Escaped,
    Invalid
}

public enum BattleVisualCue
{
    None,
    EnemyHit,
    PlayerHit,
    SpellCast,
    PlayerHeal,
    MpRecover,
    EnemyDefeat,
    ItemUse
}

public enum ConsumableEffectType
{
    HealHp,
    HealMp,
    DamageEnemy
}

public enum FieldEventActionType
{
    Dialogue,
    Recover,
    Bank
}
