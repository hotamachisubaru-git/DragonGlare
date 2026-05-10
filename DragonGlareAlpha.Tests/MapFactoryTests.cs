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

    [Fact]
    public void CreateMap_ForDungeon_UsesCastleTextMapLayout()
    {
        var map = MapFactory.CreateMap(FieldMapId.Dungeon);

        Assert.Equal(29, map.GetLength(0));
        Assert.Equal(30, map.GetLength(1));
        Assert.Equal(MapFactory.CastleTextExitTile, map[21, 14]);
    }

    [Fact]
    public void CreateMap_ForField_MarksDungeonEntranceAsGate()
    {
        var map = MapFactory.CreateMap(FieldMapId.Field);

        Assert.Equal(MapFactory.FieldGateTile, map[1, 15]);
    }

    [Fact]
    public void CreateMap_ForField_BlocksLeftSeaAndKeepsExitTilesWalkable()
    {
        var map = MapFactory.CreateMap(FieldMapId.Field);

        Assert.Equal(MapFactory.DecorationBlueTile, map[6, 1]);
        Assert.Equal(MapFactory.DecorationBlueTile, map[8, 1]);
        Assert.Equal(MapFactory.FloorTile, map[6, 2]);
        Assert.Equal(MapFactory.FloorTile, map[7, 2]);
        Assert.False(MapFactory.IsWalkableTileId(map[6, 1]));
        Assert.False(MapFactory.IsWalkableTileId(map[8, 1]));
        Assert.True(MapFactory.IsWalkableTileId(map[6, 2]));
        Assert.True(MapFactory.IsWalkableTileId(map[7, 2]));
    }

    [Theory]
    [InlineData(MapFactory.CastleTextCarpetTile, true)]
    [InlineData(MapFactory.CastleTextExitTile, true)]
    [InlineData(MapFactory.DecorationBlueTile, false)]
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

    [Theory]
    [InlineData(MapFactory.FieldGateTile, true)]
    [InlineData(MapFactory.FloorTile, false)]
    public void IsFieldGateTileId_HandlesGeneratedAndTextTiles(int tileId, bool expected)
    {
        Assert.Equal(expected, MapFactory.IsFieldGateTileId(tileId));
    }

    [Theory]
    [InlineData(MapFactory.GrassTile, true)]
    [InlineData(MapFactory.FloorTile, false)]
    public void IsGrassTileId_HandlesGeneratedAndTextTiles(int tileId, bool expected)
    {
        Assert.Equal(expected, MapFactory.IsGrassTileId(tileId));
    }
}
