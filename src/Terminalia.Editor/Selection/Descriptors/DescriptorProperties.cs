namespace Terminalia.Editor.Selection.Descriptors;

internal static class DescriptorProperties
{
    public static EditableProperty ReadOnlyPosition(Point cell)
    {
        return new EditableProperty
        {
            Key = "position",
            Label = "Position",
            Kind = EditablePropertyKind.Text,
            Value = $"{cell.X}, {cell.Y}",
            IsReadOnly = true
        };
    }

    public static EditableProperty Number(
        string key,
        string label,
        string? raw,
        int fallback,
        int minimum,
        int maximum)
    {
        return new EditableProperty
        {
            Key = key,
            Label = label,
            Kind = EditablePropertyKind.Number,
            Value = Math.Clamp(ParseInt(raw, fallback), minimum, maximum).ToString(),
            Minimum = minimum,
            Maximum = maximum
        };
    }

    public static EditableProperty Toggle(string key, string label, string? raw, bool fallback)
    {
        return new EditableProperty
        {
            Key = key,
            Label = label,
            Kind = EditablePropertyKind.Toggle,
            Value = ParseBool(raw, fallback).ToString().ToLowerInvariant()
        };
    }

    public static int ParseInt(string? raw, int fallback)
    {
        return int.TryParse(raw, out var value) ? value : fallback;
    }

    public static bool ParseBool(string? raw, bool fallback)
    {
        return bool.TryParse(raw, out var value) ? value : fallback;
    }
}
