using UnityEngine;
using UnityEngine.UI;
using DragonGlare.Domain.Battle;
using DragonGlare.Domain.Player;
using System.Collections.Generic;
using System.Linq;

namespace DragonGlare
{
    public class BattleScene : MonoBehaviour
    {
        [SerializeField] private Image enemyImage;
        [SerializeField] private RectTransform enemyTransform;
        [SerializeField] private Image backdropImage;
        [SerializeField] private GameObject commandPanel;
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private Text commandTitle;
        [SerializeField] private Transform commandGrid;
        [SerializeField] private GameObject commandCellPrefab;
        [SerializeField] private Text statusName;
        [SerializeField] private Text statusHp;
        [SerializeField] private Text statusMp;
        [SerializeField] private Text statusEx;
        [SerializeField] private Text statusStatus;
        [SerializeField] private Text messageText;
        [SerializeField] private Text enemyNameText;
        [SerializeField] private Text enemyHpText;
        [SerializeField] private RectTransform selectionCursor;
        [SerializeField] private Image playerHitFlash;
        [SerializeField] private Image spellEffect;
        [SerializeField] private Image healEffect;
        [SerializeField] private Image guardEffect;
        [SerializeField] private Image itemEffect;
        [SerializeField] private Image enemyDefeatEffect;
        [SerializeField] private Image slashEffect;
        [SerializeField] private Image statusCloudEffect;

        private Text[,] commandCells;
        private int commandRows;
        private int commandCols;

        public void Show(PlayerProgress player, BattleEncounter encounter, BattleFlowState flowState,
            int cursorRow, int cursorColumn, int listCursor, int listScroll, string message,
            IReadOnlyList<BattleSequenceStep> resolutionSteps, int resolutionStepIndex,
            UiLanguage language, int frameCounter,
            int playerActionFrames, int guardFrames, int enemyActionFrames, int itemFrames,
            int defeatFrames, int enemyHitFrames, int spellFrames, int playerHitFrames,
            int healFrames, int statusFrames, int introFrames)
        {
            gameObject.SetActive(true);
            UpdateEnemyDisplay(encounter, enemyHitFrames, enemyActionFrames, defeatFrames, frameCounter);
            UpdateVisualEffects(playerActionFrames, spellFrames, statusFrames, healFrames, guardFrames, itemFrames, defeatFrames, playerHitFrames, frameCounter);
            UpdateTopUI(player, encounter, flowState, cursorRow, cursorColumn, listCursor, listScroll, language);
            UpdateMessageWindow(message, flowState, language);
        }

        private void UpdateEnemyDisplay(BattleEncounter encounter, int hitFlashFrames, int actionFrames, int defeatFrames, int frameCounter)
        {
            if (encounter == null || (encounter.CurrentHp <= 0 && defeatFrames <= 0))
            {
                enemyImage.gameObject.SetActive(false);
                return;
            }

            bool showEnemy = hitFlashFrames <= 0 || ((hitFlashFrames - 1) / 2) % 2 == 0;
            enemyImage.gameObject.SetActive(showEnemy);
            if (!showEnemy) return;

            var sprite = GameManager.Instance.Sprites.GetEnemySprite(encounter.Enemy.SpriteAssetName);
            enemyImage.sprite = sprite;

            var center = new Vector2(320, 266);
            if (actionFrames > 0)
            {
                var pulse = Mathf.Sin((actionFrames / 10f) * Mathf.PI);
                center.y += Mathf.RoundToInt(pulse * 18f);
            }
            if (hitFlashFrames > 0)
            {
                var shake = hitFlashFrames % 4 < 2 ? -5 : 5;
                center.x += shake;
            }
            if (defeatFrames > 0)
            {
                var sink = Mathf.Max(0, 16 - defeatFrames);
                center.y += sink;
            }

            var bob = Mathf.RoundToInt(Mathf.Sin(frameCounter / 7f) * 3f);
            center.y += bob;
            enemyTransform.anchoredPosition = center;
        }

