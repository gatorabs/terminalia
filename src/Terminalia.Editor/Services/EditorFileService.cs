using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class EditorFileService
{
    public MapAssetDocument CreateDefaultMap() => MapAssetDocument.CreateDefault();

    public MapAssetDocument LoadMap(string path) => MapAssetDocument.Load(path);

    public void SaveMap(MapAssetDocument document, string path) => document.Save(path);

    public static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Terminalia.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }
}
