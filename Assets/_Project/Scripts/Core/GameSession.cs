using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public PlayerProgress Player { get; private set; }
        public BattleEncounter CurrentEncounter { get; private set; }
        public BattleFlowState BattleFlowState { get; private set; } = BattleFlowState.CommandSelection;
        public ShopPhase ShopPhase { get; private set; } = ShopPhase.Welcome;
        public BankPhase BankPhase { get; private set; } = BankPhase.Welcome;
        public FieldMapId CurrentFieldMap { get; private set; } = FieldMapId.Hub;
        public UiLanguage SelectedLanguage { get; private set; } = UiLanguage.Japanese;
        public GameState CurrentGameState { get; private set; } = GameState.ModeSelect;
        public GameState? PendingGameState { get; private set; }
        public bool CanSave { get; private set; }

        public int ModeCursor { get; set; }
        public int LanguageCursor { get; set; }
        public int OptionsCursor { get; set; }
        public int NameCursorRow { get; set; }
        public int NameCursorColumn { get; set; }
        public int SaveSlotCursor { get; set; }
        public int ActiveSaveSlot { get; set; }
        public int BattleCursorRow { get; set; }
        public int BattleCursorColumn { get; set; }
        public int BattleListCursor { get; set; }
        public int BattleListScroll { get; set; }
        public int ShopPromptCursor { get; set; }
        public int ShopItemCursor { get; set; }
        public int ShopPageIndex { get; set; }
        public int BankPromptCursor { get; set; }
        public int BankItemCursor { get; set; }
        public SaveSlotSelectionMode SaveSlotSelectionMode { get; set; } = SaveSlotSelectionMode.Save;
        public int DataOperationSourceSlot { get; set; }
        public string BattleMessage { get; set; } = string.Empty;
        public string ShopMessage { get; set; } = string.Empty;
        public string BankMessage { get; set; } = string.Empty;
        public string MenuNotice { get; set; } = string.Empty;

        public bool IsFieldDialogOpen { get; set; }
        public bool IsFieldStatusVisible { get; set; }
        public IReadOnlyList<string> ActiveFieldDialogPages { get; set; } = Array.Empty<string>();
        public int ActiveFieldDialogPageIndex { get; set; }
        public string ActiveFieldDialogPortraitAssetName { get; set; }
        public PlayerFacingDirection PlayerFacingDirection { get; set; } = PlayerFacingDirection.Down;
        public Vector2Int FieldMovementAnimationDirection { get; set; }
        public int FieldMovementAnimationFramesRemaining { get; set; }
        public int MovementCooldown { get; set; }

        public int EncounterTransitionFrames { get; set; }
        public BattleEncounter PendingEncounter { get; set; }
        public int FieldEncounterStepsRemaining { get; set; } = 7;

        public int BattleIntroFramesRemaining { get; set; }
        public int BattlePlayerActionFramesRemaining { get; set; }
        public int BattlePlayerGuardFramesRemaining { get; set; }
        public int BattleEnemyActionFramesRemaining { get; set; }
        public int BattleItemUseFramesRemaining { get; set; }
        public int BattleEnemyDefeatFramesRemaining { get; set; }
        public int EnemyHitFlashFramesRemaining { get; set; }
        public int BattleSpellEffectFramesRemaining { get; set; }
        public int PlayerHitFlashFramesRemaining { get; set; }
        public int BattlePlayerHealFramesRemaining { get; set; }
        public int BattleStatusEffectFramesRemaining { get; set; }

        public IReadOnlyList<BattleSequenceStep> BattleResolutionSteps { get; set; } = Array.Empty<BattleSequenceStep>();
        public BattleTurnResolution ActiveBattleResolution { get; set; }
        public BattleFlowState BattleReturnFlowState { get; set; } = BattleFlowState.CommandSelection;
        public int BattleResolutionStepIndex { get; set; } = -1;
        public int BattleResolutionStepFramesRemaining { get; set; }

        public int LanguageOpeningElapsedFrames { get; set; }
        public int LanguageOpeningLineIndex { get; set; }
        public int LanguageOpeningLineFrame { get; set; }
        public bool LanguageOpeningFinished { get; set; }
        public bool PrologueBgmCompleted { get; set; }

        public int SceneFadeOutFramesRemaining { get; set; }
        public int StartupFadeFrames { get; set; } = 20;
        public int FrameCounter { get; set; }
        public int MenuNoticeFrames { get; set; }

        public int ProgressSaveDelayFrames { get; set; }
        public int ProgressSaveMaxDelayFrames { get; set; }
        public bool ProgressSavePending { get; set; }
        public bool SkipSaveOnClose { get; set; }

        public LaunchSettings LaunchSettings { get; set; }
        public LaunchDisplayMode ActiveDisplayMode { get; set; }
        public LaunchDisplayMode LastWindowedDisplayMode { get; set; } = LaunchDisplayMode.Window640x480;

        public int[,] Map { get; private set; }
        public StringBuilder PlayerName { get; } = new();

        public readonly Random Random = new();
        public readonly BattleService BattleService = new();
        public readonly ProgressionService ProgressionService = new();
        public readonly ShopService ShopService = new();
        public readonly BankService BankService = new();
        public readonly FieldEventService FieldEventService = new();
        public readonly FieldTransitionService FieldTransitionService = new();

        public SaveSlotSummary[] SaveSlotSummaries { get; set; } = Array.Empty<SaveSlotSummary>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize()
        {
            Player = PlayerProgress.CreateDefault(new Vector2Int(GameConstants.PlayerStartTile.x, GameConstants.PlayerStartTile.y));
            Map = MapFactory.CreateDefaultMap();
            LaunchSettings = new LaunchSettings();
            ActiveDisplayMode = LaunchSettings.DisplayMode;
            OptionsCursor = (int)ActiveDisplayMode;
            if (ActiveDisplayMode != LaunchDisplayMode.Fullscreen)
                LastWindowedDisplayMode = ActiveDisplayMode;
        }

        public void ChangeGameState(GameState nextState)
        {
            if (nextState == CurrentGameState)
                return;
            PendingGameState = nextState;
            SceneFadeOutFramesRemaining = GameConstants.SceneFadeOutDuration;
        }

        public void ApplyPendingState()
        {
            if (!PendingGameState.HasValue)
                return;
            CurrentGameState = PendingGameState.Value;
            PendingGameState = null;
        }

        public void SetFieldMap(FieldMapId mapId)
        {
            CurrentFieldMap = mapId;
            Map = MapFactory.GetMap(mapId);
        }

        public void SyncPlayerNameBuffer(string name)
        {
            PlayerName.Clear();
            PlayerName.Append(name);
        }

        public void ShowMenuNotice(string message)
        {
            MenuNotice = message;
            MenuNoticeFrames = 120;
        }

        public void PersistProgress()
        {
            ProgressSaveDelayFrames = GameConstants.ProgressSaveDelayFrames;
            ProgressSaveMaxDelayFrames = GameConstants.ProgressSaveMaxDelayFrames;
            ProgressSavePending = true;
        }

        public void FlushSave()
        {
            if (ProgressSavePending)
            {
                ProgressSavePending = false;
                ProgressSaveDelayFrames = 0;
                ProgressSaveMaxDelayFrames = 0;
                SaveGame(ActiveSaveSlot);
            }
        }

        public void SaveGame(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > SaveManager.SlotCount)
                return;
            var saveData = SaveDataMapper.ToSaveData(Player, SelectedLanguage, CurrentFieldMap);
            GameManager.Instance.Save.SaveSlot(slotNumber, saveData);
            ActiveSaveSlot = slotNumber;
            CanSave = true;
        }

        public void RefreshSaveSlotSummaries()
        {
            SaveSlotSummaries = GameManager.Instance.Save.GetSlotSummaries();
        }

        public void ResetBattleSelectionState()
        {
            BattleFlowState = BattleFlowState.CommandSelection;
            BattleCursorRow = 0;
            BattleCursorColumn = 0;
            BattleListCursor = 0;
            BattleListScroll = 0;
            ResetBattleVisualEffects();
        }

        public void ResetBattleState(string message = null)
        {
            CurrentEncounter = null;
            PendingEncounter = null;
            EncounterTransitionFrames = 0;
            ResetBattleSelectionState();
            BattleMessage = message ?? GetDefaultBattleMessage();
            BattleResolutionStepIndex = -1;
            BattleResolutionStepFramesRemaining = 0;
            BattleReturnFlowState = BattleFlowState.CommandSelection;
            BattleIntroFramesRemaining = 0;
        }

        public void ResetBattleVisualEffects()
        {
            BattlePlayerActionFramesRemaining = 0;
            BattlePlayerGuardFramesRemaining = 0;
            BattleEnemyActionFramesRemaining = 0;
            BattleItemUseFramesRemaining = 0;
            BattleEnemyDefeatFramesRemaining = 0;
            EnemyHitFlashFramesRemaining = 0;
            BattleSpellEffectFramesRemaining = 0;
            PlayerHitFlashFramesRemaining = 0;
            BattlePlayerHealFramesRemaining = 0;
            BattleStatusEffectFramesRemaining = 0;
        }

        public void ResetFieldUiState()
        {
            IsFieldDialogOpen = false;
            IsFieldStatusVisible = false;
            MovementCooldown = 0;
            PlayerFacingDirection = PlayerFacingDirection.Down;
        }

        public void ResetEncounterCounter()
        {
            FieldEncounterStepsRemaining = 7 + Random.Next(5);
        }

        public void ResetShopState(string message = null)
        {
            ShopPhase = ShopPhase.Welcome;
            ShopPromptCursor = 0;
            ResetShopListSelection();
            ShopMessage = message ?? GetShopWelcomeMessage();
        }

        public void ResetBankState(string message = null)
        {
            BankPhase = BankPhase.Welcome;
            BankPromptCursor = 0;
            BankItemCursor = 0;
            BankMessage = message ?? GetBankWelcomeMessage();
        }

        public void ResetShopListSelection(int page = 0)
        {
            ShopItemCursor = 0;
            ShopPageIndex = Mathf.Clamp(page, 0, Math.Max(0, GetShopPageCount() - 1));
        }

        public void ApplyExplorationSession(PlayerProgress nextPlayer, FieldMapId mapId)
        {
            Player = nextPlayer;
            SetFieldMap(mapId);
            SyncPlayerNameBuffer(Player.Name);
            ResetFieldUiState();
            ResetBattleState();
            ResetShopState();
            ResetBankState();
        }

        public string GetDisplayPlayerName() =>
            string.IsNullOrWhiteSpace(Player.Name) ? (SelectedLanguage == UiLanguage.English ? "HERO" : "ゆうしゃ") : Player.Name;

        public int GetTotalAttack() => BattleService.GetPlayerAttack(Player, Player.EquippedWeapon);
        public int GetTotalDefense() => BattleService.GetPlayerDefense(Player, Player.EquippedArmor);
        public string GetExperienceSummary() => $"{Player.Experience}/{ProgressionService.GetExperienceForNextLevel(Player.Level)}";
        public string GetCurrentEquipmentNameForSlot(EquipmentSlot slot) => Player.GetEquippedItemName(slot) ?? (SelectedLanguage == UiLanguage.English ? "None" : "なし");

        public string GetEquipmentSlotLabel(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.Weapon => SelectedLanguage == UiLanguage.English ? "WPN" : "ぶき",
            EquipmentSlot.Head => SelectedLanguage == UiLanguage.English ? "HD" : "あたま",
            EquipmentSlot.Armor => SelectedLanguage == UiLanguage.English ? "ARM" : "よろい",
            EquipmentSlot.Arms => SelectedLanguage == UiLanguage.English ? "ARM" : "うで",
            EquipmentSlot.Legs => SelectedLanguage == UiLanguage.English ? "LEG" : "あし",
            EquipmentSlot.Feet => SelectedLanguage == UiLanguage.English ? "FT" : "くつ",
            _ => string.Empty
        };

        public string GetMapDisplayName(FieldMapId mapId, UiLanguage language) => mapId switch
        {
            FieldMapId.Hub => language == UiLanguage.English ? "Hub" : "拠点",
            FieldMapId.Castle => language == UiLanguage.English ? "Castle" : "城",
            FieldMapId.Dungeon => language == UiLanguage.English ? "Dungeon" : "ダンジョン",
            FieldMapId.Field => language == UiLanguage.English ? "Field" : "野外",
            _ => string.Empty
        };

        public string GetDefaultBattleMessage() => SelectedLanguage == UiLanguage.English ? "A monster appeared!" : "まものが あらわれた！";
        public string GetBattleEscapeMessage() => SelectedLanguage == UiLanguage.English ? "You got away safely!" : "うまく にげきった！";
        public string GetShopWelcomeMessage() => SelectedLanguage == UiLanguage.English ? "* \"Welcome!\n  What do you need?\"" : "＊「いらっしゃい！\n　なにを するんだい？」";
        public string GetShopBrowseMessage() => SelectedLanguage == UiLanguage.English ? "* \"What will you buy?\"" : "＊「なにを かっていくかい？」";
        public string GetShopSellBrowseMessage() => SelectedLanguage == UiLanguage.English ? "* \"What will you sell?\"" : "＊「なにを うっていくんだい？」";
        public string GetShopReturnMessage() => SelectedLanguage == UiLanguage.English ? "* \"Anything else?\"" : "＊「ほかに ようじは あるかい？」";
        public string GetShopFarewellMessage() => SelectedLanguage == UiLanguage.English ? "* \"Come again!\"" : "＊「また きてくれよな！」";
        public string GetBankWelcomeMessage() => SelectedLanguage == UiLanguage.English ? "* \"Welcome to the bank.\n  How can I help?\"" : "＊「ぎんこうへ ようこそ。\n　ごようけんは？」";
        public string GetBankDepositMessage() => SelectedLanguage == UiLanguage.English ? "* \"How much will you deposit?\n  Loans are repaid first.\"" : "＊「いくら あずける？\n　しゃっきんは さきに へんさいするよ。」";
        public string GetBankWithdrawMessage() => SelectedLanguage == UiLanguage.English ? "* \"How much will you withdraw?\"" : "＊「いくら ひきだす？」";
        public string GetBankBorrowMessage() => SelectedLanguage == UiLanguage.English ? "* \"How much will you borrow?\n  Watch the interest.\"" : "＊「いくら かりる？\n　りそくには きをつけな。」";
        public string GetBankReturnMessage() => SelectedLanguage == UiLanguage.English ? "* \"Anything else?\"" : "＊「ほかに ようじは あるかい？」";

        public int GetBattleCommandRowCount() => GameContent.BattleCommandGrid.GetLength(0);
        public int GetBattleCommandColumnCount() => GameContent.BattleCommandGrid.GetLength(1);

        public string GetBattleCommandLabel(int row, int column)
        {
            var action = GameContent.BattleCommandGrid[row, column];
            return GameContent.GetBattleActionName(action, SelectedLanguage);
        }

        public string GetBattleCommandHelpMessage() => SelectedLanguage == UiLanguage.English ? "Choose a command." : "こうどうを えらんでください。";
        public string GetBattleSubmenuHelpMessage() => SelectedLanguage == UiLanguage.English ? "Choose an item." : "どれを つかいますか？";

        public string GetBattleSelectionTitle() => BattleFlowState switch
        {
            BattleFlowState.SpellSelection => SelectedLanguage == UiLanguage.English ? "SPELL" : "まほう",
            BattleFlowState.ItemSelection => SelectedLanguage == UiLanguage.English ? "ITEM" : "どうぐ",
            BattleFlowState.EquipmentSelection => SelectedLanguage == UiLanguage.English ? "EQUIP" : "そうび",
            _ => string.Empty
        };

        public string GetBattleSelectionCounterText()
        {
            var entries = GetActiveBattleSelectionEntries();
            return $"{Mathf.Min(BattleListCursor + 1, entries.Count)}/{entries.Count}";
        }

        public string GetBattleEmptySelectionMessage(BattleFlowState state) => state switch
        {
            BattleFlowState.SpellSelection => SelectedLanguage == UiLanguage.English ? "No spells." : "まほうを おぼえていない。",
            BattleFlowState.ItemSelection => SelectedLanguage == UiLanguage.English ? "No items." : "どうぐを もっていない。",
            BattleFlowState.EquipmentSelection => SelectedLanguage == UiLanguage.English ? "No equipment." : "そうびが ない。",
            _ => string.Empty
        };

        public string GetBattleSelectionMessage(BattleFlowState state) => state switch
        {
            BattleFlowState.SpellSelection => SelectedLanguage == UiLanguage.English ? "Which spell?" : "どのまほう？",
            BattleFlowState.ItemSelection => SelectedLanguage == UiLanguage.English ? "Which item?" : "どのどうぐ？",
            BattleFlowState.EquipmentSelection => SelectedLanguage == UiLanguage.English ? "Equip what?" : "なにを そうびする？",
            _ => string.Empty
        };

        public string GetBattleEncounterMessage(string enemyName) => SelectedLanguage == UiLanguage.English ? $"{enemyName} appeared!" : $"{enemyName}が あらわれた！";
        public string GetBattleOpeningCommandMessage() => SelectedLanguage == UiLanguage.English ? "Command?" : "こうどう？";

        public IReadOnlyList<BattleSelectionEntry> GetActiveBattleSelectionEntries()
        {
            return BattleFlowState switch
            {
                BattleFlowState.SpellSelection => Player.Spells.Select(s => new BattleSelectionEntry { Label = s.Name, Detail = $"MP{s.MpCost}", Spell = s }).ToList(),
                BattleFlowState.ItemSelection => Player.Inventory.Select(i => new BattleSelectionEntry { Label = GameContent.GetItemName(i.ItemId, SelectedLanguage), Detail = $"x{i.Quantity}", Consumable = GameContent.GetConsumableById(i.ItemId) }).Where(e => e.Consumable is not null).ToList(),
                BattleFlowState.EquipmentSelection => Player.Inventory.Select(i => new BattleSelectionEntry { Label = GameContent.GetItemName(i.ItemId, SelectedLanguage), Detail = string.Empty, Equipment = (IEquipmentDefinition?)GameContent.GetWeaponById(i.ItemId) ?? GameContent.GetArmorById(i.ItemId) }).Where(e => e.Equipment is not null).ToList(),
                _ => Array.Empty<BattleSelectionEntry>()
            };
        }

        public IReadOnlyList<ShopMenuEntry> GetShopVisibleEntries()
        {
            var entries = new List<ShopMenuEntry>();
            var products = ShopService.GetProductsForField(CurrentFieldMap);
            var inventory = Player.Inventory;

            if (ShopPhase == ShopPhase.SellList)
            {
                foreach (var item in inventory)
                {
                    entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.InventoryItem, InventoryItem = item });
                }
            }
            else
            {
                var pageProducts = products.Skip(ShopPageIndex * GameConstants.ShopItemsPerPage).Take(GameConstants.ShopItemsPerPage);
                foreach (var product in pageProducts)
                {
                    entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.Product, Product = product });
                }
            }

            if (ShopPageIndex > 0)
                entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.PreviousPage });
            if (ShopPageIndex < GetShopPageCount() - 1)
                entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.NextPage });
            entries.Add(new ShopMenuEntry { Type = ShopMenuEntryType.Quit });

            return entries;
        }

        public int GetShopPageCount()
        {
            var products = ShopService.GetProductsForField(CurrentFieldMap);
            return Mathf.Max(1, Mathf.CeilToInt(products.Count / (float)GameConstants.ShopItemsPerPage));
        }

        public IReadOnlyList<BankOption> GetBankAmountOptions()
        {
            return new List<BankOption>
            {
                new() { Label = SelectedLanguage == UiLanguage.English ? "100G" : "100G", Amount = 100 },
                new() { Label = SelectedLanguage == UiLanguage.English ? "1000G" : "1000G", Amount = 1000 },
                new() { Label = SelectedLanguage == UiLanguage.English ? "10000G" : "10000G", Amount = 10000 },
                new() { Label = SelectedLanguage == UiLanguage.English ? "All" : "ぜんぶ", Amount = -1 },
                new() { Label = SelectedLanguage == UiLanguage.English ? "Back" : "もどる", Quit = true }
            };
        }

        public int ResolveBankTransactionAmount(BankOption option)
        {
            if (option.Amount > 0)
                return option.Amount;
            return BankPhase switch
            {
                BankPhase.DepositList => Player.Gold,
                BankPhase.WithdrawList => Player.BankGold,
                BankPhase.BorrowList => BankService.GetAvailableCredit(Player),
                _ => 0
            };
        }

        public void UpdateMenuNotice()
        {
            if (MenuNoticeFrames > 0)
            {
                MenuNoticeFrames--;
                if (MenuNoticeFrames <= 0)
                    MenuNotice = string.Empty;
            }
        }
    }
}
