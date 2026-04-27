using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DragonGlare.Managers;

namespace DragonGlare.Entities
{
    public class Player : Entity
    {
        private const float Speed = 300f;

        public int CurrentHP { get; set; } = 100;

        public int MaxHP { get; set; } = 100;

        public Microsoft.Xna.Framework.Point TilePosition { get; set; }

        public Player(Vector2 startPos)
        {
            Position = startPos;
            Texture = AssetManager.GetTexture("player");
        }

        public override void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var move = Vector2.Zero;

            if (InputManager.IsKeyDown(Keys.W)) move.Y -= 1;
            if (InputManager.IsKeyDown(Keys.S)) move.Y += 1;
            if (InputManager.IsKeyDown(Keys.A)) move.X -= 1;
            if (InputManager.IsKeyDown(Keys.D)) move.X += 1;

            if (move != Vector2.Zero) move.Normalize();
            Position += move * Speed * dt;

            var width = Texture?.Width ?? Bounds.Width;
            var height = Texture?.Height ?? Bounds.Height;
            Position = new Vector2(
                MathHelper.Clamp(Position.X, 0, 800 - width),
                MathHelper.Clamp(Position.Y, 0, 600 - height));
        }
    }
}
