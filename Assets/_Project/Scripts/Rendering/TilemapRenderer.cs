using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class TilemapRenderer : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Sprite defaultTileSprite;

        private Image[,] tiles;

        public void Initialize(int width, int height)
        {
            if (tiles != null)
            {
                foreach (var t in tiles)
                {
                    if (t != null) Destroy(t.gameObject);
                }
            }

            tiles = new Image[height, width];
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = width;
            grid.cellSize = new Vector2(GameConstants.TileSize, GameConstants.TileSize);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var go = Instantiate(tilePrefab, grid.transform);
                    tiles[y, x] = go.GetComponent<Image>();
                    if (defaultTileSprite != null)
                        tiles[y, x].sprite = defaultTileSprite;
                }
            }
        }

        public void SetTileColor(int x, int y, Color color)
        {
            if (tiles != null && y >= 0 && y < tiles.GetLength(0) && x >= 0 && x < tiles.GetLength(1))
            {
                tiles[y, x].color = color;
            }
        }

        public void SetTileSprite(int x, int y, Sprite sprite)
        {
            if (tiles != null && y >= 0 && y < tiles.GetLength(0) && x >= 0 && x < tiles.GetLength(1))
            {
                tiles[y, x].sprite = sprite;
            }
        }
    }
}
