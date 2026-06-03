namespace Terminalia.Editor;

public sealed class StartPage : UserControl
{
    private readonly Terminalia.Editor.RoundedPanel _panel = new()
    {
        FillColor = EditorTheme.Surface,
        BorderColor = EditorTheme.Border,
        CornerRadius = 16,
        Padding = new Padding(28)
    };

    private readonly ModernButton _newButton = new()
    {
        Text = "Create new map",
        IsPrimary = true,
        Height = 38
    };

    private readonly ModernButton _openButton = new()
    {
        Text = "Open map",
        Height = 38
    };

    private readonly ModernButton _sampleButton = new()
    {
        Text = "Open sample",
        Height = 38
    };
    private readonly Font _titleFont = new(FontFamily.GenericMonospace, 22, FontStyle.Bold);
    private readonly Font _subtitleFont = new(FontFamily.GenericMonospace, 9, FontStyle.Regular);
    private readonly Font _bodyFont = new(FontFamily.GenericMonospace, 9, FontStyle.Regular);

    public StartPage()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;
        DoubleBuffered = true;

        _newButton.Click += (_, _) => NewMapRequested?.Invoke(this, EventArgs.Empty);
        _openButton.Click += (_, _) => OpenMapRequested?.Invoke(this, EventArgs.Empty);
        _sampleButton.Click += (_, _) => OpenSampleRequested?.Invoke(this, EventArgs.Empty);
        _panel.Paint += (_, e) => DrawStartCard(e.Graphics);

        _panel.Controls.Add(_newButton);
        _panel.Controls.Add(_openButton);
        _panel.Controls.Add(_sampleButton);
        Controls.Add(_panel);
    }

    public event EventHandler? NewMapRequested;

    public event EventHandler? OpenMapRequested;

    public event EventHandler? OpenSampleRequested;

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);

        var panelWidth = Math.Min(560, Width - 80);
        var panelHeight = Math.Min(360, (int)(Height * 0.65));
        _panel.Bounds = new Rectangle(
            Math.Max(40, (Width - panelWidth) / 2),
            Math.Max(60, (Height - panelHeight) / 2),
            panelWidth,
            panelHeight);

        var buttonWidth = Math.Min(240, _panel.Width - 56);
        var buttonLeft = _panel.Width - _panel.Padding.Right - buttonWidth;
        var buttonTop = Math.Max(100, _panel.Height - 176);
        var buttonHeight = _newButton.Height;
        var buttonStep = buttonHeight + 12;
        _newButton.Bounds = new Rectangle(buttonLeft, buttonTop, buttonWidth, buttonHeight);
        _openButton.Bounds = new Rectangle(buttonLeft, buttonTop + buttonStep, buttonWidth, buttonHeight);
        _sampleButton.Bounds = new Rectangle(buttonLeft, buttonTop + buttonStep * 2, buttonWidth, buttonHeight);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(EditorTheme.AppBackground);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        DrawNoiseGrid(e.Graphics);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(EditorTheme.AppBackground);
    }

    private void DrawNoiseGrid(Graphics graphics)
    {
        using var gridPen = new Pen(Color.FromArgb(18, 255, 255, 255));
        for (var x = 0; x < Width; x += 28)
        {
            graphics.DrawLine(gridPen, x, 0, x, Height);
        }

        for (var y = 0; y < Height; y += 28)
        {
            graphics.DrawLine(gridPen, 0, y, Width, y);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _titleFont.Dispose();
            _subtitleFont.Dispose();
            _bodyFont.Dispose();
        }

        base.Dispose(disposing);
    }

    private void DrawStartCard(Graphics graphics)
    {
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        TextRenderer.DrawText(
            graphics,
            "> TERMINALIA",
            _titleFont,
            new Rectangle(28, 34, _panel.Width - 56, 40),
            EditorTheme.PrimaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        TextRenderer.DrawText(
            graphics,
            "terminal game engine editor",
            _subtitleFont,
            new Rectangle(31, 76, _panel.Width - 62, 22),
            EditorTheme.SecondaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var lines = new[]
        {
            "READY.",
            "Select a map workflow to continue.",
            "Create a blank document, open a saved map,",
            "or load the bundled sample dungeon."
        };

        for (var i = 0; i < lines.Length; i++)
        {
            TextRenderer.DrawText(
                graphics,
                lines[i],
                i == 0 ? _subtitleFont : _bodyFont,
                new Rectangle(32, 128 + (i * 22), _panel.Width / 2 - 20, 22),
                i == 0 ? EditorTheme.PrimaryText : EditorTheme.SecondaryText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
