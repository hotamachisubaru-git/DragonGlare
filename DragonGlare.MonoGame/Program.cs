using DragonGlareAlpha;
using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Security;
using DragonGlareAlpha.Services;
using System.Windows.Forms;

namespace DragonGlare.MonoGame;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var platformSupportService = new PlatformSupportService();
        if (platformSupportService.TryDetectUnsupportedPlatform(out var platformMessage))
        {
            MessageBox.Show(platformMessage, "DragonGlare Alpha", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return;
        }

        if (AntiCheatService.TryDetectStartupViolation(out var message))
        {
            MessageBox.Show(message, "DragonGlare Alpha", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return;
        }

        var launchSettingsService = new LaunchSettingsService();
        var launchSettings = launchSettingsService.Load();

        if (launchSettings.PromptOnStartup)
        {
            using var launchOptionsDialog = new LaunchOptionsDialog(launchSettings);
            if (launchOptionsDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            launchSettings = launchOptionsDialog.SelectedSettings;
            launchSettingsService.Save(launchSettings);
        }

        using var game = new global::DragonGlareAlpha.DragonGlareAlpha(launchSettings);
        game.Window.Title = "DragonGlare.Alpha";
        game.Run();
    }
}
