using System.Drawing.Drawing2D;
using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Persistence;
using System.Drawing.Text;

namespace DragonGlareAlpha;

public partial class DragonGlare
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
            // レイアウト画像がない場合、デフォルトのウィンドウを描画
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
        var menuCursorX = ScaleModeSelectX(layoutRect, 16);

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

        // オーバーレイブラシを使用して背景を透明化
        using var overlayBrush = new SolidBrush(Color.Transparent);
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

    // レイアウト矩形に基づいてモード選択のX座標をスケーリング
    private static int ScaleModeSelectX(Rectangle layoutRect, int sourceX)
    {
        return layoutRect.X + (int)Math.Round(sourceX * (layoutRect.Width / 256f));
    }

    // レイアウト矩形に基づいてモード選択のY座標をスケーリング
    private static int ScaleModeSelectY(Rectangle layoutRect, int sourceY)
    {
        return layoutRect.Y + (int)Math.Round(sourceY * (layoutRect.Height / 240f));
    }

    // レイアウト矩形に基づいてモード選択の幅をスケーリング
    private static int ScaleModeSelectWidth(Rectangle layoutRect, int sourceWidth)
    {
        return (int)Math.Round(sourceWidth * (layoutRect.Width / 256f));
    }

    // レイアウト矩形に基づいてモード選択の高さをスケーリング
    private static int ScaleModeSelectHeight(Rectangle layoutRect, int sourceHeight)
    {
        return (int)Math.Round(sourceHeight * (layoutRect.Height / 240f));
    }

    // モード選択カーソルの説明を取得
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

    // モード選択カーソルを描画
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

    // 言語オープニングの背景を描画
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

        // 仮想キャンバスのアスペクト比に近似したソース矩形を選択し、
        // ソース画像で境界を設定します。これにより、より中心に位置するトリミングが得られ、
        // 視覚的に興味深いポイントの周りをパンできます。
        var destW = UiCanvas.VirtualWidth;
        var destH = UiCanvas.VirtualHeight;

        // 目的のアスペクト比を維持するソースサイズを計算します。
        var srcAspect = openingImage.Width / (float)openingImage.Height;
        var destAspect = destW / (float)destH;

        int sourceWidth, sourceHeight;
        if (srcAspect > destAspect)
        {
            // ソースが幅広: 幅を制限
            sourceHeight = Math.Min(openingImage.Height, Math.Max(1, (int)Math.Round(openingImage.Height * 0.9)));
            sourceWidth = Math.Min(openingImage.Width, (int)Math.Round(sourceHeight * destAspect));
        }
        else
        {
            // ソースが縦長: 高さを制限
            sourceWidth = Math.Min(openingImage.Width, Math.Max(1, (int)Math.Round(openingImage.Width * 0.9)));
            sourceHeight = Math.Min(openingImage.Height, (int)Math.Round(sourceWidth / destAspect));
        }


        var maxSourceX = Math.Max(0, openingImage.Width - sourceWidth);
        var maxSourceY = Math.Max(0, openingImage.Height - sourceHeight);

        var progress = Math.Clamp(
            languageOpeningElapsedFrames / (float)Math.Max(1, LanguageOpeningTotalFrames),
            0f,
            1f);

        // 重要なスプライト（ハート、キャラクター）が見えるように、中心よりわずかに右に焦点を合わせます。
        // その後、進行に合わせてスムーズにパンします。
        var focusNormX = 0.6f;
        var focusX = (int)Math.Round(openingImage.Width * focusNormX);
        var centerSourceX = Math.Clamp(focusX - sourceWidth / 2, 0, maxSourceX);

        // 全体的な進行度に基づいてより大きなパンを適用して動きを与えます。
        var panRange = Math.Max(0, maxSourceX);
        // スクロール速度を気持ち早め（0.9に調整）
        var eased = (float)(0.9 * progress * progress * (3 - 2 * progress));
        var targetSourceX = Math.Clamp((int)Math.Round(centerSourceX + (panRange * (eased - 0.5f) * 0.9f)), 0, maxSourceX);

        // 上部のスプライトを表示するソースYを優先します。下から少し上に移動します。
        var preferredY = Math.Max(0, openingImage.Height - sourceHeight - 40);
        var targetSourceY = Math.Clamp(preferredY - (int)Math.Round((maxSourceY) * (progress * 0.2f)), 0, maxSourceY);

        // 最後に描画されたソース座標から補間することで、小さな整数値のジャンプをスムーズにします。
        var lerp = 0.35f; // 各フレームで目標に向かって移動する割合（高いほどスムーズ）
        if (languageOpeningLastSourceX < 0)
        {
            languageOpeningLastSourceX = targetSourceX;
        }

        if (languageOpeningLastSourceY < 0)
        {
            languageOpeningLastSourceY = targetSourceY;
        }

        var sourceX = (int)Math.Round(languageOpeningLastSourceX + (targetSourceX - languageOpeningLastSourceX) * lerp);
        var sourceY = (int)Math.Round(languageOpeningLastSourceY + (targetSourceY - languageOpeningLastSourceY) * lerp);

        languageOpeningLastSourceX = sourceX;
        languageOpeningLastSourceY = sourceY;

        var destinationRect = new Rectangle(0, 0, destW, destH);

        // パンニング時、知覚されるステップを減らすために、より高品質な補間を使用します。
        var prevMode = g.InterpolationMode;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(
            openingImage,
            destinationRect,
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);
        g.InterpolationMode = prevMode;
    }
    // 言語オープニングのナレーションを描画
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
        
        // 仮想キャンバス(256x240)の中央付近に配置。Y座標を完全に整数に固定。
        int startY = 220 + Math.Max(0, (48 - totalHeight) / 2);

        // ドットの崩れを防ぐため、一度等倍(1x)のビットマップにテキストを描画
        using var textBitmap = new Bitmap(UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
        using var textGraphics = Graphics.FromImage(textBitmap);
        
        // テキスト用の Graphics 設定（スケールなし、等倍描画）
        textGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
        textGraphics.SmoothingMode = SmoothingMode.None;
        textGraphics.PixelOffsetMode = PixelOffsetMode.None;

        // フォントは14pxの基準サイズ
        using var font = new Font("JF-Dot-ShinonomeMin14", 14, FontStyle.Regular, GraphicsUnit.Pixel);
        
        var shadowColor = Color.FromArgb((int)Math.Round(255f * alpha), 0, 0, 0); // 完全な黒
        using var shadowBrush = new SolidBrush(shadowColor);
        using var mainBrush = new SolidBrush(Color.FromArgb((int)Math.Round(255f * alpha), 255, 255, 255));

        // StringFormatのセンタリングを使用せず、左揃えで描画（座標を手動計算するため）
        using (var sf = new StringFormat { Alignment = StringAlignment.Near, FormatFlags = StringFormatFlags.MeasureTrailingSpaces })
        {
            // 動画と同じ十字4方向の1pxアウトライン
            var offsets = new[]
            {
                new Point(0, -1), new Point(-1, 0), new Point(1, 0), new Point(0, 1)
            };

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var y = startY + (i * UiTypography.LineHeight);
                
                // テキストの幅を計測して、X座標を手動で計算する
                var textSize = textGraphics.MeasureString(line, font, UiCanvas.VirtualWidth, sf);
                // 画面中央に配置するため、X座標を計算し、強制的に整数（ピクセルグリッド）に合わせる
                int x = (int)Math.Floor((UiCanvas.VirtualWidth - textSize.Width) / 2f);

                // 細いアウトラインを描画（裏画面へ）
                foreach (var off in offsets)
                {
                    textGraphics.DrawString(line, font, shadowBrush, x + off.X, y + off.Y, sf);
                }

                // メインの白い文字（裏画面へ）
                textGraphics.DrawString(line, font, mainBrush, x, y, sf);
            }
        }

        // 最後に、描画済みのビットマップを実際の画面（拡大スケール適用済み）に描画する。
        g.DrawImage(textBitmap, 0, 0, UiCanvas.VirtualWidth, UiCanvas.VirtualHeight);
    }
    // 現在の言語オープニングテキストを取得
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

    // 言語オープニングナレーションのアルファ値を取得
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
