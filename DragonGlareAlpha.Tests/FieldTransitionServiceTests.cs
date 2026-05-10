using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class FieldTransitionServiceTests
{
    private readonly FieldTransitionService service = new();

    [Theory]
    [InlineData(FieldMapId.Hub, 9, 0, FieldMapId.Castle, 14, 20)]
    [InlineData(FieldMapId.Hub, 19, 7, FieldMapId.Field, 2, 6)]
    [InlineData(FieldMapId.Hub, 19, 8, FieldMapId.Field, 2, 7)]
    [InlineData(FieldMapId.Castle, 14, 21, FieldMapId.Hub, 9, 2)]
    [InlineData(FieldMapId.Field, 1, 6, FieldMapId.Hub, 18, 7)]
    [InlineData(FieldMapId.Field, 1, 7, FieldMapId.Hub, 18, 8)]
    [InlineData(FieldMapId.Field, 15, 1, FieldMapId.Dungeon, 14, 20)]
    [InlineData(FieldMapId.Dungeon, 14, 21, FieldMapId.Field, 15, 2)]
    public void TryGetTransition_WhenTileMatchesPortal_ReturnsConfiguredDestination(
        FieldMapId fromMap,
        int tileX,
        int tileY,
        FieldMapId expectedMap,
        int expectedX,
        int expectedY)
    {
        var found = service.TryGetTransition(fromMap, new Point(tileX, tileY), out var transition);

        Assert.True(found);
        Assert.NotNull(transition);
        Assert.Equal(expectedMap, transition!.ToMapId);
        Assert.Equal(new Point(expectedX, expectedY), transition.DestinationTile);
    }

    [Fact]
    public void TryGetTransition_WhenTileDoesNotMatchPortal_ReturnsFalse()
    {
        var found = service.TryGetTransition(FieldMapId.Hub, new Point(5, 5), out var transition);

        Assert.False(found);
        Assert.Null(transition);
    }

    [Theory]
    [InlineData(2, 6)]
    [InlineData(2, 7)]
    public void TryGetTransition_WhenOnFieldLandingTile_ReturnsFalse(int tileX, int tileY)
    {
        var found = service.TryGetTransition(FieldMapId.Field, new Point(tileX, tileY), out var transition);

        Assert.False(found);
        Assert.Null(transition);
    }
}
