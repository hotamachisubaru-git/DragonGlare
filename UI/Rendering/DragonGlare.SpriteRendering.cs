using System.IO;
using System.Drawing.Drawing2D;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void LoadFieldSprites()
    {
        LoadHeroDirectionalSprites();
        if (heroSprites.Count == 4)
        {
            return;
        }

        if (heroSprites.Count == 0 && LoadHeroSpriteSheet("hero_4.png"))
        {
            return;
        }

        LoadHeroSpriteIfMissing(PlayerFacingDirection.Left, "hero_left.png", "hero.png");
        LoadHeroSpriteIfMissing(PlayerFacingDirection.Right, "hero_right.png", "hero_left.png", "hero.png");
        LoadHeroSpriteIfMissing(PlayerFacingDirection.Up, "hero_up.png", "hero_left.png", "hero.png");
        LoadHeroSpriteIfMissing(PlayerFacingDirection.Down, "hero_down.png", "hero_left.png", "hero.png");
    }

    private bool TryDrawFieldTileSprite(Graphics g, Point worldTile, Rectangle tileRect)
    {
        if (currentFieldMap is not (FieldMapId.Castle or FieldMapId.Dungeon or FieldMapId.Field))
        {
            return false;
        }

        var tileSheet = GetMapTileSheet();
        if (tileSheet is null)
        {
            return false;
        }

        var tileId = GetTileIdAtWorldPosition(worldTile);
        if (currentFieldMap == FieldMapId.Field && tileId == MapFactory.GrassTile)
        {
            return TryDrawOutdoorTreeTile(g, tileSheet, worldTile, tileRect);
        }

        var sourceRect = GetMapTileSourceRectangle(tileSheet, tileId, worldTile);
        if (sourceRect.IsEmpty)
        {
            return false;
        }

        var state = g.Save();
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.SmoothingMode = SmoothingMode.None;
        g.DrawImage(tileSheet, tileRect, sourceRect, GraphicsUnit.Pixel);
        g.Restore(state);
        return true;
    }

    private Image? GetMapTileSheet()
    {
        if (mapTileSheet is not null)
        {
            return mapTileSheet;
        }

        var path = ResolveAssetPath(null, "mapTile_Assets_SFC.png", "mapTile_Assets_SFCFrame1.png");
        if (path is null)
        {
            return null;
        }

        try
        {
            mapTileSheet = Image.FromFile(path);
            return mapTileSheet;
        }
        catch
        {
            return null;
        }
    }

    private bool TryDrawFieldMapImage(Graphics g, Rectangle viewport, Point cameraOrigin)
    {
        var fieldImage = GetFieldMapImage();
        if (fieldImage is null)
        {
            return false;
        }

        var mapWidthTiles = map.GetLength(1);
        var mapHeightTiles = map.GetLength(0);
        if (mapWidthTiles <= 0 || mapHeightTiles <= 0)
        {
            return false;
        }

        var sourceTileWidth = fieldImage.Width / (float)mapWidthTiles;
        var sourceTileHeight = fieldImage.Height / (float)mapHeightTiles;
        var visibleWidthTiles = GetFieldViewportWidthTiles();
        var visibleHeightTiles = GetFieldViewportHeightTiles();
        var destinationRect = new Rectangle(
            viewport.X + (isFieldStatusVisible ? 0 : GetExpandedFieldViewportHorizontalPadding()),
            viewport.Y,
            visibleWidthTiles * TileSize,
            visibleHeightTiles * TileSize);

        var state = g.Save();
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.SmoothingMode = SmoothingMode.None;
        g.DrawImage(
            fieldImage,
            destinationRect,
            cameraOrigin.X * sourceTileWidth,
            cameraOrigin.Y * sourceTileHeight,
            visibleWidthTiles * sourceTileWidth,
            visibleHeightTiles * sourceTileHeight,
            GraphicsUnit.Pixel);
        g.Restore(state);
        return true;
    }

    private Image? GetFieldMapImage()
    {
        if (fieldMapImage is not null)
        {
            return fieldMapImage;
        }

        var path = ResolveAssetPath(null, "field_map.png", "field-map.png", "field.png");
        if (path is null)
        {
            return null;
        }

        try
        {
            fieldMapImage = Image.FromFile(path);
            return fieldMapImage;
        }
        catch
        {
            return null;
        }
    }

    private Rectangle GetMapTileSourceRectangle(Image tileSheet, int tileId, Point worldTile)
    {
        if (!TryGetMapTileSheetCell(tileId, worldTile, out var cell))
        {
            return Rectangle.Empty;
        }

        const int tileSheetColumns = 8;
        const int tileSheetRows = 8;
        var sourceTileWidth = tileSheet.Width / tileSheetColumns;
        var sourceTileHeight = tileSheet.Height / tileSheetRows;
        if (sourceTileWidth <= 0 || sourceTileHeight <= 0)
        {
            return Rectangle.Empty;
        }

        return new Rectangle(
            cell.X * sourceTileWidth,
            cell.Y * sourceTileHeight,
            sourceTileWidth,
            sourceTileHeight);
    }

    private bool TryDrawOutdoorTreeTile(Graphics g, Image tileSheet, Point worldTile, Rectangle tileRect)
    {
        if (GetOutdoorTreeLocalColumn(worldTile) == 2)
        {
            return DrawTileSheetCell(g, tileSheet, new Point(3, 6), tileRect);
        }

        var isTreeBelow = IsOutdoorTreeTile(new Point(worldTile.X, worldTile.Y + 1));
        var isTreeAbove = IsOutdoorTreeTile(new Point(worldTile.X, worldTile.Y - 1));
        var sourceCell = (isTreeAbove, isTreeBelow) switch
        {
            (true, true) => new Point(1, 3),
            (true, false) => new Point(3, 6),
            _ => new Point(1, 3)
        };
        return DrawTileSheetCell(g, tileSheet, sourceCell, tileRect);
    }

    private bool DrawTileSheetCell(Graphics g, Image tileSheet, Point sourceCell, Rectangle tileRect)
    {
        var sourceRect = GetTileSheetSourceRectangle(tileSheet, sourceCell);
        if (sourceRect.IsEmpty)
        {
            return false;
        }

        var state = g.Save();
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.SmoothingMode = SmoothingMode.None;
        g.DrawImage(tileSheet, tileRect, sourceRect, GraphicsUnit.Pixel);
        g.Restore(state);
        return true;
    }

    private int GetOutdoorTreeLocalColumn(Point worldTile)
    {
        var left = worldTile.X;
        while (IsOutdoorTreeTile(new Point(left - 1, worldTile.Y)))
        {
            left--;
        }

        return worldTile.X - left;
    }

    private static Rectangle GetTileSheetSourceRectangle(Image tileSheet, Point cell)
    {
        const int tileSheetColumns = 8;
        const int tileSheetRows = 8;
        var sourceTileWidth = tileSheet.Width / tileSheetColumns;
        var sourceTileHeight = tileSheet.Height / tileSheetRows;
        if (sourceTileWidth <= 0 || sourceTileHeight <= 0)
        {
            return Rectangle.Empty;
        }

        return new Rectangle(
            cell.X * sourceTileWidth,
            cell.Y * sourceTileHeight,
            sourceTileWidth,
            sourceTileHeight);
    }

    private bool IsOutdoorTreeTile(Point worldTile)
    {
        return GetTileIdAtWorldPosition(worldTile) == MapFactory.GrassTile;
    }

    private bool TryGetMapTileSheetCell(int tileId, Point worldTile, out Point cell)
    {
        return currentFieldMap switch
        {
            FieldMapId.Dungeon => TryGetCastleTileSheetCell(tileId, worldTile, out cell),
            FieldMapId.Castle => TryGetCastleTileSheetCell(tileId, worldTile, out cell),
            FieldMapId.Field => TryGetOutdoorTileSheetCell(tileId, out cell),
            _ => TryGetHubTileSheetCell(tileId, out cell)
        };
    }

    private bool TryGetCastleTileSheetCell(int tileId, Point worldTile, out Point cell)
    {
        cell = tileId switch
        {
            MapFactory.CastleTextWallTile => new Point(2, 3),
            MapFactory.CastleTextTopWallTile => new Point(3, 4),
            MapFactory.CastleTextColumnBaseTile => new Point(5, 0),
            MapFactory.CastleTextPillarTile => new Point(4, 6),
            MapFactory.CastleTextOrnamentTile => new Point(3, 7),
            MapFactory.CastleTextRightWallTile => new Point(3, 3),
            MapFactory.CastleTextExitTile => new Point(1, 5),
            MapFactory.CastleTextCarpetTile => GetCastleCarpetTileCell(worldTile),
            _ => Point.Empty
        };

        return tileId is MapFactory.CastleTextWallTile
            or MapFactory.CastleTextTopWallTile
            or MapFactory.CastleTextColumnBaseTile
            or MapFactory.CastleTextPillarTile
            or MapFactory.CastleTextOrnamentTile
            or MapFactory.CastleTextRightWallTile
            or MapFactory.CastleTextExitTile
            or MapFactory.CastleTextCarpetTile;
    }

    private Point GetCastleCarpetTileCell(Point worldTile)
    {
        var hasLeftEdge = IsCastleCarpetBoundary(worldTile, new Point(-1, 0));
        var hasRightEdge = IsCastleCarpetBoundary(worldTile, new Point(1, 0));
        var hasTopEdge = IsCastleCarpetBoundary(worldTile, new Point(0, -1));
        var hasBottomEdge = IsCastleCarpetBoundary(worldTile, new Point(0, 1));

        return (hasLeftEdge, hasRightEdge, hasTopEdge, hasBottomEdge) switch
        {
            (true, false, true, false) => new Point(0, 4),
            (false, true, true, false) => new Point(2, 4),
            (true, false, false, true) => new Point(0, 6),
            (false, true, false, true) => new Point(2, 6),
            (true, false, _, _) => new Point(0, 5),
            (false, true, _, _) => new Point(2, 5),
            (_, _, true, false) => new Point(1, 4),
            (_, _, false, true) => new Point(1, 6),
            _ => new Point(1, 5)
        };
    }

    private bool IsCastleCarpetBoundary(Point worldTile, Point direction)
    {
        var neighborTileId = GetTileIdAtWorldPosition(new Point(worldTile.X + direction.X, worldTile.Y + direction.Y));
        return neighborTileId != MapFactory.CastleTextCarpetTile &&
            neighborTileId != MapFactory.CastleTextExitTile;
    }

    private static bool TryGetOutdoorTileSheetCell(int tileId, out Point cell)
    {
        cell = tileId switch
        {
            MapFactory.WallTile => new Point(0, 3),
            MapFactory.FloorTile => new Point(6, 2),
            MapFactory.GrassTile => new Point(1, 3),
            MapFactory.DecorationBlueTile => new Point(0, 3),
            MapFactory.FieldGateTile => new Point(6, 2),
            _ => Point.Empty
        };

        return tileId is MapFactory.WallTile
            or MapFactory.FloorTile
            or MapFactory.GrassTile
            or MapFactory.DecorationBlueTile
            or MapFactory.FieldGateTile;
    }

    private static bool TryGetHubTileSheetCell(int tileId, out Point cell)
    {
        cell = tileId switch
        {
            MapFactory.WallTile => new Point(7, 0),
            MapFactory.FloorTile => new Point(6, 2),
            MapFactory.GrassTile => new Point(1, 3),
            MapFactory.DecorationBlueTile => new Point(0, 3),
            MapFactory.FieldGateTile => new Point(6, 1),
            MapFactory.CastleGateTile => new Point(3, 7),
            MapFactory.CastleBlockTile => new Point(2, 3),
            MapFactory.CastleFloorTile => new Point(1, 5),
            _ => Point.Empty
        };

        return tileId is MapFactory.WallTile
            or MapFactory.FloorTile
            or MapFactory.GrassTile
            or MapFactory.DecorationBlueTile
            or MapFactory.FieldGateTile
            or MapFactory.CastleGateTile
            or MapFactory.CastleBlockTile
            or MapFactory.CastleFloorTile;
    }

    private Image? GetNpcSprite(string? spriteAssetName)
    {
        if (string.IsNullOrWhiteSpace(spriteAssetName))
        {
            return null;
        }

        if (npcSprites.TryGetValue(spriteAssetName, out var cachedSprite))
        {
            return cachedSprite;
        }

        var sprite = LoadSprite(Path.Combine("Sprites", "NPC"), spriteAssetName);
        if (sprite is not null)
        {
            npcSprites[spriteAssetName] = sprite;
        }

        return sprite;
    }

    private Image? GetEnemySprite(string? spriteAssetName)
    {
        if (string.IsNullOrWhiteSpace(spriteAssetName))
        {
            return null;
        }

        if (enemySprites.TryGetValue(spriteAssetName, out var cachedSprite))
        {
            return cachedSprite;
        }

        var sprite = LoadPreparedEnemySprite(spriteAssetName);
        if (sprite is not null)
        {
            enemySprites[spriteAssetName] = sprite;
        }

        return sprite;
    }

    private Image? GetNpcPortrait(string? portraitAssetName)
    {
        if (string.IsNullOrWhiteSpace(portraitAssetName))
        {
            return null;
        }

        if (npcPortraits.TryGetValue(portraitAssetName, out var cachedPortrait))
        {
            return cachedPortrait;
        }

        var portrait = LoadSprite(Path.Combine("Portraits", "NPC"), portraitAssetName);
        if (portrait is not null)
        {
            npcPortraits[portraitAssetName] = portrait;
        }

        return portrait;
    }

    private Image? GetUiImage(string? assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return null;
        }

        if (uiImages.TryGetValue(assetName, out var cachedImage))
        {
            return cachedImage;
        }

        var image = LoadSprite("UI", assetName);
        if (image is not null)
        {
            uiImages[assetName] = image;
        }

        return image;
    }

    private static Image? LoadSprite(string assetSubdirectory, string fileName)
    {
        var path = ResolveAssetPath(assetSubdirectory, fileName);
        if (path is null)
        {
            return null;
        }

        try
        {
            return Image.FromFile(path);
        }
        catch
        {
            return null;
        }
    }

    private static Image? LoadPreparedEnemySprite(string fileName)
    {
        var source = LoadSprite(Path.Combine("Sprites", "Enemies"), fileName);
        if (source is not Bitmap sourceBitmap)
        {
            return source;
        }

        try
        {
            var crop = FindLargestForegroundRegionBounds(
                sourceBitmap,
                alphaThreshold: 8,
                minimumPixelCount: 24,
                out var selectedForeground);
            if (crop.Width <= 0 || crop.Height <= 0)
            {
                crop = FindOpaqueBounds(sourceBitmap, alphaThreshold: 8);
                selectedForeground = Enumerable.Repeat(true, sourceBitmap.Width * sourceBitmap.Height).ToArray();
            }

            if (crop.Width <= 0 || crop.Height <= 0)
            {
                return new Bitmap(sourceBitmap);
            }

            using var extracted = ExtractForegroundSprite(sourceBitmap, crop, selectedForeground);
            var targetHeight = Math.Clamp(extracted.Height, 96, 176);
            return ResizeSprite(extracted, targetHeight);
        }
        finally
        {
            sourceBitmap.Dispose();
        }
    }

    private void LoadHeroSprite(PlayerFacingDirection direction, params string[] fileNames)
    {
        var sprite = LoadPreparedHeroSprite(fileNames);
        if (sprite is null)
        {
            return;
        }

        heroSprites[direction] = sprite;
    }

    private void LoadHeroSpriteIfMissing(PlayerFacingDirection direction, params string[] fileNames)
    {
        if (heroSprites.ContainsKey(direction))
        {
            return;
        }

        LoadHeroSprite(direction, fileNames);
    }

    private void LoadHeroDirectionalSprites()
    {
        LoadHero3DirectionalSprites();
        LoadHero4RightSprite();
    }

    private void LoadHero3DirectionalSprites()
    {
        if (heroSprites.ContainsKey(PlayerFacingDirection.Left)
            && heroSprites.ContainsKey(PlayerFacingDirection.Up)
            && heroSprites.ContainsKey(PlayerFacingDirection.Down))
        {
            return;
        }

        var source = LoadSprite(Path.Combine("Sprites", "Characters"), "hero_3.png");
        if (source is not Bitmap spriteSheet)
        {
            source?.Dispose();
            return;
        }

        try
        {
            var regions = FindOpaqueRegions(spriteSheet, alphaThreshold: 8, minimumPixelCount: 100)
                .OrderByDescending(region => region.PixelCount)
                .Take(3)
                .OrderBy(region => region.Bounds.X)
                .ToArray();
            if (regions.Length < 3)
            {
                return;
            }

            LoadHeroSpriteFromBounds(spriteSheet, PlayerFacingDirection.Left, regions[0].Bounds);
            LoadHeroSpriteFromBounds(spriteSheet, PlayerFacingDirection.Up, regions[1].Bounds);
            LoadHeroSpriteFromBounds(spriteSheet, PlayerFacingDirection.Down, regions[2].Bounds);
        }
        finally
        {
            spriteSheet.Dispose();
        }
    }

    private void LoadHero4RightSprite()
    {
        if (heroSprites.ContainsKey(PlayerFacingDirection.Right))
        {
            return;
        }

        var source = LoadSprite(Path.Combine("Sprites", "Characters"), "hero_4.png");
        if (source is not Bitmap spriteSheet)
        {
            source?.Dispose();
            return;
        }

        try
        {
            var regions = FindOpaqueRegions(spriteSheet, alphaThreshold: 8, minimumPixelCount: 100)
                .OrderByDescending(region => region.PixelCount)
                .Take(4)
                .OrderBy(region => region.Bounds.Y)
                .ThenBy(region => region.Bounds.X)
                .ToArray();
            if (regions.Length < 4)
            {
                return;
            }

            LoadHeroSpriteFromBounds(spriteSheet, PlayerFacingDirection.Right, regions[3].Bounds);
        }
        finally
        {
            spriteSheet.Dispose();
        }
    }

    private bool LoadHeroSpriteSheet(string fileName)
    {
        var source = LoadSprite(Path.Combine("Sprites", "Characters"), fileName);
        if (source is not Bitmap spriteSheet)
        {
            source?.Dispose();
            return false;
        }

        try
        {
            var cellWidth = spriteSheet.Width / 2;
            var cellHeight = spriteSheet.Height / 2;
            if (cellWidth <= 0 || cellHeight <= 0)
            {
                return false;
            }

            LoadHeroSpriteFromSheet(spriteSheet, PlayerFacingDirection.Left, new Rectangle(0, 0, cellWidth, cellHeight));
            LoadHeroSpriteFromSheet(spriteSheet, PlayerFacingDirection.Right, new Rectangle(cellWidth, 0, cellWidth, cellHeight));
            LoadHeroSpriteFromSheet(spriteSheet, PlayerFacingDirection.Up, new Rectangle(0, cellHeight, cellWidth, cellHeight));
            LoadHeroSpriteFromSheet(spriteSheet, PlayerFacingDirection.Down, new Rectangle(cellWidth, cellHeight, cellWidth, cellHeight));
            return heroSprites.Count > 0;
        }
        finally
        {
            spriteSheet.Dispose();
        }
    }

    private void LoadHeroSpriteFromSheet(Bitmap spriteSheet, PlayerFacingDirection direction, Rectangle sourceRegion)
    {
        using var cellBitmap = spriteSheet.Clone(sourceRegion, spriteSheet.PixelFormat);
        var crop = FindLargestOpaqueRegionBounds(cellBitmap, alphaThreshold: 8);
        if (crop.Width <= 0 || crop.Height <= 0)
        {
            return;
        }

        using var cropped = cellBitmap.Clone(crop, cellBitmap.PixelFormat);
        heroSprites[direction] = ResizeSprite(cropped, targetHeight: TileSize + 16);
    }

    private void LoadHeroSpriteFromBounds(Bitmap spriteSheet, PlayerFacingDirection direction, Rectangle sourceRegion)
    {
        if (heroSprites.ContainsKey(direction))
        {
            return;
        }

        if (sourceRegion.Width <= 0 || sourceRegion.Height <= 0)
        {
            return;
        }

        using var cropped = spriteSheet.Clone(sourceRegion, spriteSheet.PixelFormat);
        heroSprites[direction] = ResizeSprite(cropped, targetHeight: TileSize + 16);
    }

    private Image? GetHeroSprite()
    {
        if (heroSprites.TryGetValue(playerFacingDirection, out var facingSprite))
        {
            return facingSprite;
        }

        if (heroSprites.TryGetValue(PlayerFacingDirection.Down, out var downSprite))
        {
            return downSprite;
        }

        if (heroSprites.TryGetValue(PlayerFacingDirection.Left, out var leftSprite))
        {
            return leftSprite;
        }

        return heroSprites.Values.FirstOrDefault();
    }

    private static Image? LoadPreparedHeroSprite(params string[] fileNames)
    {
        Image? source = null;
        foreach (var fileName in fileNames)
        {
            source = LoadSprite(Path.Combine("Sprites", "Characters"), fileName);
            if (source is not null)
            {
                break;
            }
        }

        if (source is not Bitmap sourceBitmap)
        {
            source?.Dispose();
            return source;
        }

        try
        {
            var crop = FindOpaqueBounds(sourceBitmap, alphaThreshold: 8);
            if (crop.Width <= 0 || crop.Height <= 0)
            {
                return new Bitmap(sourceBitmap);
            }

            using var cropped = sourceBitmap.Clone(crop, sourceBitmap.PixelFormat);
            return ResizeSprite(cropped, targetHeight: TileSize + 16);
        }
        finally
        {
            sourceBitmap.Dispose();
        }
    }

    private static Rectangle FindOpaqueBounds(Bitmap bitmap, byte alphaThreshold)
    {
        var minX = bitmap.Width;
        var minY = bitmap.Height;
        var maxX = -1;
        var maxY = -1;

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).A < alphaThreshold)
                {
                    continue;
                }

                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        return maxX < minX || maxY < minY
            ? Rectangle.Empty
            : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
    }

    private static Rectangle FindLargestOpaqueRegionBounds(Bitmap bitmap, byte alphaThreshold)
    {
        return FindOpaqueRegions(bitmap, alphaThreshold, minimumPixelCount: 1)
            .OrderByDescending(region => region.PixelCount)
            .Select(region => region.Bounds)
            .FirstOrDefault();
    }

    private static Rectangle FindLargestForegroundRegionBounds(
        Bitmap bitmap,
        byte alphaThreshold,
        int minimumPixelCount,
        out bool[] selectedForeground)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var foreground = BuildForegroundMask(bitmap, alphaThreshold);
        var visited = new bool[foreground.Length];
        var queue = new Queue<int>();
        var component = new List<int>();
        var bestComponent = new List<int>();
        var bestBounds = Rectangle.Empty;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = (y * width) + x;
                if (!foreground[index] || visited[index])
                {
                    continue;
                }

                visited[index] = true;
                queue.Enqueue(index);
                component.Clear();

                var minX = x;
                var minY = y;
                var maxX = x;
                var maxY = y;

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);

                    var currentX = current % width;
                    var currentY = current / width;
                    minX = Math.Min(minX, currentX);
                    minY = Math.Min(minY, currentY);
                    maxX = Math.Max(maxX, currentX);
                    maxY = Math.Max(maxY, currentY);

                    for (var offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (var offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (offsetX == 0 && offsetY == 0)
                            {
                                continue;
                            }

                            var nextX = currentX + offsetX;
                            var nextY = currentY + offsetY;
                            if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                            {
                                continue;
                            }

                            var nextIndex = (nextY * width) + nextX;
                            if (!foreground[nextIndex] || visited[nextIndex])
                            {
                                continue;
                            }

                            visited[nextIndex] = true;
                            queue.Enqueue(nextIndex);
                        }
                    }
                }

                if (component.Count < minimumPixelCount || component.Count <= bestComponent.Count)
                {
                    continue;
                }

                bestComponent = [.. component];
                bestBounds = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            }
        }

        selectedForeground = new bool[foreground.Length];
        foreach (var index in bestComponent)
        {
            selectedForeground[index] = true;
        }

        return bestBounds;
    }

    private static bool[] BuildForegroundMask(Bitmap bitmap, byte alphaThreshold)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var backgroundCandidates = new bool[width * height];
        var borderBackground = new bool[width * height];
        var references = GetBackgroundReferenceColors(bitmap);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                backgroundCandidates[(y * width) + x] = IsBackgroundCandidate(pixel, references, alphaThreshold);
            }
        }

        var queue = new Queue<int>();
        void EnqueueBorderPixel(int x, int y)
        {
            var index = (y * width) + x;
            if (!backgroundCandidates[index] || borderBackground[index])
            {
                return;
            }

            borderBackground[index] = true;
            queue.Enqueue(index);
        }

        for (var x = 0; x < width; x++)
        {
            EnqueueBorderPixel(x, 0);
            EnqueueBorderPixel(x, height - 1);
        }

        for (var y = 0; y < height; y++)
        {
            EnqueueBorderPixel(0, y);
            EnqueueBorderPixel(width - 1, y);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentX = current % width;
            var currentY = current / width;

            for (var offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (var offsetX = -1; offsetX <= 1; offsetX++)
                {
                    if (offsetX == 0 && offsetY == 0)
                    {
                        continue;
                    }

                    var nextX = currentX + offsetX;
                    var nextY = currentY + offsetY;
                    if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                    {
                        continue;
                    }

                    var nextIndex = (nextY * width) + nextX;
                    if (!backgroundCandidates[nextIndex] || borderBackground[nextIndex])
                    {
                        continue;
                    }

                    borderBackground[nextIndex] = true;
                    queue.Enqueue(nextIndex);
                }
            }
        }

        var foreground = new bool[width * height];
        for (var index = 0; index < foreground.Length; index++)
        {
            foreground[index] = !borderBackground[index];
        }

        return foreground;
    }

    private static Color[] GetBackgroundReferenceColors(Bitmap bitmap)
    {
        var maxX = bitmap.Width - 1;
        var maxY = bitmap.Height - 1;
        return
        [
            bitmap.GetPixel(0, 0),
            bitmap.GetPixel(maxX, 0),
            bitmap.GetPixel(0, maxY),
            bitmap.GetPixel(maxX, maxY),
            bitmap.GetPixel(bitmap.Width / 2, 0),
            bitmap.GetPixel(bitmap.Width / 2, maxY),
            bitmap.GetPixel(0, bitmap.Height / 2),
            bitmap.GetPixel(maxX, bitmap.Height / 2)
        ];
    }

    private static bool IsBackgroundCandidate(Color pixel, IReadOnlyList<Color> references, byte alphaThreshold)
    {
        if (pixel.A < alphaThreshold)
        {
            return true;
        }

        return references.Any(reference =>
            reference.A >= alphaThreshold &&
            GetColorDistanceSquared(pixel, reference) <= 52 * 52);
    }

    private static int GetColorDistanceSquared(Color left, Color right)
    {
        var red = left.R - right.R;
        var green = left.G - right.G;
        var blue = left.B - right.B;
        return (red * red) + (green * green) + (blue * blue);
    }

    private static Bitmap ExtractForegroundSprite(Bitmap source, Rectangle crop, bool[] selectedForeground)
    {
        var output = new Bitmap(crop.Width, crop.Height, PixelFormat.Format32bppPArgb);
        for (var y = 0; y < crop.Height; y++)
        {
            for (var x = 0; x < crop.Width; x++)
            {
                var sourceX = crop.X + x;
                var sourceY = crop.Y + y;
                var sourceIndex = (sourceY * source.Width) + sourceX;
                if (selectedForeground.Length > sourceIndex && selectedForeground[sourceIndex])
                {
                    output.SetPixel(x, y, source.GetPixel(sourceX, sourceY));
                }
                else
                {
                    output.SetPixel(x, y, Color.Transparent);
                }
            }
        }

        return output;
    }

    private static List<(Rectangle Bounds, int PixelCount)> FindOpaqueRegions(Bitmap bitmap, byte alphaThreshold, int minimumPixelCount)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var visited = new bool[width * height];
        var queue = new Queue<Point>();
        var regions = new List<(Rectangle Bounds, int PixelCount)>();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = (y * width) + x;
                if (visited[index] || bitmap.GetPixel(x, y).A < alphaThreshold)
                {
                    continue;
                }

                visited[index] = true;
                queue.Enqueue(new Point(x, y));

                var minX = x;
                var minY = y;
                var maxX = x;
                var maxY = y;
                var pixelCount = 0;

                while (queue.Count > 0)
                {
                    var point = queue.Dequeue();
                    pixelCount++;
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);

                    for (var offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (var offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (offsetX == 0 && offsetY == 0)
                            {
                                continue;
                            }

                            var nextX = point.X + offsetX;
                            var nextY = point.Y + offsetY;
                            if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                            {
                                continue;
                            }

                            var nextIndex = (nextY * width) + nextX;
                            if (visited[nextIndex] || bitmap.GetPixel(nextX, nextY).A < alphaThreshold)
                            {
                                continue;
                            }

                            visited[nextIndex] = true;
                            queue.Enqueue(new Point(nextX, nextY));
                        }
                    }
                }

                if (pixelCount < minimumPixelCount)
                {
                    continue;
                }

                regions.Add((Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1), pixelCount));
            }
        }

        return regions;
    }

    private static Bitmap ResizeSprite(Image sprite, int targetHeight)
    {
        var scale = targetHeight / (float)sprite.Height;
        var targetWidth = Math.Max(1, (int)Math.Round(sprite.Width * scale));
        var targetSize = new Size(targetWidth, targetHeight);
        var resized = new Bitmap(targetSize.Width, targetSize.Height);

        using var graphics = Graphics.FromImage(resized);
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.SmoothingMode = SmoothingMode.None;
        graphics.DrawImage(sprite, new Rectangle(Point.Empty, targetSize));

        return resized;
    }

    private void DisposeFieldSprites()
    {
        mapTileSheet?.Dispose();
        mapTileSheet = null;
        fieldMapImage?.Dispose();
        fieldMapImage = null;

        foreach (var sprite in heroSprites.Values)
        {
            sprite.Dispose();
        }

        heroSprites.Clear();

        foreach (var sprite in npcSprites.Values)
        {
            sprite.Dispose();
        }

        npcSprites.Clear();

        foreach (var sprite in enemySprites.Values)
        {
            sprite.Dispose();
        }

        enemySprites.Clear();

        foreach (var portrait in npcPortraits.Values)
        {
            portrait.Dispose();
        }

        npcPortraits.Clear();

        foreach (var image in uiImages.Values)
        {
            image.Dispose();
        }

        uiImages.Clear();
    }
}
