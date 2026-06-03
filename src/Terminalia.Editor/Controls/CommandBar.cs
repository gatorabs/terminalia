namespace Terminalia.Editor;

public sealed class CommandBar : UserControl
{
    private const int BrandWidth = 230;
    private const int CommandHeight = 36;
    private const int CommandGap = 8;
    private const int TopPadding = 14;
    private readonly List<ToolbarCommand> _commands = [];
    private readonly System.Windows.Forms.Timer _entranceTimer = new();
    private int _entranceOffset = -90;

    public event EventHandler? NewRequested;
    public event EventHandler? OpenRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? SaveAsRequested;
    public event EventHandler? OpenSampleRequested;
    public event EventHandler? SaveSampleRequested;
    public event EventHandler? RunExternalRequested;

    public CommandBar()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;
        Padding = new Padding(18, 10, 18, 10);
        MinimumSize = new Size(0, 76);
        DoubleBuffered = true;

        AddCommand("New", 78, () => NewRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("Open", 86, () => OpenRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("Save", 86, () => SaveRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("Save As", 102, () => SaveAsRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("Open Sample", 128, () => OpenSampleRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("Save Sample", 128, () => SaveSampleRequested?.Invoke(this, EventArgs.Empty));
        AddCommand("Run External", 132, () => RunExternalRequested?.Invoke(this, EventArgs.Empty), isPrimary: true);

        _entranceTimer.Interval = EditorTheme.AnimationInterval;
        _entranceTimer.Tick += (_, _) =>
        {
            _entranceOffset = (int)Math.Round(_entranceOffset * 0.68);
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
        _entranceOffset = -90;
        _entranceTimer.Start();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);

        var rowStart = Padding.Left + BrandWidth + 24 + _entranceOffset;
        var x = rowStart;
        var y = TopPadding;
        var rightLimit = Width - Padding.Right;

        foreach (var command in _commands)
        {
            if (x + command.PreferredCommandWidth > rightLimit && x > rowStart)
            {
                x = rowStart;
                y += CommandHeight + 10;
            }

            command.Bounds = new Rectangle(x, y, command.PreferredCommandWidth, CommandHeight);
            x += command.Width + CommandGap;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(EditorTheme.AppBackground);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var bottomLine = new Pen(EditorTheme.BorderSoft);
        e.Graphics.DrawLine(bottomLine, 0, Height - 1, Width, Height - 1);
        DrawBrand(e.Graphics);
    }

    private void AddCommand(string caption, int width, Action action, bool isPrimary = false)
    {
        var command = new ToolbarCommand
        {
            Caption = caption,
            IsPrimary = isPrimary,
            PreferredCommandWidth = width
        };
        command.Click += (_, _) => action();
        _commands.Add(command);
        Controls.Add(command);
    }

    private void DrawBrand(Graphics graphics)
    {
        var brandBounds = new Rectangle(Padding.Left, 10, BrandWidth, 44);
        using var brandPath = EditorTheme.CreateRoundedPath(brandBounds, 12);
        using var brandFill = new SolidBrush(EditorTheme.CommandGroup);
        using var brandBorder = new Pen(EditorTheme.BorderSoft);
        graphics.FillPath(brandFill, brandPath);
        graphics.DrawPath(brandBorder, brandPath);

        using var promptFont = new Font(FontFamily.GenericMonospace, 11, FontStyle.Bold);
        using var subFont = new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular);
        TextRenderer.DrawText(
            graphics,
            "> TERMINALIA",
            promptFont,
            new Rectangle(brandBounds.Left + 14, brandBounds.Top + 6, brandBounds.Width - 28, 20),
            EditorTheme.PrimaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            "boot editor / map tools",
            subFont,
            new Rectangle(brandBounds.Left + 16, brandBounds.Top + 25, brandBounds.Width - 30, 16),
            EditorTheme.SecondaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
