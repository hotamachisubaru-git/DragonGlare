using DragonGlareAlpha.Domain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private Texture2D? spriteBatchPixel;
    private Texture2D? spriteBatchModeSelectLayout;
    private SpriteFont? spriteBatchUiFont;

    partial void LoadSpriteBatchUiContent()
    {
        spriteBatchPixel = new Texture2D(GraphicsDevice, 1, 1);
        spriteBatchPixel.SetData([XnaColor.White]);

        spriteBatchModeSelectLayout = LoadSpriteBatchTexture("UI", "window21.png");
        spriteBatchUiFont = LoadSpriteBatchFont();
    }

    partial void TryDrawSpriteBatchFrame(GameTime gameTime, ref bool handled)
    {
        if (gameState != GameState.ModeSelect ||
            spriteBatch is null ||
            spriteBatchPixel is null ||
            spriteBatchUiFont is null)
        {
            return;
        }

        GraphicsDevice.Clear(XnaColor.Black);

        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: GetSpriteBatchVirtualTransform());

        DrawModeSelectSpriteBatch();
        DrawSpriteBatchSceneFade();

        spriteBatch.End();
        handled = true;
    }

    private void DrawModeSelectSpriteBatch()
    {
        DrawSpriteBatchMenuBackdrop();

        var layoutRect = new XnaRectangle(64, 0, 512, 480);
        if (spriteBatchModeSelectLayout is not null)
        {
            spriteBatch!.Draw(spriteBatchModeSelectLayout, layoutRect, XnaColor.White);
        }
        else
        {
            DrawSpriteBatchWindow(new XnaRectangle(96, 32, 184, 192));
            DrawSpriteBatchWindow(new XnaRectangle(288, 32, 256, 192));
            DrawSpriteBatchWindow(new XnaRectangle(96, 232, 448, 176));
        }

        var menuItems = new[]
        {
            "はじめから",
            "つづきから",
            "データうつす",
            "データけす"
        };

        var menuStartX = ScaleModeSelectX(layoutRect, 24);
        var menuStartY = ScaleModeSelectY(layoutRect, 24);
        var menuLineHeight = ScaleModeSelectHeight(layoutRect, 24);
        var menuCursorX = ScaleModeSelectX(layoutRect, 16);

        for (var index = 0; index < menuItems.Length; index++)
        {
            var lineY = menuStartY + (index * menuLineHeight);
            if (modeCursor == index)
            {
                DrawModeSelectCursorSpriteBatch(menuCursorX, lineY + 4);
            }

            DrawSpriteBatchText(menuItems[index], new Vector2(menuStartX, lineY));
        }

        DrawSpriteBatchText(
            GetModeSelectDescription(modeCursor),
            new XnaRectangle(
                ScaleModeSelectX(layoutRect, 128),
                ScaleModeSelectY(layoutRect, 24),
                ScaleModeSelectWidth(layoutRect, 96),
                ScaleModeSelectHeight(layoutRect, 64)),
            wrap: true);

        DrawSpriteBatchText(
            string.IsNullOrWhiteSpace(menuNotice) ? "モードを選んでください。" : menuNotice,
            new XnaRectangle(
                ScaleModeSelectX(layoutRect, 24),
                ScaleModeSelectY(layoutRect, 136),
                ScaleModeSelectWidth(layoutRect, 200),
                ScaleModeSelectHeight(layoutRect, 64)),
            wrap: true);
    }

    private void DrawSpriteBatchMenuBackdrop()
    {
        spriteBatch!.Draw(spriteBatchPixel!, new XnaRectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight), new XnaColor(0, 4, 12));

        for (var y = 0; y < UiCanvas.VirtualHeight; y += 4)
        {
            spriteBatch.Draw(spriteBatchPixel!, new XnaRectangle(0, y, UiCanvas.VirtualWidth, 1), new XnaColor(24, 38, 80));
        }

        spriteBatch.Draw(spriteBatchPixel!, new XnaRectangle(0, 0, 18, UiCanvas.VirtualHeight), new XnaColor(0, 18, 66));
        spriteBatch.Draw(spriteBatchPixel!, new XnaRectangle(UiCanvas.VirtualWidth - 18, 0, 18, UiCanvas.VirtualHeight), new XnaColor(0, 18, 66));
    }

    private void DrawSpriteBatchWindow(XnaRectangle rect)
    {
        spriteBatch!.Draw(spriteBatchPixel!, rect, XnaColor.Black);
        spriteBatch.Draw(spriteBatchPixel!, new XnaRectangle(rect.Left, rect.Top, rect.Width, 1), XnaColor.White);
        spriteBatch.Draw(spriteBatchPixel!, new XnaRectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), XnaColor.White);
        spriteBatch.Draw(spriteBatchPixel!, new XnaRectangle(rect.Left, rect.Top, 1, rect.Height), XnaColor.White);
        spriteBatch.Draw(spriteBatchPixel!, new XnaRectangle(rect.Right - 1, rect.Top, 1, rect.Height), XnaColor.White);
    }

    private void DrawModeSelectCursorSpriteBatch(int x, int y)
    {
        if ((frameCounter / 18) % 2 == 1)
        {
            return;
        }

        var slices = new[]
        {
            new XnaRectangle(x, y, 2, 14),
            new XnaRectangle(x + 2, y + 1, 2, 12),
            new XnaRectangle(x + 4, y + 2, 2, 10),
            new XnaRectangle(x + 6, y + 3, 2, 8),
            new XnaRectangle(x + 8, y + 4, 2, 6),
            new XnaRectangle(x + 10, y + 5, 2, 4)
        };

        foreach (var slice in slices)
        {
            spriteBatch!.Draw(spriteBatchPixel!, slice, XnaColor.White);
        }
    }

    private void DrawSpriteBatchText(string text, Vector2 position, XnaColor? color = null)
    {
        if (string.IsNullOrEmpty(text) || spriteBatchUiFont is null)
        {
            return;
        }

        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            spriteBatch!.DrawString(
                spriteBatchUiFont,
                lines[index],
                new Vector2(position.X, position.Y + (index * UiTypography.LineHeight)),
                color ?? XnaColor.White);
        }
    }

    private void DrawSpriteBatchText(string text, XnaRectangle bounds, bool wrap)
    {
        if (string.IsNullOrEmpty(text) || spriteBatchUiFont is null)
        {
            return;
        }

        var lines = LayoutSpriteBatchTextLines(text, bounds.Width, Math.Max(1, bounds.Height / UiTypography.LineHeight), wrap);
        for (var index = 0; index < lines.Count; index++)
        {
            spriteBatch!.DrawString(
                spriteBatchUiFont,
                lines[index],
                new Vector2(bounds.X, bounds.Y + (index * UiTypography.LineHeight)),
                XnaColor.White);
        }
    }

    private List<string> LayoutSpriteBatchTextLines(string text, int maxWidth, int maxLines, bool wrap)
    {
        var output = new List<string>();
        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (!wrap)
            {
                output.Add(TrimSpriteBatchLineToWidth(rawLine, maxWidth));
            }
            else
            {
                var currentLine = string.Empty;
                foreach (var character in rawLine)
                {
                    var candidate = currentLine + character;
                    if (currentLine.Length > 0 && MeasureSpriteBatchTextWidth(candidate) > maxWidth)
                    {
                        output.Add(currentLine);
                        currentLine = character.ToString();
                    }
                    else
                    {
                        currentLine = candidate;
                    }

                    if (output.Count >= maxLines)
                    {
                        break;
                    }
                }

                if (output.Count < maxLines && (currentLine.Length > 0 || rawLine.Length == 0))
                {
                    output.Add(currentLine);
                }
            }

            if (output.Count >= maxLines)
            {
                break;
            }
        }

        return output.Count == 0 ? [string.Empty] : output;
    }

    private string TrimSpriteBatchLineToWidth(string text, int maxWidth)
    {
        if (MeasureSpriteBatchTextWidth(text) <= maxWidth)
        {
            return text;
        }

        const string ellipsis = "...";
        for (var length = text.Length - 1; length >= 0; length--)
        {
            var candidate = text[..length] + ellipsis;
            if (MeasureSpriteBatchTextWidth(candidate) <= maxWidth)
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    private int MeasureSpriteBatchTextWidth(string text)
    {
        return spriteBatchUiFont is null || string.IsNullOrEmpty(text)
            ? 0
            : (int)Math.Ceiling(spriteBatchUiFont.MeasureString(text).X);
    }

    private void DrawSpriteBatchSceneFade()
    {
        if (startupFadeFrames > 0)
        {
            var alpha = startupFadeFrames / 20f;
            spriteBatch!.Draw(spriteBatchPixel!, new XnaRectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight), XnaColor.Black * alpha);
        }

        if (pendingGameState is not null && sceneFadeOutFramesRemaining > 0)
        {
            var progress = 1f - (sceneFadeOutFramesRemaining / (float)SceneFadeOutDuration);
            spriteBatch!.Draw(spriteBatchPixel!, new XnaRectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight), XnaColor.Black * MathHelper.Clamp(progress, 0f, 1f));
        }
    }

    private Microsoft.Xna.Framework.Matrix GetSpriteBatchVirtualTransform()
    {
        var destination = GetVirtualDestination();
        var scaleX = destination.Width / (float)UiCanvas.VirtualWidth;
        var scaleY = destination.Height / (float)UiCanvas.VirtualHeight;
        return Microsoft.Xna.Framework.Matrix.CreateScale(scaleX, scaleY, 1f) *
            Microsoft.Xna.Framework.Matrix.CreateTranslation(destination.X, destination.Y, 0f);
    }

    private Texture2D? LoadSpriteBatchTexture(string assetSubdirectory, string fileName)
    {
        var path = ResolveAssetPath(assetSubdirectory, fileName);
        if (path is null)
        {
            return null;
        }

        using var stream = File.OpenRead(path);
        return Texture2D.FromStream(GraphicsDevice, stream);
    }

    private SpriteFont? LoadSpriteBatchFont()
    {
        var path = ResolveAssetPath(null, "JF-Dot-ShinonomeMin14.ttf");
        if (path is null)
        {
            return null;
        }

        var bakeResult = TtfFontBaker.Bake(
            File.ReadAllBytes(path),
            UiTypography.FontPixelSize,
            1024,
            1024,
            [
                new SpriteFontPlus.CharacterRange((char)0x0020, (char)0x007f),
                new SpriteFontPlus.CharacterRange((char)0x3000, (char)0x30ff),
                new SpriteFontPlus.CharacterRange((char)0x4e00, (char)0x9fff)
            ]);

        return bakeResult.CreateSpriteFont(GraphicsDevice);
    }

    private static int ScaleModeSelectX(XnaRectangle layoutRect, int sourceX)
    {
        return layoutRect.X + (int)Math.Round(sourceX * (layoutRect.Width / 256f));
    }

    private static int ScaleModeSelectY(XnaRectangle layoutRect, int sourceY)
    {
        return layoutRect.Y + (int)Math.Round(sourceY * (layoutRect.Height / 240f));
    }

    private static int ScaleModeSelectWidth(XnaRectangle layoutRect, int sourceWidth)
    {
        return (int)Math.Round(sourceWidth * (layoutRect.Width / 256f));
    }

    private static int ScaleModeSelectHeight(XnaRectangle layoutRect, int sourceHeight)
    {
        return (int)Math.Round(sourceHeight * (layoutRect.Height / 240f));
    }

}
