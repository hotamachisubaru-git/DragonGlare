using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Security;
using DragonGlareAlpha.Services;
using System.Runtime.InteropServices;

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
            NativeMethods.ShowError(platformMessage);
            return;
        }

        if (AntiCheatService.TryDetectStartupViolation(out var message))
        {
            NativeMethods.ShowError(message);
            return;
        }

        var launchSettingsService = new LaunchSettingsService();
        var launchSettings = launchSettingsService.Load();

        using var game = new global::DragonGlareAlpha.DragonGlareAlpha(launchSettings);
        game.Run();
    }
}

static class NativeMethods
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static void ShowError(string message)
    {
        MessageBox(IntPtr.Zero, message, "DragonGlare Alpha Error", 0x00000010);
    }
}
