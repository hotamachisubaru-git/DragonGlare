using System.Reflection;
using DragonGlareAlpha.Domain;

namespace DragonGlareAlpha;

internal static class AppMetadata
{
    public static string DisplayName => Constants.ProjectDisplayName;

    public static string Version => GetVersion();

    public static string WindowTitle => string.IsNullOrWhiteSpace(Version)
        ? DisplayName
        : $"{DisplayName} {Version}";

    public static string AppUserModelId
    {
        get
        {
            var sanitizedVersion = string.Concat(Version.Select(character =>
                char.IsLetterOrDigit(character) || character == '.'
                    ? character
                    : '.'));

            return string.IsNullOrWhiteSpace(sanitizedVersion)
                ? "DragonGlare.Alpha"
                : $"DragonGlare.Alpha.v{sanitizedVersion}";
        }
    }

    private static string GetVersion()
    {
        var assembly = typeof(AppMetadata).Assembly;
        var version = assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>()
            ?.Version
            ?? assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? assembly.GetName().Version?.ToString(3)
            ?? string.Empty;

        var metadataIndex = version.IndexOf('+', StringComparison.Ordinal);
        version = metadataIndex > 0 ? version[..metadataIndex] : version;

        return version.StartsWith(DisplayName, StringComparison.OrdinalIgnoreCase)
            ? version[DisplayName.Length..].Trim()
            : version;
    }
}
