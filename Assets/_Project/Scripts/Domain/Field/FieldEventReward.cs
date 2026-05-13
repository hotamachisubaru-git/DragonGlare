using DragonGlare.Domain;

namespace DragonGlare.Domain.Field;

public sealed record FieldEventReward(
    string? ItemId = null,
    int ItemQuantity = 0,
    int Gold = 0)
{
    public bool HasItem => !string.IsNullOrWhiteSpace(ItemId) && ItemQuantity > 0;

    public bool HasGold => Gold > 0;

    public bool HasAnyReward => HasItem || HasGold;
}
