using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class CutsceneScene : IScene
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _cutsceneBackground;

        public void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            _cutsceneBackground = AssetManager.GetTexture("CutsceneBackground");
        }

        public void Update(GameTime gameTime)
        {
            // Cutscene animation logic
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_cutsceneBackground, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}