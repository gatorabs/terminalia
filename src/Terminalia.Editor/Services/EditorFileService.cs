using System.Diagnostics;
using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class EditorFileService
{
    public MapAssetDocument CreateDefaultMap() => MapAssetDocument.CreateDefault();

    public MapAssetDocument LoadMap(string path) => MapAssetDocument.Load(path);

    public void SaveMap(MapAssetDocument document, string path) => document.Save(path);

    public bool RunInConsole(MapAssetDocument document)
    {
        var repoRoot = FindRepositoryRoot();
        if (repoRoot is null) return false;

        var projectPath = Path.Combine(repoRoot, "src", "Terminalia.Runtime.Console", "Terminalia.Runtime.Console.csproj");
        if (!File.Exists(projectPath)) return false;

        var tempMapsDir = Path.Combine(Path.GetTempPath(), "terminalia_run", "assets", "maps");
        Directory.CreateDirectory(tempMapsDir);
        var mapFile = Path.Combine(tempMapsDir, $"{document.Name}.json");
        document.Save(mapFile);

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/k dotnet run --project \"{projectPath}\" -- \"{mapFile}\"",
            UseShellExecute = true
        });

        return true;
    }

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
