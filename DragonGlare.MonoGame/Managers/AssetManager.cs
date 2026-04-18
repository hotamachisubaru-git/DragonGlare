using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DragonGlare.Managers
{
    public static class AssetManager
    {
        private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        public static SpriteFont MainFont { get; private set; }
        public static Texture2D Pixel { get; private set; }

        public static void Load(ContentManager content)
        {
            // Assetsフォルダー内のファイル名に合わせて読み込みます
            // 拡張子は不要です（例: "Assets/player.png" -> "player"）
            _textures["player"] = content.Load<Texture2D>("player");
            _textures["enemy"] = content.Load<Texture2D>("enemy");
            _textures["bullet"] = content.Load<Texture2D>("bullet");
            _textures["background"] = content.Load<Texture2D>("background");

            // フォントの読み込み
            MainFont = content.Load<SpriteFont>("font");

            // 1x1の白いピクセルを生成（HPバーなどの図形描画用）
            Pixel = new Texture2D(content.GetGraphicsDevice(), 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        public static Texture2D GetTexture(string name) => _textures.GetValueOrDefault(name);

        private static GraphicsDevice GetGraphicsDevice(this ContentManager content) => 
            ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
    }
}
