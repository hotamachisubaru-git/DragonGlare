using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
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
using Timer = System.Windows.Forms.Timer;

namespace DragonGlareAlpha;

public partial class DragonGlare : Form
{
    private const int TileSize = 32;
    private const int ShopItemsPerPage = 6;
    private const int CompactFieldViewportWidthTiles = 13;
    private const int ExpandedFieldViewportWidthTiles = 17;
    private const int CompactFieldViewportHeightTiles = 9;
    private const int ExpandedFieldViewportHeightTiles = 11;
    private const int ExpandedFieldViewportHorizontalMargin = 19;
    private const int ExpandedFieldViewportVerticalTrim = 16;
    private const int FieldMovementAnimationDuration = 6;
    private const int EncounterTransitionDuration = 26;
    private const int BattleSelectionVisibleRows = 4;
    private const int OpeningSourceViewportWidth = 256;
    private const int OpeningSourceViewportHeight = 240;
    private static readonly Point PlayerStartTile = new(3, 12);
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

    private readonly Timer gameTimer = new() { Interval = 16 };
    private readonly HashSet<Keys> heldKeys = [];
    private readonly HashSet<Keys> pressedKeys = [];
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

    private Font uiFont = Program.UiFont ?? new Font(UiTypography.DefaultFontFamilyName, UiTypography.FontPixelSize, GraphicsUnit.Pixel);
    private Font smallFont = Program.SmallFont ?? new Font(UiTypography.DefaultFontFamilyName, UiTypography.FontPixelSize, GraphicsUnit.Pixel);
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
    // Scene transition fade-out state: when non-null, we are fading out towards this pending state.
    private int sceneFadeOutFramesRemaining;
    private const int SceneFadeOutDuration = 40; // frames (approx 0.64s at 60fps)
    private GameState? pendingGameState;
    private PlayerFacingDirection playerFacingDirection = PlayerFacingDirection.Down;
    private Point fieldMovementAnimationDirection = Point.Empty;
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
    // 1行ずつの表示アニメーションのためのフィールド
    private string[] battleMessageLines = [];
    private int battleMessageVisibleLines = 0;
    private int battleMessageLineTimer = 0;
    private const int BattleMessageLineDelayFrames = 30; // 0.5秒（60FPSの場合）/ 1秒（30FPSの場合）など調整可能。約0.5秒のウェイト
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
    // Last used source coordinates for opening pan (used to smooth integer jumps)
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

