using UnityEngine;
using System;

namespace DragonGlare
{
    [Serializable]
    public struct OpeningNarrationLine
    {
        public string Text;
        public int DisplayFrames;
        public int Y;

        public OpeningNarrationLine(string text, int displayFrames, int y)
        {
            Text = text;
            DisplayFrames = displayFrames;
            Y = y;
        }
    }
}
