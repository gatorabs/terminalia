namespace Terminalia.Editor;

public sealed class EditorShell : UserControl
{
    private readonly Panel _mainHost = new();

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

    private void CenterCanvas(ScrollableControl canvasPanel)
    {
        var p = EditorTheme.CanvasPadding;
        var x = Math.Max(p, (canvasPanel.ClientSize.Width - Canvas.Width) / 2);
        var y = Math.Max(p, (canvasPanel.ClientSize.Height - Canvas.Height) / 4);
        Canvas.Location = new Point(x, y);
    }

    private void PlaceInspector()
    {
        if (!Inspector.Visible)
        {
            return;
        }

        var margin = EditorTheme.CanvasPadding;
        var top = Math.Max(margin, Math.Min(_mainHost.Height - Inspector.ActiveSectionHeight - margin, margin * 2 + 10));
        var x = EditorTheme.CommandBarWidth + margin;
        Inspector.Bounds = new Rectangle(x, top, EditorTheme.InspectorWidth, Inspector.ActiveSectionHeight);
    }
}
