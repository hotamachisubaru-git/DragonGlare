using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DragonGlare.Managers
{
    public static class UIManager
    {
        public static void DrawHPBar(SpriteBatch spriteBatch, Vector2 position, int current, int max, int width = 200, int height = 20)
        {
            float percentage = (float)current / max;
            
            // 背景（黒）
            spriteBatch.Draw(AssetManager.Pixel, new Rectangle((int)position.X, (int)position.Y, width, height), Color.Black * 0.5f);
            
            // バー（体力に応じて色を変える）
            Color barColor = percentage > 0.5f ? Color.Green : (percentage > 0.2f ? Color.Orange : Color.Red);
            spriteBatch.Draw(AssetManager.Pixel, new Rectangle((int)position.X + 2, (int)position.Y + 2, (int)((width - 4) * percentage), height - 4), barColor);
        }

        public static void DrawScore(SpriteBatch spriteBatch, int score, Vector2 position)
        {
            if (AssetManager.MainFont == null) return;
            
            string text = $"SCORE: {score:D6}";
            // 文字に影をつけて視認性を上げる
            spriteBatch.DrawString(AssetManager.MainFont, text, position + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(AssetManager.MainFont, text, position, Color.Yellow);
        }

        public static void DrawUI(SpriteBatch spriteBatch, int score, int hp, int maxHp)
        {
            // スコアを右上に表示
            DrawScore(spriteBatch, score, new Vector2(20, 20));
            // HPバーを左上に表示
            DrawHPBar(spriteBatch, new Vector2(20, 50), hp, maxHp);
        }
    }
}