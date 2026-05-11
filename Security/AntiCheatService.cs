using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DragonGlareAlpha.Security;

public sealed partial class AntiCheatService
{
    private const int ProcessScanCooldownMilliseconds = 5000;

    private static readonly string[] SuspiciousProcessKeywords =
    [
        "cheatengine",
        "cheat engine",
        "artmoney",
        "x64dbg",
        "x32dbg",
        "ollydbg",
        "dnspy",
        "ilspy",
        "processhacker",
        "scanmem"
    ];

    private static readonly string[] SuspiciousWindowKeywords =
    [
        "cheat engine",
        "artmoney",
        "x64dbg",
        "x32dbg",
        "dnspy",
        "process hacker"
    ];

    public static bool TryDetectStartupViolation(out string message)
    {
        var service = new AntiCheatService();
        if (service.TryDetectDebugger(out message))
        {
            ReportViolation(message);
            return true;
        }

        if (TryFindSuspiciousProcess(out var processLabel))
        {
            message = FormatSuspiciousProcessMessage(processLabel);
            ReportViolation(message);
            return true;
        }

        message = string.Empty;
        return false;
    }

    private static bool _isViolationDetected;
    private static string _detectedMessage = string.Empty;
    private static int _isScanning;
    private static long _lastProcessScanTimestamp;

    public bool TryDetectViolation(out string message)
    {
        if (Volatile.Read(ref _isViolationDetected))
        {
            message = _detectedMessage;
            return true;
        }

        if (TryDetectDebugger(out message))
        {
            ReportViolation(message);
            return true;
        }

        TriggerAsyncProcessScan();

        message = string.Empty;
        return false;
    }

    private void TriggerAsyncProcessScan()
    {
        if (!ShouldStartProcessScan() ||
            Interlocked.CompareExchange(ref _isScanning, 1, 0) != 0)
        {
            return;
        }

        Task.Run(() =>
        {
            try
            {
                if (TryFindSuspiciousProcess(out var processLabel))
                {
                    ReportViolation(FormatSuspiciousProcessMessage(processLabel));
                }
            }
            finally
            {
                Volatile.Write(ref _isScanning, 0);
            }
        });
    }

    private static bool ShouldStartProcessScan()
    {
        var now = Stopwatch.GetTimestamp();
        var lastScan = Volatile.Read(ref _lastProcessScanTimestamp);
        if (lastScan != 0)
        {
            var elapsedMilliseconds = (now - lastScan) * 1000d / Stopwatch.Frequency;
            if (elapsedMilliseconds < ProcessScanCooldownMilliseconds)
            {
                return false;
            }
        }

        Volatile.Write(ref _lastProcessScanTimestamp, now);
        return true;
    }

    private static bool TryFindSuspiciousProcess(out string processLabel)
    {
        processLabel = string.Empty;

        Process[] processes;
        try
        {
            processes = Process.GetProcesses();
        }
        catch
        {
            return false;
        }

        var currentProcessId = Environment.ProcessId;
        foreach (var process in processes)
        {
            using (process)
            {
                try
                {
                    if (process.Id == currentProcessId)
                    {
                        continue;
                    }

                    var processName = process.ProcessName ?? string.Empty;
                    if (ContainsSuspiciousKeyword(processName, SuspiciousProcessKeywords))
                    {
                        processLabel = processName;
                        return true;
                    }

                    var windowTitle = TryReadMainWindowTitle(process);
                    if (ContainsSuspiciousKeyword(windowTitle, SuspiciousWindowKeywords))
                    {
                        processLabel = string.IsNullOrWhiteSpace(processName)
                            ? windowTitle
                            : $"{processName} ({windowTitle})";
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        return false;
    }

    private static string TryReadMainWindowTitle(Process process)
    {
        try
        {
            return process.MainWindowTitle ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool ContainsSuspiciousKeyword(string value, IEnumerable<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool TryDetectDebugger(out string message)
    {
        try
        {
            if (Debugger.IsAttached)
            {
                message = "デバッガを検知したため起動を停止しました。";
                return true;
            }

            if (OperatingSystem.IsWindows() && (IsDebuggerPresent() || IsRemoteDebuggerAttached()))
            {
                message = "デバッガを検知したため起動を停止しました。";
                return true;
            }
        }
        catch
        {
        }

        message = string.Empty;
        return false;
    }

    private static string FormatSuspiciousProcessMessage(string processLabel)
    {
        return $"不正ツールを検知したため終了します。\n検知対象: {processLabel}";
    }

    private static void ReportViolation(string message)
    {
        _detectedMessage = message;
        Volatile.Write(ref _isViolationDetected, true);
    }

    private static bool IsRemoteDebuggerAttached()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var attached = false;
            return CheckRemoteDebuggerPresent(process.Handle, ref attached) && attached;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsDebuggerPresent();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CheckRemoteDebuggerPresent(
        IntPtr processHandle,
        [MarshalAs(UnmanagedType.Bool)] ref bool isDebuggerPresent);
}
