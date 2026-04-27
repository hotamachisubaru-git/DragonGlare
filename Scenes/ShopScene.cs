using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DragonGlare.Managers;
using DragonGlareAlpha.Data;

namespace DragonGlare.Scenes
{
    public class ShopScene : IScene
    {
        private int _selectedItem = 0;
        private readonly List<ShopItem> _shopItems = new();
        private string _shopMessage = "いらっしゃいませ！";
        private readonly string _shopName = "ドラゴンショップ";

        // UI 定数
        private const int ShopTitleTop = 30;
        private const int ItemListLeft = 50;
        private const int ItemListTop = 100;
        private const int ItemSpacing = 50;
        private const int DetailPanelLeft = 350;
        private const int DetailPanelTop = 100;
        private const int BuyButtonTop = 350;
        private const int MessageBoxTop = 420;

        public ShopScene()
        {
            // シップアイテムを初期化
            _shopItems.Add(new ShopItem { Name = "ポーション", Price = 50, Quantity = 10, Description = "HPが30回復する", TextureName = "item_potion", IsBuyable = true });
            _shopItems.Add(new ShopItem { Name = "まもりのリング", Price = 200, Quantity = 3, Description = "防御力が5上がる", TextureName = "item_ring", IsBuyable = true });
            _shopItems.Add(new ShopItem { Name = "ライフアンジェ", Price = 500, Quantity = 1, Description = "HPが最大100上がる", TextureName = "item_life", IsBuyable = true });
            _shopItems.Add(new ShopItem { Name = "きょうすいのけん", Price = 150, Quantity = 5, Description = "攻撃力が8上がる", TextureName = "item_sword", IsBuyable = true });
        }

        public void Update(GameTime gameTime)
        {
            if (InputManager.WasPressed(Keys.Up) || InputManager.WasPressed(Keys.W))
            {
                _selectedItem = (_selectedItem - 1 + _shopItems.Count) % _shopItems.Count;
            }
            if (InputManager.WasPressed(Keys.Down) || InputManager.WasPressed(Keys.S))
            {
                _selectedItem = (_selectedItem + 1) % _shopItems.Count;
            }
            if (InputManager.WasPressed(Keys.Z) || InputManager.WasPressed(Keys.Enter))
            {
                ExecutePurchase();
            }
            if (InputManager.WasPressed(Keys.Escape))
            {
                // 退出逻辑
            }
        }

