namespace Terminalia.Editor.Selection;

public sealed class SelectedObjectInfo
{
    public required char Tile { get; init; }

    public required Point Cell { get; init; }

    public required string DisplayName { get; init; }

    public string? Description { get; init; }

    public bool ShowKernelPreview { get; init; }

    public IReadOnlyList<EditableProperty> Properties { get; init; } = [];
}
