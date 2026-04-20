using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;
using DragonGlare.Scenes;
using DragonGlareAlpha.Domain;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace DragonGlare.Core
{
    public sealed class GameMain : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        private GameScene? _scene;

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = Constants.VirtualWidth;
            _graphics.PreferredBackBufferHeight = Constants.VirtualHeight;
            _graphics.SynchronizeWithVerticalRetrace = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            AssetManager.Load(Content);
            _scene = new GameScene(Content);
            AudioManager.PlayFieldBgm(_scene.CurrentMapId);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }

            InputManager.Update();
            _scene?.Update(gameTime);
            if (_scene is not null)
            {
                AudioManager.PlayFieldBgm(_scene.CurrentMapId);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(XnaColor.Black);

            if (_spriteBatch is null)
            {
                return;
            }

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _scene?.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scene?.Dispose();
                _spriteBatch?.Dispose();
                AudioManager.Stop();
            }

            base.Dispose(disposing);
        }
    }
}
