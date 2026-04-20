using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class SelectScene : IScene
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _selectBackground;

        public void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            _selectBackground = AssetManager.GetTexture("SelectBackground");
        }

        public void Update(GameTime gameTime)
        {
            // Select menu logic
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_selectBackground, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}