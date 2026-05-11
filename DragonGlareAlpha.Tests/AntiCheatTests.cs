using System.Reflection;
using DragonGlareAlpha.Security;

namespace DragonGlareAlpha.Tests;

public sealed class AntiCheatTests
{
    [Fact]
    public void ProtectedInt_WhenInternalStateWasMutated_ThrowsTamperDetectedException()
    {
        var protectedInt = new ProtectedInt(220);
        var encodedValueField = typeof(ProtectedInt).GetField("encodedValue", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(encodedValueField);
        encodedValueField!.SetValue(protectedInt, 1);

        Assert.Throws<TamperDetectedException>(() => _ = protectedInt.Value);
    }

    [Fact]
    public void ProtectedInt_WhenRekeyed_PreservesValue()
    {
        var protectedInt = new ProtectedInt(1234);

        protectedInt.Rekey();

        Assert.Equal(1234, protectedInt.Value);
    }

    [Fact]
    public void AntiCheatKeywordDetection_IgnoresCase()
    {
        var method = typeof(AntiCheatService).GetMethod(
            "ContainsSuspiciousKeyword",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(method);
        var detected = (bool)method!.Invoke(
            null,
            ["Cheat Engine 7.5", new[] { "cheat engine" }])!;

        Assert.True(detected);
    }
}
