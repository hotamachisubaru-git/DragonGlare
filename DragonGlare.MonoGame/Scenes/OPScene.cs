using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class OPScene : IScene
    {
        public void Update(GameTime gameTime)
        {
            // OP animation logic
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            var background = AssetManager.GetTexture("OPBackground");
            if (background != null)
            {
                spriteBatch.Draw(background, Vector2.Zero, Color.White);
            }

            spriteBatch.End();
        }
    }
}