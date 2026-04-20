using System.IO;
using DragonGlareAlpha.Domain;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DragonGlare.Managers
{
    public static class AudioManager
    {
        private static readonly object SyncRoot = new();

        private static BgmTrack? _currentTrack;
        private static string? _currentPath;
        private static AudioFileReader? _reader;
        private static IWavePlayer? _output;
        private static bool _isStopping;

        public static void PlayFieldBgm(FieldMapId mapId)
        {
            PlayBgm(mapId == FieldMapId.Castle ? BgmTrack.Castle : BgmTrack.Field);
        }

        public static void PlayBgm(BgmTrack track)
        {
            lock (SyncRoot)
            {
                if (_currentTrack == track && _output is not null)
                {
                    return;
                }

                var path = ResolveBgmPath(track);
                if (path is null)
                {
                    Log($"BGM path not found: {track}");
                    return;
                }

                StopLocked();

                try
                {
                    Log($"BGM start: {track} path={path} waveOutDevices={WaveOut.DeviceCount}");
                    _reader = new AudioFileReader(path)
                    {
                        Volume = 0.85f
                    };
                    _output = CreateOutputDevice();
                    _output.Init(_reader);
                    _output.PlaybackStopped += HandlePlaybackStopped;
                    _output.Play();

                    _currentTrack = track;
                    _currentPath = path;
                    Log($"BGM playing: {track} state={_output.PlaybackState}");
                }
                catch (Exception ex)
                {
                    Log($"BGM failed: {track} {ex.GetType().Name}: {ex.Message}");
                    StopLocked();
                }
            }
        }

        private static IWavePlayer CreateOutputDevice()
        {
            try
            {
                using var enumerator = new MMDeviceEnumerator();
                var endpoint = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                Log($"BGM endpoint: {endpoint.FriendlyName}");
                return new WasapiOut(endpoint, AudioClientShareMode.Shared, false, 100);
            }
            catch (Exception ex)
            {
                Log($"BGM WASAPI unavailable, fallback to WaveOutEvent: {ex.GetType().Name}: {ex.Message}");
                return new WaveOutEvent
                {
                    DesiredLatency = 100
                };
            }
        }

        public static void Stop()
        {
            lock (SyncRoot)
            {
                StopLocked();
            }
        }

        private static void HandlePlaybackStopped(object? sender, StoppedEventArgs args)
        {
            lock (SyncRoot)
            {
                if (_isStopping || _reader is null || _output is null || _currentPath is null)
                {
                    return;
                }

                _reader.Position = 0;
                _output.Play();
            }
        }

        private static void StopLocked()
        {
            _isStopping = true;
            if (_output is not null)
            {
                _output.PlaybackStopped -= HandlePlaybackStopped;
                _output.Stop();
                _output.Dispose();
                _output = null;
            }

            _reader?.Dispose();
            _reader = null;
            _currentTrack = null;
            _currentPath = null;
            _isStopping = false;
        }

        private static string? ResolveBgmPath(BgmTrack track)
        {
            var fileName = track switch
            {
                BgmTrack.Castle => "SFC_castle.mp3",
                BgmTrack.Battle => "SFC_battle.mp3",
                BgmTrack.Shop => "SFC_shop_buy.mp3",
                BgmTrack.MainMenu => "SFC_main_menu.mp3",
                _ => "SFC_field.mp3"
            };

            return GetFirstExistingPath(
                Path.Combine("Assets", "BGM", fileName),
                Path.Combine(AppContext.BaseDirectory, "Assets", "BGM", fileName),
                Path.Combine(AppContext.BaseDirectory, "BGM", fileName),
                Path.Combine("Content", "BGM", fileName),
                Path.Combine(AppContext.BaseDirectory, "Content", "BGM", fileName));
        }

        private static string? GetFirstExistingPath(params string[] paths)
        {
            foreach (var path in paths)
            {
                var normalized = Path.GetFullPath(path);
                if (File.Exists(normalized))
                {
                    return normalized;
                }
            }

            return null;
        }

        private static void Log(string message)
        {
            try
            {
                File.AppendAllText(
                    Path.Combine(AppContext.BaseDirectory, "audio-debug.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }
}