        private void ExecutePurchase()
        {
            var item = _shopItems[_selectedItem];
            if (item.IsBuyable && item.Quantity > 0)
            {
                _shopMessage = $"{item.Name}を{item.Price}ゴールドで買いますか？";
            }
            else
            {
                _shopMessage = "もう持っていません";
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // シップ背景
            var background = AssetManager.GetTexture("ShopBackground");
            if (background != null)
            {
                spriteBatch.Draw(background, new Rectangle(0, 0, 640, 480), Color.White);
            }

            // シップタイトル
            DrawShopTitle(spriteBatch);

            // アイテムリスト
            DrawItemList(spriteBatch);

            // アイテム詳細
            DrawItemDetail(spriteBatch);

            // 購入ボタン
            DrawBuyButton(spriteBatch);

            // メッセージボックス
            DrawMessageBox(spriteBatch);

            // ゴールド表示
            DrawGoldDisplay(spriteBatch);

            spriteBatch.End();
        }

        private void DrawShopTitle(SpriteBatch spriteBatch)
        {
            if (AssetManager.MainFont != null)
            {
                var titleRect = new Rectangle(200, ShopTitleTop, 240, 30);
                spriteBatch.Draw(AssetManager.Pixel, titleRect, Color.Black * 0.5f);
                spriteBatch.DrawString(AssetManager.MainFont, _shopName, new Vector2(220, ShopTitleTop + 5), Color.Yellow);
            }
        }

        private void DrawItemList(SpriteBatch spriteBatch)
        {
            if (AssetManager.MainFont == null) return;

            for (int i = 0; i < _shopItems.Count; i++)
            {
                var item = _shopItems[i];
                var y = ItemListTop + (i * ItemSpacing);

                // アイテムアイコン
                if (AssetManager.GetTexture(item.TextureName) is Texture2D icon)
                {
                    var iconRect = new Rectangle(ItemListLeft, y, 32, 32);
                    spriteBatch.Draw(icon, iconRect, Color.White);
                }

                // アイテム名
                var nameColor = i == _selectedItem ? Color.Yellow : Color.White;
                spriteBatch.DrawString(AssetManager.MainFont, item.Name, new Vector2(ItemListLeft + 40, y), nameColor);

                // 価格
                spriteBatch.DrawString(AssetManager.MainFont, $"{item.Price}G", new Vector2(ItemListLeft + 200, y), Color.Gray);

                // 在庫
                spriteBatch.DrawString(AssetManager.MainFont, $"x{item.Quantity}", new Vector2(ItemListLeft + 250, y), Color.Gray);

                // 選択インジケーター
                if (i == _selectedItem)
                {
                    spriteBatch.DrawString(AssetManager.MainFont, "►", new Vector2(ItemListLeft - 20, y), Color.Yellow);
                }
            }
        }

        private void DrawItemDetail(SpriteBatch spriteBatch)
        {
            if (AssetManager.MainFont == null || _shopItems.Count == 0) return;

            var item = _shopItems[_selectedItem];
            var x = DetailPanelLeft;
            var y = DetailPanelTop;

            // パネル背景
            var panelRect = new Rectangle(x, y, 250, 150);
            spriteBatch.Draw(AssetManager.Pixel, panelRect, Color.Black * 0.6f);

            // アイテム説明
            spriteBatch.DrawString(AssetManager.MainFont, item.Description, new Vector2(x + 10, y + 10), Color.White);

            // 効果説明
            spriteBatch.DrawString(AssetManager.MainFont, "効果: " + item.Description, new Vector2(x + 10, y + 40), Color.LightGreen);
        }

        private void DrawBuyButton(SpriteBatch spriteBatch)
        {
            if (AssetManager.MainFont == null) return;

            var item = _shopItems[_selectedItem];
            var buttonX = DetailPanelLeft;
            var buttonY = BuyButtonTop;

            // 購入ボタン
            var buttonRect = new Rectangle(buttonX, buttonY, 100, 30);
            spriteBatch.Draw(AssetManager.Pixel, buttonRect, Color.Green * 0.5f);
            spriteBatch.DrawString(AssetManager.MainFont, "かう", new Vector2(buttonX + 35, buttonY + 5), Color.White);

            // 売却ボタン
            var sellButtonRect = new Rectangle(buttonX + 110, buttonY, 100, 30);
            spriteBatch.Draw(AssetManager.Pixel, sellButtonRect, Color.Red * 0.5f);
            spriteBatch.DrawString(AssetManager.MainFont, "うる", new Vector2(buttonX + 145, buttonY + 5), Color.White);
        }

        private void DrawMessageBox(SpriteBatch spriteBatch)
        {
            var boxRect = new Rectangle(20, MessageBoxTop, 600, 50);
            spriteBatch.Draw(AssetManager.Pixel, boxRect, Color.Black * 0.7f);

            if (AssetManager.MainFont != null)
            {
                spriteBatch.DrawString(AssetManager.MainFont, _shopMessage, new Vector2(40, MessageBoxTop + 10), Color.White);
            }
        }

        private void DrawGoldDisplay(SpriteBatch spriteBatch)
        {
            if (AssetManager.MainFont == null) return;

            var gold = 1000; // 実際のGoldはPlayerから取得すべき
            var goldText = $"ゴールド: {gold}";
            var textPos = new Vector2(500, 20);

            // ゴールド表示背景
            var bgRect = new Rectangle((int)textPos.X - 10, (int)textPos.Y - 5, 120, 25);
            spriteBatch.Draw(AssetManager.Pixel, bgRect, Color.Black * 0.5f);

            spriteBatch.DrawString(AssetManager.MainFont, goldText, textPos, Color.Yellow);
        }

        // シップアイテム構造
        private class ShopItem
        {
            public string Name { get; set; } = "";
            public int Price { get; set; }
            public int Quantity { get; set; }
            public string Description { get; set; } = "";
            public string TextureName { get; set; } = "";
            public bool IsBuyable { get; set; } = true;
        }
    }
}