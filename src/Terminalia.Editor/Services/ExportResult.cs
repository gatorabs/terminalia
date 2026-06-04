namespace Terminalia.Editor.Services;

public sealed record ExportResult(bool IsSuccess, string OutputDirectory, string? ErrorMessage)
{
    public static ExportResult Success(string outputDir) => new(true, outputDir, null);
    public static ExportResult Failure(string error) => new(false, string.Empty, error);
}
