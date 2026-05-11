using System.IO;
using DragonGlareAlpha.Domain;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private const float BgmPlaybackVolume = 0.85f;
    private const float SePlaybackVolume = 0.9f;
    private const int BgmFadeInDurationFrames = 28;
    private const int BgmFadeOutDurationFrames = 18;

    private readonly object audioSyncRoot = new();
    private readonly List<ActiveSoundEffectPlayback> activeSePlaybacks = [];
    private AudioFileReader? bgmReader;
    private IWavePlayer? bgmOutput;
    private Uri? currentBgmUri;
    private BgmTrack? pendingBgmTrack;
    private Uri? pendingBgmUri;
    private BgmTrack? failedBgmTrack;
    private int bgmFadeInFramesRemaining;
    private int bgmFadeOutFramesRemaining;
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
        RegisterSe(SoundEffect.Attack, "SFC_atk1.mp3");
        RegisterSe(SoundEffect.Defend, "SFC_def.mp3");
        RegisterSe(SoundEffect.Magic, "SFC_magic.mp3");
        RegisterSe(SoundEffect.Cure, "SFC_cure.mp3");
        RegisterSe(SoundEffect.Poison, "SFC_poison.mp3");
        RegisterSe(SoundEffect.Raiden, "SFC_raiden.mp3");
        RegisterSe(SoundEffect.Fire, "SFC_fire.mp3");
        RegisterSe(SoundEffect.Equip, "SFC_equip.mp3");
        RegisterSe(SoundEffect.Cursor, "SFC_cursor.mp3");
        RegisterSe(SoundEffect.Cancel, "SFC_cancel.mp3");
        RegisterSe(SoundEffect.Escape, "SFC_escape.mp3");

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

        if (!bgmUris.TryGetValue(desiredTrack, out var bgmUri))
        {
            StopBgm();
            currentBgmTrack = null;
            failedBgmTrack = desiredTrack;
            return;
        }

        lock (audioSyncRoot)
        {
            if (currentBgmTrack == desiredTrack && bgmOutput is not null)
            {
                pendingBgmTrack = null;
                pendingBgmUri = null;
                bgmFadeOutFramesRemaining = 0;

                if (bgmOutput.PlaybackState != PlaybackState.Playing)
                {
                    bgmOutput.Play();
                }

                UpdateBgmFadeLocked();
                return;
            }

            if (failedBgmTrack == desiredTrack)
            {
                StopBgmLocked();
                return;
            }

            pendingBgmTrack = desiredTrack;
            pendingBgmUri = bgmUri;

            if (bgmOutput is null)
            {
                PlayBgm(desiredTrack, bgmUri);
                return;
            }

            if (bgmFadeOutFramesRemaining <= 0)
            {
                bgmFadeOutFramesRemaining = BgmFadeOutDurationFrames;
            }

            UpdateBgmFadeLocked();
        }
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
                    Volume = 0f
                };
                bgmOutput = CreateBgmOutputDevice();
                bgmOutput.Init(bgmReader);
                bgmOutput.PlaybackStopped += HandleBgmPlaybackStopped;
                bgmOutput.Play();

                currentBgmTrack = track;
                currentBgmUri = bgmUri;
                pendingBgmTrack = null;
                pendingBgmUri = null;
                failedBgmTrack = null;
                bgmFadeInFramesRemaining = BgmFadeInDurationFrames;
                bgmFadeOutFramesRemaining = 0;
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

    private void UpdateBgmFadeLocked()
    {
        if (bgmReader is null)
        {
            return;
        }

        if (bgmFadeOutFramesRemaining > 0)
        {
            var volume = BgmPlaybackVolume * bgmFadeOutFramesRemaining / BgmFadeOutDurationFrames;
            bgmReader.Volume = Math.Clamp(volume, 0f, BgmPlaybackVolume);
            bgmFadeOutFramesRemaining--;

            if (bgmFadeOutFramesRemaining == 0)
            {
                var nextTrack = pendingBgmTrack;
                var nextUri = pendingBgmUri;
                StopBgmLocked();

                if (nextTrack is not null && nextUri is not null)
                {
                    PlayBgm(nextTrack.Value, nextUri);
                }
            }

            return;
        }

        if (bgmFadeInFramesRemaining > 0)
        {
            var elapsedFrames = BgmFadeInDurationFrames - bgmFadeInFramesRemaining + 1;
            var volume = BgmPlaybackVolume * elapsedFrames / BgmFadeInDurationFrames;
            bgmReader.Volume = Math.Clamp(volume, 0f, BgmPlaybackVolume);
            bgmFadeInFramesRemaining--;
            return;
        }

        bgmReader.Volume = BgmPlaybackVolume;
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
        pendingBgmTrack = null;
        pendingBgmUri = null;
        bgmFadeInFramesRemaining = 0;
        bgmFadeOutFramesRemaining = 0;
        isStoppingBgm = false;
    }

    private BgmTrack GetDesiredBgmTrack()
    {
        var targetState = pendingGameState ?? gameState;
        return targetState switch
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
        if (!seUris.TryGetValue(effect, out var seUri))
        {
            return;
        }

        AudioFileReader? reader = null;
        IWavePlayer? output = null;
        ActiveSoundEffectPlayback? playback = null;

        try
        {
            reader = new AudioFileReader(seUri.LocalPath)
            {
                Volume = SePlaybackVolume
            };
            output = CreateSeOutputDevice();
            var activePlayback = new ActiveSoundEffectPlayback(reader, output);
            playback = activePlayback;
            output.Init(reader);
            output.PlaybackStopped += (_, _) => DisposeSePlayback(activePlayback);

            lock (audioSyncRoot)
            {
                activeSePlaybacks.Add(activePlayback);
            }

            output.Play();
        }
        catch
        {
            if (playback is not null)
            {
                DisposeSePlayback(playback);
            }
            else
            {
                output?.Dispose();
                reader?.Dispose();
            }
        }
    }

    private static IWavePlayer CreateSeOutputDevice()
    {
        return new WaveOutEvent
        {
            DesiredLatency = 70
        };
    }

    private void DisposeSePlayback(ActiveSoundEffectPlayback playback)
    {
        lock (audioSyncRoot)
        {
            activeSePlaybacks.Remove(playback);
        }

        playback.Dispose();
    }

    private void StopAllSoundEffects()
    {
        ActiveSoundEffectPlayback[] playbacks;
        lock (audioSyncRoot)
        {
            playbacks = activeSePlaybacks.ToArray();
            activeSePlaybacks.Clear();
        }

        foreach (var playback in playbacks)
        {
            playback.Dispose();
        }
    }

    private sealed class ActiveSoundEffectPlayback(AudioFileReader reader, IWavePlayer output) : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            output.Dispose();
            reader.Dispose();
        }
    }
}
