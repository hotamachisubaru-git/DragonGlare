using System.IO;
using DragonGlare.Domain;

namespace DragonGlare.Services;

public static class MapFactory
{
    public const int FloorTile = 0;
    public const int WallTile = 1;
    public const int CastleBlockTile = 2;
    public const int CastleGateTile = 3;
    public const int FieldGateTile = 4;
    public const int CastleFloorTile = 5;
    public const int GrassTile = 6;
    public const int DecorationBlueTile = 7;
    public const int CastleTextWallTile = '@';
    public const int CastleTextCarpetTile = '4';
    public const int CastleTextTopWallTile = '5';
    public const int CastleTextColumnBaseTile = 'g';
    public const int CastleTextPillarTile = 'l';
    public const int CastleTextOrnamentTile = 'm';
    public const int CastleTextRightWallTile = '#';
    public const int CastleTextExitTile = 'r';

    private const string CastleMapFileName = "map(mycas2).txt";
    private const string EmbeddedCastleMap = """
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@55555555555555@@@@@@@@
@@@@@@@@g44m444444m44g@@@@@@@@
@@@@@@@@g4l4l4444l4l4g@@@@@@@@
@@@@@@@@g444444444444g@@@@@@@@
@@@@@@@@g444444444444g@@@@@@@@
@@@@@@@@g444444444444g@@@@@@@@
@@@@@@@@g444444444444g@@@@@@@@
@@@@@@@@g444444444444g@@@@@@@@
@@@@@@@@g444444444444g@@@@@@@@
@@@@@@@@g444444444444g#@@@@@@@
@@@@@@@@ggggg4444gggggg@@@@@@@
@@@@@@@@ggggg4444ggggg@@@@@@@@
@@@@@@@@ggggg4444ggggg@@@@@@@@
@@@@@@@@ggggg4444ggggg@@@@@@@@
@@@@@@@@@@@@@@r@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
""";

    public static int[,] CreateDefaultMap()
    {
        return CreateMap(FieldMapId.Hub);
    }

    public static int[,] CreateMap(FieldMapId mapId)
    {
        return mapId switch
        {
            FieldMapId.Dungeon => CreateCastleMap(),
            FieldMapId.Castle => CreateCastleMap(),
            FieldMapId.Field => CreateFieldMap(),
            _ => CreateHubMap()
        };
    }

    private static int[,] CreateHubMap()
    {
        var map = CreateBoundedMap();

        PaintArea(map, 9, 0, 10, 0, CastleGateTile);
        PaintArea(map, 19, 7, 19, 8, FieldGateTile);

        for (var x = 4; x <= 15; x++)
        {
            map[10, x] = WallTile;
        }

        map[10, 9] = FloorTile;
        map[10, 10] = FloorTile;
        map[6, 6] = WallTile;
        map[6, 7] = WallTile;
        map[7, 6] = WallTile;
        return map;
    }

    private static int[,] CreateCastleMap()
    {
        return CreateTextMap(LoadCastleMapLines());
    }

    private static int[,] CreateFieldMap()
    {
        var map = CreateBoundedMap();

        PaintArea(map, 1, 1, 1, 13, DecorationBlueTile);
        PaintArea(map, 15, 1, 15, 1, FieldGateTile);
        PaintArea(map, 2, 2, 6, 5, GrassTile);
        PaintArea(map, 10, 3, 16, 6, GrassTile);
        PaintArea(map, 5, 9, 12, 12, GrassTile);
        PaintArea(map, 8, 7, 9, 8, DecorationBlueTile);
        PaintArea(map, 14, 10, 15, 11, DecorationBlueTile);
        return map;
    }

    private static int[,] CreateBoundedMap()
    {
        var map = new int[15, 20];

        for (var y = 0; y < map.GetLength(0); y++)
        {
            for (var x = 0; x < map.GetLength(1); x++)
            {
                map[y, x] = x == 0 || y == 0 || x == map.GetLength(1) - 1 || y == map.GetLength(0) - 1
                    ? WallTile
                    : FloorTile;
            }
        }

        return map;
    }

    private static void PaintArea(int[,] map, int left, int top, int right, int bottom, int tile)
    {
        for (var y = top; y <= bottom; y++)
        {
            for (var x = left; x <= right; x++)
            {
                map[y, x] = tile;
            }
        }
    }

    public static bool IsWalkableTileId(int tileId)
    {
        return tileId switch
        {
            WallTile
                or DecorationBlueTile
                or CastleTextWallTile
                or CastleTextTopWallTile
                or CastleTextColumnBaseTile
                or CastleTextPillarTile
                or CastleTextOrnamentTile
                or CastleTextRightWallTile => false,
            _ => true
        };
    }

    public static bool IsFieldGateTileId(int tileId)
    {
        return tileId is FieldGateTile;
    }

    public static bool IsGrassTileId(int tileId)
    {
        return tileId is GrassTile;
    }

    private static string[] LoadCastleMapLines()
    {
        if (TryReadCastleMapAsset() is { } assetText &&
            TryNormalizeMapLines(assetText, out var assetLines))
        {
            return assetLines;
        }

        return TryNormalizeMapLines(EmbeddedCastleMap, out var embeddedLines)
            ? embeddedLines
            : ["@"];
    }

    private static string? TryReadCastleMapAsset()
    {
        foreach (var root in GetAssetLookupRoots())
        {
            foreach (var relativePath in new[]
            {
                Path.Combine("Assets", CastleMapFileName),
                CastleMapFileName
            })
            {
                var path = Path.GetFullPath(Path.Combine(root, relativePath));
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    return File.ReadAllText(path);
                }
                catch
                {
                    return null;
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> GetAssetLookupRoots()
    {
        yield return AppContext.BaseDirectory;
        yield return Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        yield return Directory.GetCurrentDirectory();
    }

    private static bool TryNormalizeMapLines(string text, out string[] lines)
    {
        var normalizedLines = text
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        lines = normalizedLines;
        return normalizedLines.Length > 0 &&
            normalizedLines[0].Length > 0 &&
            normalizedLines.All(line => line.Length == normalizedLines[0].Length);
    }

    private static int[,] CreateTextMap(string[] lines)
    {
        var map = new int[lines.Length, lines[0].Length];

        for (var y = 0; y < lines.Length; y++)
        {
            for (var x = 0; x < lines[y].Length; x++)
            {
                map[y, x] = lines[y][x];
            }
        }

        return map;
    }
}
