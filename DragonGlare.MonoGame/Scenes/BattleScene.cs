using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;

namespace DragonGlare.Scenes
{
    public class BattleScene : IScene
    {
        public void Update(GameTime gameTime)
        {
            // Battle logic here
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var background = AssetManager.GetTexture("BattleBackground");
            if (background != null)
            {
                spriteBatch.Draw(background, Vector2.Zero, Color.White);
            }

            // Battle UI Panels
            if (AssetManager.Pixel != null)
            {
                spriteBatch.Draw(AssetManager.Pixel, new Rectangle(0, 350, 640, 130), Color.Black * 0.7f);
            }
        }
    }
}