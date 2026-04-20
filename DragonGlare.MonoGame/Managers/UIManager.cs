using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace DragonGlare.Managers
{
    public readonly record struct FieldUiStatus(
        string PlayerName,
        int Level,
        int CurrentHp,
        int MaxHp,
        int CurrentMp,
        int MaxMp,
        int Gold,
        int Experience,
        int Attack,
        int Defense,
        string WeaponName,
        string ArmorName,
        string LocationName);

    public static class UIManager
    {
        public static void DrawHPBar(SpriteBatch spriteBatch, XnaVector2 position, int current, int max, int width = 200, int height = 20)
        {
            DrawResourceBar(spriteBatch, position, "HP", current, max, XnaColor.LimeGreen, XnaColor.Orange, XnaColor.Red, width, height);
        }

        public static void DrawMPBar(SpriteBatch spriteBatch, XnaVector2 position, int current, int max, int width = 200, int height = 20)
        {
            DrawResourceBar(spriteBatch, position, "MP", current, max, XnaColor.CornflowerBlue, XnaColor.DeepSkyBlue, XnaColor.SlateBlue, width, height);
        }

        public static void DrawGold(SpriteBatch spriteBatch, int gold, XnaVector2 position)
        {
            DrawText(spriteBatch, $"G {gold}", position, XnaColor.Gold);
        }

        public static void DrawUI(SpriteBatch spriteBatch, FieldUiStatus status)
        {
            DrawFieldUI(spriteBatch, status);
        }

        public static void DrawUI(SpriteBatch spriteBatch, int gold, int hp, int maxHp)
        {
            DrawFieldUI(
                spriteBatch,
                new FieldUiStatus(
                    "PLAYER",
                    1,
                    hp,
                    maxHp,
                    0,
                    0,
                    gold,
                    0,
                    0,
                    0,
                    string.Empty,
                    string.Empty,
                    string.Empty));
        }

        public static void DrawFieldUI(SpriteBatch spriteBatch, FieldUiStatus status)
        {
            DrawFieldUI(spriteBatch, status, new XnaVector2(16, 16));
        }

        public static void DrawFieldUI(SpriteBatch spriteBatch, FieldUiStatus status, XnaVector2 position)
        {
            var hasCombatStats = status.Attack > 0 || status.Defense > 0;
            var hasEquipment = !string.IsNullOrWhiteSpace(status.WeaponName) || !string.IsNullOrWhiteSpace(status.ArmorName);
            var width = 260;
            var height = 106 + (status.MaxMp > 0 ? 28 : 0) + (hasCombatStats ? 24 : 0) + (hasEquipment ? 24 : 0);
            DrawPanel(spriteBatch, new XnaRectangle((int)position.X, (int)position.Y, width, height));

            var textX = position.X + 14;
            var y = position.Y + 10;
            DrawText(spriteBatch, $"{status.PlayerName}  Lv.{status.Level}", new XnaVector2(textX, y), XnaColor.White);

            if (!string.IsNullOrWhiteSpace(status.LocationName))
            {
                DrawText(spriteBatch, status.LocationName, new XnaVector2(position.X + width - 14, y), XnaColor.LightSteelBlue, alignRight: true);
            }

            y += 28;
            DrawHPBar(spriteBatch, new XnaVector2(textX, y), status.CurrentHp, status.MaxHp, width - 28);

            if (status.MaxMp > 0)
            {
                y += 28;
                DrawMPBar(spriteBatch, new XnaVector2(textX, y), status.CurrentMp, status.MaxMp, width - 28);
            }

            y += 30;
            DrawText(spriteBatch, $"G {status.Gold}", new XnaVector2(textX, y), XnaColor.Gold);

            if (status.Experience > 0)
            {
                DrawText(spriteBatch, $"EXP {status.Experience}", new XnaVector2(position.X + width - 14, y), XnaColor.White, alignRight: true);
            }

            if (hasCombatStats)
            {
                y += 24;
                DrawText(spriteBatch, $"ATK {status.Attack}  DEF {status.Defense}", new XnaVector2(textX, y), XnaColor.White);
            }

            if (hasEquipment)
            {
                y += 24;
                var weapon = string.IsNullOrWhiteSpace(status.WeaponName) ? "-" : status.WeaponName;
                var armor = string.IsNullOrWhiteSpace(status.ArmorName) ? "-" : status.ArmorName;
                DrawText(spriteBatch, $"ぶき {weapon}  ぼうぐ {armor}", new XnaVector2(textX, y), XnaColor.White);
            }
        }

        private static void DrawResourceBar(
            SpriteBatch spriteBatch,
            XnaVector2 position,
            string label,
            int current,
            int max,
            XnaColor highColor,
            XnaColor middleColor,
            XnaColor lowColor,
            int width,
            int height)
        {
            var percentage = max <= 0
                ? 0f
                : MathHelper.Clamp(current / (float)max, 0f, 1f);

            DrawPanel(spriteBatch, new XnaRectangle((int)position.X, (int)position.Y, width, height), 0.55f);

            var barColor = percentage > 0.5f
                ? highColor
                : percentage > 0.2f
                    ? middleColor
                    : lowColor;

            if (AssetManager.Pixel is not null)
            {
                spriteBatch.Draw(
                    AssetManager.Pixel,
                    new XnaRectangle((int)position.X + 2, (int)position.Y + 2, (int)((width - 4) * percentage), height - 4),
                    barColor);
            }

            DrawText(
                spriteBatch,
                $"{label} {Math.Clamp(current, 0, Math.Max(max, 0))}/{Math.Max(max, 0)}",
                new XnaVector2(position.X + 8, position.Y + 2),
                XnaColor.White);
        }

        private static void DrawPanel(SpriteBatch spriteBatch, XnaRectangle rectangle, float opacity = 0.72f)
        {
            if (AssetManager.Pixel is null)
            {
                return;
            }

            spriteBatch.Draw(AssetManager.Pixel, rectangle, XnaColor.Black * opacity);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1), XnaColor.White * 0.75f);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rectangle.Left, rectangle.Bottom - 1, rectangle.Width, 1), XnaColor.White * 0.45f);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height), XnaColor.White * 0.55f);
            spriteBatch.Draw(AssetManager.Pixel, new XnaRectangle(rectangle.Right - 1, rectangle.Top, 1, rectangle.Height), XnaColor.White * 0.55f);
        }

        private static void DrawText(SpriteBatch spriteBatch, string text, XnaVector2 position, XnaColor color, bool alignRight = false)
        {
            if (AssetManager.MainFont is null || string.IsNullOrEmpty(text))
            {
                return;
            }

            var drawPosition = position;
            if (alignRight)
            {
                var size = AssetManager.MainFont.MeasureString(text);
                drawPosition.X -= size.X;
            }

            spriteBatch.DrawString(AssetManager.MainFont, text, drawPosition + new XnaVector2(2, 2), XnaColor.Black);
            spriteBatch.DrawString(AssetManager.MainFont, text, drawPosition, color);
        }
    }
}
