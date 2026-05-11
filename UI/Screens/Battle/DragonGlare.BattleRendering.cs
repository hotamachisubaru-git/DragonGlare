using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private static readonly Color BattleTopWindowBackgroundColor = Color.FromArgb(35, 33, 50);
    private static readonly Color BattleMessageWindowFallbackColor = Color.FromArgb(10, 28, 36);
    private static readonly Color BattleMessageWindowTintColor = Color.FromArgb(150, 8, 24, 34);
    private static readonly Rectangle BattleMessageWindowBackdropSourceRect = new(0, 160, 256, 64);

    private void DrawBattle(Graphics g)
    {
        DrawBattleBackdrop(g);

        var enemyCenter = GetBattleEnemyCenter();
        if (ShouldDrawBattleEnemySprite())
        {
            DrawBattleEnemy(g, enemyCenter);
        }

        DrawBattleVisualEffects(g, enemyCenter);
        DrawBattleTopUi(g);
        DrawBattleMessageWindow(g);
    }

    private void DrawBattleTopUi(Graphics g)
    {
        var commandWindowRect = new Rectangle(2, 42, 272, 140);
        var statusWindowRect = new Rectangle(278, 42, 360, 140);

        DrawBattleFrameWindow(g, commandWindowRect, BattleTopWindowBackgroundColor);
        DrawBattleFrameWindow(g, statusWindowRect, BattleTopWindowBackgroundColor);

        if (battleFlowState == BattleFlowState.Intro)
        {
            return;
        }

        if (battleFlowState == BattleFlowState.CommandSelection)
        {
            DrawBattleCommandWindow(g, Rectangle.Inflate(commandWindowRect, -18, -16));
        }
        else if (battleFlowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
        {
            DrawBattleSelectionPane(g, Rectangle.Inflate(commandWindowRect, -16, -14));
        }

        DrawBattleStatusWindow(g, Rectangle.Inflate(statusWindowRect, -18, -16));
    }

    private void DrawBattleMessageWindow(Graphics g)
    {
        var messageWindowRect = new Rectangle(2, 326, 636, 146);
        DrawBattleMessageWindowBackground(g, messageWindowRect);
        DrawBattleWindowBorder(g, messageWindowRect);
        DrawBattleMessagePane(g, Rectangle.Inflate(messageWindowRect, -20, -16), string.Empty);
    }

    private void DrawEncounterTransition(Graphics g)
    {
        DrawFieldScene(g);

        var progress = 1f - (encounterTransitionFrames / (float)EncounterTransitionDuration);
        var stripeProgress = Math.Clamp((progress - 0.08f) / 0.68f, 0f, 1f);
        var finalFlash = Math.Clamp((progress - 0.72f) / 0.28f, 0f, 1f);
        var flashPulse = progress <= 0.36f
            ? 1f - Math.Abs((progress / 0.36f * 2f) - 1f)
            : 0f;

        if (flashPulse > 0f)
        {
            using var pulseBrush = new SolidBrush(Color.FromArgb((int)(flashPulse * 170f), Color.White));
            g.FillRectangle(pulseBrush, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
        }

        const int stripeCount = 12;
        var stripeHeight = (int)Math.Ceiling(UiCanvas.VirtualHeight / (float)stripeCount);
        var filledHeight = Math.Max(1, (int)Math.Ceiling(stripeHeight * stripeProgress));
        using var stripeBrush = new SolidBrush(Color.FromArgb(240, 252, 252, 255));
        for (var index = 0; index < stripeCount; index++)
        {
            var y = index * stripeHeight;
            var inset = (int)((index % 2 == 0 ? 26f : 12f) * (1f - stripeProgress));
            g.FillRectangle(stripeBrush, inset, y, UiCanvas.VirtualWidth - (inset * 2), filledHeight);
        }

        if (pendingEncounter is not null && progress >= 0.36f)
        {
            var messageRect = new Rectangle(124, 386, 392, 42);
            using var shadowBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            g.FillRectangle(shadowBrush, messageRect.X + 4, messageRect.Y + 4, messageRect.Width, messageRect.Height);
            DrawWindow(g, messageRect);
            DrawText(
                g,
                selectedLanguage == UiLanguage.English
                    ? $"{GameContent.GetEnemyName(pendingEncounter.Enemy, selectedLanguage)} is near..."
                    : $"{GameContent.GetEnemyName(pendingEncounter.Enemy, selectedLanguage)}の けはいがする…",
                Rectangle.Inflate(messageRect, -18, -10),
                smallFont,
                StringAlignment.Center,
                StringAlignment.Center);
        }

        if (finalFlash > 0f)
        {
            using var flashBrush = new SolidBrush(Color.FromArgb((int)(finalFlash * 255f), Color.White));
            g.FillRectangle(flashBrush, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
        }
    }

    private Point GetBattleEnemyCenter()
    {
        var center = new Point(320, 266);
        if (battleEnemyActionFramesRemaining > 0)
        {
            var pulse = Math.Sin((battleEnemyActionFramesRemaining / 10f) * Math.PI);
            center = new Point(center.X, center.Y + (int)Math.Round(pulse * 18d));
        }

        if (enemyHitFlashFramesRemaining > 0)
        {
            var shake = enemyHitFlashFramesRemaining % 4 < 2 ? -5 : 5;
            center = new Point(center.X + shake, center.Y);
        }

        if (battleEnemyDefeatFramesRemaining > 0)
        {
            var sink = Math.Max(0, 16 - battleEnemyDefeatFramesRemaining);
            center = new Point(center.X, center.Y + sink);
        }

        return center;
    }

    private void DrawBattleEnemy(Graphics g, Point center)
    {
        var bobOffset = (int)Math.Round(Math.Sin(frameCounter / 7d) * 3);
        center = new Point(center.X, center.Y + bobOffset);

        var enemySprite = GetEnemySprite(currentEncounter?.Enemy.SpriteAssetName);
        if (enemySprite is not null)
        {
            DrawBattleEnemySprite(g, enemySprite, center);
            return;
        }

        if (string.Equals(currentEncounter?.Enemy.Id, "moss_toad", StringComparison.Ordinal))
        {
            DrawMossToadEnemy(g, center);
            return;
        }

        DrawDefaultBattleEnemy(g, center);
    }

    private bool ShouldDrawBattleEnemySprite()
    {
        if (currentEncounter is null)
        {
            return false;
        }

        if (currentEncounter.CurrentHp <= 0 && !ShouldKeepDefeatedEnemyVisible())
        {
            return false;
        }

        if (enemyHitFlashFramesRemaining <= 0)
        {
            return true;
        }

        return ((enemyHitFlashFramesRemaining - 1) / 2) % 2 == 0;
    }

    private bool ShouldKeepDefeatedEnemyVisible()
    {
        if (battleEnemyDefeatFramesRemaining > 0)
        {
            return true;
        }

        if (battleFlowState != BattleFlowState.Resolving || battleResolutionSteps.Count == 0)
        {
            return false;
        }

        var startIndex = Math.Clamp(battleResolutionStepIndex, 0, battleResolutionSteps.Count - 1);
        return battleResolutionSteps
            .Skip(startIndex)
            .Any(step => step.VisualCue == BattleVisualCue.EnemyDefeat);
    }

    private void DrawBattleVisualEffects(Graphics g, Point enemyCenter)
    {
        if (battlePlayerActionFramesRemaining > 0)
        {
            DrawBattlePlayerActionEffect(g, enemyCenter, battlePlayerActionFramesRemaining);
        }

        if (battleSpellEffectFramesRemaining > 0)
        {
            DrawBattleSpellBurst(g, new Point(enemyCenter.X, enemyCenter.Y - 24), battleSpellEffectFramesRemaining);
        }

        if (battleStatusEffectFramesRemaining > 0)
        {
            DrawBattleStatusCloud(g, new Point(enemyCenter.X, enemyCenter.Y - 16), battleStatusEffectFramesRemaining);
        }

        if (battlePlayerHealFramesRemaining > 0)
        {
            DrawBattlePlayerHealEffect(g, battlePlayerHealFramesRemaining);
        }

        if (battlePlayerGuardFramesRemaining > 0)
        {
            DrawBattlePlayerGuardEffect(g, battlePlayerGuardFramesRemaining);
        }

        if (battleItemUseFramesRemaining > 0)
        {
            DrawBattleItemUseEffect(g, battleItemUseFramesRemaining);
        }

        if (battleEnemyDefeatFramesRemaining > 0)
        {
            DrawBattleEnemyDefeatEffect(g, enemyCenter, battleEnemyDefeatFramesRemaining);
        }

        if (playerHitFlashFramesRemaining > 0)
        {
            var alpha = Math.Clamp(playerHitFlashFramesRemaining * 14, 20, 120);
            using var flashBrush = new SolidBrush(Color.FromArgb(alpha, 180, 24, 38));
            g.FillRectangle(flashBrush, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
        }
    }

    private void DrawBattlePlayerActionEffect(Graphics g, Point enemyCenter, int framesRemaining)
    {
        var progress = 1f - Math.Clamp(framesRemaining / 8f, 0f, 1f);
        var alpha = Math.Clamp(220 - (int)Math.Round(progress * 140f), 70, 220);
        using var slashPen = new Pen(Color.FromArgb(alpha, 246, 244, 228), 4);
        using var accentPen = new Pen(Color.FromArgb(Math.Max(40, alpha - 80), 86, 180, 255), 2);

        var offset = (int)Math.Round(progress * 34f);
        g.DrawLine(slashPen, enemyCenter.X - 74 + offset, enemyCenter.Y - 78, enemyCenter.X + 18 + offset, enemyCenter.Y + 28);
        g.DrawLine(accentPen, enemyCenter.X - 54 + offset, enemyCenter.Y - 70, enemyCenter.X + 38 + offset, enemyCenter.Y + 18);
    }

    private void DrawBattleSpellBurst(Graphics g, Point center, int framesRemaining)
    {
        var pulse = 1f - Math.Clamp(framesRemaining / 16f, 0f, 1f);
        var radius = 18 + (int)Math.Round(pulse * 58f);
        using var ringPen = new Pen(Color.FromArgb(190, 255, 232, 102), 3);
        using var sparkBrush = new SolidBrush(Color.FromArgb(220, 255, 246, 170));

        g.DrawEllipse(ringPen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
        for (var index = 0; index < 10; index++)
        {
            var angle = ((frameCounter + (index * 36)) % 360) * Math.PI / 180d;
            var x = center.X + (int)Math.Round(Math.Cos(angle) * radius);
            var y = center.Y + (int)Math.Round(Math.Sin(angle) * Math.Max(12, radius / 2f));
            g.FillRectangle(sparkBrush, x - 2, y - 2, 4, 4);
        }
    }

    private void DrawBattleStatusCloud(Graphics g, Point center, int framesRemaining)
    {
        var status = currentEncounter?.EnemyStatusEffect ?? BattleStatusEffect.None;
        var baseColor = status switch
        {
            BattleStatusEffect.Poison => Color.FromArgb(90, 120, 232, 96),
            BattleStatusEffect.Sleep => Color.FromArgb(88, 116, 178, 255),
            _ => Color.FromArgb(82, 200, 120, 255)
        };

        using var cloudBrush = new SolidBrush(baseColor);
        for (var index = 0; index < 7; index++)
        {
            var wobble = Math.Sin((frameCounter + (index * 11)) / 5d);
            var x = center.X - 58 + (index * 18);
            var y = center.Y - 28 + (int)Math.Round(wobble * 8d) - ((16 - framesRemaining) / 2);
            g.FillEllipse(cloudBrush, x, y, 26, 18);
        }

        if (status == BattleStatusEffect.Sleep)
        {
            DrawText(g, "Z", center.X + 52, center.Y - 66, smallFont);
            DrawText(g, "Z", center.X + 72, center.Y - 88, smallFont);
        }
    }

    private static void DrawBattlePlayerHealEffect(Graphics g, int framesRemaining)
    {
        var alpha = Math.Clamp(framesRemaining * 12, 28, 150);
        using var healBrush = new SolidBrush(Color.FromArgb(alpha, 98, 244, 150));
        for (var index = 0; index < 5; index++)
        {
            var x = 308 + (index * 44);
            var y = 58 + ((index % 2) * 20);
            g.FillRectangle(healBrush, x + 6, y, 6, 22);
            g.FillRectangle(healBrush, x - 2, y + 8, 22, 6);
        }
    }

    private static void DrawBattlePlayerGuardEffect(Graphics g, int framesRemaining)
    {
        var alpha = Math.Clamp(framesRemaining * 16, 50, 170);
        using var guardPen = new Pen(Color.FromArgb(alpha, 154, 226, 255), 3);
        using var guardBrush = new SolidBrush(Color.FromArgb(alpha / 4, 154, 226, 255));
        var shieldRect = new Rectangle(286, 54, 72, 94);
        g.FillPie(guardBrush, shieldRect, 20, 140);
        g.DrawArc(guardPen, shieldRect, 20, 140);
        g.DrawLine(guardPen, shieldRect.X + 36, shieldRect.Y + 8, shieldRect.X + 36, shieldRect.Bottom - 10);
    }

    private void DrawBattleItemUseEffect(Graphics g, int framesRemaining)
    {
        var alpha = Math.Clamp(framesRemaining * 18, 40, 190);
        using var sparkleBrush = new SolidBrush(Color.FromArgb(alpha, 255, 238, 144));
        for (var index = 0; index < 6; index++)
        {
            var angle = ((frameCounter * 8) + (index * 60)) * Math.PI / 180d;
            var radius = 20 + ((14 - framesRemaining) * 2);
            var x = 318 + (int)Math.Round(Math.Cos(angle) * radius);
            var y = 92 + (int)Math.Round(Math.Sin(angle) * 12d);
            g.FillRectangle(sparkleBrush, x - 2, y - 2, 4, 4);
            g.FillRectangle(sparkleBrush, x - 5, y, 10, 1);
            g.FillRectangle(sparkleBrush, x, y - 5, 1, 10);
        }
    }

    private void DrawBattleEnemyDefeatEffect(Graphics g, Point center, int framesRemaining)
    {
        var alpha = Math.Clamp(framesRemaining * 14, 36, 180);
        using var dustBrush = new SolidBrush(Color.FromArgb(alpha, 210, 214, 228));
        for (var index = 0; index < 10; index++)
        {
            var angle = ((frameCounter * 5) + (index * 36)) * Math.PI / 180d;
            var radius = 24 + ((16 - framesRemaining) * 4) + (index % 3 * 6);
            var x = center.X + (int)Math.Round(Math.Cos(angle) * radius);
            var y = center.Y + 22 + (int)Math.Round(Math.Sin(angle) * Math.Max(10, radius / 3f));
            g.FillEllipse(dustBrush, x - 3, y - 3, 6, 6);
        }
    }

    private string GetBattleStatusEffectLabel(BattleStatusEffect statusEffect)
    {
        return statusEffect switch
        {
            BattleStatusEffect.Poison => selectedLanguage == UiLanguage.English ? "POISON" : "どく",
            BattleStatusEffect.Sleep => selectedLanguage == UiLanguage.English ? "SLEEP" : "ねむり",
            _ => string.Empty
        };
    }

    private void DrawBattleStatusWindow(Graphics g, Rectangle rect)
    {
        var classLabel = selectedLanguage == UiLanguage.English ? "HERO" : "ゆうしゃ";
        const int lineHeight = 30;
        const int leftColumnWidth = 126;
        const int inlineStatGap = 32;
        var statX = rect.X + 8;
        var mpY = rect.Y + (lineHeight * 2) + 8;
        var mpText = $"MP:{player.CurrentMp}";
        var exText = $"EX:{player.Experience}";
        var exX = statX + MeasureTextWidth(g, mpText, smallFont) + inlineStatGap;
        var exColumnWidth = rect.Right - exX;
        var playerStatusLabel = currentEncounter is null
            ? string.Empty
            : GetBattleStatusEffectLabel(currentEncounter.PlayerStatusEffect);

        DrawText(g, $"{GetDisplayPlayerName()}  :  {classLabel}", new Rectangle(statX, rect.Y + 2, rect.Width - 16, 24), smallFont);
        DrawText(g, $"HP:{player.CurrentHp}", new Rectangle(statX, rect.Y + lineHeight + 6, leftColumnWidth, 24), smallFont);
        DrawText(g, mpText, new Rectangle(statX, mpY, leftColumnWidth, 24), smallFont);
        DrawText(g, exText, new Rectangle(exX, mpY, exColumnWidth, 24), smallFont);
        if (!string.IsNullOrWhiteSpace(playerStatusLabel))
        {
            DrawText(g, playerStatusLabel, new Rectangle(exX, rect.Y + lineHeight + 6, exColumnWidth, 24), smallFont);
        }
    }

    private void DrawBattleLowerUi(Graphics g)
    {
        var panelRect = new Rectangle(18, 286, 604, 176);
        DrawWindow(g, panelRect);

        var innerRect = Rectangle.Inflate(panelRect, -16, -12);
        var headerRect = new Rectangle(innerRect.X, innerRect.Y, innerRect.Width, 24);
        DrawBattleUnifiedHeader(g, headerRect);
        DrawBattleWindowSeparator(g, innerRect.X, innerRect.Y + 28, innerRect.Width);

        if (battleFlowState is BattleFlowState.CommandSelection or BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
        {
            var selectionRect = new Rectangle(innerRect.X, innerRect.Y + 38, 210, innerRect.Height - 38);
            var separatorX = selectionRect.Right + 8;
            var messageRect = new Rectangle(separatorX + 14, innerRect.Y + 38, innerRect.Right - (separatorX + 14), innerRect.Height - 38);
            DrawBattleSelectionPane(g, selectionRect);
            DrawBattleVerticalSeparator(g, separatorX, innerRect.Y + 38, innerRect.Height - 38);
            DrawBattleMessagePane(g, messageRect, battleFlowState == BattleFlowState.CommandSelection
                ? GetBattleCommandHelpMessage()
                : GetBattleSubmenuHelpMessage());
            return;
        }

        var resultRect = new Rectangle(innerRect.X, innerRect.Y + 38, innerRect.Width, innerRect.Height - 38);
        DrawBattleMessagePane(
            g,
            resultRect,
            string.Empty);
    }

    private void DrawBattleSelectionPane(Graphics g, Rectangle rect)
    {
        DrawText(g, GetBattleSelectionTitle(), new Rectangle(rect.X, rect.Y, rect.Width, 20), smallFont);
        if (battleFlowState is BattleFlowState.SpellSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
        {
            DrawText(g, GetBattleSelectionCounterText(), new Rectangle(rect.X, rect.Y, rect.Width, 20), smallFont, StringAlignment.Far);
        }

        DrawBattleWindowSeparator(g, rect.X, rect.Y + 22, rect.Width);
        var contentRect = new Rectangle(rect.X, rect.Y + 32, rect.Width, rect.Height - 32);
        if (battleFlowState == BattleFlowState.CommandSelection)
        {
            DrawBattleCommandWindow(g, contentRect);
            return;
        }

        DrawBattleSelectionList(g, contentRect);
    }

    private void DrawBattleCommandWindow(Graphics g, Rectangle rect)
    {
        var commandCellWidth = rect.Width / GetBattleCommandColumnCount();
        var commandCellHeight = rect.Height / GetBattleCommandRowCount();

        for (var row = 0; row < GetBattleCommandRowCount(); row++)
        {
            for (var column = 0; column < GetBattleCommandColumnCount(); column++)
            {
                var cellRect = new Rectangle(
                    rect.X + (column * commandCellWidth),
                    rect.Y + (row * commandCellHeight),
                    commandCellWidth,
                    commandCellHeight);
                if (battleCursorRow == row && battleCursorColumn == column)
                {
                    DrawBattleSelectionPointer(g, cellRect.X + 2, cellRect.Y + (cellRect.Height / 2) - 8);
                }

                DrawText(
                    g,
                    GetBattleCommandLabel(row, column),
                    new Rectangle(cellRect.X + 22, cellRect.Y - 1, cellRect.Width - 22, cellRect.Height),
                    smallFont,
                    StringAlignment.Near,
                    StringAlignment.Center);
            }
        }
    }

    private void DrawBattleSelectionList(Graphics g, Rectangle rect)
    {
        var entries = GetActiveBattleSelectionEntries();
        if (entries.Count == 0)
        {
            DrawText(
                g,
                battleFlowState == BattleFlowState.ItemSelection ? GetBattleNoItemsMessage() : GetBattleNoEquipmentMessage(),
                rect,
                smallFont,
                wrap: true);
            return;
        }

        using var highlightBrush = new SolidBrush(Color.FromArgb(40, 54, 116, 196));
        const int rowHeight = 24;
        var visibleEntries = entries.Skip(battleListScroll).Take(BattleSelectionVisibleRows).ToArray();
        for (var index = 0; index < visibleEntries.Length; index++)
        {
            var entryIndex = battleListScroll + index;
            var rowRect = new Rectangle(rect.X, rect.Y + (index * rowHeight), rect.Width, rowHeight);
            if (entryIndex == battleListCursor)
            {
                g.FillRectangle(highlightBrush, rowRect.X + 2, rowRect.Y + 2, rowRect.Width - 4, rowRect.Height - 4);
                DrawBattleSelectionPointer(g, rowRect.X + 2, rowRect.Y + 5);
            }

            DrawText(g, visibleEntries[index].Label, new Rectangle(rowRect.X + 22, rowRect.Y, 92, rowRect.Height), smallFont, StringAlignment.Near, StringAlignment.Center);
            DrawText(g, visibleEntries[index].Detail, new Rectangle(rowRect.X + 116, rowRect.Y, rowRect.Width - 166, rowRect.Height), smallFont, StringAlignment.Far, StringAlignment.Center);
            DrawText(g, visibleEntries[index].Badge, new Rectangle(rowRect.Right - 42, rowRect.Y, 42, rowRect.Height), smallFont, StringAlignment.Far, StringAlignment.Center);
        }
    }

    private void DrawBattleUnifiedHeader(Graphics g, Rectangle rect)
    {
        var targetName = currentEncounter is null
            ? selectedLanguage == UiLanguage.English ? "MONSTER" : "まもの"
            : GameContent.GetEnemyName(currentEncounter.Enemy, selectedLanguage);
        var countLabel = selectedLanguage == UiLanguage.English ? "x1" : "1匹";
        var statusLabel = currentEncounter is null
            ? string.Empty
            : GetBattleStatusEffectLabel(currentEncounter.EnemyStatusEffect);
        var nameText = string.IsNullOrWhiteSpace(statusLabel) ? targetName : $"{targetName} [{statusLabel}]";
        DrawText(g, nameText, new Rectangle(rect.X, rect.Y, rect.Width - 176, rect.Height), smallFont);
        DrawText(g, countLabel, new Rectangle(rect.Right - 204, rect.Y, 44, rect.Height), smallFont, StringAlignment.Far);
        if (currentEncounter is null)
        {
            return;
        }

        DrawText(g, $"HP {currentEncounter.CurrentHp}/{currentEncounter.Enemy.MaxHp}", new Rectangle(rect.Right - 150, rect.Y, 150, rect.Height), smallFont, StringAlignment.Far);
    }

    private void DrawBattleMessagePane(Graphics g, Rectangle rect, string footer)
    {
        var footerHeight = string.IsNullOrWhiteSpace(footer)
            ? 0
            : footer.Contains('\n') ? 38 : 20;
        var textRect = new Rectangle(
            rect.X,
            rect.Y,
            rect.Width,
            footerHeight == 0 ? rect.Height : Math.Max(20, rect.Height - (footerHeight + 8)));
        
        DrawText(g, battleMessage, textRect, smallFont, wrap: true);
        if (string.IsNullOrWhiteSpace(footer))
        {
            return;
        }

        DrawText(
            g,
            footer,
            new Rectangle(rect.X, rect.Bottom - footerHeight, rect.Width, footerHeight),
            smallFont,
            StringAlignment.Far,
            wrap: true);
    }

    private static void DrawBattleFrameWindow(Graphics g, Rectangle rect, Color backgroundColor)
    {
        using var shadowBrush = new SolidBrush(Color.FromArgb(92, 0, 0, 0));
        using var backgroundBrush = new SolidBrush(backgroundColor);

        g.FillRectangle(shadowBrush, rect.X + 4, rect.Y + 4, rect.Width, rect.Height);
        g.FillRectangle(backgroundBrush, rect);
        DrawBattleWindowBorder(g, rect);
    }

    private static void DrawBattleWindowBorder(Graphics g, Rectangle rect)
    {
        using var outerPen = new Pen(Color.FromArgb(42, 70, 142), 3);
        using var middlePen = new Pen(Color.FromArgb(36, 124, 208), 2);
        using var innerPen = new Pen(Color.FromArgb(26, 42, 84), 2);

        g.DrawRectangle(outerPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        g.DrawRectangle(middlePen, rect.X + 4, rect.Y + 4, rect.Width - 9, rect.Height - 9);
        g.DrawRectangle(innerPen, rect.X + 8, rect.Y + 8, rect.Width - 17, rect.Height - 17);
    }

    private void DrawBattleMessageWindowBackground(Graphics g, Rectangle rect)
    {
        var backdrop = GetUiImage("SFC_battlefieldFrame1.png");
        if (backdrop is not null)
        {
            var state = g.Save();
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.DrawImage(backdrop, rect, BattleMessageWindowBackdropSourceRect, GraphicsUnit.Pixel);
            g.Restore(state);
        }
        else
        {
            using var fallbackBrush = new SolidBrush(BattleMessageWindowFallbackColor);
            g.FillRectangle(fallbackBrush, rect);
        }

        using var tintBrush = new SolidBrush(BattleMessageWindowTintColor);
        g.FillRectangle(tintBrush, rect);
    }

    private static void DrawBattleWindowSeparator(Graphics g, int x, int y, int width)
    {
        using var shadowPen = new Pen(Color.FromArgb(24, 24, 40));
        using var linePen = new Pen(Color.FromArgb(132, 206, 255));
        g.DrawLine(shadowPen, x, y + 1, x + width, y + 1);
        g.DrawLine(linePen, x, y, x + width, y);
    }

    private static void DrawBattleVerticalSeparator(Graphics g, int x, int y, int height)
    {
        using var shadowPen = new Pen(Color.FromArgb(24, 24, 40));
        using var linePen = new Pen(Color.FromArgb(132, 206, 255));
        g.DrawLine(shadowPen, x + 1, y, x + 1, y + height);
        g.DrawLine(linePen, x, y, x, y + height);
    }

    private void DrawBattleSelectionPointer(Graphics g, int x, int y)
    {
        if ((frameCounter / 18) % 2 == 1)
        {
            return;
        }

        using var shadowBrush = new SolidBrush(Color.FromArgb(44, 22, 30));
        using var baseBrush = new SolidBrush(Color.White);
        g.FillRectangle(shadowBrush, x + 2, y + 2, 14, 12);
        g.FillRectangle(baseBrush, x, y + 4, 6, 4);
        g.FillRectangle(baseBrush, x + 4, y + 2, 4, 8);
        g.FillRectangle(baseBrush, x + 8, y, 4, 12);
        g.FillRectangle(baseBrush, x + 12, y + 2, 2, 8);
    }

    private void DrawBattleBackdrop(Graphics g)
    {
        if (currentFieldMap == FieldMapId.Field)
        {
            var battlefieldBackdrop = GetUiImage("SFC_battlefieldFrame1.png");
            if (battlefieldBackdrop is not null)
            {
                DrawBattleBackdropCover(g, battlefieldBackdrop, new Rectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight));
                return;
            }
        }

        DrawBattleStoneWall(g, new Rectangle(0, 0, UiCanvas.VirtualWidth, 244));
        DrawBattlePlatform(g, new Rectangle(0, 244, UiCanvas.VirtualWidth, 68));
        DrawBattleCarpet(g, new Rectangle(0, 312, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight - 312));
    }

    private static void DrawBattleBackdropCover(Graphics g, Image backdrop, Rectangle bounds)
    {
        var scale = Math.Max(bounds.Width / (float)backdrop.Width, bounds.Height / (float)backdrop.Height);
        var sourceWidth = Math.Max(1, (int)Math.Round(bounds.Width / scale));
        var sourceHeight = Math.Max(1, (int)Math.Round(bounds.Height / scale));
        var sourceRect = new Rectangle(
            Math.Max(0, (backdrop.Width - sourceWidth) / 2),
            Math.Max(0, (backdrop.Height - sourceHeight) / 2),
            Math.Min(sourceWidth, backdrop.Width),
            Math.Min(sourceHeight, backdrop.Height));

        var state = g.Save();
        g.SetClip(bounds);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.DrawImage(backdrop, bounds, sourceRect, GraphicsUnit.Pixel);
        g.Restore(state);
    }

    private static void DrawBattleStoneWall(Graphics g, Rectangle rect)
    {
        using var mortarBrush = new SolidBrush(Color.FromArgb(28, 24, 42));
        using var brickBrushA = new SolidBrush(Color.FromArgb(90, 82, 112));
        using var brickBrushB = new SolidBrush(Color.FromArgb(74, 67, 94));
        using var brickBrushC = new SolidBrush(Color.FromArgb(60, 54, 80));
        using var trimBrush = new SolidBrush(Color.FromArgb(178, 108, 44));
        using var trimDarkBrush = new SolidBrush(Color.FromArgb(94, 54, 24));

        g.FillRectangle(mortarBrush, rect);

        const int brickWidth = 54;
        const int brickHeight = 20;
        for (var row = 0; row <= rect.Height / brickHeight; row++)
        {
            var y = rect.Y + (row * brickHeight);
            var startX = rect.X - ((row % 2) * (brickWidth / 2));
            var brush = (row % 3) switch
            {
                0 => brickBrushA,
                1 => brickBrushB,
                _ => brickBrushC
            };

            for (var x = startX; x < rect.Right; x += brickWidth)
            {
                g.FillRectangle(brush, x + 1, y + 1, brickWidth - 2, brickHeight - 2);
            }
        }

        DrawBattlePillar(g, new Rectangle(86, 28, 34, 176));
        DrawBattlePillar(g, new Rectangle(520, 28, 34, 176));
        DrawBattleArchWindow(g, new Rectangle(166, 52, 82, 84));
        DrawBattleArchWindow(g, new Rectangle(390, 52, 82, 84));

        g.FillRectangle(trimDarkBrush, rect.X, rect.Bottom - 56, rect.Width, 10);
        g.FillRectangle(trimBrush, rect.X, rect.Bottom - 52, rect.Width, 4);
        g.FillRectangle(trimBrush, rect.X, rect.Bottom - 40, rect.Width, 2);
        for (var x = rect.X + 24; x < rect.Right - 24; x += 24)
        {
            g.FillRectangle(trimBrush, x, rect.Bottom - 50, 10, 2);
            g.FillRectangle(trimBrush, x + 6, rect.Bottom - 46, 10, 2);
        }
    }

    private static void DrawBattleArchWindow(Graphics g, Rectangle rect)
    {
        using var frameBrush = new SolidBrush(Color.FromArgb(116, 106, 126));
        using var frameDarkBrush = new SolidBrush(Color.FromArgb(52, 48, 64));
        using var glassBrush = new SolidBrush(Color.FromArgb(44, 60, 116));
        using var barPen = new Pen(Color.FromArgb(180, 108, 42), 4);

        g.FillRectangle(frameDarkBrush, rect.X - 4, rect.Y + 10, rect.Width + 8, rect.Height + 10);
        g.FillRectangle(frameBrush, rect.X, rect.Y + 14, rect.Width, rect.Height);
        g.FillEllipse(frameBrush, rect.X, rect.Y, rect.Width, rect.Width);

        var innerRect = Rectangle.Inflate(rect, -10, -10);
        g.FillRectangle(glassBrush, innerRect.X, innerRect.Y + 16, innerRect.Width, innerRect.Height - 6);
        g.FillEllipse(glassBrush, innerRect.X, innerRect.Y - 2, innerRect.Width, innerRect.Width);

        g.DrawLine(barPen, innerRect.X + 8, innerRect.Bottom - 14, innerRect.Right - 8, innerRect.Y + 26);
        g.DrawLine(barPen, innerRect.X + 14, innerRect.Y + 30, innerRect.Right - 14, innerRect.Bottom - 18);
    }

    private static void DrawBattlePillar(Graphics g, Rectangle rect)
    {
        using var bodyBrush = new SolidBrush(Color.FromArgb(70, 66, 84));
        using var edgeBrush = new SolidBrush(Color.FromArgb(102, 96, 122));
        using var trimBrush = new SolidBrush(Color.FromArgb(180, 108, 42));

        g.FillRectangle(bodyBrush, rect);
        g.FillRectangle(edgeBrush, rect.X + 4, rect.Y, 6, rect.Height);
        g.FillRectangle(edgeBrush, rect.Right - 10, rect.Y, 6, rect.Height);
        g.FillRectangle(trimBrush, rect.X - 6, rect.Y + 22, rect.Width + 12, 4);
        g.FillRectangle(trimBrush, rect.X - 6, rect.Bottom - 34, rect.Width + 12, 4);
        g.FillRectangle(edgeBrush, rect.X - 10, rect.Bottom - 20, rect.Width + 20, 16);
    }

    private static void DrawBattlePlatform(Graphics g, Rectangle rect)
    {
        using var baseBrush = new SolidBrush(Color.FromArgb(56, 26, 48));
        using var stripeBrush = new SolidBrush(Color.FromArgb(72, 34, 60));
        using var highlightBrush = new SolidBrush(Color.FromArgb(170, 108, 42));
        using var shadowBrush = new SolidBrush(Color.FromArgb(40, 12, 26));

        g.FillRectangle(baseBrush, rect);
        for (var y = rect.Y + 6; y < rect.Bottom; y += 10)
        {
            g.FillRectangle(stripeBrush, rect.X, y, rect.Width, 2);
        }

        g.FillRectangle(shadowBrush, rect.X, rect.Y, rect.Width, 14);
        g.FillRectangle(highlightBrush, rect.X, rect.Bottom - 12, rect.Width, 4);
        g.FillRectangle(highlightBrush, rect.X, rect.Bottom - 4, rect.Width, 2);
    }

    private static void DrawBattleCarpet(Graphics g, Rectangle rect)
    {
        using var carpetBrush = new SolidBrush(Color.FromArgb(124, 28, 64));
        using var carpetShadeBrush = new SolidBrush(Color.FromArgb(94, 18, 50));
        using var trimBrush = new SolidBrush(Color.FromArgb(214, 156, 54));
        using var trimShadeBrush = new SolidBrush(Color.FromArgb(120, 72, 20));

        g.FillRectangle(carpetBrush, rect);

        for (var y = rect.Y + 10; y < rect.Bottom; y += 10)
        {
            g.FillRectangle(carpetShadeBrush, rect.X, y, rect.Width, 2);
        }

        var topTrimRect = new Rectangle(rect.X, rect.Y + 6, rect.Width, 22);
        g.FillRectangle(trimShadeBrush, topTrimRect);
        g.FillRectangle(trimBrush, topTrimRect.X, topTrimRect.Y + 4, topTrimRect.Width, 2);
        g.FillRectangle(trimBrush, topTrimRect.X, topTrimRect.Bottom - 6, topTrimRect.Width, 2);

        for (var x = rect.X + 12; x < rect.Right - 20; x += 24)
        {
            g.FillRectangle(trimBrush, x + 6, topTrimRect.Y + 8, 6, 6);
            g.FillRectangle(trimShadeBrush, x + 8, topTrimRect.Y + 10, 2, 2);
            g.FillRectangle(trimBrush, x + 2, topTrimRect.Y + 10, 2, 2);
            g.FillRectangle(trimBrush, x + 14, topTrimRect.Y + 10, 2, 2);
            g.FillRectangle(trimBrush, x + 8, topTrimRect.Y + 6, 2, 2);
            g.FillRectangle(trimBrush, x + 8, topTrimRect.Y + 14, 2, 2);
        }
    }

    private static void DrawBattleEnemySprite(Graphics g, Image sprite, Point center)
    {
        const int maxWidth = 250;
        const int maxHeight = 136;
        var scale = Math.Min(maxWidth / (float)sprite.Width, maxHeight / (float)sprite.Height);
        scale = Math.Min(scale, 1.1f);

        var width = Math.Max(1, (int)Math.Round(sprite.Width * scale));
        var height = Math.Max(1, (int)Math.Round(sprite.Height * scale));
        var bottom = center.Y + 56;
        var destination = new Rectangle(center.X - (width / 2), bottom - height, width, height);

        using var shadowBrush = new SolidBrush(Color.FromArgb(96, 0, 0, 0));
        g.FillEllipse(
            shadowBrush,
            center.X - Math.Max(34, width / 3),
            bottom - 8,
            Math.Max(68, (width * 2) / 3),
            14);

        var state = g.Save();
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.DrawImage(sprite, destination);
        g.Restore(state);
    }

    private static void DrawDefaultBattleEnemy(Graphics g, Point center)
    {
        using var bodyBrush = new SolidBrush(Color.FromArgb(216, 228, 240));
        using var outlinePen = new Pen(Color.FromArgb(88, 136, 204), 2);
        using var hornPen = new Pen(Color.FromArgb(216, 228, 240), 4);
        using var eyeBrush = new SolidBrush(Color.Black);
        using var shadowBrush = new SolidBrush(Color.FromArgb(88, 0, 0, 0));

        g.FillEllipse(shadowBrush, center.X - 42, center.Y + 30, 84, 16);

        var body = new Rectangle(center.X - 58, center.Y - 14, 116, 78);
        g.FillEllipse(bodyBrush, body);
        g.DrawEllipse(outlinePen, body);

        g.DrawLine(hornPen, center.X - 22, center.Y - 10, center.X - 40, center.Y - 36);
        g.DrawLine(hornPen, center.X + 22, center.Y - 10, center.X + 40, center.Y - 36);
        g.FillEllipse(eyeBrush, center.X - 22, center.Y + 12, 8, 8);
        g.FillEllipse(eyeBrush, center.X + 14, center.Y + 12, 8, 8);
        g.DrawLine(outlinePen, center.X - 18, center.Y + 42, center.X + 18, center.Y + 42);
    }

    private static void DrawMossToadEnemy(Graphics g, Point center)
    {
        using var shadowBrush = new SolidBrush(Color.FromArgb(88, 0, 0, 0));
        using var armBrush = new SolidBrush(Color.FromArgb(126, 156, 38));
        using var bodyBrush = new SolidBrush(Color.FromArgb(144, 176, 42));
        using var bellyBrush = new SolidBrush(Color.FromArgb(172, 72, 126));
        using var outlinePen = new Pen(Color.FromArgb(76, 94, 18), 3);
        using var tongueBrush = new SolidBrush(Color.FromArgb(238, 74, 118));
        using var mouthBrush = new SolidBrush(Color.FromArgb(64, 18, 26));
        using var eyeBrush = new SolidBrush(Color.FromArgb(248, 242, 228));
        using var pupilBrush = new SolidBrush(Color.Black);

        g.FillEllipse(shadowBrush, center.X - 58, center.Y + 38, 116, 18);

        g.FillEllipse(armBrush, center.X - 74, center.Y + 10, 40, 18);
        g.FillEllipse(armBrush, center.X + 32, center.Y + 10, 40, 18);
        g.FillEllipse(armBrush, center.X - 54, center.Y + 42, 28, 18);
        g.FillEllipse(armBrush, center.X + 28, center.Y + 42, 28, 18);

        var body = new Rectangle(center.X - 54, center.Y - 2, 108, 70);
        g.FillEllipse(bodyBrush, body);
        g.DrawEllipse(outlinePen, body);

        var head = new Rectangle(center.X - 36, center.Y - 36, 72, 52);
        g.FillEllipse(bodyBrush, head);
        g.DrawEllipse(outlinePen, head);

        var mouth = new Rectangle(center.X - 28, center.Y - 10, 56, 30);
        g.FillEllipse(mouthBrush, mouth);
        g.FillEllipse(tongueBrush, center.X - 2, center.Y + 6, 20, 26);
        g.FillEllipse(bellyBrush, center.X - 34, center.Y + 26, 68, 34);

        g.FillEllipse(eyeBrush, center.X - 22, center.Y - 28, 16, 16);
        g.FillEllipse(eyeBrush, center.X + 6, center.Y - 28, 16, 16);
        g.FillEllipse(pupilBrush, center.X - 18, center.Y - 24, 6, 6);
        g.FillEllipse(pupilBrush, center.X + 10, center.Y - 24, 6, 6);
    }
}
