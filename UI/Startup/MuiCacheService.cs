using Microsoft.Win32;

namespace DragonGlareAlpha;

internal static class MuiCacheService
{
    private const string MuiCacheSubKeyPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";

    public static void SyncCurrentExecutableMetadata()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return;
        }

        try
        {
            using var muiCacheKey = Registry.CurrentUser.CreateSubKey(MuiCacheSubKeyPath, writable: true);
            if (muiCacheKey is null)
            {
                return;
            }

            muiCacheKey.SetValue($"{executablePath}.ApplicationCompany", AppMetadata.DisplayName, RegistryValueKind.String);
            muiCacheKey.SetValue($"{executablePath}.FriendlyAppName", AppMetadata.WindowTitle, RegistryValueKind.String);
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
        catch (System.Security.SecurityException)
        {
        }
    }
}
