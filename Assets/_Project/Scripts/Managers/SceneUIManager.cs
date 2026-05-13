using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DragonGlare.Data;
using DragonGlare.Domain;
using DragonGlare.Domain.Battle;
using DragonGlare.Domain.Commerce;
using DragonGlare.Domain.Field;
using DragonGlare.Domain.Items;
using DragonGlare.Domain.Player;
using DragonGlare.Domain.Startup;
using DragonGlare.Services;

namespace DragonGlare
{
    public class SceneUIManager : MonoBehaviour
    {
        [Header("Scene Canvases")]
        [SerializeField] private StartupOptionsScene startupOptionsScene;
        [SerializeField] private ModeSelectScene modeSelectScene;
        [SerializeField] private LanguageSelectionScene languageSelectionScene;
        [SerializeField] private NameInputScene nameInputScene;
        [SerializeField] private SaveSlotSelectionScene saveSlotSelectionScene;
        [SerializeField] private FieldScene fieldScene;
        [SerializeField] private EncounterTransitionScene encounterTransitionScene;
        [SerializeField] private BattleScene battleScene;
        [SerializeField] private ShopScene shopScene;
        [SerializeField] private BankScene bankScene;
        [SerializeField] private FadeOverlay fadeOverlay;

        private GameState gameState = GameState.ModeSelect;
        private GameState? pendingGameState;
        private int sceneFadeOutFramesRemaining;
        private int startupFadeFrames = 20;
        private int frameCounter;

        public GameState CurrentGameState => gameState;
        public GameState? PendingGameState => pendingGameState;
        public FieldMapId CurrentFieldMap { get; private set; } = FieldMapId.Hub;
        public bool CanSave { get; private set; }

        private PlayerProgress player;
        private BattleEncounter currentEncounter;
        private BattleFlowState battleFlowState = BattleFlowState.CommandSelection;
        private ShopPhase shopPhase = ShopPhase.Welcome;
        private BankPhase bankPhase = BankPhase.Welcome;

        private readonly Random random = new();
        private readonly BattleService battleService = new();
        private readonly ProgressionService progressionService = new();
        private readonly ShopService shopService = new();
        private readonly BankService bankService = new();
        private readonly FieldEventService fieldEventService = new();
        private readonly FieldTransitionService fieldTransitionService = new();

        private int modeCursor;
        private int languageCursor;
        private int optionsCursor;
        private int nameCursorRow;
        private int nameCursorColumn;
        private int saveSlotCursor;
        private int activeSaveSlot;
        private int movementCooldown;
        private bool isFieldDialogOpen;
        private bool isFieldStatusVisible;
        private int battleCursorRow;
        private int battleCursorColumn;
        private int battleListCursor;
        private int battleListScroll;
        private int shopPromptCursor;
        private int shopItemCursor;
        private int shopPageIndex;
        private int bankPromptCursor;
        private int bankItemCursor;
        private SaveSlotSelectionMode saveSlotSelectionMode = SaveSlotSelectionMode.Save;
        private int dataOperationSourceSlot;
        private string battleMessage = string.Empty;
        private string shopMessage = string.Empty;
        private string bankMessage = string.Empty;
        private string menuNotice = string.Empty;
        private int menuNoticeFrames;
        private int languageOpeningElapsedFrames;
        private int languageOpeningLineIndex;
        private int languageOpeningLineFrame;
        private bool languageOpeningFinished;
        private bool skipSaveOnClose;
        private int encounterTransitionFrames;
        private int fieldEncounterStepsRemaining = 7;
        private int battlePlayerActionFramesRemaining;
        private int battlePlayerGuardFramesRemaining;
        private int battleEnemyActionFramesRemaining;
        private int battleItemUseFramesRemaining;
        private int battleEnemyDefeatFramesRemaining;
        private int enemyHitFlashFramesRemaining;
        private int battleSpellEffectFramesRemaining;
        private int playerHitFlashFramesRemaining;
        private int battlePlayerHealFramesRemaining;
        private int battleStatusEffectFramesRemaining;
        private int battleIntroFramesRemaining;
        private BattleEncounter pendingEncounter;
        private IReadOnlyList<string> activeFieldDialogPages = Array.Empty<string>();
        private int activeFieldDialogPageIndex;
        private string activeFieldDialogPortraitAssetName;
        private SaveSlotSummary[] saveSlotSummaries = Array.Empty<SaveSlotSummary>();
        private LaunchSettings launchSettings;
        private LaunchDisplayMode activeDisplayMode;
        private LaunchDisplayMode lastWindowedDisplayMode = LaunchDisplayMode.Window640x480;
        private int progressSaveDelayFrames;
        private int progressSaveMaxDelayFrames;
        private bool progressSavePending;
        private PlayerFacingDirection playerFacingDirection = PlayerFacingDirection.Down;
        private Vector2Int fieldMovementAnimationDirection;
        private int fieldMovementAnimationFramesRemaining;
        private IReadOnlyList<BattleSequenceStep> battleResolutionSteps = Array.Empty<BattleSequenceStep>();
        private BattleTurnResolution activeBattleResolution;
        private BattleFlowState battleReturnFlowState = BattleFlowState.CommandSelection;
        private int battleResolutionStepIndex = -1;
        private int battleResolutionStepFramesRemaining;
        private int battlePlayerActionFrames;
        private int languageOpeningLastSourceX = -1;
        private int languageOpeningLastSourceY = -1;
        private bool skipLanguageSelectionPrompt;
        private bool prologueBgmCompleted;
        private int[,] map;
        private UiLanguage selectedLanguage = UiLanguage.Japanese;
        private readonly StringBuilder playerName = new();

        private static readonly OpeningNarrationLine[] LanguageOpeningScript =
        {
            new("遠い昔。", 138, 31),
            new("世界には五つの大地があった。", 138, 31),
            new("その時代では、争いがなく。\n皆が平和に満ちていた。", 199, 38),
            new("そして、それぞれの大地で\n違う神が崇められていたという。", 245, 38),
            new("しかし", 69, 92),
            new("平和は長くは続かなかった。", 192, 38),
            new("争いによって、日の大地は\n跡形もなく崩れ落ちてしまった。", 222, 38),
            new("そして、世界には暗い月しか\n上らなくなってしまった。", 214, 38),
            new("次から次へと世界は\n闇に満ちていった。", 191, 38),
            new("ついには、世界の中心となる光の大地が\n愚かな争いにより、闇に沈んでいった。", 260, 46),
            new("やがて、光の神は\n闇に飲み込まれ", 169, 38),
            new("世界にある万物が\n人々を襲うようにしてしまった。", 207, 38),
            new("世界は、いつしか光を失い\n闇が世界を司るようになった。", 230, 0)
        };

        private void Start()
        {
            player = PlayerProgress.CreateDefault(new Vector2Int(GameConstants.PlayerStartTile.x, GameConstants.PlayerStartTile.y));
            map = MapFactory.CreateDefaultMap();
            launchSettings = new LaunchSettings();
            activeDisplayMode = launchSettings.DisplayMode;
            optionsCursor = (int)activeDisplayMode;
            if (activeDisplayMode != LaunchDisplayMode.Fullscreen)
                lastWindowedDisplayMode = activeDisplayMode;

            GameManager.Instance.Audio.InitializeAudio();
            GameManager.Instance.Sprites.LoadFieldSprites();
            RefreshSaveSlotSummaries();

            gameState = launchSettings.PromptOnStartup ? GameState.StartupOptions : GameState.ModeSelect;
            ShowCurrentScene();
        }

