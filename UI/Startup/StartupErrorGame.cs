using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace DragonGlareAlpha;

internal sealed class StartupErrorGame : Game
{
    private readonly GraphicsDeviceManager graphics;
    private readonly string message;
    private SpriteBatch? spriteBatch;
    private Texture2D? pixel;
    private TtfSpriteTextRenderer? textRenderer;

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
        textRenderer = LoadTextRenderer();
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
        textRenderer?.Dispose();
        textRenderer = null;
        pixel?.Dispose();
        spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private TtfSpriteTextRenderer? LoadTextRenderer()
    {
        var path = TtfSpriteTextRenderer.ResolveFontPath();
        if (path is null)
        {
            return null;
        }

        try
        {
            return new TtfSpriteTextRenderer(GraphicsDevice, path);
        }
        catch
        {
            return null;
        }
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
        if (textRenderer is null)
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
        if (textRenderer is null)
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
                if (current.Length > 0 && textRenderer.MeasureWidth(candidate) > maxWidth)
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
        if (textRenderer is null || string.IsNullOrEmpty(text))
        {
            return;
        }

        textRenderer.DrawLine(spriteBatch!, text, position, color);
    }
}
