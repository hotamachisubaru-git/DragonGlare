using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace DragonGlareAlpha.Persistence;

public sealed class SaveService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public bool TryLoad(string path, [NotNullWhen(true)] out SaveData? saveData)
    {
        saveData = null;

        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            var json = File.ReadAllText(path);
            saveData = JsonSerializer.Deserialize<SaveData>(json, SerializerOptions);
            return saveData is not null;
        }
        catch
        {
            return false;
        }
    }

    public void Save(string path, SaveData saveData)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(saveData, SerializerOptions);
        var tempPath = $"{path}.tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, path, overwrite: true);
    }
}
