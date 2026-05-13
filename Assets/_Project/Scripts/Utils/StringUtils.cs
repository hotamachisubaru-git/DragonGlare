using UnityEngine;

namespace DragonGlare
{
    public static class StringUtils
    {
        public static string[] SplitLines(string text)
        {
            return text.Replace("\r\n", "\n").Split('\n');
        }

        public static string TrimToWidth(string text, int maxWidth, System.Func<string, int> measureWidth)
        {
            if (measureWidth(text) <= maxWidth)
                return text;

            const string ellipsis = "...";
            if (measureWidth(ellipsis) > maxWidth)
                return string.Empty;

            for (int length = text.Length - 1; length > 0; length--)
            {
                var candidate = $"{text[..length]}{ellipsis}";
                if (measureWidth(candidate) <= maxWidth)
                    return candidate;
            }

            return ellipsis;
        }

        public static string Repeat(char c, int count)
        {
            return new string(c, count);
        }
    }
}
