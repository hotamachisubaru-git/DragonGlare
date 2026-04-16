using DragonGlareAlpha.Security;
using DragonGlareAlpha.Domain.Startup;
using DragonGlareAlpha.Services;
using System.Drawing.Text;

namespace DragonGlareAlpha;

static class Program
{
    public static Font? UiFont;
    public static Font? SmallFont;
    public static Font? TitleFont;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // フォントのロード
        var fontCollection = new PrivateFontCollection();
        fontCollection.AddFontFile("JF-Dot-ShinonomeMin14.ttf");

        // ロードしたフォントをアプリケーション全体で利用できるように静的フィールドに設定
        if (fontCollection.Families.Length > 0)
        {
            // ドットの崩れを防ぐため、基準サイズ(14px)を使用
            UiFont = new Font(fontCollection.Families[0], 14, FontStyle.Regular, GraphicsUnit.Pixel);
            SmallFont = new Font(fontCollection.Families[0], 14, FontStyle.Regular, GraphicsUnit.Pixel);
            TitleFont = new Font(fontCollection.Families[0], 14, FontStyle.Bold, GraphicsUnit.Pixel);
        }

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

        Application.Run(new DragonGlare(launchSettings));
    }
}
