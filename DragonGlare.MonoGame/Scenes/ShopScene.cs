using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class ShopScene : IScene
    {
        public void Update(GameTime gameTime)
        {
            // Shop logic
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var background = AssetManager.GetTexture("ShopBackground");
            if (background != null)
            {
                spriteBatch.Draw(background, Vector2.Zero, Color.White);
            }

            if (AssetManager.MainFont != null)
            {
                spriteBatch.DrawString(AssetManager.MainFont, "いらっしゃいませ", new Vector2(50, 50), Color.White);
                UIManager.DrawGold(spriteBatch, 120, new Vector2(500, 20));
            }
        }
    }
}