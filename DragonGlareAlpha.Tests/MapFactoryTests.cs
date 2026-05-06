using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class MapFactoryTests
{
    [Fact]
    public void CreateMap_ForCastle_UsesTextMapLayout()
    {
        var map = MapFactory.CreateMap(FieldMapId.Castle);

        Assert.Equal(29, map.GetLength(0));
        Assert.Equal(30, map.GetLength(1));
        Assert.Equal(MapFactory.CastleTextCarpetTile, map[20, 14]);
        Assert.Equal(MapFactory.CastleTextExitTile, map[21, 14]);
    }

    [Theory]
    [InlineData(MapFactory.CastleTextCarpetTile, true)]
    [InlineData(MapFactory.CastleTextExitTile, true)]
    [InlineData(MapFactory.CastleTextWallTile, false)]
    [InlineData(MapFactory.CastleTextTopWallTile, false)]
    [InlineData(MapFactory.CastleTextColumnBaseTile, false)]
    [InlineData(MapFactory.CastleTextPillarTile, false)]
    [InlineData(MapFactory.CastleTextOrnamentTile, false)]
    [InlineData(MapFactory.CastleTextRightWallTile, false)]
    public void IsWalkableTileId_HandlesCastleTextTiles(int tileId, bool expected)
    {
        Assert.Equal(expected, MapFactory.IsWalkableTileId(tileId));
    }
}
