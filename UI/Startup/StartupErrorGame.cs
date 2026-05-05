using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteFontPlus;
using SpriteCharacterRange = SpriteFontPlus.CharacterRange;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace DragonGlareAlpha;

internal sealed class StartupErrorGame : Game
{
    private readonly GraphicsDeviceManager graphics;
    private readonly string message;
    private SpriteBatch? spriteBatch;
    private Texture2D? pixel;
    private SpriteFont? font;

    private StartupErrorGame(string message)
    {
        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 640,
            PreferredBackBufferHeight = 240,
            SynchronizeWithVerticalRetrace = true
        };

        this.message = message;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = $"{AppMetadata.WindowTitle} Error";
    }

    public static void Show(string message)
    {
        using var game = new StartupErrorGame(message);
        game.Run();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData([XnaColor.White]);
        font = LoadFont();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var gamePad = GamePad.GetState(PlayerIndex.One);

        if (keyboard.IsKeyDown(Keys.Enter) ||
            keyboard.IsKeyDown(Keys.Escape) ||
            keyboard.IsKeyDown(Keys.Space) ||
            gamePad.Buttons.A == ButtonState.Pressed ||
            gamePad.Buttons.B == ButtonState.Pressed ||
            gamePad.Buttons.Back == ButtonState.Pressed)
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(XnaColor.Black);

        if (spriteBatch is null || pixel is null)
        {
            base.Draw(gameTime);
            return;
        }

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        DrawBorder(new XnaRectangle(24, 24, 592, 192));
        DrawText(AppMetadata.DisplayName, new Vector2(48, 48), XnaColor.White);
        DrawWrappedText(message, new XnaRectangle(48, 82, 544, 96));
        DrawText("Enter / Esc", new Vector2(48, 188), XnaColor.Gray);

        spriteBatch.End();
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        font = null;
        pixel?.Dispose();
        spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private SpriteFont? LoadFont()
    {
        var path = ResolveFontPath();
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
                new SpriteCharacterRange((char)0x0020, (char)0x007f),
                new SpriteCharacterRange((char)0x3000, (char)0x30ff),
                new SpriteCharacterRange((char)0x4e00, (char)0x9fff)
            ]);

        return bakeResult.CreateSpriteFont(GraphicsDevice);
    }

    private static string? ResolveFontPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "JF-Dot-ShinonomeMin14.ttf"),
            Path.Combine(AppContext.BaseDirectory, "Content", "JF-Dot-ShinonomeMin14.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "JF-Dot-ShinonomeMin14.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "JF-Dot-ShinonomeMin14.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Content", "JF-Dot-ShinonomeMin14.ttf")
        };

        return candidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
    }

    private void DrawBorder(XnaRectangle rect)
    {
        spriteBatch!.Draw(pixel!, rect, XnaColor.Black);
        spriteBatch.Draw(pixel!, new XnaRectangle(rect.Left, rect.Top, rect.Width, 1), XnaColor.White);
        spriteBatch.Draw(pixel!, new XnaRectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), XnaColor.White);
        spriteBatch.Draw(pixel!, new XnaRectangle(rect.Left, rect.Top, 1, rect.Height), XnaColor.White);
        spriteBatch.Draw(pixel!, new XnaRectangle(rect.Right - 1, rect.Top, 1, rect.Height), XnaColor.White);
    }

    private void DrawWrappedText(string text, XnaRectangle bounds)
    {
        if (font is null)
        {
            return;
        }

        var y = bounds.Y;
        foreach (var line in WrapText(text, bounds.Width))
        {
            if (y + UiTypography.LineHeight > bounds.Bottom)
            {
                return;
            }

            DrawText(line, new Vector2(bounds.X, y), XnaColor.White);
            y += UiTypography.LineHeight;
        }
    }

    private IReadOnlyList<string> WrapText(string text, int maxWidth)
    {
        if (font is null)
        {
            return [];
        }

        var lines = new List<string>();
        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var current = string.Empty;
            foreach (var character in rawLine)
            {
                var candidate = current + character;
                if (current.Length > 0 && font.MeasureString(candidate).X > maxWidth)
                {
                    lines.Add(current);
                    current = character.ToString();
                }
                else
                {
                    current = candidate;
                }
            }

            lines.Add(current);
        }

        return lines;
    }

    private void DrawText(string text, Vector2 position, XnaColor color)
    {
        if (font is null || string.IsNullOrEmpty(text))
        {
            return;
        }

        spriteBatch!.DrawString(font, text, position, color);
    }
}
