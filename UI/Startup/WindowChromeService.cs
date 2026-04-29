using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace DragonGlareAlpha;

internal sealed class WindowChromeService
{
    private static readonly string AppUserModelId = AppMetadata.AppUserModelId;

    private readonly GameWindow window;
    private readonly string title;
    private bool appUserModelIdApplied;
    private bool shellWindowPropertiesApplied;
    private bool windowIconApplied;

    public WindowChromeService(GameWindow window, string title)
    {
        this.window = window;
        this.title = title;
    }

    public bool IsIconApplied => windowIconApplied;

    public static void ApplyProcessAppUserModelId()
    {
        if (OperatingSystem.IsWindows())
        {
            _ = SetCurrentProcessExplicitAppUserModelID(AppUserModelId);
        }
    }

    public void Apply(bool forceIcon = false)
    {
        window.Title = title;
        ApplyAppUserModelId();
        ApplyShellWindowProperties();

        if (forceIcon || !windowIconApplied)
        {
            ApplyWindowIcon();
        }
    }

    public void InvalidateIcon()
    {
        windowIconApplied = false;
    }

    private void ApplyAppUserModelId()
    {
        if (appUserModelIdApplied || !OperatingSystem.IsWindows())
        {
            return;
        }

        if (SetCurrentProcessExplicitAppUserModelID(AppUserModelId) == 0)
        {
            appUserModelIdApplied = true;
        }
    }

