using System.Drawing.Drawing2D;
using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Persistence;

namespace DragonGlareAlpha;

public partial class Form1
{
    private void DrawModeSelect(Graphics g)
    {
        DrawMenuBackdrop(g);

        var layoutRect = new Rectangle(64, 0, 512, 480);
        var layoutImage = GetUiImage("window21.png");

        if (layoutImage is not null)
        {
            g.DrawImage(layoutImage, layoutRect);
        }
        else
        {
            DrawWindow(g, new Rectangle(96, 32, 184, 192));
            DrawWindow(g, new Rectangle(288, 32, 256, 192));
            DrawWindow(g, new Rectangle(96, 232, 448, 176));
        }

        var menuItems = new[]
        {
            "はじめから",
            "つづきから",
            "データうつす",
            "データけす"
        };

        var menuStartX = ScaleModeSelectX(layoutRect, 24);
        var menuStartY = ScaleModeSelectY(layoutRect, 24);
        var menuLineHeight = ScaleModeSelectHeight(layoutRect, 24);
        var menuCursorX = ScaleModeSelectX(layoutRect, 8);

        for (var index = 0; index < menuItems.Length; index++)
        {
            var lineY = menuStartY + (index * menuLineHeight);
            if (modeCursor == index)
            {
                DrawModeSelectCursor(g, menuCursorX, lineY + 4);
            }

            DrawText(g, menuItems[index], menuStartX, lineY, uiFont);
        }

        DrawText(
            g,
            GetModeSelectDescription(modeCursor),
            new Rectangle(
                ScaleModeSelectX(layoutRect, 128),
                ScaleModeSelectY(layoutRect, 24),
                ScaleModeSelectWidth(layoutRect, 96),
                ScaleModeSelectHeight(layoutRect, 64)),
            uiFont,
            wrap: true);

        DrawText(
            g,
            string.IsNullOrWhiteSpace(menuNotice) ? "モードを選んでください。" : menuNotice,
            new Rectangle(
                ScaleModeSelectX(layoutRect, 24),
                ScaleModeSelectY(layoutRect, 136),
                ScaleModeSelectWidth(layoutRect, 200),
                ScaleModeSelectHeight(layoutRect, 64)),
            uiFont,
            wrap: true);
    }

    private void DrawLanguageSelection(Graphics g)
    {
        DrawLanguageOpeningBackdrop(g);

        if (!languageOpeningFinished)
        {
            DrawLanguageOpeningNarration(g);
            return;
        }

        using var overlayBrush = new SolidBrush(Color.FromArgb(96, 0, 0, 0));
        g.FillRectangle(overlayBrush, new Rectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight));

        DrawWindow(g, new Rectangle(84, 64, 260, 128));
        DrawOption(g, languageCursor == 0, 104, 94, "にほんご");
        DrawOption(g, languageCursor == 1, 104, 134, "ENGLISH");

