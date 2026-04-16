using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Commerce;
using DragonGlareAlpha.Domain.Field;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Persistence;
using DragonGlareAlpha.Security;
using DragonGlareAlpha.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpriteFontPlus;
using DragonGlareAlpha.Data; // GameContent を使うために追加

namespace DragonGlare.MonoGame;

public class DragonGlareGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // --- ここから、古い DragonGlare.cs から移植したフィールド ---
    
    private static readonly Point PlayerStartTile = new(3, 12);
    
    private readonly StringBuilder playerName = new();
    
    // --- MonoGame用の新しいフィールド ---
    private Texture2D openingImage;
    private SpriteFont uiFont;
    private SpriteFont smallFont;
    
    // --- サービスとロジックはそのまま流用 ---
    private readonly Random random = new();
    private readonly SaveService saveService = new();
    private readonly AntiCheatService antiCheatService = new();
    private readonly BattleService battleService = new();
    private readonly ProgressionService progressionService = new();
    private readonly ShopService shopService = new();
    private readonly BankService bankService = new();
    private readonly FieldEventService fieldEventService = new();
    private readonly FieldTransitionService fieldTransitionService = new();

    // --- ゲームの状態変数 ---
    private PlayerProgress player;
    private GameState gameState = GameState.ModeSelect;
    
    private int languageOpeningElapsedFrames;
    private int languageOpeningLineIndex;
    private int languageOpeningLineFrame;
    private bool languageOpeningFinished;

    public DragonGlareGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        player = PlayerProgress.CreateDefault(PlayerStartTile); // player をここで初期化
    }

    protected override void Initialize()
    {
        // 仮想解像度を設定
        _graphics.PreferredBackBufferWidth = 640;
        _graphics.PreferredBackBufferHeight = 480;
        _graphics.ApplyChanges();

        // サービスなどの初期化
        // saveService.TryMigrateLegacySave(...);
        // RefreshSaveSlotSummaries();
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        using (var stream = new FileStream(Path.Combine(Content.RootDirectory, "UI", "SFC_opening.png"), FileMode.Open))
        {
            openingImage = Texture2D.FromStream(GraphicsDevice, stream);
        }

        var fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(Path.Combine(Content.RootDirectory, "JF-Dot-ShinonomeMin14.ttf")),
            14, 1024, 1024,
            new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.Cyrillic, CharacterRange.Japanese });

        uiFont = fontBakeResult.CreateSpriteFont(GraphicsDevice);
        smallFont = uiFont;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!languageOpeningFinished)
        {
            languageOpeningElapsedFrames++;
            if (languageOpeningLineIndex < GameContent.LanguageOpeningScript.Length)
            {
                languageOpeningLineFrame++;
                var currentLine = GameContent.LanguageOpeningScript[languageOpeningLineIndex];
                if (languageOpeningLineFrame > currentLine.DisplayFrames + currentLine.GapFrames)
                {
                    languageOpeningLineFrame = 0;
                    languageOpeningLineIndex++;
                }
            }

            if (languageOpeningElapsedFrames >= GameContent.LanguageOpeningTotalFrames)
            {
                languageOpeningFinished = true;
            }
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // オープニング背景を描画 (DrawLanguageOpeningBackdrop の簡易移植)
        if (openingImage != null)
        {
            _spriteBatch.Draw(openingImage, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
        }

        // オープニングナレーションを描画 (DrawLanguageOpeningNarration の移植)
        DrawLanguageOpeningNarration(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    // --- WinFormsの描画メソッドをMonoGame用に移植 ---
    private void DrawLanguageOpeningNarration(SpriteBatch spriteBatch)
    {
        if (languageOpeningFinished || languageOpeningLineIndex >= GameContent.LanguageOpeningScript.Length || uiFont == null)
        {
            return;
        }
        
        var currentLineData = GameContent.LanguageOpeningScript[languageOpeningLineIndex];
        var text = (languageOpeningLineFrame < currentLineData.DisplayFrames) ? currentLineData.Text : string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var lines = text.Split('\n');
        
        // 画面中央に表示するためのY座標を計算
        var totalHeight = lines.Length * uiFont.LineSpacing;
        int startY = 220 + Math.Max(0, (48 - totalHeight) / 2);

        // フェードイン・フェードアウトのアルファ値計算
        float alpha = 1f;
        int fadeFrames = 24;
        if (languageOpeningLineFrame < fadeFrames)
        {
            alpha = languageOpeningLineFrame / (float)fadeFrames;
        }
        else if (languageOpeningLineFrame > currentLineData.DisplayFrames - fadeFrames)
        {
            alpha = (currentLineData.DisplayFrames - languageOpeningLineFrame) / (float)fadeFrames;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var textSize = uiFont.MeasureString(line);
            
            // X座標を画面中央に合わせ、整数に丸める
            int x = (int)Math.Floor((_graphics.PreferredBackBufferWidth / 2f) - (textSize.X / 2f));
            int y = startY + (i * uiFont.LineSpacing);

            // 1pxの黒いアウトライン（十字4方向）
            var shadowColor = Color.Black * alpha;
            spriteBatch.DrawString(uiFont, line, new Vector2(x, y - 1), shadowColor);
            spriteBatch.DrawString(uiFont, line, new Vector2(x - 1, y), shadowColor);
            spriteBatch.DrawString(uiFont, line, new Vector2(x + 1, y), shadowColor);
            spriteBatch.DrawString(uiFont, line, new Vector2(x, y + 1), shadowColor);
            
            // メインの白い文字
            var mainColor = Color.White * alpha;
            spriteBatch.DrawString(uiFont, line, new Vector2(x, y), mainColor);
        }
    }
}
