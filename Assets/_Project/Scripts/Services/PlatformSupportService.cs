using System.Runtime.InteropServices;

namespace DragonGlare.Services;

public sealed class PlatformSupportService
{
    public const int MinimumWindows10BuildNumber = 14393;

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
            "縺薙・繧｢繝励Μ縺ｯ Windows 10 x64 莉･荳雁ｰら畑縺ｧ縺吶・ +
            $"\n迴ｾ蝨ｨ縺ｮ迺ｰ蠅・ {normalizedDescription}" +
            $"\n繧｢繝ｼ繧ｭ繝・け繝√Ε: {osArchitecture}" +
            $"\nOS繝薙Ν繝・ {version.Build}" +
            $"\n隧ｳ邏ｰ: {reason}";
        return true;
    }

    public static string? GetUnsupportedReason(bool isWindows, Version version, Architecture osArchitecture)
    {
        if (!isWindows)
        {
            return "Windows 莉･螟悶・OS縺ｧ縺ｯ襍ｷ蜍輔〒縺阪∪縺帙ｓ縲・;
        }

        if (osArchitecture != Architecture.X64)
        {
            return "x64 莉･螟悶・繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｧ縺ｯ襍ｷ蜍輔〒縺阪∪縺帙ｓ縲・;
        }

        if (version.Major < 10 || (version.Major == 10 && version.Build < MinimumWindows10BuildNumber))
        {
            return $"Windows 10 縺ｮ譛蟆上ン繝ｫ繝・({MinimumWindows10BuildNumber}) 繧呈ｺ縺溘＠縺ｦ縺・∪縺帙ｓ縲・;
        }

        return null;
    }
}
