using UnityEngine;

namespace DragonGlare
{
    public static class UiCanvas
    {
        public const int VirtualWidth = 640;
        public const int VirtualHeight = 480;
        public const int WindowClientWidth = VirtualWidth;
        public const int WindowClientHeight = VirtualHeight;
        public static readonly Vector2Int VirtualSize = new Vector2Int(VirtualWidth, VirtualHeight);
        public static readonly Vector2Int WindowClientSize = new Vector2Int(WindowClientWidth, WindowClientHeight);
        public static readonly RectInt FontFallbackWindow = new RectInt(8, 8, 624, 44);
    }
}
