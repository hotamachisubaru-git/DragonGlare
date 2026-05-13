using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain.Player;
using System.Collections.Generic;
using System.Linq;

namespace DragonGlare
{
    public class FieldScene : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup tileGrid;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Image heroImage;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private Text statusName;
        [SerializeField] private Text statusHp;
        [SerializeField] private Text statusMp;
        [SerializeField] private Text statusGold;
        [SerializeField] private Text statusAtkDef;
        [SerializeField] private Text statusExp;
        [SerializeField] private Text[] equipmentSlots;
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text dialogText;
        [SerializeField] private Text dialogFooter;

        private Image[,] tileImages;
        private int[,] currentMap;

        public void Show(PlayerProgress player, BattleEncounter encounter, bool statusVisible, bool dialogOpen,
            IReadOnlyList<string> dialogPages, int dialogPageIndex, string portraitAssetName,
            UiLanguage language, PlayerFacingDirection facing, int animFrames, Vector2Int animDir)
        {
            gameObject.SetActive(true);
            statusPanel.SetActive(statusVisible);
            dialogPanel.SetActive(dialogOpen);

            if (statusVisible)
            {
                statusName.text = $"{(string.IsNullOrWhiteSpace(player.Name) ? (language == UiLanguage.English ? "HERO" : "ゆうしゃ") : player.Name)}  Lv.{player.Level}";
                statusHp.text = $"HP {player.CurrentHp}/{player.MaxHp}";
                statusMp.text = $"MP {player.CurrentMp}/{player.MaxMp}";
                statusGold.text = $"G {player.Gold}";
                statusAtkDef.text = $"ATK {GetTotalAttack(player)}  DEF {GetTotalDefense(player)}";
                statusExp.text = $"EXP {player.Experience}";
                UpdateEquipmentSlots(player, language);
            }

            if (dialogOpen)
            {
                portraitImage.gameObject.SetActive(!string.IsNullOrWhiteSpace(portraitAssetName));
                if (!string.IsNullOrWhiteSpace(portraitAssetName))
                {
                    portraitImage.sprite = GameManager.Instance.Sprites.GetNpcPortrait(portraitAssetName);
                }
                dialogText.text = dialogPages.Count > dialogPageIndex ? dialogPages[dialogPageIndex] : string.Empty;
                dialogFooter.text = dialogPageIndex < dialogPages.Count - 1
                    ? (language == UiLanguage.Japanese ? "A/Y/Z: つぎへ" : "A/Y/Z: NEXT")
                    : (language == UiLanguage.Japanese ? "A/Y/Z / B/ESC: とじる" : "A/Y/Z / B/ESC: CLOSE");
            }

            UpdateFieldView(player, facing, animFrames, animDir);
        }

        private void UpdateFieldView(PlayerProgress player, PlayerFacingDirection facing, int animFrames, Vector2Int animDir)
        {
            heroImage.sprite = GameManager.Instance.Sprites.GetHeroSprite(facing);
            var map = MapFactory.GetMapForCurrentField();
            if (currentMap != map)
            {
                currentMap = map;
                RebuildTileGrid(map);
            }

            var cameraOrigin = GetCameraOrigin(player.TilePosition, map);
            int vw = GetViewportWidthTiles();
            int vh = GetViewportHeightTiles();

            for (int y = 0; y < vh; y++)
            {
                for (int x = 0; x < vw; x++)
                {
                    int mx = cameraOrigin.x + x;
                    int my = cameraOrigin.y + y;
                    if (my >= 0 && my < map.GetLength(0) && mx >= 0 && mx < map.GetLength(1))
                    {
                        tileImages[y, x].color = GetTileColor(map[my, mx]);
                    }
                    else
                    {
                        tileImages[y, x].color = Color.black;
                    }
                }
            }

            float offsetX = animDir.x * (1f - animFrames / (float)GameConstants.FieldMovementAnimationDuration) * GameConstants.TileSize;
            float offsetY = animDir.y * (1f - animFrames / (float)GameConstants.FieldMovementAnimationDuration) * GameConstants.TileSize;
            var playerLocal = new Vector2(
                (player.TilePosition.x - cameraOrigin.x) * GameConstants.TileSize + offsetX,
                (player.TilePosition.y - cameraOrigin.y) * GameConstants.TileSize + offsetY);
            heroImage.rectTransform.anchoredPosition = playerLocal;
        }

