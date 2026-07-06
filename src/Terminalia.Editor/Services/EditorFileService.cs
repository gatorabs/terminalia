using Terminalia.Assets.Json;

namespace Terminalia.Editor.Services;

public sealed class EditorFileService
{
    public MapAssetDocument CreateDefaultMap() => MapAssetDocument.CreateDefault();

    public MapAssetDocument LoadMap(string path) => MapAssetDocument.Load(path);

    public void SaveMap(MapAssetDocument document, string path) => document.Save(path);

    public GameAssetDocument LoadOrCreateGame()
    {
        var path = DefaultGameFilePath;
        if (!File.Exists(path))
        {
            return GameAssetDocument.CreateDefault(FindDefaultStartMap());
        }

        try
        {
            return GameAssetDocument.Load(path);
        }
        catch (Exception ex) when (ex is InvalidDataException or ArgumentException or System.Text.Json.JsonException)
        {
            return GameAssetDocument.CreateDefault(FindDefaultStartMap());
        }
    }

    public void SaveGame(GameAssetDocument document) => document.Save(DefaultGameFilePath);

    public static string DefaultGameFilePath
    {
        get
        {
            var root = FindRepositoryRoot();
            return root is not null
                ? Path.Combine(root, "game.json")
                : Path.Combine(AppContext.BaseDirectory, "game.json");
        }
    }

    private static string FindDefaultStartMap()
    {
        var mapsDir = DefaultMapsDirectory;
        var first = Directory.Exists(mapsDir)
            ? Directory.GetFiles(mapsDir, "*.json").Select(Path.GetFileNameWithoutExtension).FirstOrDefault()
            : null;
        return string.IsNullOrWhiteSpace(first) ? "first_dungeon" : first;
    }

    public static string DefaultMapsDirectory
    {
        get
        {
            var root = FindRepositoryRoot();
            return root is not null
                ? Path.Combine(root, "maps")
                : Path.Combine(AppContext.BaseDirectory, "maps");
        }
    }

    public static string AutoSavePath(string mapName) =>
        Path.Combine(DefaultMapsDirectory, $"{mapName}.json");

    public static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Terminalia.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
