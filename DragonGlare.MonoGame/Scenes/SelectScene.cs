using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DragonGlare.Managers;
using DragonGlareAlpha.Domain;

namespace DragonGlare.Scenes
{
    public class SelectScene : IScene
    {
        private int _selectedIndex = 0;
        private readonly string[] _menuItems = { "はじめから", "つづきから" };

        public void Update(GameTime gameTime)
        {
            if (InputManager.WasPressed(Keys.Up)) _selectedIndex = (_selectedIndex - 1 + _menuItems.Length) % _menuItems.Length;
            if (InputManager.WasPressed(Keys.Down)) _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
            
            if (InputManager.WasPressed(Keys.Z) || InputManager.WasPressed(Keys.Enter))
            {
                if (_selectedIndex == 0) // はじめから
                {
                    SceneManager.ChangeScene(new GameScene(SceneManager.Content, FieldMapId.Hub));
                }
                else // つづきから
                {
                    LoadGame();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            var background = AssetManager.GetTexture("SelectBackground");
            if (background != null)
            {
                spriteBatch.Draw(background, Vector2.Zero, Color.White);
            }

            for (int i = 0; i < _menuItems.Length; i++)
            {
                var color = (i == _selectedIndex) ? Color.Yellow : Color.White;
                var text = (i == _selectedIndex) ? $"> {_menuItems[i]}" : $"  {_menuItems[i]}";
                
                if (AssetManager.MainFont != null)
                {
                    spriteBatch.DrawString(AssetManager.MainFont, text, new Vector2(100, 200 + (i * 40)), color);
                }
            }

            spriteBatch.End();
        }

        private void LoadGame()
        {
            // TODO: セーブデータの保存先パスを定義してください（例: savedata.json）
            string savePath = "save.dat";

            if (File.Exists(savePath))
            {
                try
                {
                    // TODO: セーブデータの読み込みとデシリアライズ処理をここに実装
                    // var saveData = ...;

                    // 読み込んだデータに基づいてGameSceneを生成します。
                    // 現在のGameSceneはMapIdのみをコンストラクタで受け取るため、
                    // 座標や所持金、経験値などを復元するにはGameScene側の拡張が必要です。
                    SceneManager.ChangeScene(new GameScene(SceneManager.Content, FieldMapId.Hub));
                }
                catch (System.Exception)
                {
                    // 読み込み失敗（ファイル破損等）時のエラーハンドリング
                }
            }
            else
            {
                // セーブデータが存在しない場合の処理（SEを鳴らす、メッセージを出すなど）
            }
        }
    }
}