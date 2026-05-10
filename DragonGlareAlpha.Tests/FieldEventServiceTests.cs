using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Player;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class FieldEventServiceTests
{
    private readonly FieldEventService service = new();

    [Theory]
    [InlineData("field_notice_sign", 5, 4)]
    [InlineData("field_treasure_chest", 13, 8)]
    public void FieldImageEvents_CanBeInspectedFromAdjacentTiles(string eventId, int playerX, int playerY)
    {
        var fieldEvent = FieldContent.GetFieldEventById(eventId);

        Assert.NotNull(fieldEvent);
        Assert.True(fieldEvent!.CanInteractFrom(new Point(playerX, playerY)));
    }

    [Fact]
    public void FieldImageSign_DoesNotReactFromTwoTilesBelow()
    {
        var fieldEvent = FieldContent.GetFieldEventById("field_notice_sign");

        Assert.NotNull(fieldEvent);
        Assert.False(fieldEvent!.CanInteractFrom(new Point(5, 5)));
    }

    [Fact]
    public void Interact_WithFieldTreasureFirstTime_GrantsRewardAndCompletesEvent()
    {
        var player = PlayerProgress.CreateDefault(new Point(13, 8), UiLanguage.Japanese);
        var fieldEvent = FieldContent.GetFieldEventById("field_treasure_chest");

        var result = service.Interact(player, fieldEvent!, UiLanguage.Japanese);

        Assert.NotNull(fieldEvent);
        Assert.True(result.ShouldPersistProgress);
        Assert.True(player.HasCompletedFieldEvent("field_treasure_chest"));
        Assert.Equal(1, player.GetItemCount("bronze_sword"));
        Assert.Contains(result.Pages, page => page.Contains("宝箱を あけた", StringComparison.Ordinal));
        Assert.Contains(result.Pages, page => page.Contains("どうのつるぎ", StringComparison.Ordinal));
    }

    [Fact]
    public void Interact_WithCompletedFieldTreasure_DoesNotGrantRewardAgain()
    {
        var player = PlayerProgress.CreateDefault(new Point(13, 8), UiLanguage.Japanese);
        player.CompleteFieldEvent("field_treasure_chest");
        var fieldEvent = FieldContent.GetFieldEventById("field_treasure_chest");

        var result = service.Interact(player, fieldEvent!, UiLanguage.Japanese);

        Assert.NotNull(fieldEvent);
        Assert.False(result.ShouldPersistProgress);
        Assert.Equal(0, player.GetItemCount("bronze_sword"));
        Assert.Contains(result.Pages, page => page.Contains("からっぽ", StringComparison.Ordinal));
    }
}
