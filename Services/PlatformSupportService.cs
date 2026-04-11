using System.Runtime.InteropServices;

namespace DragonGlareAlpha.Services;

public sealed class PlatformSupportService
{
    public const int Windows11BuildNumber = 22000;

    public bool TryDetectUnsupportedPlatform(out string message)
    {
        return TryDetectUnsupportedPlatform(
            OperatingSystem.IsWindows(),
            Environment.OSVersion.Version,
            RuntimeInformation.OSArchitecture,
            RuntimeInformation.OSDescription,
            out message);
    }

    public static bool TryDetectUnsupportedPlatform(
        bool isWindows,
        Version version,
        Architecture osArchitecture,
        string osDescription,
        out string message)
    {
        var reason = GetUnsupportedReason(isWindows, version, osArchitecture);
        if (reason is null)
        {
            message = string.Empty;
            return false;
        }

        var normalizedDescription = string.IsNullOrWhiteSpace(osDescription)
            ? "Unknown OS"
            : osDescription.Trim();

        message =
            "このアプリは Windows 11 x64 専用です。" +
            $"\n現在の環境: {normalizedDescription}" +
            $"\nアーキテクチャ: {osArchitecture}" +
            $"\nOSビルド: {version.Build}" +
            $"\n詳細: {reason}";
        return true;
    }

    public static string? GetUnsupportedReason(bool isWindows, Version version, Architecture osArchitecture)
    {
        if (!isWindows)
        {
            return "Windows 以外のOSでは起動できません。";
        }

        if (osArchitecture != Architecture.X64)
        {
            return "x64 以外のアーキテクチャでは起動できません。";
        }

        if (version.Build < Windows11BuildNumber)
        {
            return $"Windows 11 の最小ビルド ({Windows11BuildNumber}) を満たしていません。";
        }

        return null;
    }
}
