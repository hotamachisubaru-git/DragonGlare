using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Security;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha;

static class Program
{
    [STAThread]
    static void Main()
    {
        WindowChromeService.ApplyProcessAppUserModelId();
        MuiCacheService.SyncCurrentExecutableMetadata();

        var platformSupportService = new PlatformSupportService();
        if (platformSupportService.TryDetectUnsupportedPlatform(out var platformMessage))
        {
            StartupErrorGame.Show(platformMessage);
            return;
        }

        if (AntiCheatService.TryDetectStartupViolation(out var message))
        {
            StartupErrorGame.Show(message);
            return;
        }

        var launchSettingsService = new LaunchSettingsService();
        var launchSettings = launchSettingsService.Load();

        using var game = new global::DragonGlareAlpha.DragonGlareAlpha(launchSettings);
        game.Run();
    }
}
