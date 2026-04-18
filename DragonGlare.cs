using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Persistence;
using DragonGlareAlpha.Security;
using DragonGlareAlpha.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static DragonGlareAlpha.Domain.Constants;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaSong = Microsoft.Xna.Framework.Media.Song;
using XnaPoint = Microsoft.Xna.Framework.Point;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;

namespace DragonGlareAlpha;

public enum PlayerFacingDirection { Left, Right, Up, Down }

public partial class DragonGlareAlpha : Game
{
// Constants moved to Constants.cs

    #region Fields

    private readonly GraphicsDeviceManager graphics;
    private readonly Random random = new();
    private readonly StringBuilder playerName = new();
    private readonly SaveService saveService = new();
    private readonly AntiCheatService antiCheatService = new();
    private readonly BattleService battleService = new();
    private readonly BankService bankService = new();
    private readonly FieldTransitionService fieldTransitionService = new();
    private readonly LaunchSettings launchSettings;
    private readonly Dictionary<string, Texture2D> textTextureCache = [];
    private readonly PrivateFontCollection privateFontCollection = new();

    // Asset Resources
    private SpriteBatch? spriteBatch;
    private Texture2D? pixelTexture;
    private Texture2D? openingTexture;
    private Texture2D? menuWindowTexture;
    private Texture2D? battleFieldTexture;
    private Texture2D? battleWindowTexture;
    private Texture2D? fieldTileTexture;
    private XnaSong? prologueSong;
    private Font? uiFont;

    // Input & State
    private KeyboardState currentKeyboardState;
    private KeyboardState previousKeyboardState;
    private PlayerProgress player = PlayerProgress.CreateDefault(new DrawingPoint(PlayerStartTile.X, PlayerStartTile.Y));
    private BattleEncounter? currentEncounter;
    private GameState gameState = GameState.ModeSelect;
    private FieldMapId currentFieldMap = FieldMapId.Hub;
    private int[,] map = MapFactory.CreateDefaultMap();

    // UI & Navigation
    private UiLanguage selectedLanguage = UiLanguage.Japanese;
    private LaunchDisplayMode activeDisplayMode;
    private LaunchDisplayMode lastWindowedDisplayMode = LaunchDisplayMode.Window640x480;
    private GameState? pendingGameState;
    private XnaPoint fieldMovementAnimationDirection = XnaPoint.Zero;
    private SaveSlotSelectionMode saveSlotSelectionMode = SaveSlotSelectionMode.Save;
    private IReadOnlyList<SaveSlotSummary> saveSlotSummaries = [];
    private string menuNotice = string.Empty;
    private int modeCursor;
    private int languageCursor;
    private int saveSlotCursor;
    private int activeSaveSlot;

    // Animation & Timers
    private int movementCooldown;
    private int frameCounter;
    private int startupFadeFrames = 20;
    private int sceneFadeOutFramesRemaining;
    private int fieldMovementAnimationFramesRemaining;
    private int encounterTransitionFrames;
    private int fieldEncounterStepsRemaining = 7;
    private int menuNoticeFrames;
    private int languageOpeningElapsedFrames;
    private int languageOpeningLineIndex;
    private int languageOpeningLineFrame;
    private bool languageOpeningFinished;
    private bool skipLanguageSelectionPrompt;
    private bool skipSaveOnClose;
    private bool isPrologueBgmPlaying;
    private PlayerFacingDirection playerFacingDirection = PlayerFacingDirection.Down;

    #endregion

    #region Initialization & Content

    public DragonGlareAlpha(LaunchSettings? launchSettings = null)
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = ProjectDisplayName;

        this.launchSettings = launchSettings ?? new LaunchSettings();
        activeDisplayMode = this.launchSettings.DisplayMode;
        if (activeDisplayMode != LaunchDisplayMode.Fullscreen)
        {
            lastWindowedDisplayMode = activeDisplayMode;
        }

        ApplyDisplayMode();
        RefreshSaveSlotSummaries();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData([XnaColor.White]);
        openingTexture = LoadTextureFromAssets("UI", "SFC_opening.png");
        menuWindowTexture = LoadTextureFromAssets("UI", "window21.png");
        battleFieldTexture = LoadTextureFromAssets("UI", "SFC_battlefieldFrame1.png");
        battleWindowTexture = LoadTextureFromAssets("UI", "SFC_battlewindowFrame1.png");
        fieldTileTexture = LoadTextureFromAssets("Tiles", "mapTile_Assets_SFCFrame1.png");
        prologueSong = LoadSongFromRoot("SFC_prologue02.mp3");
        LoadUiFont();
    }

    #endregion

    #region Main Game Loop

    protected override void Update(GameTime gameTime)
    {
        try
        {
            currentKeyboardState = Keyboard.GetState();
            UpdateGame();
            previousKeyboardState = currentKeyboardState;
        }
        catch (TamperDetectedException ex)
        {
            HandleSecurityViolation(ex.Message);
        }

        base.Update(gameTime);
    }

    #endregion

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(XnaColor.Black);

        if (spriteBatch is null || pixelTexture is null)
        {
            base.Draw(gameTime);
            return;
        }

        // 1. Backdrops (Full screen / Static resolution)
        DrawStaticBackdrops(spriteBatch);

        // 2. Main Scene & UI (Virtual resolution with transformation)
        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: GetVirtualTransform());

        DrawCurrentStateScene(spriteBatch);

        // 3. Global Overlays (Fades, etc.)
        DrawFade(spriteBatch);

        spriteBatch.End();

        base.Draw(gameTime);
    }

}
