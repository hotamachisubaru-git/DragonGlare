using System.IO;
using DragonGlare.Domain.Startup;
using Newtonsoft.Json;

namespace DragonGlare.Services;

public sealed class LaunchSettingsService
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.Indented
    };

    private readonly string settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DragonGlareAlpha",
        "launch_settings.json");

    public LaunchSettings Load()
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                return new LaunchSettings();
            }

            var json = File.ReadAllText(settingsPath);
            var settings = JsonConvert.DeserializeObject<LaunchSettings>(json, SerializerSettings);
            return settings ?? new LaunchSettings();
        }
        catch
        {
            return new LaunchSettings();
        }
    }

    public void Save(LaunchSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(settings, SerializerSettings);
            File.WriteAllText(settingsPath, json);
        }
        catch
        {
        }
    }
}
