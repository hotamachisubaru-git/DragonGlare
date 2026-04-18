using Microsoft.Xna.Framework;
using DragonGlare.Managers;

namespace DragonGlare.Entities
{
    public class Bullet : Entity
    {
        private float _speed = 600f;

        public Bullet(Vector2 startPos)
        {
            Position = startPos;
            Texture = AssetManager.GetTexture("bullet");
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position.Y -= _speed * dt;

            if (Position.Y < -50) IsActive = false;
        }
    }
}