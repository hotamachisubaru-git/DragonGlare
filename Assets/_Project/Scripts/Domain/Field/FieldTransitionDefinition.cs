using DragonGlare.Domain;

namespace DragonGlare.Domain.Field;

public sealed record FieldTransitionDefinition(
    FieldMapId FromMapId,
    Rectangle TriggerArea,
    FieldMapId ToMapId,
    Point DestinationTile)
{
    public FieldMapId TargetMap => ToMapId;

    public Point TargetPosition => DestinationTile;

    public bool IsTriggeredBy(Point tile)
    {
        return TriggerArea.Contains(tile);
    }
}
