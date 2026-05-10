using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DrawingColor = System.Drawing.Color;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace DragonGlareAlpha;

internal sealed class TtfSpriteTextRenderer : IDisposable
{
    private const string FontFileName = "JF-Dot-ShinonomeMin14.ttf";

    private readonly GraphicsDevice graphicsDevice;
    private readonly PrivateFontCollection privateFontCollection = new();
    private readonly Font font;
    private readonly Bitmap measureBitmap = new(1, 1, PixelFormat.Format32bppArgb);
    private readonly Graphics measureGraphics;
    private readonly Dictionary<string, Texture2D> textureCache = [];
    private readonly Dictionary<string, int> widthCache = [];
    private byte[] bitmapBytes = [];
    private XnaColor[] pixels = [];
    private bool disposed;

    public TtfSpriteTextRenderer(GraphicsDevice graphicsDevice, string fontPath)
    {
        this.graphicsDevice = graphicsDevice;
        privateFontCollection.AddFontFile(fontPath);
        if (privateFontCollection.Families.Length == 0)
        {
            throw new InvalidOperationException($"Font family was not found in {fontPath}.");
        }

        font = new Font(privateFontCollection.Families[0], UiTypography.FontPixelSize, FontStyle.Regular, GraphicsUnit.Pixel);
        measureGraphics = Graphics.FromImage(measureBitmap);
        ConfigureGraphics(measureGraphics);
    }

    public static string? ResolveFontPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, FontFileName),
            Path.Combine(AppContext.BaseDirectory, "Content", FontFileName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", FontFileName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", FontFileName),
            Path.Combine(Directory.GetCurrentDirectory(), FontFileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", FontFileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Content", FontFileName)
        };

        return candidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
    }

    public int MeasureWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        if (widthCache.TryGetValue(text, out var cachedWidth))
        {
            return cachedWidth;
        }

        var width = (int)Math.Ceiling(measureGraphics.MeasureString(text, font, PointF.Empty, TextMeasureFormat).Width);
        widthCache[text] = width;
        return width;
    }

    public void DrawLine(SpriteBatch spriteBatch, string text, Vector2 position, XnaColor color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var texture = GetOrCreateTexture(text);
        spriteBatch.Draw(texture, position, color);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (var texture in textureCache.Values)
        {
            texture.Dispose();
        }

        measureGraphics.Dispose();
        measureBitmap.Dispose();
        font.Dispose();
        privateFontCollection.Dispose();
        disposed = true;
    }

    private Texture2D GetOrCreateTexture(string text)
    {
        if (textureCache.TryGetValue(text, out var cachedTexture))
        {
            return cachedTexture;
        }

        var width = Math.Max(1, MeasureWidth(text));
        var height = Math.Max(UiTypography.LineHeight, font.Height);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            ConfigureGraphics(g);
            g.Clear(DrawingColor.Transparent);

            using var brush = new SolidBrush(DrawingColor.White);
            var textOffsetY = Math.Max(0f, (height - font.Height) / 2f);
            g.DrawString(text, font, brush, 0, textOffsetY, TextDrawFormat);
        }

        var texture = new Texture2D(graphicsDevice, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);
        texture.SetData(ConvertBitmapToPixels(bitmap));
        textureCache[text] = texture;
        return texture;
    }

    private XnaColor[] ConvertBitmapToPixels(Bitmap bitmap)
    {
        var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var stride = Math.Abs(data.Stride);
            var byteCount = stride * data.Height;
            if (bitmapBytes.Length != byteCount)
            {
                bitmapBytes = new byte[byteCount];
            }

            Marshal.Copy(data.Scan0, bitmapBytes, 0, byteCount);

            var pixelCount = bitmap.Width * bitmap.Height;
            if (pixels.Length != pixelCount)
            {
                pixels = new XnaColor[pixelCount];
            }

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var src = (y * stride) + (x * 4);
                    var dst = (y * bitmap.Width) + x;
                    pixels[dst] = new XnaColor(bitmapBytes[src + 2], bitmapBytes[src + 1], bitmapBytes[src], bitmapBytes[src + 3]);
                }
            }

            return pixels;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static void ConfigureGraphics(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
    }

    private static readonly StringFormat TextDrawFormat = new(StringFormat.GenericTypographic)
    {
        FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
    };

    private static readonly StringFormat TextMeasureFormat = new(StringFormat.GenericTypographic)
    {
        FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces
    };
}
