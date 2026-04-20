using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class ShopScene : IScene
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _shopBackground;

        public void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            _shopBackground = AssetManager.GetTexture("ShopBackground");
        }

        public void Update(GameTime gameTime)
        {
            // Shop logic
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_shopBackground, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}