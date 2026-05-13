using UnityEngine;

namespace DragonGlare.Domain;

public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public bool Contains(Point p) => p.X >= X && p.X < X + Width && p.Y >= Y && p.Y < Y + Height;
    public bool Contains(Vector2Int v) => v.x >= X && v.x < X + Width && v.y >= Y && v.y < Y + Height;
}
