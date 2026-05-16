using System.Runtime.InteropServices;

namespace DragonGlare.Services;

public sealed class PlatformSupportService
{
    public const int MinimumWindows10BuildNumber = 14393;

    public bool TryDetectUnsupportedPlatform(out string message)
    {
        return TryDetectUnsupportedPlatform(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
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
            "このアプリは Windows 10 x64 以上専用です。" +
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

        if (version.Major < 10 || (version.Major == 10 && version.Build < MinimumWindows10BuildNumber))
        {
            return $"Windows 10 の最小ビルド ({MinimumWindows10BuildNumber}) を満たしていません。";
        }

        return null;
    }
}
