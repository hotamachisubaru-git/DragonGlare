using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
using FormsKeys = System.Windows.Forms.Keys;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha : Game
{
    private const int TileSize = 32;
    private const int ShopItemsPerPage = 6;
    private const int CompactFieldViewportWidthTiles = 13;
    private const int ExpandedFieldViewportWidthTiles = 17;
    private const int CompactFieldViewportHeightTiles = 9;
    private const int ExpandedFieldViewportHeightTiles = 11;
    private const int ExpandedFieldViewportHorizontalMargin = 19;
    private const int ExpandedFieldViewportVerticalTrim = 16;
    private const int FieldMovementAnimationDuration = 12;
    private const int EncounterTransitionDuration = 26;
    private const int BattleSelectionVisibleRows = 4;
    private const int OpeningSourceViewportWidth = 256;
    private const int OpeningSourceViewportHeight = 240;
    private const int SceneFadeOutDuration = 40;
    private static readonly System.Drawing.Point PlayerStartTile = new(3, 12);
    private static readonly TimeSpan BgmLoopLeadTime = TimeSpan.FromMilliseconds(120);
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

    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch? spriteBatch;
    private Bitmap? frameBitmap;
    private Texture2D? frameTexture;
    private byte[] frameBytes = [];
    private XnaColor[] framePixels = [];

    private readonly HashSet<FormsKeys> heldKeys = [];
    private readonly HashSet<FormsKeys> pressedKeys = [];
    private readonly PrivateFontCollection privateFontCollection = new();
    private readonly StringBuilder playerName = new();
    private readonly System.Windows.Media.MediaPlayer bgmPlayer = new();
    private readonly System.Windows.Media.MediaPlayer sePlayer = new();
    private readonly Dictionary<BgmTrack, Uri> bgmUris = [];
    private readonly Dictionary<SoundEffect, Uri> seUris = [];
    private readonly Dictionary<string, Image> npcSprites = [];
    private readonly Dictionary<string, Image> npcPortraits = [];
    private readonly Dictionary<string, Image> uiImages = [];
    private readonly Dictionary<PlayerFacingDirection, Image> heroSprites = [];
    private Image? castleTileSprite;
    private readonly Random random = new();
    private readonly SaveService saveService = new();
    private readonly AntiCheatService antiCheatService = new();
    private readonly BattleService battleService = new();
    private readonly ProgressionService progressionService = new();
    private readonly ShopService shopService = new();
    private readonly BankService bankService = new();
    private readonly FieldEventService fieldEventService = new();
    private readonly FieldTransitionService fieldTransitionService = new();
    private readonly LaunchSettings launchSettings;

    private Font uiFont = new(UiTypography.DefaultFontFamilyName, UiTypography.FontPixelSize, GraphicsUnit.Pixel);
    private Font smallFont = new(UiTypography.DefaultFontFamilyName, UiTypography.FontPixelSize, GraphicsUnit.Pixel);
    private PlayerProgress player = PlayerProgress.CreateDefault(PlayerStartTile);
    private BattleEncounter? currentEncounter;
    private GameState gameState = GameState.ModeSelect;
    private FieldMapId currentFieldMap = FieldMapId.Hub;
    private int[,] map = MapFactory.CreateDefaultMap();
    private UiLanguage selectedLanguage = UiLanguage.Japanese;
    private int modeCursor;
    private int languageCursor;
    private int nameCursorRow;
    private int nameCursorColumn;
    private int saveSlotCursor;
    private int activeSaveSlot;
    private int movementCooldown;
    private bool isFieldDialogOpen;
    private bool isFieldStatusVisible;
    private bool fontLoaded;
    private int frameCounter;
    private int startupFadeFrames = 20;
    private int sceneFadeOutFramesRemaining;
    private GameState? pendingGameState;
    private PlayerFacingDirection playerFacingDirection = PlayerFacingDirection.Down;
    private System.Drawing.Point fieldMovementAnimationDirection = System.Drawing.Point.Empty;
    private int fieldMovementAnimationFramesRemaining;
    private int battleCursorRow;
    private int battleCursorColumn;
    private int battleListCursor;
    private int battleListScroll;
    private BattleFlowState battleFlowState = BattleFlowState.CommandSelection;
    private int shopPromptCursor;
    private int shopItemCursor;
    private int shopPageIndex;
    private ShopPhase shopPhase = ShopPhase.Welcome;
    private int bankPromptCursor;
    private int bankItemCursor;
    private BankPhase bankPhase = BankPhase.Welcome;
    private SaveSlotSelectionMode saveSlotSelectionMode = SaveSlotSelectionMode.Save;
    private string battleMessage = DefaultBattleMessage;
    private string[] battleMessageLines = [];
    private int battleMessageVisibleLines;
    private int battleMessageLineTimer;
    private const int BattleMessageLineDelayFrames = 30;
    private string shopMessage = ShopWelcomeMessage;
    private string bankMessage = BankWelcomeMessage;
    private BgmTrack? currentBgmTrack;
    private string menuNotice = string.Empty;
    private int menuNoticeFrames;
    private int languageOpeningElapsedFrames;
    private int languageOpeningLineIndex;
    private int languageOpeningLineFrame;
    private bool languageOpeningFinished;
    private bool skipLanguageSelectionPrompt;
    private int languageOpeningLastSourceX = -1;
    private int languageOpeningLastSourceY = -1;
    private bool skipSaveOnClose;
    private int encounterTransitionFrames;
    private int fieldEncounterStepsRemaining = 7;
    private int enemyHitFlashFramesRemaining;
    private BattleEncounter? pendingEncounter;
    private IReadOnlyList<string> activeFieldDialogPages = [];
    private int activeFieldDialogPageIndex;
    private string? activeFieldDialogPortraitAssetName;
    private IReadOnlyList<SaveSlotSummary> saveSlotSummaries = [];
    private LaunchDisplayMode activeDisplayMode;
    private LaunchDisplayMode lastWindowedDisplayMode = LaunchDisplayMode.Window640x480;

    private enum ShopMenuEntryType
    {
        Product,
        InventoryItem,
        PreviousPage,
        NextPage,
        Quit
    }

    private enum PlayerFacingDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private readonly record struct BattleSelectionEntry(
        string Label,
        string Detail,
        string Badge,
        ConsumableDefinition? Consumable = null,
        IEquipmentDefinition? Equipment = null);

    private readonly record struct ShopInventoryEntry(
        string ItemId,
        string Name,
        int Price,
        int AttackBonus,
        int DefenseBonus,
        int Count,
        string Detail);

    private readonly record struct ShopMenuEntry(
        ShopMenuEntryType Type,
        string Label,
        ShopProductDefinition? Product = null,
        ShopInventoryEntry? InventoryItem = null);

    private readonly record struct BankAmountOption(
        string Label,
        int Amount,
        bool UseMaximum = false,
        bool Quit = false);

    private readonly record struct OpeningNarrationLine(
        string Text,
        int DisplayFrames,
        int GapFrames);

    private string LegacySaveFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DragonGlareAlpha", "save1.sav");

    public DragonGlareAlpha(LaunchSettings? launchSettings = null)
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = $"DragonGlare Alpha v{typeof(DragonGlareAlpha).Assembly.GetName().Version}";

        this.launchSettings = launchSettings ?? new LaunchSettings();
        activeDisplayMode = this.launchSettings.DisplayMode;
        if (activeDisplayMode != LaunchDisplayMode.Fullscreen)
        {
            lastWindowedDisplayMode = activeDisplayMode;
        }

        ApplyDisplayMode();
        LoadCustomFont();
        InitializeAudio();
        LoadFieldSprites();
        saveService.TryMigrateLegacySave(LegacySaveFilePath);
        RefreshSaveSlotSummaries();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        frameBitmap = new Bitmap(UiCanvas.VirtualWidth, UiCanvas.VirtualHeight, PixelFormat.Format32bppArgb);
        frameTexture = new Texture2D(GraphicsDevice, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight, false, SurfaceFormat.Color);
        framePixels = new XnaColor[UiCanvas.VirtualWidth * UiCanvas.VirtualHeight];
        frameBytes = new byte[UiCanvas.VirtualWidth * UiCanvas.VirtualHeight * 4];
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            PollKeyboard();
            UpdateGame();
        }
        catch (TamperDetectedException ex)
        {
            HandleSecurityViolation(ex.Message);
        }
        finally
        {
            pressedKeys.Clear();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (spriteBatch is null || frameBitmap is null || frameTexture is null)
        {
            base.Draw(gameTime);
            return;
        }

        using (var g = Graphics.FromImage(frameBitmap))
        {
            RenderVirtualFrame(g);
        }

        UploadFrameTexture(frameBitmap, frameTexture);

        GraphicsDevice.Clear(XnaColor.Black);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(frameTexture, GetVirtualDestination(), XnaColor.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void RenderVirtualFrame(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
        DrawBackdrop(g, new System.Drawing.Rectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight));

        switch (gameState)
        {
            case GameState.ModeSelect:
                DrawModeSelect(g);
                break;
            case GameState.LanguageSelection:
                DrawLanguageSelection(g);
                break;
            case GameState.NameInput:
                DrawNameInput(g);
                break;
            case GameState.SaveSlotSelection:
                DrawSaveSlotSelection(g);
                break;
            case GameState.Field:
                DrawField(g);
                break;
            case GameState.EncounterTransition:
                DrawEncounterTransition(g);
                break;
            case GameState.Battle:
                DrawBattle(g);
                break;
            case GameState.ShopBuy:
                DrawShopBuy(g);
                break;
            case GameState.Bank:
                DrawBank(g);
                break;
        }

        if (!fontLoaded)
        {
            DrawWindow(g, UiCanvas.FontFallbackWindow);
            DrawText(g, "TTF NOT FOUND: USING FALLBACK FONT", 20, 20);
        }

        if (startupFadeFrames > 0)
        {
            var alpha = (int)(255f * startupFadeFrames / 20f);
            using var fadeBrush = new SolidBrush(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Black));
            g.FillRectangle(fadeBrush, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
        }

        if (pendingGameState is not null && sceneFadeOutFramesRemaining > 0)
        {
            var progress = 1f - (sceneFadeOutFramesRemaining / (float)SceneFadeOutDuration);
            var alpha = Math.Clamp((int)Math.Round(255f * progress), 0, 255);
            using var fadeBrush = new SolidBrush(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.Black));
            g.FillRectangle(fadeBrush, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
        }
    }

    private void PollKeyboard()
    {
        var state = Keyboard.GetState();
        var nextHeld = new HashSet<FormsKeys>();
        foreach (var xnaKey in state.GetPressedKeys())
        {
            if (TryMapKey(xnaKey, out var formsKey))
            {
                nextHeld.Add(formsKey);
                if (!heldKeys.Contains(formsKey))
                {
                    pressedKeys.Add(formsKey);
                    if (formsKey == FormsKeys.F11)
                    {
                        ToggleFullscreen();
                    }
                }
            }
        }

        heldKeys.Clear();
        foreach (var key in nextHeld)
        {
            heldKeys.Add(key);
        }
    }

    private static bool TryMapKey(XnaKeys source, out FormsKeys target)
    {
        target = source switch
        {
            XnaKeys.Up => FormsKeys.Up,
            XnaKeys.Down => FormsKeys.Down,
            XnaKeys.Left => FormsKeys.Left,
            XnaKeys.Right => FormsKeys.Right,
            XnaKeys.W => FormsKeys.W,
            XnaKeys.A => FormsKeys.A,
            XnaKeys.S => FormsKeys.S,
            XnaKeys.D => FormsKeys.D,
            XnaKeys.Z => FormsKeys.Z,
            XnaKeys.X => FormsKeys.X,
            XnaKeys.B => FormsKeys.B,
            XnaKeys.V => FormsKeys.V,
            XnaKeys.Enter => FormsKeys.Enter,
            XnaKeys.Space => FormsKeys.Space,
            XnaKeys.Escape => FormsKeys.Escape,
            XnaKeys.Back => FormsKeys.Back,
            XnaKeys.F11 => FormsKeys.F11,
            _ => FormsKeys.None
        };
        return target != FormsKeys.None;
    }

    private XnaRectangle GetVirtualDestination()
    {
        var viewport = GraphicsDevice.Viewport;
        var scale = Math.Min(viewport.Width / (float)UiCanvas.VirtualWidth, viewport.Height / (float)UiCanvas.VirtualHeight);
        var width = (int)MathF.Round(UiCanvas.VirtualWidth * scale);
        var height = (int)MathF.Round(UiCanvas.VirtualHeight * scale);
        return new XnaRectangle((viewport.Width - width) / 2, (viewport.Height - height) / 2, width, height);
    }

    private void UploadFrameTexture(Bitmap bitmap, Texture2D texture)
    {
        var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var byteCount = Math.Abs(data.Stride) * data.Height;
            if (frameBytes.Length != byteCount)
            {
                frameBytes = new byte[byteCount];
            }

            Marshal.Copy(data.Scan0, frameBytes, 0, byteCount);

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var src = (y * data.Stride) + (x * 4);
                    var dst = (y * bitmap.Width) + x;
                    framePixels[dst] = new XnaColor(frameBytes[src + 2], frameBytes[src + 1], frameBytes[src], frameBytes[src + 3]);
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        texture.SetData(framePixels);
    }

    private void ApplyDisplayMode()
    {
        if (activeDisplayMode == LaunchDisplayMode.Fullscreen)
        {
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }
        else
        {
            graphics.IsFullScreen = false;
            var size = GetWindowedClientSize(activeDisplayMode);
            graphics.PreferredBackBufferWidth = size.Width;
            graphics.PreferredBackBufferHeight = size.Height;
            lastWindowedDisplayMode = activeDisplayMode;
        }

        graphics.ApplyChanges();
    }

    private void ToggleFullscreen()
    {
        if (activeDisplayMode == LaunchDisplayMode.Fullscreen)
        {
            activeDisplayMode = lastWindowedDisplayMode;
        }
        else
        {
            lastWindowedDisplayMode = activeDisplayMode;
            activeDisplayMode = LaunchDisplayMode.Fullscreen;
        }

        ApplyDisplayMode();
    }

    private static Size GetWindowedClientSize(LaunchDisplayMode displayMode)
    {
        return displayMode switch
        {
            LaunchDisplayMode.Window720p => new Size(1280, 720),
            LaunchDisplayMode.Window1080p => new Size(1920, 1080),
            _ => new Size(640, 480)
        };
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        if (!skipSaveOnClose)
        {
            try
            {
                SaveGame();
            }
            catch (TamperDetectedException)
            {
            }
        }

        base.OnExiting(sender, args);
    }

    protected override void UnloadContent()
    {
        bgmPlayer.Stop();
        bgmPlayer.Close();
        sePlayer.Stop();
        sePlayer.Close();
        uiFont.Dispose();
        smallFont.Dispose();
        privateFontCollection.Dispose();
        DisposeFieldSprites();
        frameTexture?.Dispose();
        frameBitmap?.Dispose();
        spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private void HandleSecurityViolation(string message)
    {
        if (skipSaveOnClose)
        {
            return;
        }

        skipSaveOnClose = true;
        Exit();
    }
}
