using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void DrawField(Graphics g)
    {
        DrawFieldScene(g);

        var helpRect = GetFieldHelpWindow();
        var helpInnerRect = Rectangle.Inflate(helpRect, -18, -14);
        DrawWindow(g, helpRect);
        DrawText(g, GetText("fieldHelpLine1"), new Rectangle(helpInnerRect.X, helpInnerRect.Y, helpInnerRect.Width, 18), smallFont);
        DrawText(g, GetText("fieldHelpLine2"), new Rectangle(helpInnerRect.X, helpInnerRect.Y + 28, helpInnerRect.Width, 18), smallFont);
        DrawText(g, GetText("fieldHelpLine3"), new Rectangle(helpInnerRect.X, helpInnerRect.Y + 56, helpInnerRect.Width, 18), smallFont);

        if (isFieldStatusVisible)
        {
            DrawWindow(g, new Rectangle(446, 8, 186, 116));
            DrawText(g, $"{GetDisplayPlayerName()}  Lv.{player.Level}", new Rectangle(458, 22, 160, 24), smallFont);
            DrawText(g, $"HP {player.CurrentHp}/{player.MaxHp}", new Rectangle(458, 48, 160, 24), smallFont);
            DrawText(g, $"MP {player.CurrentMp}/{player.MaxMp}", new Rectangle(458, 72, 160, 24), smallFont);
            DrawText(g, $"G {player.Gold}", new Rectangle(458, 96, 160, 24), smallFont);

            var equipmentRect = new Rectangle(446, 132, 186, CompactFieldViewportHeightTiles * TileSize);
            DrawWindow(g, equipmentRect);
            DrawText(g, $"ATK {GetTotalAttack()}  DEF {GetTotalDefense()}", new Rectangle(458, 146, 160, 24), smallFont);
            DrawText(g, $"EXP {GetExperienceSummary()}", new Rectangle(458, 168, 160, 24), smallFont);
            DrawFieldEquipmentSlot(g, equipmentRect, GetEquipmentSlotLabel(EquipmentSlot.Weapon), EquipmentSlot.Weapon, 196);
            DrawFieldEquipmentSlot(g, equipmentRect, GetEquipmentSlotLabel(EquipmentSlot.Head), EquipmentSlot.Head, 212);
            DrawFieldEquipmentSlot(g, equipmentRect, GetEquipmentSlotLabel(EquipmentSlot.Armor), EquipmentSlot.Armor, 228);
            DrawFieldEquipmentSlot(g, equipmentRect, GetEquipmentSlotLabel(EquipmentSlot.Arms), EquipmentSlot.Arms, 244);
            DrawFieldEquipmentSlot(g, equipmentRect, GetEquipmentSlotLabel(EquipmentSlot.Legs), EquipmentSlot.Legs, 260);
            DrawFieldEquipmentSlot(g, equipmentRect, GetEquipmentSlotLabel(EquipmentSlot.Feet), EquipmentSlot.Feet, 276);
        }

        if (isFieldDialogOpen)
        {
            DrawWindow(g, FieldLayout.DialogWindow);
            var portrait = GetNpcPortrait(activeFieldDialogPortraitAssetName);
            var portraitRect = new Rectangle(FieldLayout.DialogWindow.X + 16, FieldLayout.DialogWindow.Y + 16, 96, 96);
            var textRect = portrait is null
                ? new Rectangle(72, 346, 494, 68)
                : new Rectangle(portraitRect.Right + 18, 346, 566 - (portraitRect.Right + 18), 68);
            var footerRect = portrait is null
                ? new Rectangle(72, 414, 494, 20)
                : new Rectangle(portraitRect.Right + 18, 414, 566 - (portraitRect.Right + 18), 20);

            if (portrait is not null)
            {
                DrawPortraitCover(g, portrait, portraitRect);
            }

            DrawText(g, GetCurrentFieldDialogPage(), textRect, smallFont, wrap: true);
            DrawText(
                g,
                activeFieldDialogPageIndex < activeFieldDialogPages.Count - 1
                    ? (selectedLanguage == UiLanguage.Japanese ? "A/Y/Z: つぎへ" : "A/Y/Z: NEXT")
                    : (selectedLanguage == UiLanguage.Japanese ? "A/Y/Z / B/ESC: とじる" : "A/Y/Z / B/ESC: CLOSE"),
                footerRect,
                smallFont,
                StringAlignment.Far);
        }
    }

    private void DrawFieldEquipmentSlot(Graphics g, Rectangle panelRect, string label, EquipmentSlot slot, int y)
    {
        DrawText(g, label, new Rectangle(panelRect.X + 12, y, 58, 16), smallFont);
        DrawText(g, GetCurrentEquipmentNameForSlot(slot), new Rectangle(panelRect.X + 72, y, 100, 16), smallFont, StringAlignment.Far);
    }

    private void DrawFieldScene(Graphics g)
    {
        DrawMenuBackdrop(g);

        var viewport = GetFieldViewport();
        var cameraOrigin = GetFieldCameraOrigin();
        var movementOffset = GetFieldMovementAnimationOffset();
        var cameraAnimationOffset = GetFieldCameraAnimationOffset(cameraOrigin, movementOffset);
        var playerAnimationOffset = GetPlayerAnimationOffset(cameraOrigin, movementOffset);
        var visibleWidthTiles = GetFieldViewportWidthTiles();
        var visibleHeightTiles = GetFieldViewportHeightTiles();

        var clipState = g.Save();
        g.SetClip(viewport);

        using (var voidBrush = new SolidBrush(GetTileColor(MapFactory.WallTile)))
        {
            g.FillRectangle(voidBrush, viewport);
        }

        if (currentFieldMap == FieldMapId.Field && TryDrawFieldMapImage(g, viewport, cameraOrigin))
        {
            foreach (var fieldEvent in GetRenderableCurrentFieldEvents())
            {
                var sprite = GetNpcSprite(fieldEvent.SpriteAssetName);
                DrawWorldTileEntity(g, fieldEvent.TilePosition, viewport, cameraOrigin, cameraAnimationOffset, fieldEvent.DisplayColor, sprite);
            }

            DrawPlayerTileEntity(g, viewport, cameraOrigin, playerAnimationOffset);

            g.Restore(clipState);
            DrawFieldViewportFrame(g, viewport);
            return;
        }

        for (var y = 0; y < visibleHeightTiles; y++)
        {
            for (var x = 0; x < visibleWidthTiles; x++)
            {
                var worldTile = new Point(cameraOrigin.X + x, cameraOrigin.Y + y);
                var tileRect = GetFieldTileRectangle(viewport, cameraOrigin, worldTile, cameraAnimationOffset);
                var tileId = GetTileIdAtWorldPosition(worldTile);

                if (TryDrawFieldTileSprite(g, worldTile, tileRect))
                {
                    continue;
                }

                using var tileBrush = new SolidBrush(GetTileColor(tileId));
                g.FillRectangle(tileBrush, tileRect);
            }
        }

        foreach (var fieldEvent in GetRenderableCurrentFieldEvents())
        {
            var sprite = GetNpcSprite(fieldEvent.SpriteAssetName);
            DrawWorldTileEntity(g, fieldEvent.TilePosition, viewport, cameraOrigin, cameraAnimationOffset, fieldEvent.DisplayColor, sprite);
        }

        DrawPlayerTileEntity(g, viewport, cameraOrigin, playerAnimationOffset);

        g.Restore(clipState);
        DrawFieldViewportFrame(g, viewport);
    }

    private Color GetTileColor(int tileId)
    {
        return tileId switch
        {
            MapFactory.WallTile when currentFieldMap is FieldMapId.Castle or FieldMapId.Dungeon => Color.FromArgb(58, 14, 24),
            MapFactory.WallTile => Color.FromArgb(8, 30, 90),
            MapFactory.CastleBlockTile => Color.FromArgb(120, 28, 38),
            MapFactory.CastleGateTile => Color.FromArgb(116, 58, 30),
            MapFactory.FieldGateTile => Color.FromArgb(24, 56, 40),
            MapFactory.CastleFloorTile => Color.FromArgb(108, 42, 52),
            MapFactory.GrassTile => Color.FromArgb(24, 74, 36),
            MapFactory.DecorationBlueTile when currentFieldMap is FieldMapId.Castle or FieldMapId.Dungeon => Color.FromArgb(76, 20, 34),
            MapFactory.DecorationBlueTile => Color.FromArgb(8, 30, 90),
            MapFactory.CastleTextWallTile => Color.FromArgb(28, 18, 18),
            MapFactory.CastleTextCarpetTile => Color.FromArgb(184, 36, 28),
            MapFactory.CastleTextTopWallTile => Color.FromArgb(34, 52, 112),
            MapFactory.CastleTextColumnBaseTile => Color.FromArgb(78, 34, 92),
            MapFactory.CastleTextPillarTile => Color.FromArgb(212, 174, 54),
            MapFactory.CastleTextOrnamentTile => Color.FromArgb(238, 208, 88),
            MapFactory.CastleTextRightWallTile => Color.FromArgb(42, 22, 54),
            MapFactory.CastleTextExitTile => Color.FromArgb(64, 44, 32),
            _ => Color.FromArgb(5, 5, 5)
        };
    }

    private void DrawWorldTileEntity(
        Graphics g,
        Point tile,
        Rectangle viewport,
        Point cameraOrigin,
        Point offset,
        Color color,
        Image? sprite = null)
    {
        var rect = Rectangle.Inflate(
            GetFieldTileRectangle(viewport, cameraOrigin, tile, offset),
            -4,
            -4);

        if (sprite is not null)
        {
            g.DrawImage(sprite, rect);
            return;
        }

        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, rect);
    }

    private void DrawPlayerTileEntity(Graphics g, Rectangle viewport, Point cameraOrigin, Point animationOffset)
    {
        var tileRect = GetFieldTileRectangle(viewport, cameraOrigin, player.TilePosition, new Point(-animationOffset.X, -animationOffset.Y));
        var heroSprite = GetHeroSprite();

        if (heroSprite is not null)
        {
            var rect = new Rectangle(
                tileRect.X + ((TileSize - heroSprite.Width) / 2),
                tileRect.Bottom - heroSprite.Height,
                heroSprite.Width,
                heroSprite.Height);
            g.DrawImage(heroSprite, rect);
            return;
        }

        var fallbackRect = Rectangle.Inflate(tileRect, -2, -2);
        using var brush = new SolidBrush(Color.White);
        g.FillRectangle(brush, fallbackRect);
    }

    private void DrawFieldViewportFrame(Graphics g, Rectangle rect)
    {
        using var shadowPen = new Pen(Color.FromArgb(88, 0, 0, 0), 4);
        using var glowPen = new Pen(Color.FromArgb(0, 72, 255), 4);
        using var outerPen = new Pen(Color.FromArgb(0, 120, 255), 2);
        using var innerPen = new Pen(Color.FromArgb(132, 206, 255), 1);

        g.DrawRectangle(shadowPen, rect.X + 6, rect.Y + 6, rect.Width, rect.Height);
        g.DrawRectangle(glowPen, rect);
        g.DrawRectangle(outerPen, rect);
        g.DrawRectangle(innerPen, Rectangle.Inflate(rect, -5, -5));
    }

    private static void DrawPortraitCover(Graphics g, Image portrait, Rectangle bounds)
    {
        var scale = Math.Max(bounds.Width / (float)portrait.Width, bounds.Height / (float)portrait.Height);
        var sourceWidth = Math.Max(1, (int)Math.Round(bounds.Width / scale));
        var sourceHeight = Math.Max(1, (int)Math.Round(bounds.Height / scale));
        var sourceRect = new Rectangle(
            Math.Max(0, (portrait.Width - sourceWidth) / 2),
            Math.Max(0, (portrait.Height - sourceHeight) / 2),
            Math.Min(sourceWidth, portrait.Width),
            Math.Min(sourceHeight, portrait.Height));

        var state = g.Save();
        g.SetClip(bounds);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(portrait, bounds, sourceRect, GraphicsUnit.Pixel);
        g.Restore(state);
    }
}
