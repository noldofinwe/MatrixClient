using System.Text.Json;

namespace Shared;

public class ObjectStorageHelper
{
    private readonly string folderPath;

    public ObjectStorageHelper(string appName)
    {
        folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName);
        Directory.CreateDirectory(folderPath);
    }

    public void Save<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        File.WriteAllText(Path.Combine(folderPath, key + ".json"), json);
    }

    public T? Load<T>(string key)
    {
        var path = Path.Combine(folderPath, key + ".json");
        return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path))
            : default;
    }
}
