using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha.Tests;

public sealed class GameContentTests
{
    [Theory]
    [InlineData("ぁ")]
    [InlineData("ぃ")]
    [InlineData("ぅ")]
    [InlineData("ぇ")]
    [InlineData("ぉ")]
    [InlineData("っ")]
    [InlineData("ゃ")]
    [InlineData("ゅ")]
    [InlineData("ょ")]
    [InlineData("ゎ")]
    public void JapaneseNameTable_IncludesSmallKana(string kana)
    {
        var table = GameContent.GetNameTable(UiLanguage.Japanese);

        Assert.Contains(table.SelectMany(row => row), value => value == kana);
    }
}
