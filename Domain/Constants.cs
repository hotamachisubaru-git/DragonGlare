using System.Drawing;
using System.Linq;

namespace DragonGlareAlpha.Domain;

public readonly record struct OpeningNarrationLine(
    string Text,
    int DisplayFrames,
    int GapFrames);

public static class Constants
{
    public const int VirtualWidth = 640;
    public const int VirtualHeight = 480;
    public const int TileSize = 32;
    public const int FieldMovementAnimationDuration = 6;
    public const int EncounterTransitionDuration = 26;
    public const int OpeningSourceViewportHeight = 240;
    public const int OpeningScreenFrames = 60 * 56;
    public const int OpeningNarrationFadeFrames = 24;
    public const int SceneFadeOutDuration = 40;
    public const int UiFontPixelSize = 14;
    public const int UiTextLineHeight = UiFontPixelSize + 4;
    public const string ProjectDisplayName = "DragonGlare Alpha";

    public static readonly Point PlayerStartTile = new(3, 12);
    public static readonly OpeningNarrationLine[] LanguageOpeningScript =
    [
        new("遠い昔。", 180, 40),
        new("世界には五つの大地があった。", 180, 40),
        new("その時代では、争いがなく。\n皆が平和に満ちていた。", 260, 50),
        new("そして、それぞれの大地で\n違う神が崇められていたという。", 320, 50),
        new("しかし", 90, 120),
        new("平和は長くは続かなかった。", 250, 50),
        new("争いによって、日の大地は\n跡形もなく崩れ落ちてしまった。", 290, 50),
        new("そして、世界には暗い月しか\n上らなくなってしまった。", 280, 50),
        new("次から次へと世界は\n闇に満ちていった。", 250, 50),
        new("ついには、世界の中心となる光の大地が\n愚かな争いにより、闇に沈んでいった。", 340, 60),
        new("やがて、光の神は\n闇に飲み込まれ", 220, 50),
        new("世界にある万物が\n人々を襲うようにしてしまった。", 270, 50),
        new("世界は、いつしか光を失い\n闇が世界を司るようになった。", 300, 0)
    ];

    public static readonly int LanguageOpeningTotalFrames = LanguageOpeningScript.Sum(line => line.DisplayFrames + line.GapFrames);
}
