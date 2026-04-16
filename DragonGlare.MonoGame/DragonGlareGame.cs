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
using DragonGlareAlpha.Data;

namespace DragonGlare.MonoGame;

public record struct OpeningNarrationLine(string Text, int DisplayFrames, int GapFrames);

public class DragonGlareGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;

    // --- ここから、古い DragonGlare.cs から移植したフィールド ---
    
    private static readonly Point PlayerStartTile = new(3, 12);
    
    private readonly StringBuilder playerName = new();
    
    private static readonly OpeningNarrationLine[] LanguageOpeningScript =
    [
        new("遠い昔。", 180, 40),
        new("世界には五つの大地があった。", 180, 40),
        new("その時代では、争いがなく。\n皆が平和に満ちていた。", 260, 50),
        new("そして、それぞれの大地で\n違う神が崇められていたという。", 320, 50),
        new("しかし", 90, 120),
        new("平和は長くは続かなかった。", 250, 50),
        new("争いによって、日の大地は\n跡形もなく崩れ落ちてしまった。", 290, 50),
        new("そして、世界には暗い月しか\n上らなくなってしまった。", 280, 50),
        new("次から次へと世界は\n闇に満ちていった。", 250, 50),
        new("ついには、世界の中心となる光の大地が\n愚かな争いにより、闇に沈んでいった。", 340, 60),
        new("やがて、光の神は\n闇に飲み込まれ", 220, 50),
        new("世界にある万物が\n人々を襲うようにしてしまった。", 270, 50),
        new("世界は、いつしか光を失い\n闇が世界を司るようになった。", 300, 0)
    ];
    private static readonly int LanguageOpeningTotalFrames = LanguageOpeningScript.Sum(line => line.DisplayFrames + line.GapFrames);

    // --- MonoGame用の新しいフィールド ---
    private Texture2D? openingImage;
    private SpriteFont? uiFont;
    private SpriteFont? smallFont;
    
    // --- サービスとロジックはそのまま流用 ---
    private readonly Random random = new();
    private readonly SaveService saveService = new("saves"); // 引数を追加
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
        player = PlayerProgress.CreateDefault(new System.Drawing.Point(PlayerStartTile.X, PlayerStartTile.Y)); 
    }

    protected override void Initialize()
    {
        // 仮想解像度を設定
        _graphics.PreferredBackBufferWidth = 640;
        _graphics.PreferredBackBufferHeight = 480;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        var openingImagePath = Path.Combine(Content.RootDirectory, "UI", "SFC_opening.png");
        if (File.Exists(openingImagePath))
        {
            using (var stream = new FileStream(openingImagePath, FileMode.Open))
            {
                openingImage = Texture2D.FromStream(GraphicsDevice, stream);
            }
        }

        var fontPath = Path.Combine(Content.RootDirectory, "JF-Dot-ShinonomeMin14.ttf");
        if (File.Exists(fontPath))
        {
            var fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(fontPath),
                14, 1024, 1024,
                new[] { 
                    new CharacterRange((char)0x0020, (char)0x007F), // Basic Latin
                    new CharacterRange((char)0x00A0, (char)0x00FF), // Latin-1 Supplement
                    new CharacterRange((char)0x0400, (char)0x04FF), // Cyrillic
                    new CharacterRange((char)0x3000, (char)0x30FF), // CJK symbols and Hiragana/Katakana
                    new CharacterRange((char)0x4E00, (char)0x9FFF)  // CJK Unified Ideographs
                });

            uiFont = fontBakeResult.CreateSpriteFont(GraphicsDevice);
            smallFont = uiFont;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!languageOpeningFinished)
        {
            languageOpeningElapsedFrames++;
            if (languageOpeningLineIndex < LanguageOpeningScript.Length)
            {
                languageOpeningLineFrame++;
                var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
                if (languageOpeningLineFrame > currentLine.DisplayFrames + currentLine.GapFrames)
                {
                    languageOpeningLineFrame = 0;
                    languageOpeningLineIndex++;
                }
            }

            if (languageOpeningElapsedFrames >= LanguageOpeningTotalFrames)
            {
                languageOpeningFinished = true;
            }
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        if (_spriteBatch == null) return;

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // オープニング背景を描画
        if (openingImage != null)
        {
            _spriteBatch.Draw(openingImage, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
        }

        // オープニングナレーションを描画
        DrawLanguageOpeningNarration(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    // --- WinFormsの描画メソッドをMonoGame用に移植 ---
    private void DrawLanguageOpeningNarration(SpriteBatch spriteBatch)
    {
        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length || uiFont == null)
        {
            return;
        }
        
        var currentLineData = LanguageOpeningScript[languageOpeningLineIndex];
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
