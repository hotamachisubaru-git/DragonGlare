using System.Runtime.InteropServices;
using DragonGlareAlpha.Services;

namespace DragonGlareAlpha.Tests;

public sealed class PlatformSupportServiceTests
{
    [Fact]
    public void GetUnsupportedReason_ReturnsNull_ForWindows10X64()
    {
        var reason = PlatformSupportService.GetUnsupportedReason(
            isWindows: true,
            version: new Version(10, 0, 19045, 0),
            osArchitecture: Architecture.X64);

        Assert.Null(reason);
    }

    [Fact]
    public void GetUnsupportedReason_ReturnsNull_ForWindows11X64()
    {
        var reason = PlatformSupportService.GetUnsupportedReason(
            isWindows: true,
            version: new Version(10, 0, 22000, 0),
            osArchitecture: Architecture.X64);

        Assert.Null(reason);
    }

    [Fact]
    public void GetUnsupportedReason_ReturnsReason_ForNonWindows()
    {
        var reason = PlatformSupportService.GetUnsupportedReason(
            isWindows: false,
            version: new Version(10, 0, PlatformSupportService.MinimumWindows10BuildNumber, 0),
            osArchitecture: Architecture.X64);

        Assert.Equal("Windows 以外のOSでは起動できません。", reason);
    }

    [Fact]
    public void GetUnsupportedReason_ReturnsReason_ForNonX64Architecture()
    {
        var reason = PlatformSupportService.GetUnsupportedReason(
            isWindows: true,
            version: new Version(10, 0, PlatformSupportService.MinimumWindows10BuildNumber, 0),
            osArchitecture: Architecture.Arm64);

        Assert.Equal("x64 以外のアーキテクチャでは起動できません。", reason);
    }

    [Fact]
    public void GetUnsupportedReason_ReturnsReason_ForOldWindows10Build()
    {
        var reason = PlatformSupportService.GetUnsupportedReason(
            isWindows: true,
            version: new Version(10, 0, 10240, 0),
            osArchitecture: Architecture.X64);

        Assert.Equal("Windows 10 の最小ビルド (14393) を満たしていません。", reason);
    }

    [Fact]
    public void TryDetectUnsupportedPlatform_BuildsUserFacingMessage()
    {
        var unsupported = PlatformSupportService.TryDetectUnsupportedPlatform(
            isWindows: true,
            version: new Version(10, 0, 10240, 0),
            osArchitecture: Architecture.X64,
            osDescription: "Microsoft Windows 10.0.10240",
            out var message);

        Assert.True(unsupported);
        Assert.Contains("Windows 10 x64 以上専用", message);
        Assert.Contains("10240", message);
        Assert.Contains("Microsoft Windows 10.0.10240", message);
    }
}
