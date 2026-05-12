using UnityEngine;

namespace DragonGlare
{
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
        Escape
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

    public enum SaveSlotSelectionMode
    {
        Save,
        Load,
        CopySource,
        CopyDestination,
        DeleteSelect,
        DeleteConfirm
    }

    public enum SaveSlotState
    {
        Empty,
        Occupied,
        Corrupted
    }

    public enum UiLanguage
    {
        Japanese,
        English
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

    public enum PlayerFacingDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public enum EquipmentSlot
    {
        Weapon,
        Head,
        Armor,
        Arms,
        Legs,
        Feet
    }

    public enum FieldMapId
    {
        Hub,
        Castle,
        Dungeon,
        Field
    }

    public enum BattleStatusEffect
    {
        None,
        Poison,
        Sleep
    }

    public enum BattleVisualCue
    {
        None,
        PlayerAction,
        SpellBurst,
        StatusCloud,
        PlayerHeal,
        PlayerGuard,
        ItemUse,
        EnemyDefeat,
        EnemyHitFlash,
        PlayerHitFlash
    }

    public enum BattleActionType
    {
        Attack,
        Spell,
        Item,
        Equip,
        Defend,
        Run
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
}
