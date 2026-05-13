using System.Diagnostics.CodeAnalysis;
using DragonGlare.Data;
using DragonGlare.Domain;
using DragonGlare.Domain.Field;

namespace DragonGlare.Services;

public sealed class FieldTransitionService
{
    public bool TryGetTransition(FieldMapId mapId, Point tile, [NotNullWhen(true)] out FieldTransitionDefinition? transition)
    {
        transition = FieldContent.FieldTransitions
            .FirstOrDefault(definition => definition.FromMapId == mapId && definition.IsTriggeredBy(tile));

        return transition is not null;
    }
}
