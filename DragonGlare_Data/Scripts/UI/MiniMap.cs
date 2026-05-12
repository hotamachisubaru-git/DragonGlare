using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class MiniMap : MonoBehaviour
    {
        [SerializeField] private RawImage mapImage;
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private int mapWidth = 100;
        [SerializeField] private int mapHeight = 100;

        private Texture2D mapTexture;

        private void Awake()
        {
            mapTexture = new Texture2D(mapWidth, mapHeight, TextureFormat.RGBA32, false);
            mapImage.texture = mapTexture;
        }

        public void UpdateMap(Color[] pixels, Vector2Int playerPosition)
        {
            if (pixels.Length != mapWidth * mapHeight)
                return;

            mapTexture.SetPixels(pixels);
            mapTexture.Apply();

            var normalizedPos = new Vector2(
                (float)playerPosition.x / mapWidth,
                (float)playerPosition.y / mapHeight);
            playerMarker.anchorMin = normalizedPos;
            playerMarker.anchorMax = normalizedPos;
        }
    }
}
