using DragonGlareAlpha.Domain.Startup;

using var game = new DragonGlareAlpha.DragonGlareAlpha(new LaunchSettings
{
    DisplayMode = LaunchDisplayMode.Window720p,
    PromptOnStartup = false
});

game.Window.Title = "DragonGlare.Alpha";
game.Run();
