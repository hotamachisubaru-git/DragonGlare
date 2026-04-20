using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class BattleScene : IScene
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _battleBackground;

        public void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            _battleBackground = AssetManager.GetTexture("BattleBackground");
        }

        public void Update(GameTime gameTime)
        {
            // Battle logic here
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_battleBackground, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}