        private void RebuildTileGrid(int[,] map)
        {
            if (tileImages != null)
            {
                foreach (var img in tileImages)
                {
                    if (img != null) Destroy(img.gameObject);
                }
            }

            int vw = GetViewportWidthTiles();
            int vh = GetViewportHeightTiles();
            tileImages = new Image[vh, vw];
            tileGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            tileGrid.constraintCount = vw;
            tileGrid.cellSize = new Vector2(GameConstants.TileSize, GameConstants.TileSize);

            for (int y = 0; y < vh; y++)
            {
                for (int x = 0; x < vw; x++)
                {
                    var go = Instantiate(tilePrefab, tileGrid.transform);
                    tileImages[y, x] = go.GetComponent<Image>();
                }
            }
        }

        private static Vector2Int GetCameraOrigin(Vector2Int playerPos, int[,] map)
        {
            int vw = GetViewportWidthTiles();
            int vh = GetViewportHeightTiles();
            int mw = map.GetLength(1);
            int mh = map.GetLength(0);
            int cx = Mathf.Clamp(playerPos.x - vw / 2, 0, Mathf.Max(0, mw - vw));
            int cy = Mathf.Clamp(playerPos.y - vh / 2, 0, Mathf.Max(0, mh - vh));
            return new Vector2Int(cx, cy);
        }

        private static int GetViewportWidthTiles() => GameConstants.CompactFieldViewportWidthTiles;
        private static int GetViewportHeightTiles() => GameConstants.CompactFieldViewportHeightTiles;

        private static Color GetTileColor(int tileId)
        {
            return tileId switch
            {
                1 => new Color(8f / 255f, 30f / 255f, 90f / 255f),
                2 => new Color(24f / 255f, 56f / 255f, 40f / 255f),
                3 => new Color(24f / 255f, 74f / 255f, 36f / 255f),
                4 => new Color(120f / 255f, 28f / 255f, 38f / 255f),
                5 => new Color(116f / 255f, 58f / 255f, 30f / 255f),
                6 => new Color(108f / 255f, 42f / 255f, 52f / 255f),
                _ => new Color(5f / 255f, 5f / 255f, 5f / 255f)
            };
        }

        private void UpdateEquipmentSlots(PlayerProgress player, UiLanguage language)
        {
            var labels = language == UiLanguage.English
                ? new[] { "WPN", "HD", "ARM", "ARM", "LEG", "FT" }
                : new[] { "ぶき", "あたま", "よろい", "うで", "あし", "くつ" };
            var slots = new[] { EquipmentSlot.Weapon, EquipmentSlot.Head, EquipmentSlot.Armor, EquipmentSlot.Arms, EquipmentSlot.Legs, EquipmentSlot.Feet };
            for (int i = 0; i < equipmentSlots.Length && i < slots.Length; i++)
            {
                var name = player.GetEquippedItemName(slots[i]) ?? (language == UiLanguage.English ? "None" : "なし");
                equipmentSlots[i].text = $"{labels[i]}: {name}";
            }
        }

        private static int GetTotalAttack(PlayerProgress player)
        {
            var battleService = new DragonGlare.Services.BattleService();
            return battleService.GetPlayerAttack(player, player.EquippedWeapon);
        }

        private static int GetTotalDefense(PlayerProgress player)
        {
            var battleService = new DragonGlare.Services.BattleService();
            return battleService.GetPlayerDefense(player, player.EquippedArmor);
        }
    }
}
