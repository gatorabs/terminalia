namespace Terminalia.Editor.Selection;

public sealed class EditableProperty
{
    public required string Key { get; init; }

    public required string Label { get; init; }

    public EditablePropertyKind Kind { get; init; } = EditablePropertyKind.Text;

    public string Value { get; init; } = string.Empty;

    public int Minimum { get; init; }

    public int Maximum { get; init; } = int.MaxValue;

    public bool IsReadOnly { get; init; }
}
