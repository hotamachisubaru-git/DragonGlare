using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DragonGlare.Entities
{
    public abstract class Entity
    {
        public Vector2 Position;
        public Texture2D Texture;
        public bool IsActive = true;

        public abstract void Update(GameTime gameTime);
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive && Texture != null)
            {
                spriteBatch.Draw(Texture, Position, Color.White);
            }
        }
    }
}
