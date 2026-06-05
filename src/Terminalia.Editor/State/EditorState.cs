using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class EditorState
{
    public MapAssetDocument Document { get; set; } = MapAssetDocument.CreateDefault();

    public string? CurrentFile { get; set; }

    public bool HasUnsavedChanges { get; set; }

    public char SelectedTile { get; set; } = '#';

    public EditorToolMode ToolMode { get; set; } = EditorToolMode.Paint;

    public Point? CursorCell { get; set; }

    public string? SelectedZone { get; set; }
}
