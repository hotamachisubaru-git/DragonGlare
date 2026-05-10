using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha.Persistence;

public sealed class SaveData
{
    public const int CurrentVersion = 10;

    public int Version { get; set; } = CurrentVersion;

    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;

    public string Language { get; set; } = "ja";

    public string Name { get; set; } = string.Empty;

    public int SlotNumber { get; set; }

    public FieldMapId CurrentFieldMap { get; set; } = FieldMapId.Hub;

    public int PlayerX { get; set; }

    public int PlayerY { get; set; }

    public int Level { get; set; }

    public int Experience { get; set; }

    public int MaxHp { get; set; }

    public int CurrentHp { get; set; }

    public int MaxMp { get; set; }

    public int CurrentMp { get; set; }

    public int BaseAttack { get; set; }

    public int BaseDefense { get; set; }

    public int Gold { get; set; }

    public int BankGold { get; set; }

    public int LoanBalance { get; set; }

    public int LoanStepCounter { get; set; }

    public string? EquippedWeaponId { get; set; }

    public string? EquippedArmorId { get; set; }

    public string? EquippedHeadId { get; set; }

    public string? EquippedArmsId { get; set; }

    public string? EquippedLegsId { get; set; }

    public string? EquippedFeetId { get; set; }

    public List<InventoryEntry> Inventory { get; set; } = [];

    public List<string> CompletedFieldEventIds { get; set; } = [];

    public string Signature { get; set; } = string.Empty;
}
