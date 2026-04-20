using System;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaPoint = Microsoft.Xna.Framework.Point;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace DragonGlare.Scenes
{
    public sealed class Field : IDisposable
    {
        private const int TileSize = Constants.TileSize;

        private readonly Texture2D _pixel;
        private int[,] _tiles;
        private bool _disposed;

        public Field(ContentManager content)
            : this(content, FieldMapId.Hub)
        {
        }

        public Field(ContentManager content, FieldMapId mapId)
        {
            ArgumentNullException.ThrowIfNull(content);

            var graphicsDevice = GetGraphicsDevice(content);
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData([XnaColor.White]);
            _tiles = MapFactory.CreateMap(mapId);
            MapId = mapId;
        }

        public FieldMapId MapId { get; private set; }

        public int WidthTiles => _tiles.GetLength(1);

        public int HeightTiles => _tiles.GetLength(0);

        public XnaRectangle Bounds => new(0, 0, WidthTiles * TileSize, HeightTiles * TileSize);

        public void SetMap(FieldMapId mapId)
        {
            MapId = mapId;
            _tiles = MapFactory.CreateMap(mapId);
        }

        public void Update(GameTime gameTime)
        {
            ArgumentNullException.ThrowIfNull(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, XnaVector2.Zero);
        }

        public void Draw(SpriteBatch spriteBatch, XnaVector2 origin)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

            for (var y = 0; y < HeightTiles; y++)
            {
                for (var x = 0; x < WidthTiles; x++)
                {
                    var tile = new XnaPoint(x, y);
                    var destination = new XnaRectangle(
                        (int)origin.X + (x * TileSize),
                        (int)origin.Y + (y * TileSize),
                        TileSize,
                        TileSize);

                    DrawTile(spriteBatch, GetTileId(tile), destination);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, XnaRectangle viewport, XnaPoint cameraOrigin)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

            spriteBatch.Draw(_pixel, viewport, GetTileColor(MapFactory.WallTile));

            var visibleWidthTiles = (int)Math.Ceiling(viewport.Width / (float)TileSize);
            var visibleHeightTiles = (int)Math.Ceiling(viewport.Height / (float)TileSize);
            for (var y = 0; y < visibleHeightTiles; y++)
            {
                for (var x = 0; x < visibleWidthTiles; x++)
                {
                    var worldTile = new XnaPoint(cameraOrigin.X + x, cameraOrigin.Y + y);
                    var destination = new XnaRectangle(
                        viewport.X + (x * TileSize),
                        viewport.Y + (y * TileSize),
                        TileSize,
                        TileSize);

                    DrawTile(spriteBatch, GetTileId(worldTile), destination);
                }
            }
        }

        public int GetTileId(XnaPoint tile)
        {
            return IsInsideMap(tile)
                ? _tiles[tile.Y, tile.X]
                : MapFactory.WallTile;
        }

        public bool IsInsideMap(XnaPoint tile)
        {
            return tile.X >= 0 &&
                tile.Y >= 0 &&
                tile.X < WidthTiles &&
                tile.Y < HeightTiles;
        }

        public bool IsWalkable(XnaPoint tile)
        {
            var tileId = GetTileId(tile);
            return tileId != MapFactory.WallTile &&
                tileId != MapFactory.CastleBlockTile;
        }

        public XnaPoint WorldToTile(XnaVector2 worldPosition)
        {
            return new XnaPoint(
                (int)Math.Floor(worldPosition.X / TileSize),
                (int)Math.Floor(worldPosition.Y / TileSize));
        }

        public XnaRectangle GetTileBounds(XnaPoint tile)
        {
            return new XnaRectangle(tile.X * TileSize, tile.Y * TileSize, TileSize, TileSize);
        }

        public XnaVector2 GetTileCenter(XnaPoint tile)
        {
            return new XnaVector2(
                (tile.X * TileSize) + (TileSize / 2f),
                (tile.Y * TileSize) + (TileSize / 2f));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _pixel.Dispose();
            _disposed = true;
        }

        private void DrawTile(SpriteBatch spriteBatch, int tileId, XnaRectangle destination)
        {
            spriteBatch.Draw(_pixel, destination, GetTileColor(tileId));
        }

        private XnaColor GetTileColor(int tileId)
        {
            return tileId switch
            {
                MapFactory.WallTile when MapId == FieldMapId.Castle => new XnaColor(58, 14, 24),
                MapFactory.WallTile => new XnaColor(8, 30, 90),
                MapFactory.CastleBlockTile => new XnaColor(120, 28, 38),
                MapFactory.CastleGateTile => new XnaColor(116, 58, 30),
                MapFactory.FieldGateTile => new XnaColor(24, 56, 40),
                MapFactory.CastleFloorTile => new XnaColor(108, 42, 52),
                MapFactory.GrassTile => new XnaColor(24, 74, 36),
                MapFactory.DecorationBlueTile when MapId == FieldMapId.Castle => new XnaColor(76, 20, 34),
                MapFactory.DecorationBlueTile => new XnaColor(8, 30, 90),
                _ => new XnaColor(5, 5, 5)
            };
        }

        private static GraphicsDevice GetGraphicsDevice(ContentManager content)
        {
            var service = content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            return service?.GraphicsDevice
                ?? throw new InvalidOperationException("GraphicsDeviceService is not available from the ContentManager.");
        }
    }
}