        public void UpdateCurrentScene()
        {
            frameCounter++;

            if (startupFadeFrames > 0)
                startupFadeFrames--;

            if (pendingGameState.HasValue && sceneFadeOutFramesRemaining > 0)
            {
                sceneFadeOutFramesRemaining--;
                if (sceneFadeOutFramesRemaining <= 0)
                {
                    gameState = pendingGameState.Value;
                    pendingGameState = null;
                    ShowCurrentScene();
                }
                else
                {
                    var progress = 1f - (sceneFadeOutFramesRemaining / (float)GameConstants.SceneFadeOutDuration);
                    fadeOverlay.SetAlpha(Mathf.Clamp01(progress));
                }
                return;
            }

            if (pendingGameState.HasValue)
            {
                gameState = pendingGameState.Value;
                pendingGameState = null;
                ShowCurrentScene();
            }

            fadeOverlay.SetAlpha(0f);

            switch (gameState)
            {
                case GameState.StartupOptions:
                    UpdateStartupOptions();
                    break;
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

            UpdateProgressSaveDelay();
        }

        private void ShowCurrentScene()
        {
            startupOptionsScene?.gameObject.SetActive(gameState == GameState.StartupOptions);
            modeSelectScene?.gameObject.SetActive(gameState == GameState.ModeSelect);
            languageSelectionScene?.gameObject.SetActive(gameState == GameState.LanguageSelection);
            nameInputScene?.gameObject.SetActive(gameState == GameState.NameInput);
            saveSlotSelectionScene?.gameObject.SetActive(gameState == GameState.SaveSlotSelection);
            fieldScene?.gameObject.SetActive(gameState == GameState.Field || gameState == GameState.EncounterTransition);
            encounterTransitionScene?.gameObject.SetActive(gameState == GameState.EncounterTransition);
            battleScene?.gameObject.SetActive(gameState == GameState.Battle);
            shopScene?.gameObject.SetActive(gameState == GameState.ShopBuy);
            bankScene?.gameObject.SetActive(gameState == GameState.Bank);

            switch (gameState)
            {
                case GameState.StartupOptions:
                    startupOptionsScene.Show(optionsCursor, activeDisplayMode, launchSettings.PromptOnStartup);
                    break;
                case GameState.ModeSelect:
                    modeSelectScene.Show(modeCursor, selectedLanguage, menuNotice);
                    break;
                case GameState.LanguageSelection:
                    languageSelectionScene.Show(languageCursor, languageOpeningFinished, languageOpeningElapsedFrames, LanguageOpeningScript);
                    break;
                case GameState.NameInput:
                    nameInputScene.Show(selectedLanguage, nameCursorRow, nameCursorColumn, playerName.ToString());
                    break;
                case GameState.SaveSlotSelection:
                    saveSlotSelectionScene.Show(selectedLanguage, saveSlotSelectionMode, saveSlotCursor, saveSlotSummaries, dataOperationSourceSlot, menuNotice);
                    break;
                case GameState.Field:
                    fieldScene.Show(player, currentEncounter, isFieldStatusVisible, isFieldDialogOpen, activeFieldDialogPages, activeFieldDialogPageIndex, activeFieldDialogPortraitAssetName, selectedLanguage, playerFacingDirection, fieldMovementAnimationFramesRemaining, fieldMovementAnimationDirection);
                    break;
                case GameState.EncounterTransition:
                    encounterTransitionScene.Show(encounterTransitionFrames, pendingEncounter, selectedLanguage);
                    break;
                case GameState.Battle:
                    battleScene.Show(player, currentEncounter, battleFlowState, battleCursorRow, battleCursorColumn, battleListCursor, battleListScroll, battleMessage, battleResolutionSteps, battleResolutionStepIndex, selectedLanguage, frameCounter,
                        battlePlayerActionFramesRemaining, battlePlayerGuardFramesRemaining, battleEnemyActionFramesRemaining, battleItemUseFramesRemaining, battleEnemyDefeatFramesRemaining,
                        enemyHitFlashFramesRemaining, battleSpellEffectFramesRemaining, playerHitFlashFramesRemaining, battlePlayerHealFramesRemaining, battleStatusEffectFramesRemaining, battleIntroFramesRemaining);
                    break;
                case GameState.ShopBuy:
                    shopScene.Show(player, shopPhase, shopPromptCursor, shopItemCursor, shopPageIndex, shopMessage, selectedLanguage);
                    break;
                case GameState.Bank:
                    bankScene.Show(player, bankPhase, bankPromptCursor, bankItemCursor, bankMessage, selectedLanguage);
                    break;
            }
        }

        private void ChangeGameState(GameState nextState)
        {
            if (nextState == gameState)
                return;
            pendingGameState = nextState;
            sceneFadeOutFramesRemaining = GameConstants.SceneFadeOutDuration;
        }

        public void FlushSave()
        {
            if (progressSavePending)
            {
                FlushQueuedProgressSave(refreshSlotSummaries: false);
            }
            else
            {
                SaveGame(refreshSlotSummaries: false);
            }
        }

        private void UpdateStartupOptions()
        {
            var input = GameManager.Instance.Input;
            var previousCursor = optionsCursor;
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                optionsCursor = Math.Max(0, optionsCursor - 1);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                optionsCursor = Math.Min(5, optionsCursor + 1);
            PlayCursorSeIfChanged(previousCursor, optionsCursor);

            if (!input.WasPrimaryConfirmPressed())
                return;

            if (optionsCursor < 4)
            {
                SetStartupDisplayMode((LaunchDisplayMode)optionsCursor);
                return;
            }

            if (optionsCursor == 4)
            {
                launchSettings = new LaunchSettings { DisplayMode = activeDisplayMode, PromptOnStartup = !launchSettings.PromptOnStartup };
                return;
            }

            SaveLaunchSettings();
            ChangeGameState(GameState.ModeSelect);
        }

        private void SetStartupDisplayMode(LaunchDisplayMode displayMode)
        {
            activeDisplayMode = displayMode;
            launchSettings = new LaunchSettings { DisplayMode = displayMode, PromptOnStartup = launchSettings.PromptOnStartup };
            ApplyDisplayMode();
        }

        private void ApplyDisplayMode()
        {
            if (activeDisplayMode == LaunchDisplayMode.Fullscreen)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            }
            else
            {
                var size = activeDisplayMode switch
                {
                    LaunchDisplayMode.Window720p => new Vector2Int(1280, 720),
                    LaunchDisplayMode.Window1080p => new Vector2Int(1920, 1080),
                    _ => new Vector2Int(640, 480)
                };
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                lastWindowedDisplayMode = activeDisplayMode;
            }
        }

