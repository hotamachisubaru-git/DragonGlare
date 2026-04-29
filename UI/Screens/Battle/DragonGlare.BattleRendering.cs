using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void DrawBattle(Graphics g)
    {
        DrawBattleBackdrop(g);

        if (ShouldDrawBattleEnemySprite())
        {
            DrawBattleEnemy(g, new Point(320, 266));
        }

        DrawBattleTopUi(g);
        DrawBattleMessageWindow(g);
    }

    private void DrawBattleTopUi(Graphics g)
    {
        var commandWindowRect = new Rectangle(2, 42, 272, 140);
        var statusWindowRect = new Rectangle(284, 42, 354, 140);

        DrawBattleFrameWindow(g, commandWindowRect);
        DrawBattleFrameWindow(g, statusWindowRect);

        if (battleFlowState == BattleFlowState.Intro)
        {
            return;
        }

        if (battleFlowState == BattleFlowState.CommandSelection)
        {
            DrawBattleCommandWindow(g, Rectangle.Inflate(commandWindowRect, -18, -16));
        }
        else if (battleFlowState is BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
        {
            DrawBattleSelectionPane(g, Rectangle.Inflate(commandWindowRect, -16, -14));
        }

        DrawBattleStatusWindow(g, Rectangle.Inflate(statusWindowRect, -18, -16));
    }

    private void DrawBattleMessageWindow(Graphics g)
    {
        var messageWindowRect = new Rectangle(2, 326, 636, 130);
        DrawBattleFrameWindow(g, messageWindowRect);
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
                $"{pendingEncounter.Enemy.Name}の けはいがする…",
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
        if (currentEncounter is null || currentEncounter.CurrentHp <= 0)
        {
            return false;
        }

        if (enemyHitFlashFramesRemaining <= 0)
        {
            return true;
        }

        return ((enemyHitFlashFramesRemaining - 1) / 2) % 2 == 0;
    }

    private void DrawBattleStatusWindow(Graphics g, Rectangle rect)
    {
        var classLabel = selectedLanguage == UiLanguage.English ? "HERO" : "ゆうしゃ";
        const int lineHeight = 30;
        const int leftColumnWidth = 126;
        var rightColumnX = rect.X + 156;
        var rightColumnWidth = rect.Right - rightColumnX;

        DrawText(g, $"{GetDisplayPlayerName()}  :  {classLabel}", new Rectangle(rect.X + 8, rect.Y + 2, rect.Width - 16, 24), smallFont);
        DrawText(g, $"HP:{player.CurrentHp}", new Rectangle(rect.X + 8, rect.Y + lineHeight + 6, leftColumnWidth, 24), smallFont);
        DrawText(g, $"MP:{player.CurrentMp}", new Rectangle(rect.X + 8, rect.Y + (lineHeight * 2) + 8, leftColumnWidth, 24), smallFont);
        DrawText(g, $"EX:{player.Experience}", new Rectangle(rightColumnX, rect.Y + (lineHeight * 2) + 8, rightColumnWidth, 24), smallFont);
    }

    private void DrawBattleLowerUi(Graphics g)
    {
        var panelRect = new Rectangle(18, 286, 604, 176);
        DrawWindow(g, panelRect);

        var innerRect = Rectangle.Inflate(panelRect, -16, -12);
        var headerRect = new Rectangle(innerRect.X, innerRect.Y, innerRect.Width, 24);
        DrawBattleUnifiedHeader(g, headerRect);
        DrawBattleWindowSeparator(g, innerRect.X, innerRect.Y + 28, innerRect.Width);

        if (battleFlowState is BattleFlowState.CommandSelection or BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
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
            selectedLanguage == UiLanguage.English ? "ENTER / Z / X: NEXT" : "ENTER / Z / X: つぎへ");
    }

    private void DrawBattleSelectionPane(Graphics g, Rectangle rect)
    {
        DrawText(g, GetBattleSelectionTitle(), new Rectangle(rect.X, rect.Y, rect.Width, 20), smallFont);
        if (battleFlowState is BattleFlowState.ItemSelection or BattleFlowState.EquipmentSelection)
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
        var targetName = currentEncounter?.Enemy.Name ?? (selectedLanguage == UiLanguage.English ? "MONSTER" : "まもの");
        var countLabel = selectedLanguage == UiLanguage.English ? "x1" : "1匹";
        DrawText(g, targetName, new Rectangle(rect.X, rect.Y, rect.Width - 176, rect.Height), smallFont);
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
        
        string displayMessage = battleMessage;
        // バトルのリザルト等でアニメーション表示が有効な場合
        if (battleMessageLines.Length > 0 && (battleFlowState is BattleFlowState.Victory or BattleFlowState.Defeat or BattleFlowState.Escaped))
        {
            var visibleCount = Math.Min(battleMessageVisibleLines, battleMessageLines.Length);
            displayMessage = string.Join("\n", battleMessageLines.Take(visibleCount));
        }

        DrawText(g, displayMessage, textRect, smallFont, wrap: true);
        if (string.IsNullOrWhiteSpace(footer))
        {
            return;
        }

        // 次へ進むためのフッターメッセージ（決定キーでスキップできることも知らせる）
        DrawText(
            g,
            footer,
            new Rectangle(rect.X, rect.Bottom - footerHeight, rect.Width, footerHeight),
            smallFont,
            StringAlignment.Far,
            wrap: true);
    }

    private static void DrawBattleFrameWindow(Graphics g, Rectangle rect)
    {
        using var shadowBrush = new SolidBrush(Color.FromArgb(92, 0, 0, 0));
        using var backgroundBrush = new SolidBrush(Color.Black);
        using var outerPen = new Pen(Color.FromArgb(42, 70, 142), 3);
        using var middlePen = new Pen(Color.FromArgb(36, 124, 208), 2);
        using var innerPen = new Pen(Color.FromArgb(26, 42, 84), 2);

        g.FillRectangle(shadowBrush, rect.X + 4, rect.Y + 4, rect.Width, rect.Height);
        g.FillRectangle(backgroundBrush, rect);
        g.DrawRectangle(outerPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        g.DrawRectangle(middlePen, rect.X + 4, rect.Y + 4, rect.Width - 9, rect.Height - 9);
        g.DrawRectangle(innerPen, rect.X + 8, rect.Y + 8, rect.Width - 17, rect.Height - 17);
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
