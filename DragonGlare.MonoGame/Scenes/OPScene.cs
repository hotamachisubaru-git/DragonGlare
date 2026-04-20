using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class OPScene : IScene
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _opBackground;

        public void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            _opBackground = AssetManager.GetTexture("OPBackground");
        }

        public void Update(GameTime gameTime)
        {
            // OP animation logic
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_opBackground, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}