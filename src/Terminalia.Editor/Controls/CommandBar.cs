namespace Terminalia.Editor;

public sealed class CommandBar : UserControl
{
    private readonly List<ToolbarCommand> _commands = [];
    private readonly System.Windows.Forms.Timer _entranceTimer = new();
    private readonly ToolTip _toolTip = new();
    private int _entranceOffset = -56;

    public event EventHandler? NewRequested;
    public event EventHandler? OpenRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? SaveAsRequested;
    public event EventHandler? OpenSampleRequested;
    public event EventHandler? SaveSampleRequested;
    public event EventHandler? RunExternalRequested;
    public event EventHandler? MapPanelRequested;
    public event EventHandler? ViewPanelRequested;
    public event EventHandler? ToolPanelRequested;
    public event EventHandler? PalettePanelRequested;
    public event EventHandler? RunPanelRequested;

    public CommandBar()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;
        MinimumSize = new Size(EditorTheme.CommandBarWidth, 0);
        DoubleBuffered = true;

        _toolTip.BackColor = EditorTheme.Surface;
        _toolTip.ForeColor = EditorTheme.PrimaryText;
        _toolTip.OwnerDraw = true;
        _toolTip.Draw += DrawToolTip;

        AddCommand("[#]", "Map", () => MapPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[Z]", "View", () => ViewPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[T]", "Tool", () => ToolPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[P]", "Palette", () => PalettePanelRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[R]", "Run", () => RunPanelRequested?.Invoke(this, EventArgs.Empty));
        AddSeparator();
        AddCommand("[+]", "New", () => NewRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[O]", "Open", () => OpenRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[S]", "Save", () => SaveRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[A]", "Save As", () => SaveAsRequested?.Invoke(this, EventArgs.Empty));
        AddSeparator();
        AddCommand("[L]", "Open Sample", () => OpenSampleRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[W]", "Save Sample", () => SaveSampleRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("[>]", "Run External", () => RunExternalRequested?.Invoke(this, EventArgs.Empty), isPrimary: true);

        _entranceTimer.Interval = EditorTheme.AnimationInterval;
        _entranceTimer.Tick += (_, _) =>
        {
            _entranceOffset = (int)Math.Round(_entranceOffset * 0.66);
            if (Math.Abs(_entranceOffset) <= 1)
            {
                _entranceOffset = 0;
                _entranceTimer.Stop();
            }

            PerformLayout();
            Invalidate();
        };
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        _entranceOffset = -56;
        _entranceTimer.Start();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);

        var x = ((Width - EditorTheme.CommandSize) / 2) + _entranceOffset;
        var y = EditorTheme.BrandSectionHeight;

        foreach (var command in _commands)
        {
            if (command.Tag is SeparatorTag)
            {
                command.Bounds = new Rectangle(x, y + 4, EditorTheme.CommandSize, 14);
                y += EditorTheme.CommandGap * 3;
                continue;
            }

            command.Bounds = new Rectangle(x, y, EditorTheme.CommandSize, EditorTheme.CommandSize);
            y += EditorTheme.CommandSize + EditorTheme.CommandGap;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(EditorTheme.AppBackground);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rail = new Rectangle(EditorTheme.CommandPadding, EditorTheme.CommandPadding, Width - EditorTheme.CommandPadding * 2, Height - EditorTheme.CommandPadding * 2);
        using var railPath = EditorTheme.CreateRoundedPath(rail, 18);
        using var railFill = new SolidBrush(EditorTheme.Surface);
        using var railBorder = new Pen(EditorTheme.BorderSoft);
        e.Graphics.FillPath(railFill, railPath);
        e.Graphics.DrawPath(railBorder, railPath);

        DrawBrand(e.Graphics);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    private void AddCommand(string glyph, string caption, Action action, bool isPrimary = false)
    {
        var command = new ToolbarCommand
        {
            IconGlyph = glyph,
            Caption = caption,
            IsPrimary = isPrimary,
            PreferredCommandWidth = EditorTheme.CommandSize
        };
        command.Click += (_, _) => action();
        _toolTip.SetToolTip(command, caption);
        _commands.Add(command);
        Controls.Add(command);
    }

    private void AddSeparator()
    {
        var command = new ToolbarCommand
        {
            Tag = SeparatorTag.Instance,
            Enabled = false,
            PreferredCommandWidth = EditorTheme.CommandSize
        };
        _commands.Add(command);
        Controls.Add(command);
    }

    private void DrawBrand(Graphics graphics)
    {
        using var titleFont = new Font(FontFamily.GenericMonospace, 14, FontStyle.Bold);
        using var smallFont = new Font(FontFamily.GenericMonospace, 7, FontStyle.Regular);

        TextRenderer.DrawText(
            graphics,
            ">_",
            titleFont,
            new Rectangle(0, 28, Width, 24),
            EditorTheme.PrimaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(
            graphics,
            "TERM",
            smallFont,
            new Rectangle(0, 52, Width, 16),
            EditorTheme.SecondaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private void DrawToolTip(object? sender, DrawToolTipEventArgs e)
    {
        using var fill = new SolidBrush(EditorTheme.Surface);
        using var border = new Pen(EditorTheme.Border);
        e.Graphics.FillRectangle(fill, e.Bounds);
        e.Graphics.DrawRectangle(border, new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1));
        TextRenderer.DrawText(
            e.Graphics,
            e.ToolTipText,
            EditorTheme.UiFont,
            e.Bounds,
            EditorTheme.PrimaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private sealed class SeparatorTag
    {
        public static readonly SeparatorTag Instance = new();
    }
}
