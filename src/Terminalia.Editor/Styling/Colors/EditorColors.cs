namespace Terminalia.Editor.Styling.Colors;

internal static class EditorColors
{
    public static readonly Color AppBackground = Color.FromArgb(3, 3, 3);
    public static readonly Color Workspace = Color.FromArgb(6, 6, 6);
    public static readonly Color Navigation = Color.FromArgb(8, 8, 8);
    public static readonly Color Surface = Color.FromArgb(10, 10, 10);
    public static readonly Color Section = Color.FromArgb(14, 14, 14);
    public static readonly Color StatusBar = Color.FromArgb(5, 5, 5);
    public static readonly Color InputBackground = Color.FromArgb(2, 2, 2);
    public static readonly Color ButtonBackground = Color.FromArgb(18, 18, 18);
    public static readonly Color ButtonHover = Color.FromArgb(32, 32, 32);
    public static readonly Color ButtonPressed = Color.FromArgb(8, 8, 8);
    public static readonly Color CommandBackground = Color.FromArgb(12, 12, 12);
    public static readonly Color CommandHover = Color.FromArgb(28, 28, 28);
    public static readonly Color CommandGroup = Color.FromArgb(4, 4, 4);
    public static readonly Color PanelHighlight = Color.FromArgb(24, 24, 24);
    public static readonly Color OverlayScrim = Color.FromArgb(184, 0, 0, 0);
    public static readonly Color LoadingCard = Color.FromArgb(12, 12, 12);
    public static readonly Color Border = Color.FromArgb(68, 68, 68);
    public static readonly Color BorderSoft = Color.FromArgb(38, 38, 38);
    public static readonly Color GridLine = Color.FromArgb(42, 42, 42);
    public static readonly Color PrimaryText = Color.FromArgb(238, 238, 238);
    public static readonly Color SecondaryText = Color.FromArgb(156, 156, 156);
    public static readonly Color Accent = Color.FromArgb(236, 236, 236);
    public static readonly Color AccentHover = Color.FromArgb(255, 255, 255);
    public static readonly Color AccentPressed = Color.FromArgb(188, 188, 188);
    public static readonly Color TerminalBackground = Color.FromArgb(2, 2, 2);
    public static readonly Color TerminalHeader = Color.FromArgb(12, 12, 12);
    public static readonly Color SelectAccent = Color.FromArgb(255, 196, 64);
    public static readonly Color SelectAccentText = Color.FromArgb(12, 12, 12);
    public static readonly Color SelectKernelFill = Color.FromArgb(26, 255, 196, 64);
    public static readonly Color SelectKernelBorder = Color.FromArgb(220, 255, 196, 64);
    public static readonly Color FillToolAccent = Color.FromArgb(80, 140, 255);

    public static int ColorDistance(Color a, Color b) =>
        Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);

    public static Color Lerp(Color from, Color to, double amount)
    {
        amount = Math.Clamp(amount, 0, 1);
        return Color.FromArgb(
            from.A + (int)((to.A - from.A) * amount),
            from.R + (int)((to.R - from.R) * amount),
            from.G + (int)((to.G - from.G) * amount),
            from.B + (int)((to.B - from.B) * amount));
    }
}
