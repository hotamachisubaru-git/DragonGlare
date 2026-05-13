namespace DragonGlare.Domain;

public readonly record struct Color(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Color Cyan = new(0, 255, 255);
    public static readonly Color Gold = new(255, 215, 0);
    public static readonly Color MediumSpringGreen = new(0, 250, 154);

    public static Color FromArgb(byte r, byte g, byte b) => new(r, g, b);
    public static Color FromArgb(int r, int g, int b) => new((byte)r, (byte)g, (byte)b);
}
