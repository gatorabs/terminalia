namespace Terminalia.Sample.Rpg;

internal static class SampleTheme
{
    public static readonly Color AppBackground = Color.FromArgb(9, 13, 20);
    public static readonly Color TerminalBackground = Color.FromArgb(13, 17, 23);
    public static readonly Color TerminalHeader = Color.FromArgb(21, 30, 42);
    public static readonly Color Border = Color.FromArgb(45, 58, 74);
    public static readonly Color PrimaryText = Color.FromArgb(226, 232, 240);
    public static readonly Color MutedText = Color.FromArgb(148, 163, 184);
    public static readonly Font TerminalFont = new(FontFamily.GenericMonospace, 16, FontStyle.Bold);
    public static readonly Font HudFont = new("Segoe UI", 9, FontStyle.Regular);
    public static readonly Font TitleFont = new("Segoe UI Semibold", 11, FontStyle.Bold);

    public static Color GetTileColor(char tile) => tile switch
    {
        '#' => Color.FromArgb(148, 163, 184),
        '.' => Color.FromArgb(71, 85, 105),
        '@' => Color.FromArgb(96, 165, 250),
        '*' => Color.FromArgb(250, 204, 21),
        _ => PrimaryText
    };
}
