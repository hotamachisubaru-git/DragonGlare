using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DragonGlareAlpha.Security;

public sealed partial class AntiCheatService
{
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
        return new AntiCheatService().TryDetectViolation(out message);
    }

    private static bool _isViolationDetected;
    private static string _detectedMessage = string.Empty;
    private static bool _isScanning;

    public bool TryDetectViolation(out string message)
    {
        if (_isViolationDetected)
        {
            message = _detectedMessage;
            return true;
        }

        if (Debugger.IsAttached || IsDebuggerPresent() || IsRemoteDebuggerAttached())
        {
            message = "デバッガを検知したため起動を停止しました。";
            return true;
        }

        // 重いプロセス走査は非同期で裏側で回す
        TriggerAsyncProcessScan();

        message = string.Empty;
        return false;
    }

    private void TriggerAsyncProcessScan()
    {
        if (_isScanning) return;

        Task.Run(() =>
        {
            _isScanning = true;
            try
            {
                if (TryFindSuspiciousProcess(out var processLabel))
                {
                    _detectedMessage = $"不正ツールを検知したため終了します。\n検知対象: {processLabel}";
                    _isViolationDetected = true;
                }
            }
            finally
            {
                _isScanning = false;
            }
        });
    }

    private static bool TryFindSuspiciousProcess(out string processLabel)
    {
        processLabel = string.Empty;

        try
        {
            using var currentProcess = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcesses())
            {
                using (process)
                {
                    if (process.Id == currentProcess.Id)
                    {
                        continue;
                    }

                    var processName = process.ProcessName ?? string.Empty;
                    var windowTitle = string.Empty;
                    try
                    {
                        windowTitle = process.MainWindowTitle ?? string.Empty;
                    }
                    catch
                    {
                    }

                    if (ContainsSuspiciousKeyword(processName, SuspiciousProcessKeywords) ||
                        ContainsSuspiciousKeyword(windowTitle, SuspiciousWindowKeywords))
                    {
                        processLabel = string.IsNullOrWhiteSpace(windowTitle)
                            ? processName
                            : $"{processName} ({windowTitle})";
                        return true;
                    }
                }
            }
        }
        catch
        {
        }

        return false;
    }

    private static bool ContainsSuspiciousKeyword(string value, IEnumerable<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
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
