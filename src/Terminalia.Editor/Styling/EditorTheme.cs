namespace Terminalia.Editor;

internal static class EditorTheme
{
    public static readonly Color AppBackground = Color.FromArgb(9, 13, 20);
    public static readonly Color Workspace = Color.FromArgb(11, 16, 24);
    public static readonly Color Navigation = Color.FromArgb(14, 20, 29);
    public static readonly Color Surface = Color.FromArgb(16, 23, 33);
    public static readonly Color Section = Color.FromArgb(18, 26, 38);
    public static readonly Color StatusBar = Color.FromArgb(12, 18, 27);
    public static readonly Color InputBackground = Color.FromArgb(10, 15, 23);
    public static readonly Color ButtonBackground = Color.FromArgb(23, 33, 47);
    public static readonly Color ButtonHover = Color.FromArgb(30, 43, 61);
    public static readonly Color ButtonPressed = Color.FromArgb(20, 29, 41);
    public static readonly Color Border = Color.FromArgb(45, 58, 74);
    public static readonly Color GridLine = Color.FromArgb(47, 62, 78);
    public static readonly Color PrimaryText = Color.FromArgb(226, 232, 240);
    public static readonly Color SecondaryText = Color.FromArgb(148, 163, 184);
    public static readonly Color Accent = Color.FromArgb(59, 130, 246);
    public static readonly Color AccentHover = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentPressed = Color.FromArgb(29, 78, 216);
    public static readonly Color TerminalBackground = Color.FromArgb(13, 17, 23);
    public static readonly Color TerminalHeader = Color.FromArgb(21, 30, 42);
    public static readonly Font UiFont = new("Segoe UI", 9, FontStyle.Regular);
    public static readonly Font SectionFont = new("Segoe UI Semibold", 9, FontStyle.Bold);

    public static Button CreateButton(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Accent : ButtonBackground,
            ForeColor = primary ? Color.White : PrimaryText,
            Font = UiFont,
            Padding = new Padding(8, 0, 8, 0),
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderColor = primary ? Accent : Border;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = primary ? AccentHover : ButtonHover;
        button.FlatAppearance.MouseDownBackColor = primary ? AccentPressed : ButtonPressed;
        return button;
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
