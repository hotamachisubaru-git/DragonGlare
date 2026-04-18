using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Entities;
using DragonGlare.Managers;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System;

namespace DragonGlare.Scenes
{
    public class GameScene : IScene
    {
        private Player _player;
        private List<Entity> _enemies = new List<Entity>();
        private List<Entity> _bullets = new List<Entity>();
        private SceneManager _sceneManager;
        private int _score = 0;
        private double _spawnTimer = 0;
        private double _shootTimer = 0;
        private Random _random = new Random();

        public GameScene(SceneManager manager)
        {
            _sceneManager = manager;
            _player = new Player(new Vector2(400, 500));
        }

        public void Update(GameTime gameTime)
        {
            _player.Update(gameTime);

            // プレイヤーの射撃 (0.2秒間隔)
            _shootTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (InputManager.IsKeyDown(Keys.Space) && _shootTimer > 0.2)
            {
                var bulletPos = _player.Position + new Vector2(_player.Texture.Width / 2 - 4, 0);
                _bullets.Add(new Bullet(bulletPos));
                _shootTimer = 0;
            }

            // 弾の更新とクリーンアップ
            _bullets.ForEach(b => b.Update(gameTime));
            _bullets.RemoveAll(b => !b.IsActive);

            // 敵の更新と生成ロジック（WinForms版のタイマー処理相当）
            UpdateEnemies(gameTime);

            // 当たり判定
            _score += CollisionManager.CheckCollisions(_player, _enemies, _bullets);
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            _spawnTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_spawnTimer > 1.0) // 1秒ごとに敵を生成
            {
                _enemies.Add(new Enemy(new Vector2(_random.Next(0, 750), -50)));
                _spawnTimer = 0;
            }

            _enemies.ForEach(e => e.Update(gameTime));
            _enemies.RemoveAll(e => !e.IsActive);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _player.Draw(spriteBatch);
            _bullets.ForEach(b => b.Draw(spriteBatch));
            _enemies.ForEach(e => e.Draw(spriteBatch));

            // UIの描画
            UIManager.DrawUI(spriteBatch, _score, _player.CurrentHP, _player.MaxHP);
        }
    }
}
