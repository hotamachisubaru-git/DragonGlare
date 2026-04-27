using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace DragonGlareAlpha;

internal sealed class WindowChromeService
{
    private const string AppUserModelId = "DragonGlare.Alpha";

    private readonly GameWindow window;
    private readonly string title;
    private bool appUserModelIdApplied;
    private bool windowIconApplied;

    public WindowChromeService(GameWindow window, string title)
    {
        this.window = window;
        this.title = title;
    }

    public bool IsIconApplied => windowIconApplied;

    public void Apply(bool forceIcon = false)
    {
        window.Title = title;
        ApplyAppUserModelId();

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

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);
}