        private void SaveLaunchSettings()
        {
            PlayerPrefs.SetInt("DisplayMode", (int)activeDisplayMode);
            PlayerPrefs.SetInt("PromptOnStartup", launchSettings.PromptOnStartup ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void UpdateModeSelect()
        {
            var input = GameManager.Instance.Input;
            var previousCursor = modeCursor;
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                modeCursor = Math.Max(0, modeCursor - 1);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                modeCursor = Math.Min(3, modeCursor + 1);
            PlayCursorSeIfChanged(previousCursor, modeCursor);

            if (input.WasShopBackPressed())
            {
                PlayCancelSe();
                return;
            }

            if (!input.WasPrimaryConfirmPressed())
                return;

            switch (modeCursor)
            {
                case 0:
                    ChangeGameState(GameState.LanguageSelection);
                    break;
                case 1:
                    saveSlotSelectionMode = SaveSlotSelectionMode.Load;
                    ChangeGameState(GameState.SaveSlotSelection);
                    break;
                case 2:
                    saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                    ChangeGameState(GameState.SaveSlotSelection);
                    break;
                case 3:
                    saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                    ChangeGameState(GameState.SaveSlotSelection);
                    break;
            }
        }

        private void UpdateLanguageSelection()
        {
            var input = GameManager.Instance.Input;
            if (!languageOpeningFinished)
            {
                languageOpeningElapsedFrames++;
                var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
                languageOpeningLineFrame++;
                if (languageOpeningLineFrame >= currentLine.DisplayFrames)
                {
                    languageOpeningLineFrame = 0;
                    languageOpeningLineIndex++;
                    if (languageOpeningLineIndex >= LanguageOpeningScript.Length)
                    {
                        languageOpeningFinished = true;
                    }
                }

                if (input.WasPrimaryConfirmPressed() || input.WasShopBackPressed())
                {
                    languageOpeningFinished = true;
                }
                return;
            }

            var previousCursor = languageCursor;
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                languageCursor = Math.Max(0, languageCursor - 1);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                languageCursor = Math.Min(1, languageCursor + 1);
            PlayCursorSeIfChanged(previousCursor, languageCursor);

            if (input.WasShopBackPressed())
            {
                PlayCancelSe();
                ChangeGameState(GameState.ModeSelect);
                return;
            }

            if (!input.WasPrimaryConfirmPressed())
                return;

            selectedLanguage = languageCursor == 0 ? UiLanguage.Japanese : UiLanguage.English;
            ChangeGameState(GameState.NameInput);
        }

        private void UpdateNameInput()
        {
            var input = GameManager.Instance.Input;
            var table = GameContent.GetNameTable(selectedLanguage);
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                MoveNameCursor(0, -1, table);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                MoveNameCursor(0, 1, table);
            else if (input.WasPressed(KeyCode.Left) || input.WasPressed(KeyCode.A))
                MoveNameCursor(-1, 0, table);
            else if (input.WasPressed(KeyCode.Right) || input.WasPressed(KeyCode.D))
                MoveNameCursor(1, 0, table);

            if (input.WasPrimaryConfirmPressed())
                AddSelectedCharacter(table);
            else if (input.WasPressed(KeyCode.X))
                RemoveLastCharacter();
            else if (input.WasShopBackPressed())
            {
                PlayCancelSe();
                ChangeGameState(GameState.LanguageSelection);
            }
        }

        private void MoveNameCursor(int deltaX, int deltaY, string[][] table)
        {
            var previousRow = nameCursorRow;
            var previousColumn = nameCursorColumn;
            nameCursorRow = Mathf.Clamp(nameCursorRow + deltaY, 0, table.Length - 1);
            var maxColumn = table[nameCursorRow].Length - 1;
            nameCursorColumn = Mathf.Clamp(nameCursorColumn + deltaX, 0, maxColumn);
            if (previousRow != nameCursorRow || previousColumn != nameCursorColumn)
                GameManager.Instance.Audio.PlaySe(SoundEffect.Cursor);
        }

        private void AddSelectedCharacter(string[][] table)
        {
            var selected = table[nameCursorRow][nameCursorColumn];
            var deleteToken = selectedLanguage == UiLanguage.Japanese ? "けす" : "DEL";
            var endToken = selectedLanguage == UiLanguage.Japanese ? "おわり" : "END";

            if (selected == deleteToken)
            {
                RemoveLastCharacter();
                return;
            }

            if (selected == endToken)
            {
                if (playerName.Length > 0)
                {
                    player.Name = TrimPlayerName(playerName.ToString());
                    saveSlotSelectionMode = SaveSlotSelectionMode.Save;
                    ChangeGameState(GameState.SaveSlotSelection);
                }
                return;
            }

            if (playerName.Length < GameConstants.MaxPlayerNameLength)
                playerName.Append(selected);
        }

        private void RemoveLastCharacter()
        {
            if (playerName.Length > 0)
            {
                playerName.Remove(playerName.Length - 1, 1);
                PlayCancelSe();
            }
        }

        private static string TrimPlayerName(string name)
        {
            return name.Trim();
        }

        private void UpdateSaveSlotSelection()
        {
            var input = GameManager.Instance.Input;
            var previousCursor = saveSlotCursor;
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                saveSlotCursor = Math.Max(0, saveSlotCursor - 1);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                saveSlotCursor = Math.Min(SaveManager.SlotCount - 1, saveSlotCursor + 1);
            PlayCursorSeIfChanged(previousCursor, saveSlotCursor);

            if (input.WasShopBackPressed())
            {
                PlayCancelSe();
                switch (saveSlotSelectionMode)
                {
                    case SaveSlotSelectionMode.Save:
                        ChangeGameState(GameState.NameInput);
                        break;
                    case SaveSlotSelectionMode.Load:
                    case SaveSlotSelectionMode.CopySource:
                    case SaveSlotSelectionMode.DeleteSelect:
                        ChangeGameState(GameState.ModeSelect);
                        break;
                    case SaveSlotSelectionMode.CopyDestination:
                        saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                        dataOperationSourceSlot = 0;
                        break;
                    case SaveSlotSelectionMode.DeleteConfirm:
                        saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                        break;
                }
                return;
            }

            if (!input.WasPrimaryConfirmPressed())
                return;

            var slotNumber = saveSlotCursor + 1;
            switch (saveSlotSelectionMode)
            {
                case SaveSlotSelectionMode.Save:
                    SaveGame(slotNumber);
                    ApplyExplorationSession(player, CurrentFieldMap);
                    ChangeGameState(GameState.Field);
                    break;
                case SaveSlotSelectionMode.Load:
                    if (GameManager.Instance.Save.TryLoadSlot(slotNumber, out var saveData) && saveData != null)
                    {
                        player = SaveDataMapper.ToPlayerProgress(saveData);
                        ApplyExplorationSession(player, saveData.CurrentFieldMap);
                        ChangeGameState(GameState.Field);
                    }
                    else
                    {
                        ShowMenuNotice(selectedLanguage == UiLanguage.English ? "Could not load data." : "よみこめませんでした。");
                    }
                    break;
                case SaveSlotSelectionMode.CopySource:
                    dataOperationSourceSlot = slotNumber;
                    saveSlotSelectionMode = SaveSlotSelectionMode.CopyDestination;
                    break;
                case SaveSlotSelectionMode.CopyDestination:
                    if (dataOperationSourceSlot != slotNumber)
                    {
                        if (GameManager.Instance.Save.CopySlot(dataOperationSourceSlot, slotNumber))
                        {
                            RefreshSaveSlotSummaries();
                            ShowMenuNotice(selectedLanguage == UiLanguage.English ? "Copy complete." : "コピーしました。");
                        }
                        else
                        {
                            ShowMenuNotice(selectedLanguage == UiLanguage.English ? "Copy failed." : "コピーできませんでした。");
                        }
                    }
                    saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                    dataOperationSourceSlot = 0;
                    break;
                case SaveSlotSelectionMode.DeleteSelect:
                    dataOperationSourceSlot = slotNumber;
                    saveSlotSelectionMode = SaveSlotSelectionMode.DeleteConfirm;
                    break;
                case SaveSlotSelectionMode.DeleteConfirm:
                    GameManager.Instance.Save.DeleteSlot(slotNumber);
                    RefreshSaveSlotSummaries();
                    ShowMenuNotice(selectedLanguage == UiLanguage.English ? "Deleted." : "けしました。");
                    saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                    dataOperationSourceSlot = 0;
                    break;
            }
        }

        private void UpdateField()
        {
            var input = GameManager.Instance.Input;
            if (isFieldDialogOpen)
            {
                if (input.WasFieldInteractPressed())
                    AdvanceFieldDialog();
                else if (input.WasPressed(KeyCode.Escape))
                {
                    PlayCancelSe();
                    CloseFieldDialog();
                }
                return;
            }

            if (input.WasPressed(KeyCode.B))
            {
                EnterBattle();
                return;
            }

            if (input.WasPressed(KeyCode.V))
            {
                EnterShopBuy();
                return;
            }

            if (input.WasPressed(KeyCode.X))
            {
                isFieldStatusVisible = !isFieldStatusVisible;
                return;
            }

            if (movementCooldown > 0)
                movementCooldown--;

            var movement = Vector2Int.zero;
            if (input.HeldKeys.Contains(KeyCode.Up) || input.HeldKeys.Contains(KeyCode.W))
                movement = new Vector2Int(0, -1);
            else if (input.HeldKeys.Contains(KeyCode.Down) || input.HeldKeys.Contains(KeyCode.S))
                movement = new Vector2Int(0, 1);
            else if (input.HeldKeys.Contains(KeyCode.Left) || input.HeldKeys.Contains(KeyCode.A))
                movement = new Vector2Int(-1, 0);
            else if (input.HeldKeys.Contains(KeyCode.Right) || input.HeldKeys.Contains(KeyCode.D))
                movement = new Vector2Int(1, 0);

            if (movement != Vector2Int.zero && movementCooldown == 0)
            {
                SetPlayerFacingDirection(movement);
                var moved = TryMovePlayer(movement);
                if (!moved)
                    GameManager.Instance.Audio.PlaySe(SoundEffect.Collision);
                movementCooldown = GameConstants.FieldMovementAnimationDuration;
                if (gameState != GameState.Field)
                    return;
            }

            if (input.WasFieldInteractPressed())
            {
                var fieldEvent = GetInteractableFieldEvent();
                if (fieldEvent != null)
                    OpenFieldDialog(fieldEvent);
            }
        }

        private void EnterBattle()
        {
            StartEncounterTransition(battleService.CreateEncounter(random, CurrentFieldMap, player.Level));
        }

        private bool TryMovePlayer(Vector2Int movement)
        {
            var target = new Vector2Int(player.TilePosition.X + movement.x, player.TilePosition.Y + movement.y);
            if (TryTransitionFromTile(target))
            {
                bankService.AccrueStepInterest(player);
                return true;
            }

            if (!IsWalkableTile(target) || IsBlockedByFieldEvent(target))
                return false;

            player.TilePosition = target;
            bankService.AccrueStepInterest(player);
            StartFieldMovementAnimation(movement);

            if (TryTriggerRandomEncounter())
            {
                PersistProgress();
                return true;
            }

            PersistProgress();
            return true;
        }

        private void SetPlayerFacingDirection(Vector2Int movement)
        {
            if (movement.x < 0) playerFacingDirection = PlayerFacingDirection.Left;
            else if (movement.x > 0) playerFacingDirection = PlayerFacingDirection.Right;
            else if (movement.y < 0) playerFacingDirection = PlayerFacingDirection.Up;
            else if (movement.y > 0) playerFacingDirection = PlayerFacingDirection.Down;
        }

        private bool IsWalkableTile(Vector2Int tile)
        {
            if (tile.x < 0 || tile.y < 0 || tile.x >= map.GetLength(1) || tile.y >= map.GetLength(0))
                return false;
            return MapFactory.IsWalkableTileId(map[tile.y, tile.x]);
        }

        private bool IsBlockedByFieldEvent(Vector2Int tile)
        {
            var events = fieldEventService.GetEventsForMap(CurrentFieldMap);
            return events.Any(e => e.TilePosition.x == tile.x && e.TilePosition.y == tile.y && e.IsBlocking);
        }

        private bool TryTransitionFromTile(Vector2Int tile)
        {
            var transition = fieldTransitionService.GetTransition(CurrentFieldMap, tile);
            if (transition == null) return false;
            CurrentFieldMap = transition.TargetMap;
            map = MapFactory.GetMap(transition.TargetMap);
            player.TilePosition = transition.TargetPosition;
            ResetFieldUiState();
            return true;
        }

        private bool TryTriggerRandomEncounter()
        {
            if (CurrentFieldMap != FieldMapId.Field)
                return false;
            var tileId = map[player.TilePosition.Y, player.TilePosition.X];
            if (MapFactory.IsFieldGateTileId(tileId))
                return false;
            fieldEncounterStepsRemaining -= MapFactory.IsGrassTileId(tileId) ? 2 : 1;
            if (fieldEncounterStepsRemaining > 0)
                return false;
            StartEncounterTransition(battleService.CreateEncounter(random, CurrentFieldMap, player.Level));
            return true;
        }

        private void StartEncounterTransition(BattleEncounter encounter)
        {
            pendingEncounter = encounter;
            encounterTransitionFrames = GameConstants.EncounterTransitionDuration;
            ResetBattleSelectionState();
            ResetEncounterCounter();
            ChangeGameState(GameState.EncounterTransition);
            GameManager.Instance.Audio.PlaySe(SoundEffect.Dialog);
        }

        private void UpdateEncounterTransition()
        {
            if (encounterTransitionFrames > 0)
                encounterTransitionFrames--;

            if (encounterTransitionFrames > 0)
                return;

            if (pendingEncounter == null)
            {
                ChangeGameState(GameState.Field);
                return;
            }

            currentEncounter = pendingEncounter;
            pendingEncounter = null;
            ResetBattleSelectionState();
            battleFlowState = BattleFlowState.Intro;
            battleIntroFramesRemaining = GameConstants.BattleIntroDurationFrames;
            battleMessage = GetBattleEncounterMessage(GameContent.GetEnemyName(currentEncounter.Enemy, selectedLanguage));
            ChangeGameState(GameState.Battle);
        }

        private void UpdateBattle()
        {
            var input = GameManager.Instance.Input;
            if (currentEncounter == null)
            {
                ResetBattleState();
                ChangeGameState(GameState.Field);
                return;
            }

            if (battleFlowState == BattleFlowState.Intro)
            {
                if (battleIntroFramesRemaining > 0)
                    battleIntroFramesRemaining--;
                if (battleIntroFramesRemaining <= 0 || input.WasConfirmPressed())
                {
                    battleIntroFramesRemaining = 0;
                    battleFlowState = BattleFlowState.CommandSelection;
                    battleMessage = GetBattleOpeningCommandMessage();
                }
                return;
            }

            if (battleFlowState == BattleFlowState.Resolving)
            {
                UpdateBattleResolutionSequence();
                return;
            }

            if (battleFlowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
            {
                UpdateBattleSelectionMenu();
                return;
            }

            if (battleFlowState != BattleFlowState.CommandSelection)
            {
                if (input.WasConfirmPressed() || input.WasPressed(KeyCode.Escape))
                    FinishBattle();
                return;
            }

            UpdateBattleCommandCursor();

            if (input.WasPressed(KeyCode.Escape))
            {
                var escapeResult = battleService.ResolveTurn(player, currentEncounter, BattleActionType.Run, null, null, random);
                ApplyBattleResolution(escapeResult);
                return;
            }

            if (!input.WasConfirmPressed())
                return;

            var action = GameContent.BattleCommandGrid[battleCursorRow, battleCursorColumn];
            switch (action)
            {
                case BattleActionType.Spell:
                    OpenBattleSelectionMenu(BattleFlowState.SpellSelection);
                    return;
                case BattleActionType.Item:
                    OpenBattleSelectionMenu(BattleFlowState.ItemSelection);
                    return;
                case BattleActionType.Equip:
                    OpenBattleSelectionMenu(BattleFlowState.EquipmentSelection);
                    return;
            }

            var result = battleService.ResolveTurn(player, currentEncounter, action, null, null, random);
            ApplyBattleResolution(result);
        }

        private void UpdateBattleCommandCursor()
        {
            var input = GameManager.Instance.Input;
            var previousRow = battleCursorRow;
            var previousColumn = battleCursorColumn;
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                battleCursorRow = Math.Max(0, battleCursorRow - 1);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                battleCursorRow = Math.Min(GetBattleCommandRowCount() - 1, battleCursorRow + 1);
            else if (input.WasPressed(KeyCode.Left) || input.WasPressed(KeyCode.A))
                battleCursorColumn = Math.Max(0, battleCursorColumn - 1);
            else if (input.WasPressed(KeyCode.Right) || input.WasPressed(KeyCode.D))
                battleCursorColumn = Math.Min(GetBattleCommandColumnCount() - 1, battleCursorColumn + 1);

            if (previousRow != battleCursorRow || previousColumn != battleCursorColumn)
                GameManager.Instance.Audio.PlaySe(SoundEffect.Cursor);
        }

        private void UpdateBattleSelectionMenu()
        {
            var input = GameManager.Instance.Input;
            var entries = GetActiveBattleSelectionEntries();
            if (entries.Count == 0)
            {
                CloseBattleSelectionMenu(GetBattleEmptySelectionMessage(battleFlowState));
                return;
            }

            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                MoveBattleSelectionCursor(-1, entries.Count);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                MoveBattleSelectionCursor(1, entries.Count);

            if (input.WasBattleSubmenuBackPressed())
            {
                PlayCancelSe();
                CloseBattleSelectionMenu();
                return;
            }

            if (!input.WasBattleSubmenuConfirmPressed() || currentEncounter == null)
                return;

            var selectedEntry = entries[battleListCursor];
            var action = battleFlowState switch
            {
                BattleFlowState.SpellSelection => BattleActionType.Spell,
                BattleFlowState.ItemSelection => BattleActionType.Item,
                _ => BattleActionType.Equip
            };
            var result = battleService.ResolveTurn(player, currentEncounter, action, selectedEntry.Spell, selectedEntry.Consumable, selectedEntry.Equipment, random);
            ApplyBattleResolution(result);
        }

        private void MoveBattleSelectionCursor(int delta, int itemCount)
        {
            var previousCursor = battleListCursor;
            battleListCursor = Mathf.Clamp(battleListCursor + delta, 0, itemCount - 1);
            PlayCursorSeIfChanged(previousCursor, battleListCursor);
            if (battleListCursor < battleListScroll)
                battleListScroll = battleListCursor;
            else if (battleListCursor >= battleListScroll + GameConstants.BattleSelectionVisibleRows)
                battleListScroll = battleListCursor - GameConstants.BattleSelectionVisibleRows + 1;
            battleMessage = GetBattleSelectionMessage(battleFlowState);
        }

        private void OpenBattleSelectionMenu(BattleFlowState nextState)
        {
            battleFlowState = nextState;
            battleListCursor = 0;
            battleListScroll = 0;
            var entries = GetActiveBattleSelectionEntries();
            if (entries.Count == 0)
            {
                CloseBattleSelectionMenu(GetBattleEmptySelectionMessage(nextState));
                return;
            }
            battleMessage = GetBattleSelectionMessage(nextState);
        }

        private void CloseBattleSelectionMenu(string message = null)
        {
            battleFlowState = BattleFlowState.CommandSelection;
            battleListCursor = 0;
            battleListScroll = 0;
            battleMessage = message ?? GetBattleCommandPromptMessage();
        }

        private void UpdateBattleResolutionSequence()
        {
            if (battleResolutionStepFramesRemaining > 0)
            {
                battleResolutionStepFramesRemaining--;
                return;
            }

            battleResolutionStepIndex++;
            if (battleResolutionStepIndex >= battleResolutionSteps.Count)
            {
                battleFlowState = activeBattleResolution?.PlayerWon == true ? BattleFlowState.Victory :
                    activeBattleResolution?.PlayerEscaped == true ? BattleFlowState.Escape : BattleFlowState.Defeat;
                battleMessage = activeBattleResolution?.SummaryMessage ?? battleMessage;
                ResetBattleVisualEffects();
                return;
            }

            var step = battleResolutionSteps[battleResolutionStepIndex];
            battleMessage = step.Message;
            battleResolutionStepFramesRemaining = Mathf.Max(GameConstants.BattleStepMinimumFrames, step.Message.Length / 2 + GameConstants.BattleStepMessageHoldFrames);
            ApplyBattleVisualCue(step.VisualCue);
        }

        private void ApplyBattleResolution(BattleTurnResolution resolution)
        {
            activeBattleResolution = resolution;
            battleResolutionSteps = resolution.Steps;
            battleResolutionStepIndex = -1;
            battleResolutionStepFramesRemaining = 0;
            battleFlowState = BattleFlowState.Resolving;
            battleReturnFlowState = BattleFlowState.CommandSelection;
            UpdateBattleResolutionSequence();
        }

        private void ApplyBattleVisualCue(BattleVisualCue cue)
        {
            switch (cue)
            {
                case BattleVisualCue.PlayerAction:
                    battlePlayerActionFramesRemaining = 8;
                    break;
                case BattleVisualCue.SpellBurst:
                    battleSpellEffectFramesRemaining = 16;
                    break;
                case BattleVisualCue.StatusCloud:
                    battleStatusEffectFramesRemaining = 16;
                    break;
                case BattleVisualCue.PlayerHeal:
                    battlePlayerHealFramesRemaining = 16;
                    break;
                case BattleVisualCue.PlayerGuard:
                    battlePlayerGuardFramesRemaining = 16;
                    break;
                case BattleVisualCue.ItemUse:
                    battleItemUseFramesRemaining = 14;
                    break;
                case BattleVisualCue.EnemyDefeat:
                    battleEnemyDefeatFramesRemaining = 16;
                    break;
                case BattleVisualCue.EnemyHitFlash:
                    enemyHitFlashFramesRemaining = 8;
                    break;
                case BattleVisualCue.PlayerHitFlash:
                    playerHitFlashFramesRemaining = 8;
                    break;
            }
        }

        private void FinishBattle()
        {
            ResetEncounterCounter();
            ChangeGameState(GameState.Field);
            PersistProgress();
        }

        private void UpdateShopBuy()
        {
            var input = GameManager.Instance.Input;
            if (shopPhase == ShopPhase.Welcome)
            {
                var previousCursor = shopPromptCursor;
                if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                    shopPromptCursor = Math.Max(0, shopPromptCursor - 1);
                else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                    shopPromptCursor = Math.Min(2, shopPromptCursor + 1);
                PlayCursorSeIfChanged(previousCursor, shopPromptCursor);

                if (input.WasShopBackPressed())
                {
                    PlayCancelSe();
                    ChangeGameState(GameState.Field);
                    return;
                }

                if (!input.WasShopConfirmPressed())
                    return;

                if (shopPromptCursor == 0)
                {
                    shopPhase = ShopPhase.BuyList;
                    ResetShopListSelection();
                    shopMessage = GetShopBrowseMessage();
                }
                else if (shopPromptCursor == 1)
                {
                    shopPhase = ShopPhase.SellList;
                    ResetShopListSelection();
                    shopMessage = GetShopSellBrowseMessage();
                }
                else
                {
                    ChangeGameState(GameState.Field);
                    PlayCancelSe();
                }
                return;
            }

            var visibleEntries = GetShopVisibleEntries();
            var maxIndex = visibleEntries.Count - 1;
            var previousItemCursor = shopItemCursor;
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                shopItemCursor = Math.Max(0, shopItemCursor - 1);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                shopItemCursor = Math.Min(maxIndex, shopItemCursor + 1);
            PlayCursorSeIfChanged(previousItemCursor, shopItemCursor);

            if (input.WasShopBackPressed())
            {
                PlayCancelSe();
                ReturnToShopPrompt(GetShopReturnMessage());
                return;
            }

            if (!input.WasShopConfirmPressed())
                return;

            var selectedEntry = visibleEntries[shopItemCursor];
            if (selectedEntry.Type == ShopMenuEntryType.PreviousPage)
            {
                ChangeShopPage(-1);
                return;
            }
            if (selectedEntry.Type == ShopMenuEntryType.NextPage)
            {
                ChangeShopPage(1);
                return;
            }
            if (selectedEntry.Type == ShopMenuEntryType.Quit)
            {
                PlayCancelSe();
                ReturnToShopPrompt(GetShopFarewellMessage());
                return;
            }

            if (shopPhase == ShopPhase.SellList)
            {
                if (selectedEntry.InventoryItem != null)
                {
                    var sellResult = shopService.SellItem(player, selectedEntry.InventoryItem.Value.ItemId);
                    shopMessage = sellResult.Message;
                    if (sellResult.Success)
                    {
                        ResetShopListSelection(Math.Min(shopPageIndex, Math.Max(0, GetShopPageCount() - 1)));
                        PersistProgress();
                    }
                }
                return;
            }

            if (selectedEntry.Product != null)
            {
                var purchaseResult = shopService.PurchaseProduct(player, selectedEntry.Product);
                shopMessage = purchaseResult.Message;
                if (purchaseResult.Success)
                {
                    if (purchaseResult.Equipped)
                        GameManager.Instance.Audio.PlaySe(SoundEffect.Equip);
                    PersistProgress();
                }
            }
        }

        private void EnterShopBuy()
        {
            shopPhase = ShopPhase.Welcome;
            shopPromptCursor = 0;
            ResetShopListSelection();
            shopMessage = GetShopWelcomeMessage();
            ChangeGameState(GameState.ShopBuy);
            GameManager.Instance.Audio.PlaySe(SoundEffect.Dialog);
        }

        private void UpdateBank()
        {
            var input = GameManager.Instance.Input;
            if (bankPhase == BankPhase.Welcome)
            {
                var previousCursor = bankPromptCursor;
                if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                    bankPromptCursor = Math.Max(0, bankPromptCursor - 1);
                else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                    bankPromptCursor = Math.Min(3, bankPromptCursor + 1);
                PlayCursorSeIfChanged(previousCursor, bankPromptCursor);

                if (input.WasShopBackPressed())
                {
                    PlayCancelSe();
                    ChangeGameState(GameState.Field);
                    return;
                }

                if (!input.WasShopConfirmPressed())
                    return;

                switch (bankPromptCursor)
                {
                    case 0:
                        OpenBankList(BankPhase.DepositList);
                        break;
                    case 1:
                        OpenBankList(BankPhase.WithdrawList);
                        break;
                    case 2:
                        OpenBankList(BankPhase.BorrowList);
                        break;
                    default:
                        PlayCancelSe();
                        ChangeGameState(GameState.Field);
                        break;
                }
                return;
            }

            var options = GetBankAmountOptions();
            var previousItemCursor = bankItemCursor;
            if (input.WasPressed(KeyCode.Up) || input.WasPressed(KeyCode.W))
                bankItemCursor = Math.Max(0, bankItemCursor - 1);
            else if (input.WasPressed(KeyCode.Down) || input.WasPressed(KeyCode.S))
                bankItemCursor = Math.Min(options.Count - 1, bankItemCursor + 1);
            PlayCursorSeIfChanged(previousItemCursor, bankItemCursor);

            if (input.WasShopBackPressed())
            {
                PlayCancelSe();
                ReturnToBankPrompt(GetBankReturnMessage());
                return;
            }

            if (!input.WasShopConfirmPressed())
                return;

            var selectedOption = options[bankItemCursor];
            if (selectedOption.Quit)
            {
                PlayCancelSe();
                ReturnToBankPrompt(GetBankReturnMessage());
                return;
            }

            var amount = ResolveBankTransactionAmount(selectedOption);
            var result = bankPhase switch
            {
                BankPhase.DepositList => bankService.Deposit(player, amount),
                BankPhase.WithdrawList => bankService.Withdraw(player, amount),
                BankPhase.BorrowList => bankService.Borrow(player, amount),
                _ => new BankTransactionResult(false, 0, 0, GetBankReturnMessage())
            };

            bankMessage = result.Message;
            if (result.Success)
                PersistProgress();
        }

        private void EnterBank()
        {
            bankPhase = BankPhase.Welcome;
            bankPromptCursor = 0;
            bankItemCursor = 0;
            bankMessage = GetBankWelcomeMessage();
            ChangeGameState(GameState.Bank);
            GameManager.Instance.Audio.PlaySe(SoundEffect.Dialog);
        }

        private void OpenBankList(BankPhase nextPhase)
        {
            bankPhase = nextPhase;
            bankItemCursor = 0;
            bankMessage = nextPhase switch
            {
                BankPhase.DepositList => GetBankDepositMessage(),
                BankPhase.WithdrawList => GetBankWithdrawMessage(),
                BankPhase.BorrowList => GetBankBorrowMessage(),
                _ => GetBankWelcomeMessage()
            };
        }

        private void ReturnToBankPrompt(string message)
        {
            bankPhase = BankPhase.Welcome;
            bankPromptCursor = 0;
            bankItemCursor = 0;
            bankMessage = message;
        }

        private void ReturnToShopPrompt(string message)
        {
            shopPhase = ShopPhase.Welcome;
            shopPromptCursor = 0;
            ResetShopListSelection();
            shopMessage = message;
        }

        private void ChangeShopPage(int pageDelta)
        {
            ResetShopListSelection(shopPageIndex + pageDelta);
            shopMessage = shopPhase == ShopPhase.SellList ? GetShopSellBrowseMessage() : GetShopBrowseMessage();
        }

        private void ResetShopListSelection(int page = 0)
        {
            shopItemCursor = 0;
            shopPageIndex = Mathf.Clamp(page, 0, Math.Max(0, GetShopPageCount() - 1));
        }

        private void ResetBattleSelectionState()
        {
            battleFlowState = BattleFlowState.CommandSelection;
            battleCursorRow = 0;
            battleCursorColumn = 0;
            battleListCursor = 0;
            battleListScroll = 0;
            ResetBattleVisualEffects();
        }

        private void ResetBattleState(string message = null)
        {
            currentEncounter = null;
            pendingEncounter = null;
            encounterTransitionFrames = 0;
            ResetBattleSelectionState();
            battleMessage = message ?? GetDefaultBattleMessage();
            battleResolutionStepIndex = -1;
            battleResolutionStepFramesRemaining = 0;
            battleReturnFlowState = BattleFlowState.CommandSelection;
            battleIntroFramesRemaining = 0;
        }

        private void ResetBattleVisualEffects()
        {
            battlePlayerActionFramesRemaining = 0;
            battlePlayerGuardFramesRemaining = 0;
            battleEnemyActionFramesRemaining = 0;
            battleItemUseFramesRemaining = 0;
            battleEnemyDefeatFramesRemaining = 0;
            enemyHitFlashFramesRemaining = 0;
            battleSpellEffectFramesRemaining = 0;
            playerHitFlashFramesRemaining = 0;
            battlePlayerHealFramesRemaining = 0;
            battleStatusEffectFramesRemaining = 0;
        }

        private void ResetFieldUiState()
        {
            CloseFieldDialog();
            isFieldStatusVisible = false;
            movementCooldown = 0;
            playerFacingDirection = PlayerFacingDirection.Down;
        }

        private void ResetEncounterCounter()
        {
            fieldEncounterStepsRemaining = 7 + random.Next(5);
        }

        private void StartFieldMovementAnimation(Vector2Int direction)
        {
            fieldMovementAnimationDirection = direction;
            fieldMovementAnimationFramesRemaining = GameConstants.FieldMovementAnimationDuration;
        }

        private void OpenFieldDialog(FieldEventDefinition fieldEvent)
        {
            isFieldDialogOpen = true;
            activeFieldDialogPages = fieldEvent.DialogPages;
            activeFieldDialogPageIndex = 0;
            activeFieldDialogPortraitAssetName = fieldEvent.PortraitAssetName;
        }

        private void AdvanceFieldDialog()
        {
            activeFieldDialogPageIndex++;
            if (activeFieldDialogPageIndex >= activeFieldDialogPages.Count)
            {
                CloseFieldDialog();
                var fieldEvent = GetInteractableFieldEvent();
                if (fieldEvent != null)
                {
                    var result = fieldEventService.Interact(fieldEvent, player);
                    if (result.TransitionToBattle)
                    {
                        StartEncounterTransition(battleService.CreateEncounter(random, CurrentFieldMap, player.Level));
                    }
                    else if (result.TransitionToShop)
                    {
                        EnterShopBuy();
                    }
                    else if (result.TransitionToBank)
                    {
                        EnterBank();
                    }
                }
            }
        }

        private void CloseFieldDialog()
        {
            isFieldDialogOpen = false;
            activeFieldDialogPages = Array.Empty<string>();
            activeFieldDialogPageIndex = 0;
            activeFieldDialogPortraitAssetName = null;
        }

        private FieldEventDefinition GetInteractableFieldEvent()
        {
            var events = fieldEventService.GetEventsForMap(CurrentFieldMap);
            return events.FirstOrDefault(e => e.TilePosition.x == player.TilePosition.X && e.TilePosition.y == player.TilePosition.Y && e.IsInteractable);
        }

        private void PersistProgress()
        {
            progressSaveDelayFrames = GameConstants.ProgressSaveDelayFrames;
            progressSaveMaxDelayFrames = GameConstants.ProgressSaveMaxDelayFrames;
            progressSavePending = true;
        }

        private void UpdateProgressSaveDelay()
        {
            if (!progressSavePending)
                return;
            if (progressSaveDelayFrames > 0)
                progressSaveDelayFrames--;
            if (progressSaveMaxDelayFrames > 0)
                progressSaveMaxDelayFrames--;
            if (progressSaveDelayFrames <= 0 || progressSaveMaxDelayFrames <= 0)
            {
                FlushQueuedProgressSave();
            }
        }

        private void FlushQueuedProgressSave(bool refreshSlotSummaries = true)
        {
            progressSavePending = false;
            progressSaveDelayFrames = 0;
            progressSaveMaxDelayFrames = 0;
            SaveGame(activeSaveSlot);
            if (refreshSlotSummaries)
                RefreshSaveSlotSummaries();
        }

        private void SaveGame(int slotNumber, bool refreshSlotSummaries = true)
        {
            if (slotNumber < 1 || slotNumber > SaveManager.SlotCount)
                return;
            var saveData = SaveDataMapper.ToSaveData(player, selectedLanguage, CurrentFieldMap);
            GameManager.Instance.Save.SaveSlot(slotNumber, saveData);
            activeSaveSlot = slotNumber;
            CanSave = true;
            if (refreshSlotSummaries)
                RefreshSaveSlotSummaries();
        }

        private void RefreshSaveSlotSummaries()
        {
            saveSlotSummaries = GameManager.Instance.Save.GetSlotSummaries();
        }

        private void ApplyExplorationSession(PlayerProgress nextPlayer, FieldMapId mapId)
        {
            player = nextPlayer;
            CurrentFieldMap = mapId;
            map = MapFactory.GetMap(mapId);
            SyncPlayerNameBuffer(player.Name);
            ResetFieldUiState();
            ResetBattleState();
            shopPhase = ShopPhase.Welcome;
            shopPromptCursor = 0;
            ResetShopListSelection();
            shopMessage = GetShopWelcomeMessage();
            bankPhase = BankPhase.Welcome;
            bankPromptCursor = 0;
            bankItemCursor = 0;
            bankMessage = GetBankWelcomeMessage();
        }

        private void SyncPlayerNameBuffer(string name)
        {
            playerName.Clear();
            playerName.Append(name);
        }

        private void ShowMenuNotice(string message)
        {
            menuNotice = message;
            menuNoticeFrames = 120;
        }

        private void PlayCursorSeIfChanged(int previous, int current)
        {
            if (previous != current)
                GameManager.Instance.Audio.PlaySe(SoundEffect.Cursor);
        }

        private void PlayCancelSe()
        {
            GameManager.Instance.Audio.PlaySe(SoundEffect.Cancel);
        }

        private int GetBattleCommandRowCount() => GameContent.BattleCommandGrid.GetLength(0);
        private int GetBattleCommandColumnCount() => GameContent.BattleCommandGrid.GetLength(1);

        private string GetBattleCommandLabel(int row, int column)
        {
            var action = GameContent.BattleCommandGrid[row, column];
            return GameContent.GetBattleActionName(action, selectedLanguage);
        }

        private string GetBattleCommandHelpMessage() => selectedLanguage == UiLanguage.English ? "Choose a command." : "こうどうを えらんでください。";
        private string GetBattleSubmenuHelpMessage() => selectedLanguage == UiLanguage.English ? "Choose an item." : "どれを つかいますか？";
        private string GetBattleSelectionTitle() => battleFlowState switch
        {
            BattleFlowState.SpellSelection => selectedLanguage == UiLanguage.English ? "SPELL" : "まほう",
            BattleFlowState.ItemSelection => selectedLanguage == UiLanguage.English ? "ITEM" : "どうぐ",
            BattleFlowState.EquipmentSelection => selectedLanguage == UiLanguage.English ? "EQUIP" : "そうび",
            _ => string.Empty
        };

        private string GetBattleSelectionCounterText()
        {
            var entries = GetActiveBattleSelectionEntries();
            return $"{Mathf.Min(battleListCursor + 1, entries.Count)}/{entries.Count}";
        }

        private string GetBattleEmptySelectionMessage(BattleFlowState state) => state switch
        {
            BattleFlowState.SpellSelection => selectedLanguage == UiLanguage.English ? "No spells." : "まほうを おぼえていない。",
            BattleFlowState.ItemSelection => selectedLanguage == UiLanguage.English ? "No items." : "どうぐを もっていない。",
            BattleFlowState.EquipmentSelection => selectedLanguage == UiLanguage.English ? "No equipment." : "そうびが ない。",
            _ => string.Empty
        };

        private string GetBattleSelectionMessage(BattleFlowState state) => state switch
        {
            BattleFlowState.SpellSelection => selectedLanguage == UiLanguage.English ? "Which spell?" : "どのまほう？",
            BattleFlowState.ItemSelection => selectedLanguage == UiLanguage.English ? "Which item?" : "どのどうぐ？",
            BattleFlowState.EquipmentSelection => selectedLanguage == UiLanguage.English ? "Equip what?" : "なにを そうびする？",
            _ => string.Empty
        };

        private string GetBattleEncounterMessage(string enemyName) => selectedLanguage == UiLanguage.English ? $"{enemyName} appeared!" : $"{enemyName}が あらわれた！";
        private string GetBattleOpeningCommandMessage() => selectedLanguage == UiLanguage.English ? "Command?" : "こうどう？";
        private string GetDefaultBattleMessage() => selectedLanguage == UiLanguage.English ? "A monster appeared!" : "まものが あらわれた！";
        private string GetBattleEscapeMessage() => selectedLanguage == UiLanguage.English ? "You got away safely!" : "うまく にげきった！";

        private IReadOnlyList<BattleSelectionEntry> GetActiveBattleSelectionEntries()
        {
            return battleFlowState switch
            {
                BattleFlowState.SpellSelection => player.Spells.Select(s => new BattleSelectionEntry { Label = s.Name, Detail = $"MP{s.MpCost}", Spell = s }).ToList(),
                BattleFlowState.ItemSelection => player.Inventory.Select(i => new BattleSelectionEntry { Label = GameContent.GetItemName(i.ItemId, selectedLanguage), Detail = $"x{i.Quantity}", Consumable = GameContent.GetConsumableById(i.ItemId) }).Where(e => e.Consumable is not null).ToList(),
                BattleFlowState.EquipmentSelection => player.Inventory.Select(i => new BattleSelectionEntry { Label = GameContent.GetItemName(i.ItemId, selectedLanguage), Detail = string.Empty, Equipment = (IEquipmentDefinition?)GameContent.GetWeaponById(i.ItemId) ?? GameContent.GetArmorById(i.ItemId) }).Where(e => e.Equipment is not null).ToList(),
                _ => Array.Empty<BattleSelectionEntry>()
            };
        }

        private IReadOnlyList<ShopMenuEntry> GetShopVisibleEntries()
        {
            var entries = new List<ShopMenuEntry>();
            var products = shopService.GetProductsForField(CurrentFieldMap);
            var inventory = player.Inventory;

            if (shopPhase == ShopPhase.SellList)
            {
                foreach (var item in inventory)
                {
                    entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.InventoryItem, InventoryItem = item });
                }
            }
            else
            {
                var pageProducts = products.Skip(shopPageIndex * GameConstants.ShopItemsPerPage).Take(GameConstants.ShopItemsPerPage);
                foreach (var product in pageProducts)
                {
                    entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.Product, Product = product });
                }
            }

