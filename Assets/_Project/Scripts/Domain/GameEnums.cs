namespace DragonGlare.Domain;

public enum GameState
{
    StartupOptions,
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
    Field,
    Dungeon
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
    Collision,
    Attack,
    Defend,
    Magic,
    Cure,
    Poison,
    Raiden,
    Fire,
    Equip,
    Cursor,
    Cancel,
    Escape
}

public enum BattleFlowState
{
    Intro,
    CommandSelection,
    SpellSelection,
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
    PlayerAction,
    PlayerGuard,
    EnemyAction,
    EnemyHit,
    PlayerHit,
    SpellCast,
    PlayerHeal,
    MpRecover,
    EnemyDefeat,
    ItemUse,
    EnemyStatus,
    PlayerStatus,
    PoisonTick
}

public enum ConsumableEffectType
{
    HealHp,
    HealMp,
    DamageEnemy
}

public enum SpellEffectType
{
    DamageEnemy,
    HealPlayer,
    PoisonEnemy,
    SleepEnemy,
    CurePlayerStatus
}

public enum BattleStatusEffect
{
    None,
    Poison,
    Sleep
}

public enum FieldEventActionType
{
    Dialogue,
    Recover,
    Bank,
    Treasure
}

public enum SaveSlotState
{
    Empty,
    Occupied,
    Corrupted
}

public enum PlayerFacingDirection
{
    Left,
    Right,
    Up,
    Down
}

public enum ShopMenuEntryType
{
    Product,
    InventoryItem,
    PreviousPage,
    NextPage,
    Quit
}

public enum LaunchDisplayMode
{
    Window640x480,
    Window720p,
    Window1080p,
    Fullscreen
}
