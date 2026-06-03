using System.Drawing.Drawing2D;

namespace Terminalia.Editor;

internal static class EditorTheme
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
    public static readonly Font UiFont = new("Segoe UI", 9, FontStyle.Regular);
    public static readonly Font SectionFont = new("Segoe UI Semibold", 9, FontStyle.Bold);
    public const int CornerRadius = 10;
    public const int ButtonRadius = 8;
    public const int AnimationInterval = 15;

    public static Button CreateButton(string text, bool primary = false)
    {
        return new ModernButton
        {
            Text = text,
            Height = 32,
            Font = UiFont,
            Padding = new Padding(8, 0, 8, 0),
            IsPrimary = primary
        };
    }

    public static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static Color Lerp(Color from, Color to, double amount)
    {
        amount = Math.Clamp(amount, 0, 1);
        return Color.FromArgb(
            from.A + (int)((to.A - from.A) * amount),
            from.R + (int)((to.R - from.R) * amount),
            from.G + (int)((to.G - from.G) * amount),
            from.B + (int)((to.B - from.B) * amount));
    }

    public static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = SecondaryText
        };
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = InputBackground;
        textBox.ForeColor = PrimaryText;
    }

    public static void StyleNumberInput(NumericUpDown input)
    {
        input.BorderStyle = BorderStyle.FixedSingle;
        input.BackColor = InputBackground;
        input.ForeColor = PrimaryText;
    }
}
