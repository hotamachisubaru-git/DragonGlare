using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DragonGlare.Scenes
{
    public interface IScene
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