            if (shopPageIndex > 0)
                entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.PreviousPage });
            if (shopPageIndex < GetShopPageCount() - 1)
                entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.NextPage });
            entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.Quit });

            return entries;
        }

        private int GetShopPageCount()
        {
            var products = shopService.GetProductsForField(CurrentFieldMap);
            return Mathf.Max(1, Mathf.CeilToInt(products.Count / (float)GameConstants.ShopItemsPerPage));
        }

        private IReadOnlyList<BankOption> GetBankAmountOptions()
        {
            var options = new List<BankOption>
            {
                new() { Label = selectedLanguage == UiLanguage.English ? "100G" : "100G", Amount = 100 },
                new() { Label = selectedLanguage == UiLanguage.English ? "1000G" : "1000G", Amount = 1000 },
                new() { Label = selectedLanguage == UiLanguage.English ? "10000G" : "10000G", Amount = 10000 },
                new() { Label = selectedLanguage == UiLanguage.English ? "All" : "ぜんぶ", Amount = -1 },
                new() { Label = selectedLanguage == UiLanguage.English ? "Back" : "もどる", Quit = true }
            };
            return options;
        }

        private int ResolveBankTransactionAmount(BankOption option)
        {
            if (option.Amount > 0)
                return option.Amount;
            return bankPhase switch
            {
                BankPhase.DepositList => player.Gold,
                BankPhase.WithdrawList => player.BankGold,
                BankPhase.BorrowList => bankService.GetAvailableCredit(player),
                _ => 0
            };
        }

        private string GetShopWelcomeMessage() => selectedLanguage == UiLanguage.English ? "* \"Welcome!\n  What do you need?\"" : "＊「いらっしゃい！\n　なにを するんだい？」";
        private string GetShopBrowseMessage() => selectedLanguage == UiLanguage.English ? "* \"What will you buy?\"" : "＊「なにを かっていくかい？」";
        private string GetShopSellBrowseMessage() => selectedLanguage == UiLanguage.English ? "* \"What will you sell?\"" : "＊「なにを うっていくんだい？」";
        private string GetShopReturnMessage() => selectedLanguage == UiLanguage.English ? "* \"Anything else?\"" : "＊「ほかに ようじは あるかい？」";
        private string GetShopFarewellMessage() => selectedLanguage == UiLanguage.English ? "* \"Come again!\"" : "＊「また きてくれよな！」";
        private string GetBankWelcomeMessage() => selectedLanguage == UiLanguage.English ? "* \"Welcome to the bank.\n  How can I help?\"" : "＊「ぎんこうへ ようこそ。\n　ごようけんは？」";
        private string GetBankDepositMessage() => selectedLanguage == UiLanguage.English ? "* \"How much will you deposit?\n  Loans are repaid first.\"" : "＊「いくら あずける？\n　しゃっきんは さきに へんさいするよ。」";
        private string GetBankWithdrawMessage() => selectedLanguage == UiLanguage.English ? "* \"How much will you withdraw?\"" : "＊「いくら ひきだす？」";
        private string GetBankBorrowMessage() => selectedLanguage == UiLanguage.English ? "* \"How much will you borrow?\n  Watch the interest.\"" : "＊「いくら かりる？\n　りそくには きをつけな。」";
        private string GetBankReturnMessage() => selectedLanguage == UiLanguage.English ? "* \"Anything else?\"" : "＊「ほかに ようじは あるかい？」";

        private string GetDisplayPlayerName() => string.IsNullOrWhiteSpace(player.Name) ? (selectedLanguage == UiLanguage.English ? "HERO" : "ゆうしゃ") : player.Name;
        private int GetTotalAttack() => battleService.GetPlayerAttack(player, player.EquippedWeapon);
        private int GetTotalDefense() => battleService.GetPlayerDefense(player, player.EquippedArmor);
        private string GetExperienceSummary() => $"{player.Experience}/{progressionService.GetExperienceForNextLevel(player.Level)}";
        private string GetCurrentEquipmentNameForSlot(EquipmentSlot slot) => player.GetEquippedItemName(slot) ?? (selectedLanguage == UiLanguage.English ? "None" : "なし");
        private string GetEquipmentSlotLabel(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.Weapon => selectedLanguage == UiLanguage.English ? "WPN" : "ぶき",
            EquipmentSlot.Head => selectedLanguage == UiLanguage.English ? "HD" : "あたま",
            EquipmentSlot.Armor => selectedLanguage == UiLanguage.English ? "ARM" : "よろい",
            EquipmentSlot.Arms => selectedLanguage == UiLanguage.English ? "ARM" : "うで",
            EquipmentSlot.Legs => selectedLanguage == UiLanguage.English ? "LEG" : "あし",
            EquipmentSlot.Feet => selectedLanguage == UiLanguage.English ? "FT" : "くつ",
            _ => string.Empty
        };

        private string GetMapDisplayName(FieldMapId mapId, UiLanguage language) => mapId switch
        {
            FieldMapId.Hub => language == UiLanguage.English ? "Hub" : "拠点",
            FieldMapId.Castle => language == UiLanguage.English ? "Castle" : "城",
            FieldMapId.Dungeon => language == UiLanguage.English ? "Dungeon" : "ダンジョン",
            FieldMapId.Field => language == UiLanguage.English ? "Field" : "野外",
            _ => string.Empty
        };

        private void OnApplicationQuit()
        {
            if (!skipSaveOnClose)
                FlushSave();
        }
    }
}
