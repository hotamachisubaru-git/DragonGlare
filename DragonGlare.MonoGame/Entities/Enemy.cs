using Microsoft.Xna.Framework;
using DragonGlare.Managers;
using System;

namespace DragonGlare.Entities
{
    public class Enemy : Entity
    {
        private float _speed = 150f;

        public Enemy(Vector2 startPos)
        {
            Position = startPos;
            Texture = AssetManager.GetTexture("enemy");
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position.Y += _speed * dt;

            if (Position.Y > 650) IsActive = false;
        }
    }
}