using System;
using DragonGlare.Domain;

namespace DragonGlare.Persistence
{
    public enum SaveSlotState
    {
        Empty,
        Occupied,
        Corrupted
    }

    public sealed class SaveSlotSummary
    {
        public int SlotNumber { get; set; }
        public SaveSlotState State { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Gold { get; set; }
        public FieldMapId CurrentFieldMap { get; set; } = FieldMapId.Hub;
        public DateTime? SavedAtLocal { get; set; }
    }
}
