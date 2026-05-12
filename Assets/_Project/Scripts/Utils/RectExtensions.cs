using UnityEngine;

namespace DragonGlare
{
    public static class RectExtensions
    {
        public static Rect ToRect(this RectInt rectInt)
        {
            return new Rect(rectInt.x, rectInt.y, rectInt.width, rectInt.height);
        }

        public static RectInt ToRectInt(this Rect rect)
        {
            return new RectInt(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
        }

        public static Vector2 Center(this RectInt rect)
        {
            return new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
        }

        public static RectInt Inflate(this RectInt rect, int amount)
        {
            return new RectInt(rect.x - amount, rect.y - amount, rect.width + amount * 2, rect.height + amount * 2);
        }
    }
}
