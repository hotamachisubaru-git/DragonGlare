using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
using XnaSong = Microsoft.Xna.Framework.Media.Song;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
using DragonGlareAlpha.Domain.Startup;
using static DragonGlareAlpha.Domain.Constants;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private bool WasConfirmPressed() => WasPressed(XnaKeys.Enter) || WasPressed(XnaKeys.Z) || WasPressed(XnaKeys.Space);
    private bool WasPressed(XnaKeys key) => currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    private bool IsDown(XnaKeys key) => currentKeyboardState.IsKeyDown(key);

    private void ApplyDisplayMode()
    {
        if (activeDisplayMode == LaunchDisplayMode.Fullscreen)
        {
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }
        else
        {
            graphics.IsFullScreen = false;
            var size = GetWindowedClientSize(activeDisplayMode);
            graphics.PreferredBackBufferWidth = size.Width;
            graphics.PreferredBackBufferHeight = size.Height;
            lastWindowedDisplayMode = activeDisplayMode;
        }
        graphics.ApplyChanges();
    }

    private void ToggleFullscreen()
    {
        activeDisplayMode = (activeDisplayMode == LaunchDisplayMode.Fullscreen) ? lastWindowedDisplayMode : LaunchDisplayMode.Fullscreen;
        ApplyDisplayMode();
    }

    private static (int Width, int Height) GetWindowedClientSize(LaunchDisplayMode mode) => mode switch {
        LaunchDisplayMode.Window720p => (1280, 720),
        LaunchDisplayMode.Window1080p => (1920, 1080),
        _ => (640, 480)
    };

    private Texture2D? LoadTextureFromAssets(params string[] parts)
    {
        var paths = new[] { Path.Combine([AppContext.BaseDirectory, "Assets", .. parts]), Path.Combine([Directory.GetCurrentDirectory(), "Assets", .. parts]) };
        foreach (var p in paths.Select(Path.GetFullPath).Distinct())
        {
            if (!File.Exists(p)) continue;
            using var s = File.OpenRead(p); return Texture2D.FromStream(GraphicsDevice, s);
        }
        return null;
    }

    private XnaSong? LoadSongFromRoot(string fileName)
    {
        var paths = new[] { Path.Combine(AppContext.BaseDirectory, fileName), Path.Combine(Directory.GetCurrentDirectory(), fileName) };
        foreach (var p in paths.Select(Path.GetFullPath).Distinct())
        {
            if (!File.Exists(p)) continue;
            try { return XnaSong.FromUri(Path.GetFileNameWithoutExtension(p), new Uri(p, UriKind.Absolute)); } catch { }
        }
        return null;
    }

    private void StartPrologueBgm()
    {
        if (isPrologueBgmPlaying || prologueSong is null) return;
        try { XnaMediaPlayer.IsRepeating = false; XnaMediaPlayer.Volume = 0.85f; XnaMediaPlayer.Play(prologueSong); isPrologueBgmPlaying = true; }
        catch { isPrologueBgmPlaying = false; }
    }

    private void StopPrologueBgm()
    {
        if (!isPrologueBgmPlaying) return;
        try { XnaMediaPlayer.Stop(); } catch { }
        isPrologueBgmPlaying = false;
    }

    private void LoadUiFont()
    {
        var paths = new[] { Path.Combine(AppContext.BaseDirectory, "JF-Dot-ShinonomeMin14.ttf"), Path.Combine(Directory.GetCurrentDirectory(), "JF-Dot-ShinonomeMin14.ttf") };
        foreach (var p in paths.Select(Path.GetFullPath).Distinct())
        {
            if (!File.Exists(p)) continue;
            privateFontCollection.AddFontFile(p);
            if (privateFontCollection.Families.Length > 0) { uiFont = new Font(privateFontCollection.Families[0], UiFontPixelSize, GraphicsUnit.Pixel); return; }
        }
        uiFont = new Font("MS Gothic", UiFontPixelSize, GraphicsUnit.Pixel);
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        if (!skipSaveOnClose) TryPersistProgress();
        base.OnExiting(sender, args);
    }

    protected override void UnloadContent()
    {
        foreach (var texture in textTextureCache.Values) texture.Dispose();
        textTextureCache.Clear();
        openingTexture?.Dispose();
        menuWindowTexture?.Dispose();
        battleFieldTexture?.Dispose();
        battleWindowTexture?.Dispose();
        fieldTileTexture?.Dispose();
        prologueSong?.Dispose();
        pixelTexture?.Dispose();
        uiFont?.Dispose();
        privateFontCollection.Dispose();
        base.UnloadContent();
    }
}
