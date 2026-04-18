using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;
using DragonGlare.Scenes;

namespace DragonGlare.Core
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SceneManager _sceneManager;

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets"; // アセットフォルダ指定
            IsMouseVisible = true;
            
            // ウィンドウサイズ設定（WinForms版に合わせる）
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            _sceneManager = new SceneManager();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // 最初のアセット読み込みやシーンの初期化
            AssetManager.Load(Content);
            _sceneManager.ChangeScene(new TitleScene(_sceneManager));
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update(); // 入力状態の更新
            _sceneManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _sceneManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
