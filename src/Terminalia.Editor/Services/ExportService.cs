using System.Diagnostics;
using Terminalia.Assets.Json;

namespace Terminalia.Editor.Services;

public sealed class ExportService
{
    public async Task<ExportResult> ExportAsync(
        MapAssetDocument document,
        string outputDirectory,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        var repoRoot = FindRepositoryRoot();
        if (repoRoot is null)
            return ExportResult.Failure("Cannot locate Terminalia.sln. Export requires the project source directory.");

        var projectPath = Path.Combine(repoRoot, "src", "Terminalia.Runtime.Windows", "Terminalia.Runtime.Windows.csproj");
        if (!File.Exists(projectPath))
            return ExportResult.Failure($"Runtime project not found at:\n{projectPath}");

        try
        {
            Directory.CreateDirectory(outputDirectory);
        }
        catch (Exception ex)
        {
            return ExportResult.Failure($"Cannot create output directory:\n{ex.Message}");
        }

        progress?.Report("Building executable...");

        var publishArgs = $"publish \"{projectPath}\" -c Release --self-contained -r win-x64 -o \"{outputDirectory}\" --nologo -v quiet";
        var (exitCode, stdErr) = await RunProcessAsync("dotnet", publishArgs, ct);

        if (exitCode != 0)
            return ExportResult.Failure($"Build failed:\n{stdErr}");

        progress?.Report("Copying map assets...");

        try
        {
            var mapsDir = Path.Combine(outputDirectory, "maps");
            Directory.CreateDirectory(mapsDir);
            document.Save(Path.Combine(mapsDir, document.Name + ".json"));
        }
        catch (Exception ex)
        {
            return ExportResult.Failure($"Failed to copy map assets:\n{ex.Message}");
        }

        RenameExeToGameName(outputDirectory, document.Name);

        return ExportResult.Success(outputDirectory);
    }

    private static void RenameExeToGameName(string outputDirectory, string gameName)
    {
        var source = Path.Combine(outputDirectory, "Terminalia.Runtime.Windows.exe");
        if (!File.Exists(source))
            return;

        var safeName = string.Concat(gameName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var target = Path.Combine(outputDirectory, safeName + ".exe");

        if (!File.Exists(target))
            File.Move(source, target);
    }

    private static string? FindRepositoryRoot()
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

    private static async Task<(int ExitCode, string StdErr)> RunProcessAsync(string exe, string args, CancellationToken ct)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        process.Start();
        var stdErr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        return (process.ExitCode, stdErr);
    }
}