        DrawWindow(g, new Rectangle(116, 270, 410, 180));
        DrawText(g, "げんごをえらんでください", 140, 310);
        DrawText(g, "CHOOSE A LANGUAGE", 140, 350);
        DrawText(g, "ENTER/Z: けってい  ESC: もどる", 140, 390, smallFont);
    }

    private void DrawNameInput(Graphics g)
    {
        var table = GameContent.GetNameTable(selectedLanguage);
        const int originX = 44;
        const int originY = 52;
        const int cellWidth = 56;
        const int cellHeight = 44;

        for (var row = 0; row < table.Length; row++)
        {
            for (var column = 0; column < table[row].Length; column++)
            {
                var textX = originX + (column * cellWidth);
                var textY = originY + (row * cellHeight);
                if (row == nameCursorRow && column == nameCursorColumn)
                {
                    DrawText(g, "▶", textX - 24, textY);
                }

                DrawText(g, table[row][column], textX, textY);
            }
        }

        DrawWindow(g, new Rectangle(116, 270, 410, 180));
        DrawText(g, "なまえをきめてください", 140, 300);
        DrawText(g, "CHOOSE A NAME", 140, 338);
        DrawText(g, playerName.Length == 0 ? "..." : playerName.ToString(), 140, 384);

        DrawText(g, selectedLanguage == UiLanguage.Japanese ? "ESC: もどる" : "ESC: BACK", 14, 442);
    }

    private void DrawSaveSlotSelection(Graphics g)
    {
        DrawMenuBackdrop(g);

        var titleRect = new Rectangle(98, 24, 444, 64);
        DrawWindow(g, titleRect);
        DrawText(
            g,
            saveSlotSelectionMode == SaveSlotSelectionMode.Save
                ? "ぼうけんのしょを えらんでください"
                : "よみこむ ぼうけんのしょを えらんでください",
            new Rectangle(126, 40, 388, 22),
            smallFont);
        DrawText(
            g,
            saveSlotSelectionMode == SaveSlotSelectionMode.Save
                ? "CHOOSE A SAVE SLOT"
                : "CHOOSE A FILE TO LOAD",
            new Rectangle(126, 62, 388, 18),
            smallFont);

        for (var index = 0; index < SaveService.SlotCount; index++)
        {
            var slotNumber = index + 1;
            var summary = saveSlotSummaries.ElementAtOrDefault(index) ?? new SaveSlotSummary
            {
                SlotNumber = slotNumber,
                State = SaveSlotState.Empty
            };

            var slotRect = new Rectangle(98, 108 + (index * 86), 444, 76);
            DrawWindow(g, slotRect);
            if (saveSlotCursor == index)
            {
                DrawSelectionMarker(g, slotRect.X + 14, slotRect.Y + 24);
            }

            DrawText(g, $"ぼうけんのしょ {slotNumber}", slotRect.X + 38, slotRect.Y + 10, smallFont);

            switch (summary.State)
            {
                case SaveSlotState.Occupied:
                    DrawText(g, $"{summary.Name}   LV {summary.Level}   G {summary.Gold}", slotRect.X + 38, slotRect.Y + 30, smallFont);
                    DrawText(
                        g,
                        $"{GetMapDisplayName(summary.CurrentFieldMap)}  {summary.SavedAtLocal:yyyy/MM/dd HH:mm}",
                        new Rectangle(slotRect.X + 38, slotRect.Y + 50, 380, 16),
                        smallFont);
                    break;
                case SaveSlotState.Corrupted:
                    DrawText(g, "BROKEN DATA / よみこめません", slotRect.X + 38, slotRect.Y + 32, smallFont);
                    break;
                default:
                    DrawText(g, "NO DATA / まだ きろくがありません", slotRect.X + 38, slotRect.Y + 32, smallFont);
                    break;
            }
        }

        var helpRect = new Rectangle(116, 408, 408, 40);
        DrawWindow(g, helpRect);
        DrawText(
            g,
            saveSlotSelectionMode == SaveSlotSelectionMode.Save
                ? "ENTER: きろく  ESC: なまえにもどる"
                : "ENTER: よみこむ  ESC: モードにもどる",
            new Rectangle(136, 420, 368, 18),
            smallFont);

        if (!string.IsNullOrWhiteSpace(menuNotice))
        {
            var noticeRect = new Rectangle(110, 366, 420, 30);
            DrawWindow(g, noticeRect);
            DrawText(g, menuNotice, Rectangle.Inflate(noticeRect, -18, -10), smallFont);
        }
    }

    private static int ScaleModeSelectX(Rectangle layoutRect, int sourceX)
    {
        return layoutRect.X + (int)Math.Round(sourceX * (layoutRect.Width / 256f));
    }

    private static int ScaleModeSelectY(Rectangle layoutRect, int sourceY)
    {
        return layoutRect.Y + (int)Math.Round(sourceY * (layoutRect.Height / 240f));
    }

    private static int ScaleModeSelectWidth(Rectangle layoutRect, int sourceWidth)
    {
        return (int)Math.Round(sourceWidth * (layoutRect.Width / 256f));
    }

    private static int ScaleModeSelectHeight(Rectangle layoutRect, int sourceHeight)
    {
        return (int)Math.Round(sourceHeight * (layoutRect.Height / 240f));
    }

    private static string GetModeSelectDescription(int cursor)
    {
        return cursor switch
        {
            0 => "ゲームを最初から\nはじめる。",
            1 => "前回のつづきから\nはじめる。",
            2 => "データを別の枠へ\nうつす。",
            3 => "いらないデータを\nけす。",
            _ => string.Empty
        };
    }

    private void DrawModeSelectCursor(Graphics g, int x, int y)
    {
        if ((frameCounter / 18) % 2 == 1)
        {
            return;
        }

        using var brush = new SolidBrush(Color.White);
        g.FillPolygon(
            brush,
            [
                new Point(x, y),
                new Point(x, y + 14),
                new Point(x + 12, y + 7)
            ]);
    }

       private void DrawLanguageOpeningBackdrop(Graphics g)
    {
        var openingImage = GetUiImage("SFC_opening.png");
        if (openingImage is null)
        {
            DrawMenuBackdrop(g);
            return;
        }

        g.Clear(Color.Black);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        const int sourceWidth = 170;
        const int sourceHeight = 140;
        var maxSourceX = Math.Max(0, openingImage.Width - sourceWidth);
        var progress = Math.Clamp(
            languageOpeningElapsedFrames / (float)Math.Max(1, LanguageOpeningTotalFrames),
            0f,
            1f);

        var sourceX = (int)Math.Round(maxSourceX * progress);
        var sourceY = Math.Max(0, openingImage.Height - sourceHeight - 20);

        var destinationRect = new Rectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);

        g.DrawImage(
            openingImage,
            destinationRect,
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);
    }
        private void DrawLanguageOpeningNarration(Graphics g)
    {
        var narration = GetCurrentLanguageOpeningText();
        var alpha = GetLanguageOpeningNarrationAlpha();
        if (string.IsNullOrWhiteSpace(narration) || alpha <= 0f)
        {
            return;
        }

        using var font = new Font("JF-Dot-ShinonomeMin14", 20, FontStyle.Regular, GraphicsUnit.Pixel);
        var textArea = new Rectangle(80, 220, 480, 48);
        var lines = NormalizeTextLines(narration);
        var totalHeight = lines.Count * UiTypography.LineHeight;
        var startY = textArea.Y + Math.Max(0, (textArea.Height - totalHeight) / 2);
        var textOffsetY = Math.Max(0f, (UiTypography.LineHeight - font.Height) / 2f);

        using (var sf = new StringFormat { Alignment = StringAlignment.Center })
        {
            g.DrawString(narration, font, Brushes.Black, textArea, sf);
            g.DrawString(narration, font, Brushes.White, textArea, sf);
        }
    }
    private string GetCurrentLanguageOpeningText()
    {
        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            return string.Empty;
        }

        var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
        return languageOpeningLineFrame < currentLine.DisplayFrames
            ? currentLine.Text
            : string.Empty;
    }

    private float GetLanguageOpeningNarrationAlpha()
    {
        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length)
        {
            return 0f;
        }

        var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
        if (languageOpeningLineFrame >= currentLine.DisplayFrames)
        {
            return 0f;
        }

        var fadeFrames = Math.Min(24, Math.Max(12, currentLine.DisplayFrames / 4));
        if (languageOpeningLineFrame < fadeFrames)
        {
            return languageOpeningLineFrame / (float)fadeFrames;
        }

        var fadeOutStart = currentLine.DisplayFrames - fadeFrames;
        if (languageOpeningLineFrame > fadeOutStart)
        {
            return (currentLine.DisplayFrames - languageOpeningLineFrame) / (float)fadeFrames;
        }

        return 1f;
    }
}
