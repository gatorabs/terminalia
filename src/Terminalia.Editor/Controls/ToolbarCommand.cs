namespace Terminalia.Editor;

internal sealed class ToolbarCommand
{
    public string Glyph { get; init; } = string.Empty;
    public string Caption { get; init; } = string.Empty;
    public Action? Click { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsSeparator { get; init; }
    public Rectangle Bounds { get; set; }
    public Color BackColor { get; set; }
    public Color TargetColor { get; set; }
    public double Pulse { get; set; }
}
