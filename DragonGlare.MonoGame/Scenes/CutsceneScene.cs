using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class CutsceneScene : IScene
    {
        private int _currentLine = 0;
        private float _lineTimer = 0f;
        private readonly List<CutsceneLine> _lines = new();
        private bool _isAutoAdvance = false;
        public CutsceneScene()
        {
            // カットシーンデータを初期化
            _lines.Add(new CutsceneLine { CharacterName = "主人公", Text = "ここは...どこだ？", Portrait = "portrait_hero", Position = 200 });
            _lines.Add(new CutsceneLine { CharacterName = "謎の少女", Text = "あなたが目覚めましたね", Portrait = "portrait_girl", Position = 400 });
            _lines.Add(new CutsceneLine { CharacterName = "謎の少女", Text = "この世界は危機に瀕しています", Portrait = "portrait_girl", Position = 400 });
            _lines.Add(new CutsceneLine { CharacterName = "主人公", Text = "何を言っているの？", Portrait = "portrait_hero", Position = 200 });
            _lines.Add(new CutsceneLine { CharacterName = "謎の少女", Text = "どうか、力を貸してください", Portrait = "portrait_girl", Position = 400 });

            // BGM再生
            AudioManager.PlayBgm(DragonGlareAlpha.Domain.BgmTrack.MainMenu);
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lineTimer += deltaTime;

            // 自動で次のセリフに進む
            if (_lineTimer > 3f)
            {
                _isAutoAdvance = true;
            }

            // 入力があるか自動進行時に次のセリフへ
            if (InputManager.WasPressed(Keys.Z) || InputManager.WasPressed(Keys.Enter) || _isAutoAdvance)
            {
                if (_currentLine < _lines.Count - 1)
                {
                    _currentLine++;
                    _lineTimer = 0f;
                    _isAutoAdvance = false;
                }
                else
                {
                    // カットシーン終了
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // 背景
            DrawBackground(spriteBatch);

            // キャラクター
            DrawCharacter(spriteBatch);

            // メッセージボックス
            DrawMessageBox(spriteBatch);

            // 名前表示
            DrawNameBox(spriteBatch);

            // 自動進行インジケーター
            DrawAutoAdvanceIndicator(spriteBatch);

            spriteBatch.End();
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            // 背景を描画
            var bgRect = new Rectangle(0, 0, 640, 480);
            spriteBatch.Draw(AssetManager.Pixel, bgRect, Color.DarkBlue * 0.3f);
        }

        private void DrawCharacter(SpriteBatch spriteBatch)
        {
            var currentLine = _lines[_currentLine];
            if (AssetManager.GetTexture(currentLine.Portrait) is Texture2D portrait)
            {
                var charRect = new Rectangle(currentLine.Position, CharacterBottom - portrait.Height, portrait.Width, portrait.Height);
                spriteBatch.Draw(portrait, charRect, Color.White);
            }
        }

        private void DrawMessageBox(SpriteBatch spriteBatch)
        {
            var boxRect = new Rectangle(20, MessageBoxTop, 600, 100);
            spriteBatch.Draw(AssetManager.Pixel, boxRect, Color.Black * 0.8f);

            if (AssetManager.MainFont != null)
            {
                var currentLine = _lines[_currentLine];
                var textPos = new Vector2(40, MessageBoxTop + 10);
                spriteBatch.DrawString(AssetManager.MainFont, currentLine.Text, textPos, Color.White);
            }
        }

        private void DrawNameBox(SpriteBatch spriteBatch)
        {
            var currentLine = _lines[_currentLine];
            if (string.IsNullOrEmpty(currentLine.CharacterName)) return;

            var nameBoxHeight = NameBoxHeight;
            var nameBoxWidth = AssetManager.MainFont?.MeasureString(currentLine.CharacterName).X ?? 60;
            var nameBoxRect = new Rectangle(40, MessageBoxTop - nameBoxHeight, (int)nameBoxWidth + 20, nameBoxHeight);

            spriteBatch.Draw(AssetManager.Pixel, nameBoxRect, Color.Brown * 0.8f);

            if (AssetManager.MainFont != null)
            {
                spriteBatch.DrawString(AssetManager.MainFont, currentLine.CharacterName, new Vector2(50, MessageBoxTop - nameBoxHeight + 5), Color.White);
            }
        }

        private void DrawAutoAdvanceIndicator(SpriteBatch spriteBatch)
        {
            if (! _isAutoAdvance) return;

            // 点滅する矢印
            var blink = MathF.Sin((float)GameTime.TotalGameTime.TotalMilliseconds * 0.005f) > 0;
            if (blink)
            {
                spriteBatch.DrawString(AssetManager.MainFont, "▼", new Vector2(580, AutoAdvanceIndicatorY), Color.Yellow);
            }
        }

        // カットシーンセリフ構造
        private class CutsceneLine
        {
            public string CharacterName { get; set; } = "";
            public string Text { get; set; } = "";
            public string Portrait { get; set; } = "";
            public float Position { get; set; }
        }
    }
}