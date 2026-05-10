using System.Drawing.Drawing2D;
using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Persistence;
using System.Drawing.Text;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void DrawStartupOptions(Graphics g)
    {
        g.Clear(Color.Black);
        DrawWindow(g, new Rectangle(64, 40, 512, 400));

        DrawText(g, "起動設定", 96, 70, uiFont);

        var modes = new[] { "ウィンドウ 640x480（標準）", "ウィンドウ 1280x720（720p）", "ウィンドウ 1920x1080（1080p）", "フルスクリーン" };

        DrawText(g, "画面モード:", 110, 120, smallFont);
        for (int i = 0; i < modes.Length; i++)
        {
            var isSelected = (optionsCursor == i);
            if (isSelected) DrawText(g, "▶", 120, 150 + (i * 30), smallFont);
            var activeMarker = activeDisplayMode == (LaunchDisplayMode)i ? "[x]" : "[ ]";
            DrawText(g, $"{activeMarker} {modes[i]}", 150, 150 + (i * 30), smallFont);
        }

        bool isPromptSelected = (optionsCursor == 4);
        if (isPromptSelected) DrawText(g, "▶", 120, 300, smallFont);
        DrawText(g, $"次回もこの画面を表示: [{(launchSettings.PromptOnStartup ? "はい" : "いいえ")}]", 150, 300, smallFont);

        bool isStartSelected = (optionsCursor == 5);
        if (isStartSelected) DrawText(g, "▶", 240, 370, uiFont);
        DrawText(g, "ゲームをはじめる", 250, 370, uiFont);

        DrawText(g, "十字/LS: えらぶ  A/Y/Enter/Z: 変更・決定", 96, 410, smallFont);
    }

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
            selectedLanguage == UiLanguage.English ? "NEW GAME" : "はじめから",
            selectedLanguage == UiLanguage.English ? "CONTINUE" : "つづきから",
            selectedLanguage == UiLanguage.English ? "COPY DATA" : "データうつす",
            selectedLanguage == UiLanguage.English ? "DELETE DATA" : "データけす"
        };

        var menuStartX = ScaleModeSelectX(layoutRect, 28);
        var menuStartY = ScaleModeSelectY(layoutRect, 24);
        var menuLineHeight = ScaleModeSelectHeight(layoutRect, 24);
        var menuCursorX = ScaleModeSelectX(layoutRect, 20);

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
            string.IsNullOrWhiteSpace(menuNotice)
                ? selectedLanguage == UiLanguage.English ? "Choose a mode." : "モードを選んでください。"
                : menuNotice,
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

        DrawLanguageSelectionLayout(g);

        DrawLanguageOption(g, languageCursor == 0, 104, 78, "にほんご");
        DrawLanguageOption(g, languageCursor == 1, 104, 118, "ENGLISH");

        DrawText(g, "げんごをえらんでください", 140, 276);
        DrawText(g, "CHOOSE A LANGUAGE", 140, 316);
        DrawText(g, "A/Y/ENTER/Z: けってい  B/ESC: もどる", 140, 356, smallFont);
    }

    private void DrawLanguageSelectionLayout(Graphics g)
    {
        var layoutImage = GetUiImage("SFC_language.png");
        if (layoutImage is not null)
        {
            var previousInterpolationMode = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(layoutImage, new Rectangle(0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight));
            g.InterpolationMode = previousInterpolationMode;
            return;
        }

        g.Clear(Color.Black);
        DrawWindow(g, new Rectangle(75, 46, 241, 120));
        DrawWindow(g, new Rectangle(108, 236, 422, 187));
    }

    private void DrawLanguageOption(Graphics g, bool selected, int x, int y, string text)
    {
        if (selected)
        {
            DrawLanguageSelectionCursor(g, x - 28, y + 4);
        }

        DrawText(g, text, x, y);
    }

    private void DrawLanguageSelectionCursor(Graphics g, int x, int y)
    {
        if ((frameCounter / 18) % 2 == 1)
        {
            return;
        }

        DrawMenuCursorArrow(g, x, y);
    }

    private void DrawNameInput(Graphics g)
    {
        g.Clear(Color.Black);

        var table = GameContent.GetNameTable(selectedLanguage);
        const int originX = 34;
        const int originY = 40;
        const int tableBottomY = 248;
        var maxColumns = Math.Max(1, table.Max(row => row.Length));
        var cellWidth = Math.Min(56, (UiCanvas.VirtualWidth - (originX * 2)) / maxColumns);
        var cellHeight = Math.Min(44, (tableBottomY - originY) / Math.Max(1, table.Length));

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
        DrawText(g, selectedLanguage == UiLanguage.English ? "CHOOSE A NAME" : "なまえをきめてください", 140, 300);
        DrawText(g, playerName.Length == 0 ? "..." : playerName.ToString(), 140, 384);

        DrawText(
            g,
            selectedLanguage == UiLanguage.Japanese
                ? "A/Y/Enter/Z: 入力  X/Back: けす  B/Esc: もどる"
                : "A/Y/Enter/Z: INPUT  X/Back: DEL  B/Esc: BACK",
            14,
            458,
            smallFont);
    }

    private void DrawSaveSlotSelection(Graphics g)
    {
        DrawMenuBackdrop(g);

        const int slotWindowX = 98;
        const int slotWindowWidth = 444;

        var titleRect = new Rectangle(slotWindowX, 14, slotWindowWidth, 70);
        DrawWindow(g, titleRect);
        DrawText(
            g,
            GetSaveSlotSelectionTitle(),
            new Rectangle(titleRect.X + 32, titleRect.Y + 24, titleRect.Width - 64, 24),
            smallFont);

        for (var index = 0; index < SaveService.SlotCount; index++)
        {
            var slotNumber = index + 1;
            var summary = saveSlotSummaries.ElementAtOrDefault(index) ?? new SaveSlotSummary
            {
                SlotNumber = slotNumber,
                State = SaveSlotState.Empty
            };

            var slotRect = new Rectangle(slotWindowX, 98 + (index * 80), slotWindowWidth, 72);
            DrawWindow(g, slotRect);
            if (saveSlotCursor == index)
            {
                DrawSelectionMarker(g, slotRect.X + 14, slotRect.Y + 24);
            }

            DrawText(g, selectedLanguage == UiLanguage.English ? $"FILE {slotNumber}" : $"ぼうけんのしょ {slotNumber}", slotRect.X + 38, slotRect.Y + 8, smallFont);
            var operationBadge = GetSaveSlotOperationBadge(slotNumber);
            if (!string.IsNullOrWhiteSpace(operationBadge))
            {
                DrawText(g, operationBadge, new Rectangle(slotRect.Right - 134, slotRect.Y + 8, 96, 16), smallFont, StringAlignment.Far);
            }

            switch (summary.State)
            {
                case SaveSlotState.Occupied:
                    DrawText(g, $"{summary.Name}   LV {summary.Level}   G {summary.Gold}", slotRect.X + 38, slotRect.Y + 28, smallFont);
                    DrawText(
                        g,
                        $"{GetMapDisplayName(summary.CurrentFieldMap, selectedLanguage)}  {summary.SavedAtLocal:yyyy/MM/dd HH:mm}",
                        new Rectangle(slotRect.X + 38, slotRect.Y + 48, 380, 16),
                        smallFont);
                    break;
                case SaveSlotState.Corrupted:
                    DrawText(g, selectedLanguage == UiLanguage.English ? "BROKEN DATA" : "BROKEN DATA / よみこめません", slotRect.X + 38, slotRect.Y + 30, smallFont);
                    break;
                default:
                    DrawText(g, selectedLanguage == UiLanguage.English ? "NO DATA" : "NO DATA / まだ きろくがありません", slotRect.X + 38, slotRect.Y + 30, smallFont);
                    break;
            }
        }

        var helpRect = new Rectangle(slotWindowX, 390, slotWindowWidth, 56);
        DrawWindow(g, helpRect);
        DrawText(
            g,
            GetSaveSlotSelectionHelpText(),
            new Rectangle(helpRect.X + 32, helpRect.Y + 18, helpRect.Width - 64, 24),
            smallFont);

        if (!string.IsNullOrWhiteSpace(menuNotice))
        {
            var noticeRect = new Rectangle(slotWindowX, 344, slotWindowWidth, 30);
            DrawWindow(g, noticeRect);
            DrawText(
                g,
                menuNotice,
                new Rectangle(noticeRect.X + 18, noticeRect.Y + 5, noticeRect.Width - 36, 20),
                smallFont,
                lineAlignment: StringAlignment.Center);
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

    private string GetModeSelectDescription(int cursor)
    {
        if (selectedLanguage == UiLanguage.English)
        {
            return cursor switch
            {
                0 => "Start a new\nadventure.",
                1 => "Continue from\nsaved data.",
                2 => "Copy data to\nanother slot.",
                3 => "Delete unwanted\ndata.",
                _ => string.Empty
            };
        }

        return cursor switch
        {
            0 => "ゲームを最初から\nはじめる。",
            1 => "前回のつづきから\nはじめる。",
            2 => "データを別の枠へ\nうつす。",
            3 => "いらないデータを\nけす。",
            _ => string.Empty
        };
    }

    private string GetSaveSlotSelectionTitle()
    {
        return selectedLanguage == UiLanguage.English
            ? GetSaveSlotSelectionTitleEnglish()
            : GetSaveSlotSelectionTitleJapanese();
    }

    private string GetSaveSlotSelectionTitleJapanese()
    {
        return saveSlotSelectionMode switch
        {
            SaveSlotSelectionMode.Save => "ぼうけんのしょを えらんでください",
            SaveSlotSelectionMode.Load => "よみこむ ぼうけんのしょを えらんでください",
            SaveSlotSelectionMode.CopySource => "うつす ぼうけんのしょを えらんでください",
            SaveSlotSelectionMode.CopyDestination => "うつすさきを えらんでください",
            SaveSlotSelectionMode.DeleteSelect => "けす ぼうけんのしょを えらんでください",
            SaveSlotSelectionMode.DeleteConfirm => "ほんとうに けしますか？",
            _ => string.Empty
        };
    }

    private string GetSaveSlotSelectionTitleEnglish()
    {
        return saveSlotSelectionMode switch
        {
            SaveSlotSelectionMode.Save => "CHOOSE A SAVE SLOT",
            SaveSlotSelectionMode.Load => "CHOOSE A FILE TO LOAD",
            SaveSlotSelectionMode.CopySource => "CHOOSE A FILE TO COPY",
            SaveSlotSelectionMode.CopyDestination => "CHOOSE A DESTINATION",
            SaveSlotSelectionMode.DeleteSelect => "CHOOSE A FILE TO DELETE",
            SaveSlotSelectionMode.DeleteConfirm => "CONFIRM DELETE",
            _ => string.Empty
        };
    }

    private string GetSaveSlotSelectionHelpText()
    {
        if (selectedLanguage == UiLanguage.English)
        {
            return saveSlotSelectionMode switch
            {
                SaveSlotSelectionMode.Save => "A/Enter/Z: SAVE  B/Esc: NAME",
                SaveSlotSelectionMode.Load => "A/Enter/Z: LOAD  B/Esc: MODE",
                SaveSlotSelectionMode.CopySource => "A/Enter/Z: SOURCE  B/Esc: MODE",
                SaveSlotSelectionMode.CopyDestination => "A/Enter/Z: COPY  B/Esc: BACK",
                SaveSlotSelectionMode.DeleteSelect => "A/Enter/Z: DELETE  B/Esc: MODE",
                SaveSlotSelectionMode.DeleteConfirm => "A/Enter/Z: DELETE  B/Esc: CANCEL",
                _ => string.Empty
            };
        }

        return saveSlotSelectionMode switch
        {
            SaveSlotSelectionMode.Save => "A/Enter/Z: きろく  B/Esc: なまえにもどる",
            SaveSlotSelectionMode.Load => "A/Enter/Z: よみこむ  B/Esc: モードにもどる",
            SaveSlotSelectionMode.CopySource => "A/Enter/Z: うつすもと  B/Esc: モードにもどる",
            SaveSlotSelectionMode.CopyDestination => "A/Enter/Z: うつす  B/Esc: もどる",
            SaveSlotSelectionMode.DeleteSelect => "A/Enter/Z: けすデータ  B/Esc: モードにもどる",
            SaveSlotSelectionMode.DeleteConfirm => "A/Enter/Z: けす  B/Esc: やめる",
            _ => string.Empty
        };
    }

    private string GetSaveSlotOperationBadge(int slotNumber)
    {
        if (slotNumber != dataOperationSourceSlot)
        {
            return string.Empty;
        }

        return saveSlotSelectionMode switch
        {
            SaveSlotSelectionMode.CopyDestination => selectedLanguage == UiLanguage.English ? "SOURCE" : "うつすもと",
            SaveSlotSelectionMode.DeleteConfirm => selectedLanguage == UiLanguage.English ? "DELETE" : "けすデータ",
            _ => string.Empty
        };
    }

    private void DrawModeSelectCursor(Graphics g, int x, int y)
    {
        if ((frameCounter / 18) % 2 == 1)
        {
            return;
        }

        DrawMenuCursorArrow(g, x, y);
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
        var sourceWidth = Math.Min(OpeningSourceViewportWidth, openingImage.Width);
        var sourceHeight = Math.Min(OpeningSourceViewportHeight, openingImage.Height);
        var maxSourceX = Math.Max(0, openingImage.Width - sourceWidth);

        var progress = Math.Clamp(
            languageOpeningElapsedFrames / (float)Math.Max(1, LanguageOpeningTotalFrames),
            0f,
            1f);

        var sourceX = Math.Clamp((int)Math.Round(maxSourceX * progress), 0, maxSourceX);
        var sourceY = 0;
        languageOpeningLastSourceX = sourceX;
        languageOpeningLastSourceY = sourceY;

        var destinationRect = GetOpeningViewportDestination(sourceWidth, sourceHeight);
        var previousInterpolationMode = g.InterpolationMode;
        var previousPixelOffsetMode = g.PixelOffsetMode;
        try
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(
                openingImage,
                destinationRect,
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);
        }
        finally
        {
            g.InterpolationMode = previousInterpolationMode;
            g.PixelOffsetMode = previousPixelOffsetMode;
        }
    }

    private static Rectangle GetOpeningViewportDestination(int sourceWidth, int sourceHeight)
    {
        var scale = Math.Max(1, Math.Min(
            UiCanvas.VirtualWidth / Math.Max(1, sourceWidth),
            UiCanvas.VirtualHeight / Math.Max(1, sourceHeight)));
        var width = sourceWidth * scale;
        var height = sourceHeight * scale;
        return new Rectangle(
            (UiCanvas.VirtualWidth - width) / 2,
            (UiCanvas.VirtualHeight - height) / 2,
            width,
            height);
    }

    private void DrawLanguageOpeningNarration(Graphics g)
    {
        var narration = GetCurrentLanguageOpeningText();
        var alpha = GetLanguageOpeningNarrationAlpha();
        if (string.IsNullOrWhiteSpace(narration) || alpha <= 0f)
        {
            return;
        }

        var lines = NormalizeTextLines(narration);
        var totalHeight = lines.Count * UiTypography.LineHeight;
        var openingDestination = GetOpeningViewportDestination(OpeningSourceViewportWidth, OpeningSourceViewportHeight);
        
        int startY = openingDestination.Y + 220 + Math.Max(0, (48 - totalHeight) / 2);

        using var textBitmap = new Bitmap(UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
        using var textGraphics = Graphics.FromImage(textBitmap);
        
        textGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
        textGraphics.SmoothingMode = SmoothingMode.None;
        textGraphics.PixelOffsetMode = PixelOffsetMode.None;
        
        var shadowColor = Color.FromArgb((int)Math.Round(255f * alpha), 0, 0, 0);
        using var shadowBrush = new SolidBrush(shadowColor);
        using var mainBrush = new SolidBrush(Color.FromArgb((int)Math.Round(255f * alpha), 255, 255, 255));

        var offsets = new[]
        {
            new Point(0, -1), new Point(-1, 0), new Point(1, 0), new Point(0, 1)
        };

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var y = startY + (i * UiTypography.LineHeight);

            var textWidth = MeasureTextWidth(textGraphics, line, uiFont);
            int x = openingDestination.X + (int)Math.Floor((openingDestination.Width - textWidth) / 2f);

            foreach (var off in offsets)
            {
                DrawTextLine(textGraphics, line, uiFont, shadowBrush, x + off.X, y + off.Y);
            }

            DrawTextLine(textGraphics, line, uiFont, mainBrush, x, y);
        }

        g.DrawImage(textBitmap, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
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