        private void UpdateVisualEffects(int playerAction, int spell, int status, int heal, int guard, int item, int defeat, int playerHit, int frameCounter)
        {
            slashEffect.gameObject.SetActive(playerAction > 0);
            if (playerAction > 0)
            {
                var progress = 1f - Mathf.Clamp01(playerAction / 8f);
                var alpha = Mathf.Clamp(220 - Mathf.RoundToInt(progress * 140f), 70, 220);
                slashEffect.color = new Color(246f / 255f, 244f / 255f, 228f / 255f, alpha / 255f);
            }

            spellEffect.gameObject.SetActive(spell > 0);
            if (spell > 0)
            {
                var pulse = 1f - Mathf.Clamp01(spell / 16f);
                var radius = 18 + Mathf.RoundToInt(pulse * 58f);
                spellEffect.rectTransform.sizeDelta = new Vector2(radius * 2, radius * 2);
            }

            statusCloudEffect.gameObject.SetActive(status > 0);
            healEffect.gameObject.SetActive(heal > 0);
            guardEffect.gameObject.SetActive(guard > 0);
            itemEffect.gameObject.SetActive(item > 0);
            enemyDefeatEffect.gameObject.SetActive(defeat > 0);
            playerHitFlash.gameObject.SetActive(playerHit > 0);
            if (playerHit > 0)
            {
                var alpha = Mathf.Clamp(playerHit * 14, 20, 120);
                playerHitFlash.color = new Color(180f / 255f, 24f / 255f, 38f / 255f, alpha / 255f);
            }
        }

        private void UpdateTopUI(PlayerProgress player, BattleEncounter encounter, BattleFlowState flowState,
            int cursorRow, int cursorColumn, int listCursor, int listScroll, UiLanguage language)
        {
            commandPanel.SetActive(flowState == BattleFlowState.CommandSelection ||
                flowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection);
            statusPanel.SetActive(true);

            var classLabel = language == UiLanguage.English ? "HERO" : "ゆうしゃ";
            statusName.text = $"{(string.IsNullOrWhiteSpace(player.Name) ? classLabel : player.Name)}  {classLabel}";
            statusHp.text = $"HP:{player.CurrentHp}";
            statusMp.text = $"MP:{player.CurrentMp}";
            statusEx.text = $"EX:{player.Experience}";

            if (encounter != null)
            {
                enemyNameText.text = GameContent.GetEnemyName(encounter.Enemy, language);
                enemyHpText.text = $"HP {encounter.CurrentHp}/{encounter.Enemy.MaxHp}";
                statusStatus.text = encounter.PlayerStatusEffect != BattleStatusEffect.None
                    ? GetStatusLabel(encounter.PlayerStatusEffect, language)
                    : string.Empty;
            }
            else
            {
                enemyNameText.text = language == UiLanguage.English ? "MONSTER" : "まもの";
                enemyHpText.text = string.Empty;
                statusStatus.text = string.Empty;
            }

            if (flowState == BattleFlowState.CommandSelection)
            {
                EnsureCommandGrid();
                for (int r = 0; r < commandRows; r++)
                {
                    for (int c = 0; c < commandCols; c++)
                    {
                        var action = GameContent.BattleCommandGrid[r, c];
                        commandCells[r, c].text = GameContent.GetBattleActionName(action, language);
                    }
                }
                selectionCursor.position = commandCells[cursorRow, cursorColumn].rectTransform.position;
            }
            else if (flowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
            {
                commandTitle.text = flowState switch
                {
                    BattleFlowState.SpellSelection => language == UiLanguage.English ? "SPELL" : "まほう",
                    BattleFlowState.ItemSelection => language == UiLanguage.English ? "ITEM" : "どうぐ",
                    BattleFlowState.EquipmentSelection => language == UiLanguage.English ? "EQUIP" : "そうび",
                    _ => string.Empty
                };
            }
        }

        private void UpdateMessageWindow(string message, BattleFlowState flowState, UiLanguage language)
        {
            messageText.text = message;
        }

        private void EnsureCommandGrid()
        {
            if (commandCells != null) return;
            commandRows = GameContent.BattleCommandGrid.GetLength(0);
            commandCols = GameContent.BattleCommandGrid.GetLength(1);
            commandCells = new Text[commandRows, commandCols];
            for (int r = 0; r < commandRows; r++)
            {
                for (int c = 0; c < commandCols; c++)
                {
                    var go = Instantiate(commandCellPrefab, commandGrid);
                    commandCells[r, c] = go.GetComponent<Text>();
                }
            }
        }

        private static string GetStatusLabel(BattleStatusEffect effect, UiLanguage language)
        {
            return effect switch
            {
                BattleStatusEffect.Poison => language == UiLanguage.English ? "POISON" : "どく",
                BattleStatusEffect.Sleep => language == UiLanguage.English ? "SLEEP" : "ねむり",
                _ => string.Empty
            };
        }
    }
}