    public DragonGlare(LaunchSettings? launchSettings = null)
    {
        this.launchSettings = launchSettings ?? new LaunchSettings();
        activeDisplayMode = this.launchSettings.DisplayMode;
        if (activeDisplayMode != LaunchDisplayMode.Fullscreen)
        {
            lastWindowedDisplayMode = activeDisplayMode;
        }

        InitializeComponent();
        ConfigureWindow();
        LoadCustomFont();
        InitializeAudio();
        LoadFieldSprites();
        saveService.TryMigrateLegacySave(LegacySaveFilePath);
        RefreshSaveSlotSummaries();

        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        FormClosed += (_, _) => CleanupResources();

        gameTimer.Tick += (_, _) =>
        {
            try
            {
                UpdateGame();
                Invalidate();
            }
            catch (TamperDetectedException ex)
            {
                HandleSecurityViolation(ex.Message);
            }
            finally
            {
                pressedKeys.Clear();
            }
        };
        gameTimer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        try
        {
            base.OnPaint(e);

            var scale = Math.Min((float)ClientSize.Width / UiCanvas.VirtualWidth, (float)ClientSize.Height / UiCanvas.VirtualHeight);
            var drawWidth = UiCanvas.VirtualWidth * scale;
            var drawHeight = UiCanvas.VirtualHeight * scale;
            var offsetX = (ClientSize.Width - drawWidth) / 2f;
            var offsetY = (ClientSize.Height - drawHeight) / 2f;

            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel; // 完全にアンチエイリアスを切り、ドットを白黒2値で描画
            DrawBackdrop(e.Graphics, ClientRectangle, scale);

            e.Graphics.TranslateTransform(offsetX, offsetY);
            e.Graphics.ScaleTransform(scale, scale);

            switch (gameState)
            {
                case GameState.ModeSelect:
                    DrawModeSelect(e.Graphics);
                    break;
                case GameState.LanguageSelection:
                    DrawLanguageSelection(e.Graphics);
                    break;
                case GameState.NameInput:
                    DrawNameInput(e.Graphics);
                    break;
                case GameState.SaveSlotSelection:
                    DrawSaveSlotSelection(e.Graphics);
                    break;
                case GameState.Field:
                    DrawField(e.Graphics);
                    break;
                case GameState.EncounterTransition:
                    DrawEncounterTransition(e.Graphics);
                    break;
                case GameState.Battle:
                    DrawBattle(e.Graphics);
                    break;
                case GameState.ShopBuy:
                    DrawShopBuy(e.Graphics);
                    break;
                case GameState.Bank:
                    DrawBank(e.Graphics);
                    break;
            }

            if (!fontLoaded)
            {
                DrawWindow(e.Graphics, UiCanvas.FontFallbackWindow);
                DrawText(e.Graphics, "TTF NOT FOUND: USING FALLBACK FONT", 20, 20);
            }

            if (startupFadeFrames > 0)
            {
                var alpha = (int)(255f * startupFadeFrames / 20f);
                using var fadeBrush = new SolidBrush(Color.FromArgb(alpha, Color.Black));
                e.Graphics.FillRectangle(fadeBrush, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
            }

            // Draw scene fade-out overlay if a pending state is set and we are fading out.
            if (pendingGameState is not null && sceneFadeOutFramesRemaining > 0)
            {
                var progress = 1f - (sceneFadeOutFramesRemaining / (float)SceneFadeOutDuration);
                var alpha = (int)Math.Round(255f * progress);
                alpha = Math.Clamp(alpha, 0, 255);
                using var fadeBrush = new SolidBrush(Color.FromArgb(alpha, Color.Black));
                e.Graphics.FillRectangle(fadeBrush, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
            }
        }
        catch (TamperDetectedException ex)
        {
            HandleSecurityViolation(ex.Message);
        }
    }

    private void ConfigureWindow()
    {
        Text = $"DragonGlare Alpha v{Application.ProductVersion}";
        BackColor = Color.Black;
        ShowIcon = true;
        KeyPreview = true;
        DoubleBuffered = true;
        ApplyDisplayMode();

        try
        {
            using var applicationIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (applicationIcon is not null)
            {
                Icon = (Icon)applicationIcon.Clone();
            }
        }
        catch
        {
        }
    }

    private void ApplyDisplayMode()
    {
        var activeScreen = GetCurrentDisplayScreen();
        WindowState = FormWindowState.Normal;

        if (activeDisplayMode == LaunchDisplayMode.Fullscreen)
        {
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            Bounds = activeScreen.Bounds;
            return;
        }

        lastWindowedDisplayMode = activeDisplayMode;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.Manual;
        var workingArea = activeScreen.WorkingArea;
        ClientSize = ConstrainWindowedClientSize(GetWindowedClientSize(activeDisplayMode), workingArea);

        Location = new Point(
            workingArea.X + Math.Max(0, (workingArea.Width - Width) / 2),
            workingArea.Y + Math.Max(0, (workingArea.Height - Height) / 2));
    }

    private Screen GetCurrentDisplayScreen()
    {
        return IsHandleCreated ? Screen.FromHandle(Handle) : Screen.FromPoint(Cursor.Position);
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
        Invalidate();
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

    private Size ConstrainWindowedClientSize(Size desiredClientSize, Rectangle workingArea)
    {
        var desiredWindowSize = SizeFromClientSize(desiredClientSize);
        if (desiredWindowSize.Width <= workingArea.Width && desiredWindowSize.Height <= workingArea.Height)
        {
            return desiredClientSize;
        }

        var scale = Math.Min(
            workingArea.Width / (float)desiredWindowSize.Width,
            workingArea.Height / (float)desiredWindowSize.Height);

        var scaledClientSize = new Size(
            Math.Max(UiCanvas.VirtualWidth, (int)Math.Floor(desiredClientSize.Width * scale)),
            Math.Max(UiCanvas.VirtualHeight, (int)Math.Floor(desiredClientSize.Height * scale)));

        return scaledClientSize;
    }

    private void CleanupResources()
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

        gameTimer.Stop();
        gameTimer.Dispose();
        bgmPlayer.Stop();
        bgmPlayer.Close();
        sePlayer.Stop();
        sePlayer.Close();
        uiFont.Dispose();
        smallFont.Dispose();
        privateFontCollection.Dispose();
        DisposeFieldSprites();
    }

    private void HandleSecurityViolation(string message)
    {
        if (skipSaveOnClose)
        {
            return;
        }

        skipSaveOnClose = true;
        gameTimer.Stop();
        Hide();
        MessageBox.Show(message, "DragonGlare Alpha", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        Close();
    }
}
