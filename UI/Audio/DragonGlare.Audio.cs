using System.IO;
using DragonGlareAlpha.Domain;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using XnaSoundEffect = Microsoft.Xna.Framework.Audio.SoundEffect;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private readonly object audioSyncRoot = new();
    private AudioFileReader? bgmReader;
    private IWavePlayer? bgmOutput;
    private Uri? currentBgmUri;
    private BgmTrack? failedBgmTrack;
    private bool isStoppingBgm;

    private void LoadCustomFont()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "JF-Dot-ShinonomeMin14.ttf"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "JF-Dot-ShinonomeMin14.ttf"),
            Path.Combine(Directory.GetCurrentDirectory(), "JF-Dot-ShinonomeMin14.ttf")
        };

        string? fontPath = null;
        foreach (var candidate in candidates)
        {
            var normalized = Path.GetFullPath(candidate);
            if (File.Exists(normalized))
            {
                fontPath = normalized;
                break;
            }
        }

        if (fontPath is null)
        {
            return;
        }

        privateFontCollection.AddFontFile(fontPath);
        if (privateFontCollection.Families.Length == 0)
        {
            return;
        }

        uiFont.Dispose();
        smallFont.Dispose();
        uiFont = new Font(privateFontCollection.Families[0], UiTypography.FontPixelSize, FontStyle.Regular, GraphicsUnit.Pixel);
        smallFont = new Font(privateFontCollection.Families[0], UiTypography.FontPixelSize, FontStyle.Regular, GraphicsUnit.Pixel);
        fontLoaded = true;
    }

    private void InitializeAudio()
    {
        RegisterBgm(BgmTrack.MainMenu, "main_menu", "glare");
        RegisterBgm(BgmTrack.Prologue, "prologue02", "prologue2");
        RegisterBgm(BgmTrack.Field, "field");
        RegisterBgm(BgmTrack.Castle, "castle");
        RegisterBgm(BgmTrack.Battle, "battle");
        RegisterBgm(BgmTrack.Shop, "shop_buy", "shop", "ショップ", "ショップのシーン", "(ショップのシーン)", "(ショップのシーン）", "field");

        RegisterSe(SoundEffect.Dialog, "Serif_SE.mp3");
        RegisterSe(SoundEffect.Collision, "当たり判定SFC.mp3", "当たり判定SFC.wav");

        UpdateBgm();
    }

    private void RegisterBgm(BgmTrack track, string sceneName, params string[] fallbackSceneNames)
    {
        var sceneNames = new string[1 + fallbackSceneNames.Length];
        sceneNames[0] = sceneName;
        for (var index = 0; index < fallbackSceneNames.Length; index++)
        {
            sceneNames[index + 1] = fallbackSceneNames[index];
        }

        var path = ResolveBgmPath(sceneNames);
        if (path is not null)
        {
            bgmUris[track] = new Uri(path, UriKind.Absolute);
        }
    }

    private void RegisterSe(SoundEffect effect, params string[] fileNames)
    {
        var path = ResolveAssetPath("SE", fileNames);
        if (path is not null)
        {
            seUris[effect] = new Uri(path, UriKind.Absolute);
        }
    }

    private static string GetBgmFileName(string sceneName)
    {
        return $"SFC_{sceneName}.mp3";
    }

    private static string? ResolveBgmPath(params string[] sceneNames)
    {
        foreach (var sceneName in sceneNames)
        {
            var path = ResolveAssetPath("BGM", BuildBgmCandidateNames(sceneName).ToArray());
            if (path is not null)
            {
                return path;
            }
        }

        var bgmDirectory = ResolveAssetDirectory("BGM");
        if (bgmDirectory is null)
        {
            return null;
        }

        var audioFiles = Directory.EnumerateFiles(bgmDirectory)
            .Where(path => path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var sceneName in sceneNames)
        {
            var token = NormalizeAssetLookupToken(sceneName);
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            var match = audioFiles.FirstOrDefault(path =>
            {
                var fileToken = NormalizeAssetLookupToken(Path.GetFileNameWithoutExtension(path));
                return fileToken.Contains(token, StringComparison.Ordinal) || token.Contains(fileToken, StringComparison.Ordinal);
            });

            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static IEnumerable<string> BuildBgmCandidateNames(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            yield break;
        }

        if (Path.HasExtension(sceneName))
        {
            yield return sceneName;
            yield break;
        }

        if (sceneName.StartsWith("SFC_", StringComparison.OrdinalIgnoreCase))
        {
            yield return $"{sceneName}.mp3";
            yield return $"{sceneName}.wav";
            yield break;
        }

        yield return GetBgmFileName(sceneName);
        yield return $"SFC_{sceneName}.wav";
        yield return $"{sceneName}.mp3";
        yield return $"{sceneName}.wav";
    }

    private static string NormalizeAssetLookupToken(string value)
    {
        return string.Concat(value.Where(char.IsLetterOrDigit)).ToLowerInvariant();
    }

    private static string? ResolveAssetPath(string? assetSubdirectory, params string[] fileNames)
    {
        foreach (var name in fileNames)
        {
            var relativeCandidates = assetSubdirectory is null
                ? new[]
                {
                    Path.Combine("アセット", name),
                    Path.Combine("Assets", name),
                    Path.Combine("Assets", "Audio", name)
                }
                : new[]
                {
                    Path.Combine("アセット", name),
                    Path.Combine("Assets", assetSubdirectory, name),
                    Path.Combine("Assets", name),
                    Path.Combine("Assets", "Audio", name)
                };

            foreach (var relativePath in relativeCandidates)
            {
                var roots = new[]
                {
                    AppContext.BaseDirectory,
                    Path.Combine(AppContext.BaseDirectory, "..", "..", ".."),
                    Directory.GetCurrentDirectory()
                };

                foreach (var root in roots)
                {
                    var normalized = Path.GetFullPath(Path.Combine(root, relativePath));
                    if (File.Exists(normalized))
                    {
                        return normalized;
                    }
                }
            }
        }

        return null;
    }

    private static string? ResolveAssetDirectory(string? assetSubdirectory)
    {
        var relativeCandidates = assetSubdirectory is null
            ? new[]
            {
                "アセット",
                "Assets",
                Path.Combine("Assets", "Audio")
            }
            : new[]
            {
                "アセット",
                Path.Combine("Assets", assetSubdirectory),
                "Assets",
                Path.Combine("Assets", "Audio")
            };

        foreach (var relativePath in relativeCandidates)
        {
            var roots = new[]
            {
                AppContext.BaseDirectory,
                Path.Combine(AppContext.BaseDirectory, "..", "..", ".."),
                Directory.GetCurrentDirectory()
            };

            foreach (var root in roots)
            {
                var normalized = Path.GetFullPath(Path.Combine(root, relativePath));
                if (Directory.Exists(normalized))
                {
                    return normalized;
                }
            }
        }

        return null;
    }

    private void UpdateBgm()
    {
        var desiredTrack = GetDesiredBgmTrack();
        if (desiredTrack == BgmTrack.Prologue && prologueBgmCompleted)
        {
            return;
        }

        if (currentBgmTrack == desiredTrack && bgmOutput is not null)
        {
            if (bgmOutput.PlaybackState != PlaybackState.Playing)
            {
                bgmOutput.Play();
            }

            return;
        }

        if (failedBgmTrack == desiredTrack)
        {
            return;
        }

        if (!bgmUris.TryGetValue(desiredTrack, out var bgmUri))
        {
            StopBgm();
            currentBgmTrack = null;
            failedBgmTrack = desiredTrack;
            return;
        }

        PlayBgm(desiredTrack, bgmUri);
    }

    private void PlayBgm(BgmTrack track, Uri bgmUri)
    {
        lock (audioSyncRoot)
        {
            StopBgmLocked();

            try
            {
                bgmReader = new AudioFileReader(bgmUri.LocalPath)
                {
                    Volume = 0.85f
                };
                bgmOutput = CreateBgmOutputDevice();
                bgmOutput.Init(bgmReader);
                bgmOutput.PlaybackStopped += HandleBgmPlaybackStopped;
                bgmOutput.Play();

                currentBgmTrack = track;
                currentBgmUri = bgmUri;
                failedBgmTrack = null;
            }
            catch
            {
                StopBgmLocked();
                currentBgmTrack = null;
                currentBgmUri = null;
                failedBgmTrack = track;
            }
        }
    }

    private IWavePlayer CreateBgmOutputDevice()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var endpoint = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return new WasapiOut(endpoint, AudioClientShareMode.Shared, false, 100);
        }
        catch
        {
            return new WaveOutEvent
            {
                DesiredLatency = 100
            };
        }
    }

    private void HandleBgmPlaybackStopped(object? sender, StoppedEventArgs args)
    {
        lock (audioSyncRoot)
        {
            if (isStoppingBgm || bgmReader is null || bgmOutput is null || currentBgmUri is null)
            {
                return;
            }

            if (args.Exception is not null)
            {
                StopBgmLocked();
                currentBgmTrack = null;
                currentBgmUri = null;
                failedBgmTrack = GetDesiredBgmTrack();
                return;
            }

            if (currentBgmTrack == BgmTrack.Prologue)
            {
                prologueBgmCompleted = true;
                StopBgmLocked();
                return;
            }

            bgmReader.Position = 0;
            bgmOutput.Play();
        }
    }

    private void StopBgm()
    {
        lock (audioSyncRoot)
        {
            StopBgmLocked();
        }
    }

    private void StopBgmLocked()
    {
        isStoppingBgm = true;

        if (bgmOutput is not null)
        {
            bgmOutput.PlaybackStopped -= HandleBgmPlaybackStopped;
            bgmOutput.Stop();
            bgmOutput.Dispose();
            bgmOutput = null;
        }

        bgmReader?.Dispose();
        bgmReader = null;
        currentBgmTrack = null;
        currentBgmUri = null;
        isStoppingBgm = false;
    }

    private BgmTrack GetDesiredBgmTrack()
    {
        return gameState switch
        {
            GameState.LanguageSelection when !languageOpeningFinished => BgmTrack.Prologue,
            GameState.Battle => BgmTrack.Battle,
            GameState.ShopBuy => BgmTrack.Shop,
            GameState.Bank => BgmTrack.Shop,
            GameState.EncounterTransition => GetFieldBgmTrack(currentFieldMap),
            GameState.Field => GetFieldBgmTrack(currentFieldMap),
            _ => BgmTrack.MainMenu
        };
    }

    private static BgmTrack GetFieldBgmTrack(FieldMapId mapId)
    {
        return mapId switch
        {
            FieldMapId.Dungeon => BgmTrack.Castle,
            FieldMapId.Castle => BgmTrack.Castle,
            FieldMapId.Field => BgmTrack.Field,
            _ => BgmTrack.Field
        };
    }

    private void PlaySe(SoundEffect effect)
    {
        if (!seUris.TryGetValue(effect, out var seUri) ||
            !seUri.LocalPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var stream = File.OpenRead(seUri.LocalPath);
        using var soundEffect = XnaSoundEffect.FromStream(stream);
        soundEffect.Play(0.9f, 0f, 0f);
    }
}
