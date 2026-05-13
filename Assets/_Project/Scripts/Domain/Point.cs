using UnityEngine;

namespace DragonGlare.Domain;

public readonly record struct Point(int X, int Y)
{
    public int x => X;
    public int y => Y;

    public static implicit operator Vector2Int(Point p) => new(p.X, p.Y);
    public static implicit operator Point(Vector2Int v) => new(v.x, v.y);
}
