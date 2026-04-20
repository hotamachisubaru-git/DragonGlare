using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace DragonGlare.Managers
{
    public static class AssetManager
    {
        private static readonly Dictionary<string, Texture2D> Textures = new();

        public static SpriteFont? MainFont { get; private set; }

        public static Texture2D? Pixel { get; private set; }

        public static void Load(ContentManager content)
        {
            var graphicsDevice = GetGraphicsDevice(content);
            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData([XnaColor.White]);

            Textures.Clear();
            TryLoadHeroTexture(graphicsDevice, content.RootDirectory);
            TryLoadTexture(graphicsDevice, content.RootDirectory, "player", "Sprites", "Characters", "hero.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "field_scout", "Sprites", "NPC", "field_scout.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "guide_npc", "Sprites", "NPC", "guide_npc.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "town_child", "Sprites", "NPC", "town_child.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "castle_guard", "Sprites", "NPC", "castle_guard.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "guide-4", "Portraits", "NPC", "guide-4.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "young-5", "Portraits", "NPC", "young-5.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "castle-guard-4", "Portraits", "NPC", "castle-guard-4.png");
            TryLoadTexture(graphicsDevice, content.RootDirectory, "mihari-3", "Portraits", "NPC", "mihari-3.png");

            MainFont = LoadFont(graphicsDevice, content.RootDirectory);
        }

        public static Texture2D? GetTexture(string name)
        {
            return Textures.GetValueOrDefault(name);
        }

        private static void TryLoadTexture(GraphicsDevice graphicsDevice, string rootDirectory, string key, params string[] pathParts)
        {
            if (Textures.ContainsKey(key))
            {
                return;
            }

            var path = GetAssetPath(rootDirectory, pathParts);
            if (path is null)
            {
                return;
            }

            using var stream = File.OpenRead(path);
            Textures[key] = Texture2D.FromStream(graphicsDevice, stream);
        }

        private static void TryLoadHeroTexture(GraphicsDevice graphicsDevice, string rootDirectory)
        {
            if (Textures.ContainsKey("player_down"))
            {
                return;
            }

            var heroSheetPath = GetAssetPath(rootDirectory, ["Sprites", "Characters", "hero_4.png"]);
            if (heroSheetPath is not null && TryLoadHeroDirectionsFromSheet(graphicsDevice, heroSheetPath))
            {
                return;
            }

            var heroPath = GetAssetPath(rootDirectory, ["Sprites", "Characters", "hero_full.png"]);
            if (heroPath is not null)
            {
                TryLoadPreparedBitmapTexture(graphicsDevice, "player", heroPath, targetHeight: 48);
                CopyHeroFallbackDirections();
            }
        }

        private static bool TryLoadHeroDirectionsFromSheet(GraphicsDevice graphicsDevice, string path)
        {
            try
            {
                using var source = new Bitmap(path);
                var cellWidth = source.Width / 2;
                var cellHeight = source.Height / 2;
                if (cellWidth <= 0 || cellHeight <= 0)
                {
                    return false;
                }

                LoadHeroDirectionFromSheet(graphicsDevice, source, "player_left", new Rectangle(0, 0, cellWidth, cellHeight));
                LoadHeroDirectionFromSheet(graphicsDevice, source, "player_right", new Rectangle(cellWidth, 0, cellWidth, cellHeight));
                LoadHeroDirectionFromSheet(graphicsDevice, source, "player_up", new Rectangle(0, cellHeight, cellWidth, cellHeight));
                LoadHeroDirectionFromSheet(graphicsDevice, source, "player_down", new Rectangle(cellWidth, cellHeight, cellWidth, cellHeight));

                if (!Textures.TryGetValue("player_down", out var downTexture))
                {
                    return false;
                }

                Textures["player"] = downTexture;
                return Textures.ContainsKey("player_left") &&
                    Textures.ContainsKey("player_right") &&
                    Textures.ContainsKey("player_up") &&
                    Textures.ContainsKey("player_down");
            }
            catch
            {
                return false;
            }
        }

        private static void LoadHeroDirectionFromSheet(GraphicsDevice graphicsDevice, Bitmap source, string key, Rectangle sourceRegion)
        {
            using var cell = source.Clone(sourceRegion, source.PixelFormat);
            var crop = FindOpaqueBounds(cell);
            if (crop.Width <= 0 || crop.Height <= 0)
            {
                return;
            }

            using var cropped = cell.Clone(crop, cell.PixelFormat);
            using var resized = ResizeBitmap(cropped, targetHeight: 48);
            Textures[key] = CreateTexture(graphicsDevice, resized);
        }

        private static void CopyHeroFallbackDirections()
        {
            if (!Textures.TryGetValue("player", out var texture))
            {
                return;
            }

            Textures.TryAdd("player_left", texture);
            Textures.TryAdd("player_right", texture);
            Textures.TryAdd("player_up", texture);
            Textures.TryAdd("player_down", texture);
        }

        private static void TryLoadPreparedBitmapTexture(GraphicsDevice graphicsDevice, string key, string path, int targetHeight)
        {
            if (Textures.ContainsKey(key))
            {
                return;
            }

            try
            {
                using var source = new Bitmap(path);
                var crop = FindOpaqueBounds(source);
                using var cropped = crop.Width > 0 && crop.Height > 0
                    ? source.Clone(crop, source.PixelFormat)
                    : new Bitmap(source);
                using var resized = ResizeBitmap(cropped, targetHeight);
                Textures[key] = CreateTexture(graphicsDevice, resized);
            }
            catch
            {
            }
        }

        private static Rectangle FindOpaqueBounds(Bitmap bitmap)
        {
            var minX = bitmap.Width;
            var minY = bitmap.Height;
            var maxX = -1;
            var maxY = -1;

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).A < 8)
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

        private static Bitmap ResizeBitmap(Bitmap bitmap, int targetHeight)
        {
            var scale = targetHeight / (float)bitmap.Height;
            var targetWidth = Math.Max(1, (int)Math.Round(bitmap.Width * scale));
            var resized = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppPArgb);

            using var graphics = Graphics.FromImage(resized);
            graphics.Clear(Color.Transparent);
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.DrawImage(bitmap, new Rectangle(0, 0, targetWidth, targetHeight));

            return resized;
        }

        private static Texture2D CreateTexture(GraphicsDevice graphicsDevice, Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return Texture2D.FromStream(graphicsDevice, stream);
        }

        private static SpriteFont? LoadFont(GraphicsDevice graphicsDevice, string rootDirectory)
        {
            var path = GetFirstExistingPath(
                Path.Combine(rootDirectory, "JF-Dot-ShinonomeMin14.ttf"),
                Path.Combine(AppContext.BaseDirectory, rootDirectory, "JF-Dot-ShinonomeMin14.ttf"),
                Path.Combine("Assets", "JF-Dot-ShinonomeMin14.ttf"),
                Path.Combine(AppContext.BaseDirectory, "Assets", "JF-Dot-ShinonomeMin14.ttf"),
                "JF-Dot-ShinonomeMin14.ttf",
                Path.Combine(AppContext.BaseDirectory, "JF-Dot-ShinonomeMin14.ttf"));
            if (path is null)
            {
                return null;
            }

            var bakeResult = TtfFontBaker.Bake(
                File.ReadAllBytes(path),
                14,
                1024,
                1024,
                [
                    new SpriteFontPlus.CharacterRange((char)0x0020, (char)0x007f),
                    new SpriteFontPlus.CharacterRange((char)0x3000, (char)0x30ff),
                    new SpriteFontPlus.CharacterRange((char)0xff00, (char)0xffef),
                    new SpriteFontPlus.CharacterRange((char)0x4e00, (char)0x9fff)
                ]);

            return bakeResult.CreateSpriteFont(graphicsDevice);
        }

        private static string? GetAssetPath(string rootDirectory, string[] pathParts)
        {
            var relativePath = Path.Combine(pathParts);
            return GetFirstExistingPath(
                Path.Combine(rootDirectory, relativePath),
                Path.Combine(AppContext.BaseDirectory, rootDirectory, relativePath),
                Path.Combine("Assets", relativePath),
                Path.Combine(AppContext.BaseDirectory, "Assets", relativePath));
        }

        private static string? GetFirstExistingPath(params string[] paths)
        {
            return paths.FirstOrDefault(File.Exists);
        }

        private static GraphicsDevice GetGraphicsDevice(ContentManager content)
        {
            var service = content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            return service?.GraphicsDevice
                ?? throw new InvalidOperationException("GraphicsDeviceService is not available from the ContentManager.");
        }
    }
}
