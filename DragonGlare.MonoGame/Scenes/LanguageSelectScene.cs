using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class LanguageSelectScene : IScene
    {
        private int _selectedIndex = 0;
        private readonly string[] _languages = { "日本語", "English" };

        public void Update(GameTime gameTime)
        {
            if (InputManager.WasPressed(Keys.Up)) _selectedIndex = 0;
            if (InputManager.WasPressed(Keys.Down)) _selectedIndex = 1;

            if (InputManager.WasPressed(Keys.Z) || InputManager.WasPressed(Keys.Enter))
            {
                // TODO: _selectedIndex に基づいて言語設定を適用する処理
                SceneManager.ChangeScene(new SelectScene());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (AssetManager.Pixel != null)
            {
                spriteBatch.Draw(AssetManager.Pixel, new Rectangle(0, 0, 640, 480), Color.Black);
            }

            for (int i = 0; i < _languages.Length; i++)
            {
                var color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                var text = _languages[i];
                if (AssetManager.MainFont != null)
                {
                    spriteBatch.DrawString(AssetManager.MainFont, text, new Vector2(280, 200 + (i * 50)), color);
                }
            }
        }
    }
}