    private void ApplyShellWindowProperties()
    {
        if (shellWindowPropertiesApplied || !OperatingSystem.IsWindows())
        {
            return;
        }

        var hwnd = GetNativeWindowHandle();
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var propertyStoreInterfaceId = typeof(IPropertyStore).GUID;
        if (SHGetPropertyStoreForWindow(hwnd, ref propertyStoreInterfaceId, out var propertyStore) != 0)
        {
            return;
        }

        try
        {
            SetStringProperty(propertyStore, PropertyKeys.AppUserModelId, AppUserModelId);
            SetStringProperty(propertyStore, PropertyKeys.RelaunchDisplayNameResource, title);

            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(processPath))
            {
                SetStringProperty(propertyStore, PropertyKeys.RelaunchCommand, $"\"{processPath}\"");
            }

            propertyStore.Commit();
            shellWindowPropertiesApplied = true;
        }
        catch
        {
        }
        finally
        {
            Marshal.FinalReleaseComObject(propertyStore);
        }
    }

    private IntPtr GetNativeWindowHandle()
    {
        var hwnd = GetSdlNativeWindowHandle();
        if (hwnd != IntPtr.Zero)
        {
            return hwnd;
        }

        if (IsCurrentProcessWindow(window.Handle))
        {
            return window.Handle;
        }

        return FindCurrentProcessWindowByTitle();
    }

    private IntPtr GetSdlNativeWindowHandle()
    {
        if (window.Handle == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        try
        {
            SdlGetVersion(out var version);
            var info = new SdlSysWmInfo
            {
                Version = version
            };

            return SdlGetWindowWMInfo(window.Handle, ref info) != 0 && info.Subsystem == SdlSysWmType.Windows
                ? info.Window
                : IntPtr.Zero;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    private static bool IsCurrentProcessWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero || !IsWindow(hwnd))
        {
            return false;
        }

        _ = GetWindowThreadProcessId(hwnd, out var processId);
        return processId == (uint)Environment.ProcessId;
    }

    private IntPtr FindCurrentProcessWindowByTitle()
    {
        var currentProcessId = (uint)Environment.ProcessId;
        var expectedTitle = title;
        var found = IntPtr.Zero;

        EnumWindows((hwnd, lParam) =>
        {
            if (!IsWindowVisible(hwnd))
            {
                return true;
            }

            _ = GetWindowThreadProcessId(hwnd, out var processId);
            if (processId != currentProcessId)
            {
                return true;
            }

            var windowTitle = GetWindowText(hwnd);
            if (!string.Equals(windowTitle, expectedTitle, StringComparison.Ordinal))
            {
                return true;
            }

            found = hwnd;
            return false;
        }, IntPtr.Zero);

        return found;
    }

    private static string GetWindowText(IntPtr hwnd)
    {
        var length = GetWindowTextLength(hwnd);
        if (length <= 0)
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder(length + 1);
        _ = GetWindowText(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    private static void SetStringProperty(IPropertyStore propertyStore, PropertyKey propertyKey, string value)
    {
        var key = propertyKey;
        var variant = PropVariant.FromString(value);
        try
        {
            propertyStore.SetValue(ref key, ref variant);
        }
        finally
        {
            PropVariantClear(ref variant);
        }
    }

    private void ApplyWindowIcon()
    {
        if (!OperatingSystem.IsWindows() || window.Handle == IntPtr.Zero)
        {
            return;
        }

        var iconPath = ResolveWindowIconPath();
        if (iconPath is null)
        {
            return;
        }

        try
        {
            using var icon = new Icon(iconPath);
            using var bitmap = icon.ToBitmap();
            using var argbBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(argbBitmap))
            {
                graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }

            var rect = new System.Drawing.Rectangle(0, 0, argbBitmap.Width, argbBitmap.Height);
            var data = argbBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var surface = SdlCreateRgbSurfaceFrom(
                    data.Scan0,
                    argbBitmap.Width,
                    argbBitmap.Height,
                    32,
                    data.Stride,
                    0x00ff0000,
                    0x0000ff00,
                    0x000000ff,
                    0xff000000);

                if (surface == IntPtr.Zero)
                {
                    return;
                }

                try
                {
                    SdlSetWindowIcon(window.Handle, surface);
                    windowIconApplied = true;
                }
                finally
                {
                    SdlFreeSurface(surface);
                }
            }
            finally
            {
                argbBitmap.UnlockBits(data);
            }
        }
        catch
        {
        }
    }

    private static string? ResolveWindowIconPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Dragon_glare.ico"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Dragon_glare.ico"),
            Path.Combine(Directory.GetCurrentDirectory(), "Dragon_glare.ico"),
            Path.Combine(AppContext.BaseDirectory, "Resources", "App", "dragon_glare.ico"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", "App", "dragon_glare.ico"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "App", "dragon_glare.ico")
        };

        foreach (var candidate in candidates)
        {
            var normalized = Path.GetFullPath(candidate);
            if (File.Exists(normalized))
            {
                return normalized;
            }
        }

        return null;
    }

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateRGBSurfaceFrom")]
    private static extern IntPtr SdlCreateRgbSurfaceFrom(
        IntPtr pixels,
        int width,
        int height,
        int depth,
        int pitch,
        uint rmask,
        uint gmask,
        uint bmask,
        uint amask);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowIcon")]
    private static extern void SdlSetWindowIcon(IntPtr window, IntPtr icon);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_FreeSurface")]
    private static extern void SdlFreeSurface(IntPtr surface);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetVersion")]
    private static extern void SdlGetVersion(out SdlVersion version);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowWMInfo")]
    private static extern int SdlGetWindowWMInfo(IntPtr window, ref SdlSysWmInfo info);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);

    [DllImport("shell32.dll", PreserveSig = true)]
    private static extern int SHGetPropertyStoreForWindow(
        IntPtr hwnd,
        ref Guid riid,
        out IPropertyStore propertyStore);

    [DllImport("ole32.dll", PreserveSig = true)]
    private static extern int PropVariantClear(ref PropVariant propVariant);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder text, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

    private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        void GetCount(out uint propertyCount);

        void GetAt(uint propertyIndex, out PropertyKey propertyKey);

        void GetValue(ref PropertyKey propertyKey, out PropVariant value);

        void SetValue(ref PropertyKey propertyKey, ref PropVariant value);

        void Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private readonly struct PropertyKey
    {
        public PropertyKey(Guid formatId, uint propertyId)
        {
            FormatId = formatId;
            PropertyId = propertyId;
        }

        public readonly Guid FormatId;
        public readonly uint PropertyId;
    }

    private static class PropertyKeys
    {
        private static readonly Guid AppUserModelFormatId = new("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");

        public static readonly PropertyKey RelaunchCommand = new(AppUserModelFormatId, 2);
        public static readonly PropertyKey RelaunchDisplayNameResource = new(AppUserModelFormatId, 4);
        public static readonly PropertyKey AppUserModelId = new(AppUserModelFormatId, 5);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PropVariant
    {
        private const ushort VtLpwstr = 31;

        private ushort valueType;
        private ushort reserved1;
        private ushort reserved2;
        private ushort reserved3;
        private IntPtr pointerValue;

        public static PropVariant FromString(string value)
        {
            return new PropVariant
            {
                valueType = VtLpwstr,
                pointerValue = Marshal.StringToCoTaskMemUni(value)
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SdlVersion
    {
        public byte Major;
        public byte Minor;
        public byte Patch;
    }

    private enum SdlSysWmType
    {
        Unknown,
        Windows
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SdlSysWmInfo
    {
        public SdlVersion Version;
        public SdlSysWmType Subsystem;
        public IntPtr Window;
        public IntPtr Hdc;
        public IntPtr HInstance;
    }
}
