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
        private readonly string[] _menuItems = {
            "ぼうけんモードをする",
            "ひょうじそくどをかえる",
            "ぼうけんのしょをつくうつす",
            "ぼうけんのしょをうつす",
            "ぼうけんのしょをけす"
        };

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

            // Black background
            spriteBatch.Draw(
                AssetManager.GetTexture("SelectBackground") ?? CreateSolidTexture(Color.Black),
                Vector2.Zero,
                Color.White
            );

            // Draw white border box
            var boxWidth = 500f;
            var boxHeight = 280f;
            var boxX = (800 - boxWidth) / 2;
            var boxY = (600 - boxHeight) / 2;
            var borderColor = Color.White;
            var borderThickness = 2f;

            // Top and bottom borders
            spriteBatch.Draw(CreateSolidTexture(borderColor), new Rectangle((int)boxX, (int)boxY, (int)boxWidth, (int)borderThickness), Color.White);
            spriteBatch.Draw(CreateSolidTexture(borderColor), new Rectangle((int)boxX, (int)(boxY + boxHeight - borderThickness), (int)boxWidth, (int)borderThickness), Color.White);
            // Left and right borders
            spriteBatch.Draw(CreateSolidTexture(borderColor), new Rectangle((int)boxX, (int)boxY, (int)borderThickness, (int)boxHeight), Color.White);
            spriteBatch.Draw(CreateSolidTexture(borderColor), new Rectangle((int)(boxX + boxWidth - borderThickness), (int)boxY, (int)borderThickness, (int)boxHeight), Color.White);

            // Menu items
            var menuStartY = boxY + 40;
            var menuSpacing = 40;
            var textOrigin = Vector2.Zero;

            for (int i = 0; i < _menuItems.Length; i++)
            {
                var isSelected = (i == _selectedIndex);
                var marker = isSelected ? "▶" : " ";
                var color = isSelected ? Color.Yellow : Color.White;
                var text = $"{marker}{_menuItems[i]}";

                if (AssetManager.MainFont != null)
                {
                    var textSize = AssetManager.MainFont.MeasureString(text);
                    var textPos = new Vector2(
                        boxX + (boxWidth - textSize.X) / 2,
                        menuStartY + (i * menuSpacing)
                    );
                    spriteBatch.DrawString(AssetManager.MainFont, text, textPos, color);
                }
            }

            spriteBatch.End();
        }

        private Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(SceneManager.GraphicsDevice, 1, 1);
            texture.SetData(new Color[] { color });
            return texture;
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