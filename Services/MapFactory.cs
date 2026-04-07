namespace DragonGlareAlpha.Services;

public static class MapFactory
{
    public static int[,] CreateDefaultMap()
    {
        var map = new int[15, 20];

        for (var y = 0; y < map.GetLength(0); y++)
        {
            for (var x = 0; x < map.GetLength(1); x++)
            {
                map[y, x] = x == 0 || y == 0 || x == map.GetLength(1) - 1 || y == map.GetLength(0) - 1 ? 1 : 0;
            }
        }

        for (var y = 1; y <= 4; y++)
        {
            for (var x = 1; x <= 4; x++)
            {
                map[y, x] = 2;
            }
        }

        for (var x = 4; x <= 15; x++)
        {
            map[10, x] = 1;
        }

        map[10, 9] = 0;
        map[10, 10] = 0;
        map[6, 6] = 1;
        map[6, 7] = 1;
        map[7, 6] = 1;
        return map;
    }
}
