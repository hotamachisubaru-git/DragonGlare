using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Persistence;
using DragonGlareAlpha.Services;
using static DragonGlareAlpha.Domain.Constants;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void DrawStaticBackdrops(SpriteBatch batch)
    {
        if (UsesMenuBackdrop())
        {
            batch.Begin(samplerState: SamplerState.PointClamp);
            DrawMenuBackdrop(batch);
            batch.End();
        }
    }

    private void DrawCurrentStateScene(SpriteBatch batch)
    {
        switch (gameState)
        {
            case GameState.LanguageSelection: DrawLanguageSelection(batch); break;
            case GameState.Field:
            case GameState.EncounterTransition: DrawFieldScene(batch); break;
            case GameState.SaveSlotSelection: DrawSaveSlotSelection(batch); break;
            case GameState.Battle: DrawBattleScene(batch); break;
            case GameState.ShopBuy:
            case GameState.Bank: DrawSimpleDialog(batch); break;
            default: DrawMainMenu(batch); break;
        }
    }

    private void DrawLanguageSelection(SpriteBatch batch)
    {
        DrawOpeningBackdrop(batch);
        if (!languageOpeningFinished) DrawOpeningNarration(batch);
        else DrawCursorPanel(batch, languageCursor, 2, 96, 320, 448, 88);
    }

    private void DrawFieldScene(SpriteBatch batch)
    {
        DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), new XnaColor(5, 8, 16));
        var mapWidth = map.GetLength(1);
        var mapHeight = map.GetLength(0);
        var originX = Math.Max(0, (VirtualWidth - (mapWidth * TileSize)) / 2);
        var originY = Math.Max(0, (VirtualHeight - (mapHeight * TileSize)) / 2);
        DrawMap(batch, originX, originY);
        DrawPlayerCharacter(batch, originX, originY);
        DrawFieldTransitionOverlay(batch);
    }

    private void DrawSimpleDialog(SpriteBatch batch) => DrawSolidPanel(batch, new XnaRectangle(80, 80, 480, 320), new XnaColor(22, 28, 36));

    private void DrawMainMenu(SpriteBatch batch)
    {
        var layout = new XnaRectangle(32, 34, 576, 420);
        DrawMenuWindow(batch, layout);
        DrawMenuItems(batch, layout);
        DrawMenuDescription(batch, layout);
        DrawMenuNotice(batch, layout);
    }

    private void DrawMenuWindow(SpriteBatch batch, XnaRectangle layout)
    {
        if (menuWindowTexture is not null) batch.Draw(menuWindowTexture, layout, XnaColor.White);
        else DrawSolidPanel(batch, layout, new XnaColor(48, 45, 50));
    }

    private void DrawMenuItems(SpriteBatch batch, XnaRectangle layout)
    {
        string[] menuItems = { "はじめから", "つづきから", "データうつす", "データけす" };
        var menuStartX = ScaleMenuX(layout, 24);
        var menuStartY = ScaleMenuY(layout, 24);
        var menuLineHeight = ScaleMenuHeight(layout, 24);
        for (var i = 0; i < menuItems.Length; i++)
        {
            var lineY = menuStartY + (i * menuLineHeight);
            if (modeCursor == i) DrawText(batch, "▶", menuStartX - 10, lineY);
            DrawText(batch, menuItems[i], menuStartX, lineY);
        }
    }

    private void DrawMenuDescription(SpriteBatch batch, XnaRectangle layout) => DrawText(batch, GetModeSelectDescription(modeCursor), ScaleMenuX(layout, 128), ScaleMenuY(layout, 24));

    private void DrawMenuNotice(SpriteBatch batch, XnaRectangle layout)
    {
        if (!string.IsNullOrEmpty(menuNotice))
            DrawSolidPanel(batch, new XnaRectangle(ScaleMenuX(layout, 22), ScaleMenuY(layout, 132), ScaleMenuWidth(layout, 210), ScaleMenuHeight(layout, 24)), new XnaColor(74, 35, 35) * 0.35f);

        DrawText(batch, string.IsNullOrWhiteSpace(menuNotice) ? "モードを選んでください。" : menuNotice, ScaleMenuX(layout, 24), ScaleMenuY(layout, 136));
    }

    private void DrawSaveSlotSelection(SpriteBatch batch)
    {
        var isLoadMode = saveSlotSelectionMode == SaveSlotSelectionMode.Load;
        DrawWindow(batch, new XnaRectangle(16, 8, 608, 64));
        DrawText(batch, isLoadMode ? "よみこむ ぼうけんのしょを えらんでください" : "きろくする ぼうけんのしょを えらんでください", 48, 24);
        DrawText(batch, isLoadMode ? "CHOOSE A FILE TO LOAD" : "CHOOSE A FILE TO SAVE", 48, 48);

        for (var i = 0; i < SaveService.SlotCount; i++)
        {
            var slotRect = new XnaRectangle(16, 98 + (i * 98), 608, 82);
            DrawWindow(batch, slotRect);
            if (saveSlotCursor == i)
            {
                DrawSolidPanel(batch, new XnaRectangle(slotRect.X + 18, slotRect.Y + 34, 16, 16), new XnaColor(0, 112, 255));
                DrawText(batch, "▶", slotRect.X + 22, slotRect.Y + 31);
            }
            var summary = saveSlotSummaries.FirstOrDefault(s => s.SlotNumber == i + 1);
            DrawText(batch, $"ぼうけんのしょ {i + 1}", slotRect.X + 54, slotRect.Y + 16);
            DrawSaveSlotSummary(batch, summary, slotRect.X + 54, slotRect.Y + 40);
        }
        if (!string.IsNullOrWhiteSpace(menuNotice)) { DrawWindow(batch, new XnaRectangle(32, 392, 576, 34)); DrawText(batch, menuNotice, 54, 403); }
        DrawWindow(batch, new XnaRectangle(40, 438, 560, 34));
        DrawText(batch, isLoadMode ? "ENTER: よみこむ  ESC: モードにもどる" : "ENTER: きろく  ESC: もどる", 64, 449);
    }

    private void DrawSaveSlotSummary(SpriteBatch batch, SaveSlotSummary? summary, int x, int y)
    {
        if (summary == null || summary.State == SaveSlotState.Empty) { DrawText(batch, "NO DATA / まだ きろくがありません", x, y); return; }
        if (summary.State == SaveSlotState.Corrupted) { DrawText(batch, "BROKEN DATA / よみこめません", x, y); return; }
        DrawText(batch, $"{summary.Name}   LV {summary.Level}   G {summary.Gold}", x, y);
        var savedAt = summary.SavedAtLocal?.ToString("yyyy/MM/dd HH:mm") ?? string.Empty;
        DrawText(batch, $"{summary.CurrentFieldMap.ToString().ToUpperInvariant()}   {savedAt}", x, y + UiTextLineHeight);
    }

    private void DrawText(SpriteBatch batch, string text, int x, int y)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var tex = GetTextTexture(lines[i]);
            if (tex != null) batch.Draw(tex, new Vector2(x, y + (i * UiTextLineHeight)), XnaColor.White);
        }
    }

    private Texture2D? GetTextTexture(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (textTextureCache.TryGetValue(text, out var cached)) return cached;
        if (uiFont is null) return null;

        using var measureBitmap = new Bitmap(1, 1);
        using var measureGraphics = Graphics.FromImage(measureBitmap);
        measureGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        var measured = measureGraphics.MeasureString(text, uiFont, PointF.Empty, StringFormat.GenericTypographic);
        var width = Math.Max(1, (int)Math.Ceiling((double)measured.Width) + 2);
        var height = Math.Max(1, (int)Math.Ceiling((double)measured.Height) + 2);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(System.Drawing.Color.Transparent);
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            using var brush = new SolidBrush(System.Drawing.Color.White);
            g.DrawString(text, uiFont, brush, new PointF(0, 0), StringFormat.GenericTypographic);
        }

        var bounds = new DrawingRectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var bytes = new byte[Math.Abs(data.Stride) * data.Height];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
        bitmap.UnlockBits(data);

        var colors = new XnaColor[bitmap.Width * bitmap.Height];
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
            {
                var src = (y * data.Stride) + (x * 4);
                colors[(y * bitmap.Width) + x] = new XnaColor(bytes[src + 2], bytes[src + 1], bytes[src], bytes[src + 3]);
            }

        var texture = new Texture2D(GraphicsDevice, bitmap.Width, bitmap.Height);
        texture.SetData(colors);
        textTextureCache[text] = texture;
        return texture;
    }

    private void DrawOpeningBackdrop(SpriteBatch batch)
    {
        if (openingTexture is null) { DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), new XnaColor(106, 91, 22)); return; }
        var sourceHeight = Math.Min(OpeningSourceViewportHeight, openingTexture.Height);
        var sourceWidth = Math.Min(openingTexture.Width, (int)(VirtualWidth * (sourceHeight / (float)VirtualHeight)));
        var progress = OpeningScreenFrames == 0 ? 0f : MathHelper.Clamp(languageOpeningElapsedFrames / (float)OpeningScreenFrames, 0f, 1f);
        var sourceX = (int)((openingTexture.Width - sourceWidth) * progress);
        batch.Draw(openingTexture, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), new XnaRectangle(sourceX, 0, sourceWidth, sourceHeight), XnaColor.White);
    }

    private void DrawOpeningNarration(SpriteBatch batch)
    {
        if (languageOpeningFinished || languageOpeningLineIndex >= LanguageOpeningScript.Length) return;
        var currentLine = LanguageOpeningScript[languageOpeningLineIndex];
        if (languageOpeningLineFrame >= currentLine.DisplayFrames) return;

        var alpha = GetOpeningNarrationAlpha(currentLine);
        var lines = currentLine.Text.Replace("\r\n", "\n").Split('\n');
        var startY = Math.Max(0, (VirtualHeight - (lines.Length * UiTextLineHeight)) / 2);
        for (var i = 0; i < lines.Length; i++)
        {
            var tex = GetTextTexture(lines[i]);
            if (tex == null) continue;
            var x = (VirtualWidth - tex.Width) / 2;
            var y = startY + (i * UiTextLineHeight);
            var color = XnaColor.White * alpha;
            var shadow = XnaColor.Black * alpha;
            batch.Draw(tex, new Vector2(x + 1, y), shadow); batch.Draw(tex, new Vector2(x - 1, y), shadow);
            batch.Draw(tex, new Vector2(x, y + 1), shadow); batch.Draw(tex, new Vector2(x, y - 1), shadow);
            batch.Draw(tex, new Vector2(x, y), color);
        }
    }

    private void DrawMap(SpriteBatch batch, int originX, int originY)
    {
        for (var y = 0; y < map.GetLength(0); y++)
            for (var x = 0; x < map.GetLength(1); x++)
                DrawFieldTile(batch, map[y, x], new XnaRectangle(originX + (x * TileSize), originY + (y * TileSize), TileSize, TileSize));
    }

    private void DrawPlayerCharacter(SpriteBatch batch, int originX, int originY)
    {
        DrawSolidPanel(batch, new XnaRectangle(originX + (player.TilePosition.X * TileSize) + 7, originY + (player.TilePosition.Y * TileSize) + 4, 18, 24), GetPlayerColor());
    }

    private void DrawBattleScene(SpriteBatch batch)
    {
        DrawBattleBackground(batch);
        DrawBattleEnemy(batch);
        DrawBattleStatusWindow(batch);
        DrawBattleMessageWindow(batch);
    }

    private void DrawBattleBackground(SpriteBatch batch)
    {
        if (battleFieldTexture is not null) batch.Draw(battleFieldTexture, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), XnaColor.White);
        else { DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), new XnaColor(28, 24, 42)); DrawBattleBrickBackdrop(batch); }
    }

    private void DrawBattleStatusWindow(SpriteBatch batch)
    {
        var r = new XnaRectangle(96, 30, 112, 96);
        DrawWindow(batch, r);
        DrawText(batch, GetDisplayPlayerName(), r.X + 14, r.Y + 14);
        DrawText(batch, $"LV.{player.Level}", r.X + 70, r.Y + 14);
        DrawText(batch, $"HP {player.CurrentHp}/{player.MaxHp}", r.X + 14, r.Y + 36);
        DrawText(batch, $"MP {player.CurrentMp}/{player.MaxMp}", r.X + 14, r.Y + 54);
        DrawText(batch, $"ATK {player.BaseAttack + player.Level}", r.X + 14, r.Y + 72);
    }

    private void DrawBattleMessageWindow(SpriteBatch batch)
    {
        var r = new XnaRectangle(80, 286, 480, 136);
        DrawWindow(batch, r);
        var enemyName = currentEncounter?.Enemy.Name ?? "ラヴァドレイク";
        DrawText(batch, enemyName, r.X + 18, r.Y + 16);
        DrawText(batch, "1匹", r.X + 314, r.Y + 16);
        DrawText(batch, currentEncounter is null ? "HP --/--" : $"HP {currentEncounter.CurrentHp}/{currentEncounter.Enemy.MaxHp}", r.Right - 84, r.Y + 16);
        DrawSolidPanel(batch, new XnaRectangle(r.X + 16, r.Y + 44, r.Width - 32, 1), new XnaColor(104, 126, 156));
        DrawText(batch, $"{enemyName}が あらわれた！", r.X + 18, r.Y + 58);
        DrawText(batch, "ENTER / Z / X: つぎへ", r.Right - 128, r.Bottom - 30);
    }

    private void DrawFieldTile(SpriteBatch batch, int tileId, XnaRectangle dest)
    {
        if (fieldTileTexture is null) DrawSolidPanel(batch, dest, GetTileColor(tileId));
        else batch.Draw(fieldTileTexture, dest, GetFieldTileSource(tileId), XnaColor.White);
    }

    private static XnaRectangle GetFieldTileSource(int id) => id switch {
        MapFactory.WallTile => new XnaRectangle(32, 0, 32, 32),
        MapFactory.CastleBlockTile => new XnaRectangle(0, 64, 32, 32),
        MapFactory.CastleGateTile => new XnaRectangle(64, 96, 32, 32),
        MapFactory.FieldGateTile => new XnaRectangle(96, 32, 32, 32),
        MapFactory.CastleFloorTile => new XnaRectangle(32, 64, 32, 32),
        MapFactory.GrassTile => new XnaRectangle(0, 0, 32, 32),
        MapFactory.DecorationBlueTile => new XnaRectangle(96, 96, 32, 32),
        _ => new XnaRectangle(64, 64, 32, 32)
    };

    private void DrawBattleBrickBackdrop(SpriteBatch batch)
    {
        var brick = new XnaColor(72, 67, 92); var mortar = new XnaColor(28, 24, 42);
        for (var y = 0; y < 210; y += 16)
            for (var x = (y / 16) % 2 == 0 ? 80 : 60; x < VirtualWidth - 60; x += 42)
            {
                DrawSolidPanel(batch, new XnaRectangle(x, y, 40, 14), brick);
                DrawSolidPanel(batch, new XnaRectangle(x, y + 14, 40, 2), mortar);
            }
        DrawSolidPanel(batch, new XnaRectangle(80, 210, 480, 24), new XnaColor(144, 91, 28));
        DrawSolidPanel(batch, new XnaRectangle(80, 234, 480, 52), new XnaColor(76, 24, 58));
    }

    private void DrawBattleEnemy(SpriteBatch batch)
    {
        var cx = VirtualWidth / 2; var cy = 235;
        DrawEllipse(batch, new XnaRectangle(cx - 44, cy - 40, 88, 56), new XnaColor(218, 231, 244));
        DrawSolidPanel(batch, new XnaRectangle(cx - 18, cy - 1, 6, 6), XnaColor.Black);
        DrawSolidPanel(batch, new XnaRectangle(cx + 16, cy - 1, 6, 6), XnaColor.Black);
        DrawSolidPanel(batch, new XnaRectangle(cx - 18, cy + 18, 36, 2), new XnaColor(88, 122, 172));
        DrawSolidPanel(batch, new XnaRectangle(cx - 32, cy - 56, 3, 42), new XnaColor(218, 231, 244));
        DrawSolidPanel(batch, new XnaRectangle(cx + 30, cy - 56, 3, 42), new XnaColor(218, 231, 244));
    }

    private void DrawEllipse(SpriteBatch batch, XnaRectangle bounds, XnaColor color)
    {
        if (pixelTexture is null) return;
        float rx = bounds.Width / 2f, ry = bounds.Height / 2f, cx = bounds.X + rx, cy = bounds.Y + ry;
        for (var y = bounds.Y; y < bounds.Bottom; y++)
        {
            var halfWidth = rx * MathF.Sqrt(MathF.Max(0f, 1f - MathF.Pow((y - cy) / ry, 2)));
            var left = (int)MathF.Round(cx - halfWidth);
            batch.Draw(pixelTexture, new XnaRectangle(left, y, Math.Max(1, (int)MathF.Round(cx + halfWidth) - left), 1), color);
        }
    }

    private void DrawFade(SpriteBatch batch)
    {
        if (startupFadeFrames > 0) DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), XnaColor.Black * (startupFadeFrames / 20f));
        if (pendingGameState is not null && sceneFadeOutFramesRemaining > 0)
            DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), XnaColor.Black * (1f - (sceneFadeOutFramesRemaining / (float)SceneFadeOutDuration)));
    }

    private void DrawCursorPanel(SpriteBatch batch, int cursor, int count, int x, int y, int w, int h)
    {
        DrawSolidPanel(batch, new XnaRectangle(x, y, w, h), new XnaColor(36, 34, 42));
        if (cursor < 0) return;
        var rh = Math.Max(1, h / count);
        DrawSolidPanel(batch, new XnaRectangle(x + 16, y + (cursor * rh) + 8, w - 32, Math.Max(8, rh - 16)), new XnaColor(0, 96, 180));
    }

    private void DrawSolidPanel(SpriteBatch batch, XnaRectangle rect, XnaColor color) { if (pixelTexture != null) batch.Draw(pixelTexture, rect, color); }
    private void DrawWindow(SpriteBatch batch, XnaRectangle rect) { DrawSolidPanel(batch, rect, new XnaColor(23, 22, 34)); DrawWindowFrame(batch, rect); }
    private void DrawWindowFrame(SpriteBatch batch, XnaRectangle r)
    {
        DrawBorder(batch, r, 3, new XnaColor(0, 78, 160));
        DrawBorder(batch, new XnaRectangle(r.X + 3, r.Y + 3, r.Width - 6, r.Height - 6), 2, new XnaColor(34, 132, 210));
        DrawBorder(batch, new XnaRectangle(r.X + 7, r.Y + 7, r.Width - 14, r.Height - 14), 1, new XnaColor(30, 58, 98));
    }

    private void DrawBorder(SpriteBatch batch, XnaRectangle r, int t, XnaColor c)
    {
        DrawSolidPanel(batch, new XnaRectangle(r.X, r.Y, r.Width, t), c);
        DrawSolidPanel(batch, new XnaRectangle(r.X, r.Bottom - t, r.Width, t), c);
        DrawSolidPanel(batch, new XnaRectangle(r.X, r.Y, t, r.Height), c);
        DrawSolidPanel(batch, new XnaRectangle(r.Right - t, r.Y, t, r.Height), c);
    }

    private void DrawMenuBackdrop(SpriteBatch batch) => DrawSolidPanel(batch, new XnaRectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), XnaColor.Black);
    private void DrawFieldTransitionOverlay(SpriteBatch batch)
    {
        if (gameState == GameState.EncounterTransition)
            DrawSolidPanel(batch, new XnaRectangle(0, 0, VirtualWidth, VirtualHeight), XnaColor.Black * MathHelper.Clamp(encounterTransitionFrames / (float)EncounterTransitionDuration, 0f, 1f));
    }

    private static int ScaleMenuX(XnaRectangle l, int x) => l.X + (int)Math.Round(x * l.Width / 256f);
    private static int ScaleMenuY(XnaRectangle l, int y) => l.Y + (int)Math.Round(y * l.Height / 240f);
    private static int ScaleMenuWidth(XnaRectangle l, int w) => (int)Math.Round(w * l.Width / 256f);
    private static int ScaleMenuHeight(XnaRectangle l, int h) => (int)Math.Round(h * l.Height / 240f);

    private XnaColor GetTileColor(int id) => id switch {
        MapFactory.WallTile => currentFieldMap == FieldMapId.Castle ? new XnaColor(58, 14, 24) : new XnaColor(8, 30, 90),
        MapFactory.CastleBlockTile => new XnaColor(120, 28, 38),
        MapFactory.GrassTile => new XnaColor(24, 74, 36),
        _ => new XnaColor(18, 18, 18)
    };

    private XnaColor GetPlayerColor() => playerFacingDirection switch {
        PlayerFacingDirection.Left => new XnaColor(210, 235, 255),
        PlayerFacingDirection.Right => new XnaColor(255, 235, 210),
        PlayerFacingDirection.Up => new XnaColor(220, 255, 220),
        _ => XnaColor.White
    };

    private float GetOpeningNarrationAlpha(OpeningNarrationLine line)
    {
        if (languageOpeningLineFrame < OpeningNarrationFadeFrames) return languageOpeningLineFrame / (float)OpeningNarrationFadeFrames;
        var fadeOutStart = Math.Max(OpeningNarrationFadeFrames, line.DisplayFrames - OpeningNarrationFadeFrames);
        if (languageOpeningLineFrame >= fadeOutStart) return Math.Max(0f, (line.DisplayFrames - languageOpeningLineFrame) / (float)OpeningNarrationFadeFrames);
        return 1f;
    }

    private static string GetModeSelectDescription(int cursor) => cursor switch {
        0 => "ゲームを最初から\nはじめる。",
        1 => "保存したデータから\nつづきをはじめる。",
        2 => "セーブデータを\nうつす。",
        3 => "セーブデータを\nけす。",
        _ => string.Empty
    };

    private string GetDisplayPlayerName() => string.IsNullOrWhiteSpace(player.Name) ? (selectedLanguage == UiLanguage.Japanese ? "ゆうしゃ" : "HERO") : player.Name;
    private XnaMatrix GetVirtualTransform()
    {
        var vp = GraphicsDevice.Viewport;
        var s = Math.Min(vp.Width / (float)VirtualWidth, vp.Height / (float)VirtualHeight);
        return XnaMatrix.CreateScale(s, s, 1f) * XnaMatrix.CreateTranslation((vp.Width - (VirtualWidth * s)) / 2f, (vp.Height - (VirtualHeight * s)) / 2f, 0f);
    }
    private bool UsesMenuBackdrop() => gameState == GameState.ModeSelect || gameState == GameState.NameInput || gameState == GameState.SaveSlotSelection;
}
