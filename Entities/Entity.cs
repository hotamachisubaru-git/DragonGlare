using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace DragonGlare.Entities
{
    public abstract class Entity
    {
        public Vector2 Position { get; set; }

        public Texture2D? Texture { get; protected set; }

        public bool IsActive = true;

        public virtual XnaRectangle Bounds => new(
            (int)Position.X,
            (int)Position.Y,
            Texture?.Width ?? 28,
            Texture?.Height ?? 28);

        public abstract void Update(GameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive)
            {
                return;
            }

            if (Texture is not null)
            {
                spriteBatch.Draw(Texture, Position, XnaColor.White);
                return;
            }

            if (AssetManager.Pixel is not null)
            {
                spriteBatch.Draw(AssetManager.Pixel, Bounds, XnaColor.White);
            }
        }
    }
}
