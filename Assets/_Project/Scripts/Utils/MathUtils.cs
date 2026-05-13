using UnityEngine;

namespace DragonGlare
{
    public static class MathUtils
    {
        public static int Mod(int a, int b)
        {
            int result = a % b;
            return result < 0 ? result + b : result;
        }

        public static float Mod(float a, float b)
        {
            float result = a % b;
            return result < 0 ? result + b : result;
        }

        public static int Wrap(int value, int min, int max)
        {
            if (value < min) return max;
            if (value > max) return min;
            return value;
        }

        public static float Wrap(float value, float min, float max)
        {
            if (value < min) return max;
            if (value > max) return min;
            return value;
        }

        public static Vector2Int Clamp(Vector2Int value, Vector2Int min, Vector2Int max)
        {
            return new Vector2Int(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y)
            );
        }
    }
}
