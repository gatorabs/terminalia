namespace Terminalia.Editor.Styling.Colors;

internal static class EditorTileColors
{
    public static readonly Color[] ZonePalette =
    [
        Color.FromArgb(60, 255, 100, 100),
        Color.FromArgb(60, 100, 255, 100),
        Color.FromArgb(60, 100, 100, 255),
        Color.FromArgb(60, 255, 255, 100),
        Color.FromArgb(60, 255, 100, 255),
        Color.FromArgb(60, 100, 255, 255)
    ];

    public static Color ZoneOverlay(string zoneName) =>
        ZonePalette[Math.Abs(zoneName.GetHashCode()) % ZonePalette.Length];

    public static Color ZoneCursor(string zoneName)
    {
        var overlay = ZoneOverlay(zoneName);
        return Color.FromArgb(255, overlay.R, overlay.G, overlay.B);
    }

    public static Color TileBackground(char tile) => tile switch
    {
        '#' => Color.FromArgb(30, 30, 30),
        '.' => Color.FromArgb(8, 8, 8),
        '@' => Color.FromArgb(18, 18, 18),
        '*' => Color.FromArgb(22, 22, 22),
        _ => EditorColors.Section
    };

    public static Color TileText(char tile) => tile switch
    {
        '#' => Color.FromArgb(230, 230, 230),
        '.' => Color.FromArgb(86, 86, 86),
        '@' => Color.FromArgb(255, 255, 255),
        '*' => Color.FromArgb(214, 214, 214),
        _ => EditorColors.PrimaryText
    };
}
