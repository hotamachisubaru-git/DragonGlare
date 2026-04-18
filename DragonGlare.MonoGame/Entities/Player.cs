using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DragonGlare.Managers;

namespace DragonGlare.Entities
{
    public class Player : Entity
    {
        private float _speed = 300f;
        public int CurrentHP { get; set; } = 100;
        public int MaxHP { get; set; } = 100;

        public Player(Vector2 startPos)
        {
            Position = startPos;
            Texture = AssetManager.GetTexture("player");
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 move = Vector2.Zero;

            if (InputManager.IsKeyDown(Keys.W)) move.Y -= 1;
            if (InputManager.IsKeyDown(Keys.S)) move.Y += 1;
            if (InputManager.IsKeyDown(Keys.A)) move.X -= 1;
            if (InputManager.IsKeyDown(Keys.D)) move.X += 1;

            if (move != Vector2.Zero) move.Normalize();
            Position += move * _speed * dt;

            // 画面外へ出ないように制限（WinForms版の境界チェック相当）
            Position.X = MathHelper.Clamp(Position.X, 0, 800 - Texture.Width);
            Position.Y = MathHelper.Clamp(Position.Y, 0, 600 - Texture.Height);
        }
    }
}
