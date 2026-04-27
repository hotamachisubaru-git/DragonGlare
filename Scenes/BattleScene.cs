using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Data;

namespace DragonGlare.Scenes
{
    public class BattleScene : IScene
    {
        private readonly List<BattleEnemy> _enemies = new();
        private int _selectedAction = 0;
        private int _selectedTarget = 0;
        private bool _isAnimating;
        private float _animationTimer;
        private string? _battleMessage;
        private readonly Rectangle _viewport = new(0, 0, 640, 480);

        // UI 定数
        private const int BattleAreaTop = 50;
        private const int BattleAreaHeight = 250;
        private const int ActionMenuTop = 350;
        private const int MessageBoxTop = 410;
        private const int MessageBoxHeight = 70;

        public BattleScene()
        {
            // 敵を初期化
            _enemies.Add(new BattleEnemy { Name = "スライム", Hp = 30, MaxHp = 30, TextureName = "enemy_slime", IsAlive = true });
            _enemies.Add(new BattleEnemy { Name = "ゴブリン", Hp = 45, MaxHp = 45, TextureName = "enemy_goblin", IsAlive = true });
        }

        public void Update(GameTime gameTime)
        {
            if (_isAnimating)
            {
                _animationTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_animationTimer <= 0)
                {
                    _isAnimating = false;
                }
                return;
            }

            if (InputManager.WasPressed(Keys.Up) || InputManager.WasPressed(Keys.W))
            {
                _selectedAction = (_selectedAction - 1 + 4) % 4;
            }
            if (InputManager.WasPressed(Keys.Down) || InputManager.WasPressed(Keys.S))
            {
                _selectedAction = (_selectedAction + 1) % 4;
            }
            if (InputManager.WasPressed(Keys.Z) || InputManager.WasPressed(Keys.Enter))
            {
                ExecuteAction();
            }
            if (InputManager.WasPressed(Keys.Escape))
            {
                // 逃跑逻辑
            }
        }

        private void ExecuteAction()
        {
            _isAnimating = true;
            _animationTimer = 1000f;

            switch (_selectedAction)
            {
                case 0: // 攻撃
                    var target = _enemies.FirstOrDefault(e => e.IsAlive);
                    if (target != null)
                    {
                        target.Hp -= 15;
                        _battleMessage = $"{target.Name}に15ダメージ！";
                        if (target.Hp <= 0)
                        {
                            target.IsAlive = false;
                            _battleMessage += $"{target.Name}を倒した！";
                        }
                    }
                    break;
                case 1: // 魔法
                    _battleMessage = "魔法は未実装です";
                    break;
                case 2: // 防御
                    _battleMessage = "防御態勢！";
                    break;
                case 3: // 逃跑
                    _battleMessage = "逃げ出した！";
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // バトル背景
            var background = AssetManager.GetTexture("BattleBackground");
            if (background != null)
            {
                spriteBatch.Draw(background, new Rectangle(0, 0, 640, 480), Color.White);
            }

            // 敵の描画
            DrawEnemies(spriteBatch);

            // アクションメニュー
            DrawActionMenu(spriteBatch);

            // メッセージボックス
            DrawMessageBox(spriteBatch);

            // HPバー
            DrawHpBars(spriteBatch);

            spriteBatch.End();
        }

        private void DrawEnemies(SpriteBatch spriteBatch)
        {
            var startX = 150f;
            var spacing = 120f;

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (!enemy.IsAlive) continue;

                var x = startX + (i * spacing);
                var y = BattleAreaTop + 80;

                // 敵スプライト
                if (AssetManager.GetTexture(enemy.TextureName) is Texture2D texture)
                {
                    var rect = new Rectangle((int)x, (int)y, 64, 64);
                    spriteBatch.Draw(texture, rect, Color.White);
                }
                else
                {
                    // テクスチャがない場合はプレースホルダー
                    spriteBatch.Draw(AssetManager.Pixel, rect, Color.Gray);
                }

                // 敵名
                if (AssetManager.MainFont != null)
                {
                    spriteBatch.DrawString(AssetManager.MainFont, enemy.Name, new Vector2(x, y - 20), Color.White);
                }
            }
        }

        private void DrawActionMenu(SpriteBatch spriteBatch)
        {
            var actions = new[] { "たたかう", "まほう", "ぼうぎょ", "にげる" };
            var menuX = 30f;
            var menuY = ActionMenuTop;
            var lineHeight = 30f;

            if (AssetManager.MainFont == null) return;

            for (int i = 0; i < actions.Length; i++)
            {
                var y = menuY + (i * lineHeight);
                var color = i == _selectedAction ? Color.Yellow : Color.White;

                // 選択インジケーター
                if (i == _selectedAction)
                {
                    spriteBatch.DrawString(AssetManager.MainFont, "►", new Vector2(menuX - 20, y), color);
                }

                spriteBatch.DrawString(AssetManager.MainFont, actions[i], new Vector2(menuX, y), color);
            }
        }

        private void DrawMessageBox(SpriteBatch spriteBatch)
        {
            // メッセージボックス背景
            var boxRect = new Rectangle(20, MessageBoxTop, 600, MessageBoxHeight);
            spriteBatch.Draw(AssetManager.Pixel, boxRect, Color.Black * 0.7f);

            // テキスト
            if (AssetManager.MainFont != null)
            {
                var message = _battleMessage ?? "どのこうげきをする？";
                spriteBatch.DrawString(AssetManager.MainFont, message, new Vector2(40, MessageBoxTop + 25), Color.White);
            }
        }

        private void DrawHpBars(SpriteBatch spriteBatch)
        {
            var barX = 450f;
            var barY = 20f;
            var barWidth = 150f;
            var barHeight = 15f;

            // プレイヤーHP
            if (AssetManager.MainFont != null)
            {
                spriteBatch.DrawString(AssetManager.MainFont, "HP", new Vector2(barX, barY), Color.White);
                var hpBarRect = new Rectangle((int)barX, (int)(barY + 20), (int)barWidth, (int)barHeight);
                spriteBatch.Draw(AssetManager.Pixel, hpBarRect, Color.DarkRed);
                spriteBatch.Draw(AssetManager.Pixel, new Rectangle((int)barX, (int)(barY + 20), (int)(barWidth * 0.8f), (int)barHeight), Color.Green);
            }
        }

        // 敵データ構造
        private class BattleEnemy
        {
            public string Name { get; set; } = "";
            public int Hp { get; set; }
            public int MaxHp { get; set; }
            public string TextureName { get; set; } = "";
            public bool IsAlive { get; set; } = true;
        }
    }
}