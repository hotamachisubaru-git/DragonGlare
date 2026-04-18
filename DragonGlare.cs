using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
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
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaSong = Microsoft.Xna.Framework.Media.Song;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha : Game
{
    private const int VirtualWidth = 640;
    private const int VirtualHeight = 480;
    private const int TileSize = 32;
    private const int FieldMovementAnimationDuration = 6;
    private const int EncounterTransitionDuration = 26;
    private const int OpeningSourceViewportHeight = 240;
    private const int OpeningScreenFrames = 60 * 56;
    private const int OpeningNarrationFadeFrames = 24;
    private const int SceneFadeOutDuration = 40;
    private const int UiFontPixelSize = 14;
    private const int UiTextLineHeight = UiFontPixelSize + 4;
    private const string ProjectDisplayName = "DragonGlare Alpha";

    private static readonly DrawingPoint PlayerStartTile = new(3, 12);
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

    private SpriteBatch? spriteBatch;
    private Texture2D? pixelTexture;
    private Texture2D? openingTexture;
    private Texture2D? menuWindowTexture;
    private XnaSong? prologueSong;
    private Font? uiFont;
    private KeyboardState currentKeyboardState;
    private KeyboardState previousKeyboardState;
    private PlayerProgress player = PlayerProgress.CreateDefault(PlayerStartTile);
    private BattleEncounter? currentEncounter;
    private GameState gameState = GameState.ModeSelect;
    private FieldMapId currentFieldMap = FieldMapId.Hub;
    private int[,] map = MapFactory.CreateDefaultMap();
    private UiLanguage selectedLanguage = UiLanguage.Japanese;
    private LaunchDisplayMode activeDisplayMode;
    private LaunchDisplayMode lastWindowedDisplayMode = LaunchDisplayMode.Window640x480;
    private GameState? pendingGameState;
    private DrawingPoint fieldMovementAnimationDirection = DrawingPoint.Empty;
    private SaveSlotSelectionMode saveSlotSelectionMode = SaveSlotSelectionMode.Save;
    private IReadOnlyList<SaveSlotSummary> saveSlotSummaries = [];
    private string menuNotice = string.Empty;
    private int modeCursor;
    private int languageCursor;
    private int saveSlotCursor;
    private int activeSaveSlot;
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

    private enum PlayerFacingDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private PlayerFacingDirection playerFacingDirection = PlayerFacingDirection.Down;

    private readonly record struct OpeningNarrationLine(
        string Text,
        int DisplayFrames,
        int GapFrames);

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
        prologueSong = LoadSongFromRoot("SFC_prologue02.mp3");
        LoadUiFont();
    }

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

    private partial void UpdateGame();

    private partial void UpdateGame()
    {
        frameCounter++;
        UpdateFieldMovementAnimation();
        RunAntiCheatChecks();

        if (WasPressed(XnaKeys.F11))
        {
            ToggleFullscreen();
        }

        if (WasPressed(XnaKeys.Escape) && gameState == GameState.ModeSelect)
        {
            Exit();
            return;
        }

        if (pendingGameState is not null)
        {
            UpdateSceneFadeOut();
            return;
        }

        if (startupFadeFrames > 0)
        {
            startupFadeFrames--;
        }

        if (menuNoticeFrames > 0)
        {
            menuNoticeFrames--;
            if (menuNoticeFrames == 0)
            {
                menuNotice = string.Empty;
            }
        }

        switch (gameState)
        {
            case GameState.ModeSelect:
                UpdateModeSelect();
                break;
            case GameState.LanguageSelection:
                UpdateLanguageSelection();
                break;
            case GameState.NameInput:
                UpdateNameInput();
                break;
            case GameState.SaveSlotSelection:
                UpdateSaveSlotSelection();
                break;
            case GameState.Field:
                UpdateField();
                break;
            case GameState.EncounterTransition:
                UpdateEncounterTransition();
                break;
            case GameState.Battle:
                UpdateBattle();
                break;
            case GameState.ShopBuy:
                UpdateShopBuy();
                break;
            case GameState.Bank:
                UpdateBank();
                break;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(XnaColor.Black);

        if (spriteBatch is null || pixelTexture is null)
        {
            base.Draw(gameTime);
            return;
        }

        if (UsesMenuBackdrop())
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawMenuBackdrop(spriteBatch);
            spriteBatch.End();
        }

        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: GetVirtualTransform());

        switch (gameState)
        {
            case GameState.LanguageSelection:
                DrawOpeningBackdrop(spriteBatch);
                if (!languageOpeningFinished)
                {
                    DrawOpeningNarration(spriteBatch);
                }
                else
                {
                    DrawCursorPanel(spriteBatch, languageCursor, 2, 96, 320, 448, 88);
                }
                break;
            case GameState.Field:
            case GameState.EncounterTransition:
                DrawField(spriteBatch);
                break;
            case GameState.Battle:
                DrawSolidPanel(spriteBatch, new XnaRectangle(64, 64, 512, 352), new XnaColor(28, 20, 24));
                break;
            case GameState.ShopBuy:
            case GameState.Bank:
                DrawSolidPanel(spriteBatch, new XnaRectangle(80, 80, 480, 320), new XnaColor(22, 28, 36));
                break;
            default:
                DrawMenu(spriteBatch);
                break;
        }

        DrawFade(spriteBatch);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        if (!skipSaveOnClose)
        {
            TryPersistProgress();
        }

        base.OnExiting(sender, args);
    }

    protected override void UnloadContent()
    {
        foreach (var texture in textTextureCache.Values)
        {
            texture.Dispose();
        }

        textTextureCache.Clear();
        openingTexture?.Dispose();
        menuWindowTexture?.Dispose();
        prologueSong?.Dispose();
        pixelTexture?.Dispose();
        uiFont?.Dispose();
        privateFontCollection.Dispose();

        base.UnloadContent();
    }

    private void UpdateSceneFadeOut()
    {
        if (sceneFadeOutFramesRemaining > 0)
        {
            sceneFadeOutFramesRemaining--;
        }

        if (sceneFadeOutFramesRemaining > 0 || pendingGameState is null)
        {
            return;
        }

        gameState = pendingGameState.Value;
        pendingGameState = null;
        startupFadeFrames = 20;
    }

    private void UpdateModeSelect()
    {
        if (WasPressed(XnaKeys.Up) || WasPressed(XnaKeys.W))
        {
            modeCursor = Math.Max(0, modeCursor - 1);
        }
        else if (WasPressed(XnaKeys.Down) || WasPressed(XnaKeys.S))
        {
            modeCursor = Math.Min(3, modeCursor + 1);
        }

        if (!WasConfirmPressed())
        {
            return;
        }

        if (modeCursor == 0)
        {
            StartNewGame();
            return;
        }

        if (modeCursor == 1)
        {
            OpenSaveSlotSelection(SaveSlotSelectionMode.Load);
            return;
        }

        ShowTransientNotice(modeCursor == 2
            ? "データうつしは まだ未実装です。"
            : "データけしは まだ未実装です。");
    }

    private void UpdateLanguageSelection()
    {
        if (!languageOpeningFinished)
        {
            UpdateLanguageOpening();
            return;
        }

        if (WasPressed(XnaKeys.Up) || WasPressed(XnaKeys.W))
        {
            languageCursor = 0;
        }
        else if (WasPressed(XnaKeys.Down) || WasPressed(XnaKeys.S))
        {
            languageCursor = 1;
        }

        if (WasPressed(XnaKeys.Escape))
        {
            ChangeGameState(GameState.ModeSelect);
            return;
        }

        if (!WasConfirmPressed())
        {
            return;
        }

        selectedLanguage = languageCursor == 0 ? UiLanguage.Japanese : UiLanguage.English;
        player.Language = selectedLanguage;

        if (skipLanguageSelectionPrompt)
        {
            skipLanguageSelectionPrompt = false;
            BeginFieldSession();
            return;
        }

        ChangeGameState(GameState.NameInput);
    }

    private void UpdateLanguageOpening()
    {
        StartPrologueBgm();

        if (WasPressed(XnaKeys.Escape))
        {
            StopPrologueBgm();
            ChangeGameState(GameState.ModeSelect);
            return;
        }

        if (WasConfirmPressed())
        {
            languageOpeningFinished = true;
            StopPrologueBgm();
            return;
        }

        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            languageOpeningFinished = true;
            return;
        }

        languageOpeningElapsedFrames = Math.Min(LanguageOpeningTotalFrames, languageOpeningElapsedFrames + 1);
        languageOpeningLineFrame++;

        var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
        if (languageOpeningLineFrame < currentLine.DisplayFrames + currentLine.GapFrames)
        {
            return;
        }

        languageOpeningLineIndex++;
        languageOpeningLineFrame = 0;
        if (languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            languageOpeningFinished = true;
            StopPrologueBgm();
        }
    }

    private void UpdateNameInput()
    {
        if (WasPressed(XnaKeys.Escape))
        {
            ChangeGameState(GameState.LanguageSelection);
            return;
        }

        // MonoGame版の文字入力UIは後続で移植する。現時点では進行用の仮名を確定する。
        if (WasConfirmPressed())
        {
            playerName.Clear();
            playerName.Append(selectedLanguage == UiLanguage.Japanese ? "ゆうしゃ" : "HERO");
            player.Name = playerName.ToString();
            OpenSaveSlotSelection(SaveSlotSelectionMode.Save);
        }
    }

    private void UpdateSaveSlotSelection()
    {
        if (WasPressed(XnaKeys.Up) || WasPressed(XnaKeys.W))
        {
            saveSlotCursor = Math.Max(0, saveSlotCursor - 1);
        }
        else if (WasPressed(XnaKeys.Down) || WasPressed(XnaKeys.S))
        {
            saveSlotCursor = Math.Min(SaveService.SlotCount - 1, saveSlotCursor + 1);
        }

        if (WasPressed(XnaKeys.Escape))
        {
            ChangeGameState(saveSlotSelectionMode == SaveSlotSelectionMode.Save
                ? GameState.NameInput
                : GameState.ModeSelect);
            return;
        }

        if (!WasConfirmPressed())
        {
            return;
        }

        var selectedSlot = saveSlotCursor + 1;
        if (saveSlotSelectionMode == SaveSlotSelectionMode.Load)
        {
            if (TryLoadGame(selectedSlot))
            {
                ChangeGameState(GameState.Field);
                return;
            }

            RefreshSaveSlotSummaries();
            ShowTransientNotice("NO SAVE DATA / セーブデータがありません");
            return;
        }

        activeSaveSlot = selectedSlot;
        TryPersistProgress();
        ChangeGameState(GameState.Field);
    }

    private void UpdateField()
    {
        if (WasPressed(XnaKeys.Escape))
        {
            ChangeGameState(GameState.ModeSelect);
            return;
        }

        if (WasPressed(XnaKeys.B))
        {
            EnterBattle();
            return;
        }

        if (WasPressed(XnaKeys.V))
        {
            ChangeGameState(GameState.ShopBuy);
            return;
        }

        if (WasPressed(XnaKeys.N))
        {
            ChangeGameState(GameState.Bank);
            return;
        }

        if (movementCooldown > 0)
        {
            movementCooldown--;
            return;
        }

        var movement = DrawingPoint.Empty;
        if (IsDown(XnaKeys.Left) || IsDown(XnaKeys.A))
        {
            movement = new DrawingPoint(-1, 0);
        }
        else if (IsDown(XnaKeys.Right) || IsDown(XnaKeys.D))
        {
            movement = new DrawingPoint(1, 0);
        }
        else if (IsDown(XnaKeys.Up) || IsDown(XnaKeys.W))
        {
            movement = new DrawingPoint(0, -1);
        }
        else if (IsDown(XnaKeys.Down) || IsDown(XnaKeys.S))
        {
            movement = new DrawingPoint(0, 1);
        }

        if (movement != DrawingPoint.Empty)
        {
            TryMovePlayer(movement);
        }
    }

    private void UpdateEncounterTransition()
    {
        encounterTransitionFrames--;
        if (encounterTransitionFrames <= 0)
        {
            ChangeGameState(GameState.Battle);
        }
    }

    private void UpdateBattle()
    {
        if (WasPressed(XnaKeys.Escape))
        {
            ResetBattleState();
            ChangeGameState(GameState.Field);
            return;
        }

        if (WasConfirmPressed())
        {
            if (currentEncounter is not null)
            {
                currentEncounter.CurrentHp = 0;
            }

            ResetBattleState();
            ChangeGameState(GameState.Field);
        }
    }

    private void UpdateShopBuy()
    {
        if (WasPressed(XnaKeys.Escape) || WasConfirmPressed())
        {
            ChangeGameState(GameState.Field);
        }
    }

    private void UpdateBank()
    {
        if (WasPressed(XnaKeys.Escape) || WasConfirmPressed())
        {
            bankService.AccrueStepInterest(player);
            ChangeGameState(GameState.Field);
        }
    }

    private void StartNewGame()
    {
        player = PlayerProgress.CreateDefault(PlayerStartTile);
        selectedLanguage = UiLanguage.Japanese;
        currentFieldMap = FieldMapId.Hub;
        map = MapFactory.CreateMap(currentFieldMap);
        activeSaveSlot = 0;
        fieldEncounterStepsRemaining = 7;
        playerName.Clear();
        ResetBattleState();
        ResetOpening();
        skipLanguageSelectionPrompt = false;
        ChangeGameState(GameState.LanguageSelection);
    }

    private void BeginFieldSession()
    {
        if (string.IsNullOrWhiteSpace(player.Name))
        {
            player.Name = selectedLanguage == UiLanguage.Japanese ? "ゆうしゃ" : "HERO";
        }

        currentFieldMap = FieldMapId.Hub;
        map = MapFactory.CreateMap(currentFieldMap);
        ResetBattleState();
        ChangeGameState(GameState.Field);
    }

    private void OpenSaveSlotSelection(SaveSlotSelectionMode mode)
    {
        saveSlotSelectionMode = mode;
        RefreshSaveSlotSummaries();
        saveSlotCursor = Math.Clamp(activeSaveSlot - 1, 0, SaveService.SlotCount - 1);
        if (mode == SaveSlotSelectionMode.Save && activeSaveSlot == 0)
        {
            saveSlotCursor = 0;
        }

        menuNotice = string.Empty;
        menuNoticeFrames = 0;
        ChangeGameState(GameState.SaveSlotSelection);
    }

    private void ChangeGameState(GameState nextState)
    {
        if (gameState == nextState)
        {
            return;
        }

        pendingGameState = nextState;
        sceneFadeOutFramesRemaining = SceneFadeOutDuration;
    }

    private bool TryMovePlayer(DrawingPoint movement)
    {
        SetPlayerFacingDirection(movement);
        var target = new DrawingPoint(player.TilePosition.X + movement.X, player.TilePosition.Y + movement.Y);
        if (!IsWalkableTile(target))
        {
            movementCooldown = 3;
            return false;
        }

        player.TilePosition = target;
        bankService.AccrueStepInterest(player);
        StartFieldMovementAnimation(movement);
        movementCooldown = 6;

        if (TryTransitionFromTile(target))
        {
            return true;
        }

        if (TryTriggerRandomEncounter())
        {
            TryPersistProgress();
            return true;
        }

        TryPersistProgress();
        return true;
    }

    private bool IsWalkableTile(DrawingPoint tile)
    {
        if (tile.X < 0 || tile.Y < 0 || tile.X >= map.GetLength(1) || tile.Y >= map.GetLength(0))
        {
            return false;
        }

        return map[tile.Y, tile.X] != MapFactory.WallTile;
    }

    private bool TryTransitionFromTile(DrawingPoint tile)
    {
        if (!fieldTransitionService.TryGetTransition(currentFieldMap, tile, out var transition))
        {
            return false;
        }

        currentFieldMap = transition.ToMapId;
        map = MapFactory.CreateMap(currentFieldMap);
        player.TilePosition = transition.DestinationTile;
        ResetEncounterCounter();
        TryPersistProgress();
        return true;
    }

    private bool TryTriggerRandomEncounter()
    {
        fieldEncounterStepsRemaining--;
        if (fieldEncounterStepsRemaining > 0 || currentFieldMap == FieldMapId.Hub)
        {
            return false;
        }

        ResetEncounterCounter();
        EnterBattle();
        return true;
    }

    private void EnterBattle()
    {
        currentEncounter = battleService.CreateEncounter(random, currentFieldMap, player.Level);
        encounterTransitionFrames = EncounterTransitionDuration;
        ChangeGameState(GameState.EncounterTransition);
    }

    private void ResetBattleState()
    {
        currentEncounter = null;
        encounterTransitionFrames = 0;
    }

    private void RefreshSaveSlotSummaries()
    {
        saveSlotSummaries = saveService.GetSlotSummaries();
    }

    private bool TryLoadGame(int slotNumber)
    {
        if (!saveService.TryLoadSlot(slotNumber, out var saveData) || saveData is null)
        {
            return false;
        }

        var restored = SaveDataMapper.Restore(saveData, PlayerStartTile);
        selectedLanguage = restored.Language;
        currentFieldMap = restored.MapId;
        player = restored.Player;
        map = MapFactory.CreateMap(currentFieldMap);
        activeSaveSlot = slotNumber;
        ResetBattleState();
        ResetEncounterCounter();
        return true;
    }

    private void TryPersistProgress()
    {
        if (activeSaveSlot <= 0)
        {
            return;
        }

        try
        {
            saveService.SaveSlot(activeSaveSlot, SaveDataMapper.Create(player, selectedLanguage, currentFieldMap, activeSaveSlot));
        }
        catch
        {
        }
    }

    private void RunAntiCheatChecks()
    {
        if (frameCounter % 30 == 0)
        {
            player.RekeySensitiveValues();
            currentEncounter?.RekeySensitiveValues();
        }

        if (frameCounter % 120 != 0)
        {
            return;
        }

        player.ValidateIntegrity();
        currentEncounter?.ValidateIntegrity();

        if (antiCheatService.TryDetectViolation(out var message))
        {
            throw new TamperDetectedException(message);
        }
    }

    private void UpdateFieldMovementAnimation()
    {
        if (fieldMovementAnimationFramesRemaining <= 0)
        {
            fieldMovementAnimationDirection = DrawingPoint.Empty;
            return;
        }

        fieldMovementAnimationFramesRemaining--;
    }

    private void StartFieldMovementAnimation(DrawingPoint movement)
    {
        fieldMovementAnimationDirection = movement;
        fieldMovementAnimationFramesRemaining = FieldMovementAnimationDuration;
    }

    private void ResetEncounterCounter()
    {
        fieldEncounterStepsRemaining = random.Next(6, 12);
    }

    private void ResetOpening()
    {
        languageCursor = 0;
        languageOpeningElapsedFrames = 0;
        languageOpeningLineIndex = 0;
        languageOpeningLineFrame = 0;
        languageOpeningFinished = false;
    }

    private void ShowTransientNotice(string message, int frames = 180)
    {
        menuNotice = message;
        menuNoticeFrames = frames;
    }

    private void SetPlayerFacingDirection(DrawingPoint movement)
    {
        if (movement.X < 0)
        {
            playerFacingDirection = PlayerFacingDirection.Left;
        }
        else if (movement.X > 0)
        {
            playerFacingDirection = PlayerFacingDirection.Right;
        }
        else if (movement.Y < 0)
        {
            playerFacingDirection = PlayerFacingDirection.Up;
        }
        else if (movement.Y > 0)
        {
            playerFacingDirection = PlayerFacingDirection.Down;
        }
    }

    private bool WasConfirmPressed()
    {
        return WasPressed(XnaKeys.Enter) || WasPressed(XnaKeys.Z) || WasPressed(XnaKeys.Space);
    }

    private bool WasPressed(XnaKeys key)
    {
        return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }

    private bool IsDown(XnaKeys key)
    {
        return currentKeyboardState.IsKeyDown(key);
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

    private static (int Width, int Height) GetWindowedClientSize(LaunchDisplayMode displayMode)
    {
        return displayMode switch
        {
            LaunchDisplayMode.Window720p => (1280, 720),
            LaunchDisplayMode.Window1080p => (1920, 1080),
            _ => (640, 480)
        };
    }

    private Texture2D? LoadTextureFromAssets(params string[] relativeParts)
    {
        var candidates = new[]
        {
            Path.Combine([AppContext.BaseDirectory, "Assets", .. relativeParts]),
            Path.Combine([Directory.GetCurrentDirectory(), "Assets", .. relativeParts])
        };

        foreach (var candidate in candidates.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            using var stream = File.OpenRead(candidate);
            return Texture2D.FromStream(GraphicsDevice, stream);
        }

        return null;
    }

    private XnaSong? LoadSongFromRoot(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, fileName),
            Path.Combine(Directory.GetCurrentDirectory(), fileName)
        };

        foreach (var candidate in candidates.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            try
            {
                return XnaSong.FromUri(Path.GetFileNameWithoutExtension(candidate), new Uri(candidate, UriKind.Absolute));
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private void StartPrologueBgm()
    {
        if (isPrologueBgmPlaying || prologueSong is null)
        {
            return;
        }

        try
        {
            XnaMediaPlayer.IsRepeating = false;
            XnaMediaPlayer.Volume = 0.85f;
            XnaMediaPlayer.Play(prologueSong);
            isPrologueBgmPlaying = true;
        }
        catch
        {
            isPrologueBgmPlaying = false;
        }
    }

    private void StopPrologueBgm()
    {
        if (!isPrologueBgmPlaying)
        {
            return;
        }

        try
        {
            XnaMediaPlayer.Stop();
        }
        catch
        {
        }

        isPrologueBgmPlaying = false;
    }

    private void LoadUiFont()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "JF-Dot-ShinonomeMin14.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "JF-Dot-ShinonomeMin14.ttf")
        };

        foreach (var candidate in candidates.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            privateFontCollection.AddFontFile(candidate);
            if (privateFontCollection.Families.Length > 0)
            {
                uiFont = new Font(privateFontCollection.Families[0], UiFontPixelSize, GraphicsUnit.Pixel);
                return;
            }
        }

        uiFont = new Font("MS Gothic", UiFontPixelSize, GraphicsUnit.Pixel);
    }

    private XnaMatrix GetVirtualTransform()
    {
        var viewport = GraphicsDevice.Viewport;
        var scale = Math.Min(viewport.Width / (float)VirtualWidth, viewport.Height / (float)VirtualHeight);
        var offsetX = (viewport.Width - (VirtualWidth * scale)) / 2f;
        var offsetY = (viewport.Height - (VirtualHeight * scale)) / 2f;

        return XnaMatrix.CreateScale(scale, scale, 1f) * XnaMatrix.CreateTranslation(offsetX, offsetY, 0f);
    }

    private bool UsesMenuBackdrop()
    {
        return gameState == GameState.ModeSelect ||
               gameState == GameState.NameInput ||
               gameState == GameState.SaveSlotSelection;
    }

    private void DrawMenu(SpriteBatch batch)
    {
        var layout = new XnaRectangle(32, 34, 576, 420);
        if (menuWindowTexture is not null)
        {
            batch.Draw(menuWindowTexture, layout, XnaColor.White);
        }
        else
        {
            DrawSolidPanel(batch, layout, new XnaColor(48, 45, 50));
        }

        var menuItems = new[]
        {
            "はじめから",
            "つづきから",
            "データうつす",
            "データけす"
        };

        var menuStartX = ScaleMenuX(layout, 24);
        var menuStartY = ScaleMenuY(layout, 24);
        var menuLineHeight = ScaleMenuHeight(layout, 24);
        var menuCursorX = menuStartX - 10;

        for (var index = 0; index < menuItems.Length; index++)
        {
            var lineY = menuStartY + (index * menuLineHeight);
            if (modeCursor == index)
            {
                DrawText(batch, "▶", menuCursorX, lineY);
            }

            DrawText(batch, menuItems[index], menuStartX, lineY);
        }

        if (!string.IsNullOrEmpty(menuNotice))
        {
            DrawSolidPanel(batch, new XnaRectangle(ScaleMenuX(layout, 22), ScaleMenuY(layout, 132), ScaleMenuWidth(layout, 210), ScaleMenuHeight(layout, 24)), new XnaColor(74, 35, 35) * 0.35f);
        }

        DrawText(batch, GetModeSelectDescription(modeCursor), ScaleMenuX(layout, 128), ScaleMenuY(layout, 24));
        DrawText(
            batch,
            string.IsNullOrWhiteSpace(menuNotice) ? "モードを選んでください。" : menuNotice,
            ScaleMenuX(layout, 24),
            ScaleMenuY(layout, 136));
    }

    private void DrawMenuBackdrop(SpriteBatch batch)
    {
        var viewport = GraphicsDevice.Viewport;
        DrawSolidPanel(batch, new XnaRectangle(0, 0, viewport.Width, viewport.Height), XnaColor.Black);
    }

    private void DrawText(SpriteBatch batch, string text, int x, int y)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var texture = GetTextTexture(lines[index]);
            if (texture is null)
            {
                continue;
            }

            batch.Draw(texture, new Vector2(x, y + (index * UiTextLineHeight)), XnaColor.White);
        }
    }

    private Texture2D? GetTextTexture(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (textTextureCache.TryGetValue(text, out var cachedTexture))
        {
            return cachedTexture;
        }

        if (uiFont is null)
        {
            return null;
        }

        using var measureBitmap = new Bitmap(1, 1);
        using var measureGraphics = Graphics.FromImage(measureBitmap);
        measureGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        var measured = measureGraphics.MeasureString(text, uiFont, PointF.Empty, StringFormat.GenericTypographic);
        var width = Math.Max(1, (int)Math.Ceiling(measured.Width) + 2);
        var height = Math.Max(1, (int)Math.Ceiling(measured.Height) + 2);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics2D = Graphics.FromImage(bitmap))
        {
            graphics2D.Clear(System.Drawing.Color.Transparent);
            graphics2D.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            graphics2D.SmoothingMode = SmoothingMode.None;
            graphics2D.InterpolationMode = InterpolationMode.NearestNeighbor;
            using var brush = new SolidBrush(System.Drawing.Color.White);
            graphics2D.DrawString(text, uiFont, brush, new PointF(0, 0), StringFormat.GenericTypographic);
        }

        var bounds = new DrawingRectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var bytes = new byte[Math.Abs(data.Stride) * data.Height];
        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
        bitmap.UnlockBits(data);

        var colors = new XnaColor[bitmap.Width * bitmap.Height];
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var sourceIndex = (y * data.Stride) + (x * 4);
                var targetIndex = (y * bitmap.Width) + x;
                colors[targetIndex] = new XnaColor(bytes[sourceIndex + 2], bytes[sourceIndex + 1], bytes[sourceIndex], bytes[sourceIndex + 3]);
            }
        }

        var texture = new Texture2D(GraphicsDevice, bitmap.Width, bitmap.Height);
        texture.SetData(colors);
        textTextureCache[text] = texture;
        return texture;
    }

    private void DrawOpeningBackdrop(SpriteBatch batch)
    {
        if (openingTexture is null)
        {
            DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), new XnaColor(106, 91, 22));
            return;
        }

        var sourceHeight = Math.Min(OpeningSourceViewportHeight, openingTexture.Height);
        var sourceWidth = Math.Min(openingTexture.Width, Math.Max(1, (int)Math.Round(VirtualWidth * (sourceHeight / (float)VirtualHeight))));
        var maxSourceX = Math.Max(0, openingTexture.Width - sourceWidth);
        var progress = OpeningScreenFrames == 0
            ? 0f
            : Math.Clamp(languageOpeningElapsedFrames / (float)OpeningScreenFrames, 0f, 1f);
        var sourceX = (int)MathF.Round(maxSourceX * progress);
        var source = new XnaRectangle(sourceX, 0, sourceWidth, sourceHeight);
        batch.Draw(openingTexture, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), source, XnaColor.White);
    }

    private void DrawOpeningNarration(SpriteBatch batch)
    {
        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            return;
        }

        var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
        if (languageOpeningLineFrame >= currentLine.DisplayFrames)
        {
            return;
        }

        var alpha = GetOpeningNarrationAlpha(currentLine);
        var lines = currentLine.Text.Replace("\r\n", "\n").Split('\n');
        var totalHeight = lines.Length * UiTextLineHeight;
        var startY = VirtualHeight - 96 - Math.Max(0, totalHeight / 2);

        for (var index = 0; index < lines.Length; index++)
        {
            var texture = GetTextTexture(lines[index]);
            if (texture is null)
            {
                continue;
            }

            var x = (VirtualWidth - texture.Width) / 2;
            var y = startY + (index * UiTextLineHeight);
            var shadowColor = XnaColor.Black * Math.Clamp(alpha, 0f, 1f);
            var mainColor = XnaColor.White * Math.Clamp(alpha, 0f, 1f);

            batch.Draw(texture, new Vector2(x + 1, y), shadowColor);
            batch.Draw(texture, new Vector2(x - 1, y), shadowColor);
            batch.Draw(texture, new Vector2(x, y + 1), shadowColor);
            batch.Draw(texture, new Vector2(x, y - 1), shadowColor);
            batch.Draw(texture, new Vector2(x, y), mainColor);
        }
    }

    private float GetOpeningNarrationAlpha(OpeningNarrationLine currentLine)
    {
        if (languageOpeningLineFrame < OpeningNarrationFadeFrames)
        {
            return languageOpeningLineFrame / (float)OpeningNarrationFadeFrames;
        }

        var fadeOutStart = Math.Max(OpeningNarrationFadeFrames, currentLine.DisplayFrames - OpeningNarrationFadeFrames);
        if (languageOpeningLineFrame >= fadeOutStart)
        {
            return Math.Max(0f, (currentLine.DisplayFrames - languageOpeningLineFrame) / (float)OpeningNarrationFadeFrames);
        }

        return 1f;
    }

    private void DrawField(SpriteBatch batch)
    {
        DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), new XnaColor(5, 8, 16));

        var mapWidth = map.GetLength(1);
        var mapHeight = map.GetLength(0);
        var originX = Math.Max(0, (VirtualWidth - (mapWidth * TileSize)) / 2);
        var originY = Math.Max(0, (VirtualHeight - (mapHeight * TileSize)) / 2);

        for (var y = 0; y < mapHeight; y++)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                var rect = new XnaRectangle(originX + (x * TileSize), originY + (y * TileSize), TileSize, TileSize);
                DrawSolidPanel(batch, rect, GetTileColor(map[y, x]));
            }
        }

        var playerRect = new XnaRectangle(
            originX + (player.TilePosition.X * TileSize) + 7,
            originY + (player.TilePosition.Y * TileSize) + 4,
            18,
            24);
        DrawSolidPanel(batch, playerRect, GetPlayerColor());

        if (gameState == GameState.EncounterTransition)
        {
            var alpha = Math.Clamp(encounterTransitionFrames / (float)EncounterTransitionDuration, 0f, 1f);
            DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), XnaColor.Black * alpha);
        }
    }

    private void DrawFade(SpriteBatch batch)
    {
        if (startupFadeFrames > 0)
        {
            DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), XnaColor.Black * (startupFadeFrames / 20f));
        }

        if (pendingGameState is not null && sceneFadeOutFramesRemaining > 0)
        {
            var progress = 1f - (sceneFadeOutFramesRemaining / (float)SceneFadeOutDuration);
            DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), XnaColor.Black * Math.Clamp(progress, 0f, 1f));
        }
    }

    private void DrawCursorPanel(SpriteBatch batch, int cursor, int itemCount, int x, int y, int width, int height)
    {
        DrawSolidPanel(batch, new XnaRectangle(x, y, width, height), new XnaColor(36, 34, 42));
        if (cursor < 0)
        {
            return;
        }

        var rowHeight = Math.Max(1, height / itemCount);
        var marker = new XnaRectangle(x + 16, y + (cursor * rowHeight) + 8, width - 32, Math.Max(8, rowHeight - 16));
        DrawSolidPanel(batch, marker, new XnaColor(0, 96, 180));
    }

    private void DrawSolidPanel(SpriteBatch batch, XnaRectangle rect, XnaColor color)
    {
        if (pixelTexture is not null)
        {
            batch.Draw(pixelTexture, rect, color);
        }
    }

    private static int ScaleMenuX(XnaRectangle layout, int sourceX)
    {
        return layout.X + (int)Math.Round(sourceX * layout.Width / 256f);
    }

    private static int ScaleMenuY(XnaRectangle layout, int sourceY)
    {
        return layout.Y + (int)Math.Round(sourceY * layout.Height / 240f);
    }

    private static int ScaleMenuWidth(XnaRectangle layout, int sourceWidth)
    {
        return (int)Math.Round(sourceWidth * layout.Width / 256f);
    }

    private static int ScaleMenuHeight(XnaRectangle layout, int sourceHeight)
    {
        return (int)Math.Round(sourceHeight * layout.Height / 240f);
    }

    private static string GetModeSelectDescription(int cursor)
    {
        return cursor switch
        {
            0 => "ゲームを最初から\nはじめる。",
            1 => "保存したデータから\nつづきをはじめる。",
            2 => "セーブデータを\nうつす。",
            3 => "セーブデータを\nけす。",
            _ => string.Empty
        };
    }

    private XnaColor GetTileColor(int tileId)
    {
        return tileId switch
        {
            MapFactory.WallTile when currentFieldMap == FieldMapId.Castle => new XnaColor(58, 14, 24),
            MapFactory.WallTile => new XnaColor(8, 30, 90),
            MapFactory.CastleBlockTile => new XnaColor(120, 28, 38),
            MapFactory.CastleGateTile => new XnaColor(116, 58, 30),
            MapFactory.FieldGateTile => new XnaColor(24, 56, 40),
            MapFactory.CastleFloorTile => new XnaColor(108, 42, 52),
            MapFactory.GrassTile => new XnaColor(24, 74, 36),
            MapFactory.DecorationBlueTile when currentFieldMap == FieldMapId.Castle => new XnaColor(76, 20, 34),
            MapFactory.DecorationBlueTile => new XnaColor(8, 30, 90),
            _ => new XnaColor(18, 18, 18)
        };
    }

    private XnaColor GetPlayerColor()
    {
        return playerFacingDirection switch
        {
            PlayerFacingDirection.Left => new XnaColor(210, 235, 255),
            PlayerFacingDirection.Right => new XnaColor(255, 235, 210),
            PlayerFacingDirection.Up => new XnaColor(220, 255, 220),
            _ => XnaColor.White
        };
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
