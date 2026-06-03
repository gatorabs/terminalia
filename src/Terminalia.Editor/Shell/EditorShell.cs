namespace Terminalia.Editor;

public sealed class EditorShell : UserControl
{
    private readonly Panel _mainHost = new();
    private ClickOutsideFilter? _clickOutsideFilter;

    public EditorShell()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;
        Font = EditorTheme.UiFont;

        CommandBar = new CommandBar();
        Inspector = new InspectorPanel();
        Inspector.Visible = false;
        Inspector.Width = EditorTheme.InspectorWidth;
        Inspector.Dock = DockStyle.None;
        Canvas = new MapCanvas();
        Runtime = new EmbeddedGameSurface();
        StatusLabel = BuildStatusLabel();

        var canvasPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = EditorTheme.Workspace,
            Padding = new Padding(EditorTheme.CanvasPadding)
        };
        canvasPanel.Controls.Add(Canvas);
        canvasPanel.Resize += (_, _) => CenterCanvas(canvasPanel);
        canvasPanel.MouseWheel += (_, e) => Canvas.CellSize += e.Delta > 0 ? 4 : -4;
        Canvas.SizeChanged += (_, _) => CenterCanvas(canvasPanel);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = EditorTheme.AppBackground
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, EditorTheme.CommandBarWidth));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        mainLayout.Controls.Add(CommandBar, 0, 0);
        mainLayout.Controls.Add(canvasPanel, 1, 0);
        mainLayout.Controls.Add(Runtime, 2, 0);

        _mainHost.Dock = DockStyle.Fill;
        _mainHost.BackColor = EditorTheme.AppBackground;
        _mainHost.Controls.Add(mainLayout);
        _mainHost.Controls.Add(Inspector);
        _mainHost.Resize += (_, _) => PlaceInspector();

        var shellLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = EditorTheme.AppBackground
        };
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, EditorTheme.StatusBarHeight));
        shellLayout.Controls.Add(_mainHost, 0, 0);
        shellLayout.Controls.Add(StatusLabel, 0, 1);

        Controls.Add(shellLayout);

        HandleCreated += (_, _) =>
        {
            _clickOutsideFilter = new ClickOutsideFilter(Inspector, CommandBar);
            Application.AddMessageFilter(_clickOutsideFilter);
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _clickOutsideFilter is not null)
            Application.RemoveMessageFilter(_clickOutsideFilter);
        base.Dispose(disposing);
    }

    public CommandBar CommandBar { get; }

    public InspectorPanel Inspector { get; }

    public MapCanvas Canvas { get; }

    public EmbeddedGameSurface Runtime { get; }

    public Label StatusLabel { get; }

    public void SetStatus(string text)
    {
        StatusLabel.Text = text;
    }

    public void ToggleInspectorSection(InspectorSection section)
    {
        if (Inspector.Visible && Inspector.ActiveSection == section)
        {
            Inspector.Visible = false;
            return;
        }

        ShowInspectorSection(section);
    }

    public void ShowInspectorSection(InspectorSection section)
    {
        Inspector.SetActiveSection(section);
        Inspector.Visible = true;
        PlaceInspector();
        Inspector.BringToFront();
    }

    private static Label BuildStatusLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Height = EditorTheme.StatusBarHeight,
            Font = EditorTheme.SmallFont,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            BackColor = EditorTheme.StatusBar,
            ForeColor = EditorTheme.SecondaryText
        };
    }

    private sealed class ClickOutsideFilter : IMessageFilter
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        private readonly InspectorPanel _inspector;
        private readonly CommandBar _commandBar;

        public ClickOutsideFilter(InspectorPanel inspector, CommandBar commandBar)
        {
            _inspector = inspector;
            _commandBar = commandBar;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN && _inspector.Visible)
            {
                var cursor = Cursor.Position;
                var inspectorBounds = new Rectangle(_inspector.PointToScreen(Point.Empty), _inspector.Size);
                var commandBarBounds = new Rectangle(_commandBar.PointToScreen(Point.Empty), _commandBar.Size);
                if (!inspectorBounds.Contains(cursor) && !commandBarBounds.Contains(cursor))
                    _inspector.Visible = false;
            }
            return false;
        }
    }

    private void CenterCanvas(ScrollableControl canvasPanel)
    {
        var p = EditorTheme.CanvasPadding;
        var x = Math.Max(p, (canvasPanel.ClientSize.Width - Canvas.Width) / 2);
        var y = Math.Max(p, (canvasPanel.ClientSize.Height - Canvas.Height) / 4);
        Canvas.Location = new Point(x, y);
    }

    private void PlaceInspector()
    {
        if (!Inspector.Visible) return;

        const int flyoutGap = 6;

        // CommandBar sits at (0,0) inside _mainHost, so itemBounds.Top maps directly.
        var anchorY = itemBounds.Top;
        var maxTop = _mainHost.Height - Inspector.ActiveSectionHeight - flyoutGap;
        var top = Math.Max(flyoutGap, Math.Min(maxTop, anchorY));
        Inspector.Bounds = new Rectangle(x, top, EditorTheme.InspectorWidth, Inspector.ActiveSectionHeight);
    }
}
