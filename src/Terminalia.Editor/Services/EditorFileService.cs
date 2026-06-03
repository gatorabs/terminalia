using System.Diagnostics;
using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class EditorFileService
{
    public MapAssetDocument CreateDefaultMap() => MapAssetDocument.CreateDefault();

    public MapAssetDocument LoadMap(string path) => MapAssetDocument.Load(path);

    public void SaveMap(MapAssetDocument document, string path) => document.Save(path);

    public string? GetSampleMapPath()
    {
        var repositoryRoot = FindRepositoryRoot();
        return repositoryRoot is null
            ? null
            : Path.Combine(repositoryRoot, "src", "Samples", "Terminalia.Sample.Rpg", "assets", "maps", "first_dungeon.json");
    }

    public bool PlaytestSample()
    {
        var repositoryRoot = FindRepositoryRoot();
        if (repositoryRoot is null)
        {
            return false;
        }

        var projectPath = Path.Combine(repositoryRoot, "src", "Samples", "Terminalia.Sample.Rpg", "Terminalia.Sample.Rpg.csproj");
        Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\" --no-restore",
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        return true;
    }

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
