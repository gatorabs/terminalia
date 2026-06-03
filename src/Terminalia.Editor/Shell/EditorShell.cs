namespace Terminalia.Editor;

public sealed class EditorShell : UserControl
{
    public EditorShell()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;

        CommandBar = new CommandBar();
        Inspector = new InspectorPanel();
        Canvas = new MapCanvas();
        Runtime = new EmbeddedGameSurface();
        StatusLabel = BuildStatusLabel();

        var canvasPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = EditorTheme.Workspace,
            Padding = new Padding(22)
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
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 370));
        mainLayout.Controls.Add(Inspector, 0, 0);
        mainLayout.Controls.Add(canvasPanel, 1, 0);
        mainLayout.Controls.Add(Runtime, 2, 0);

        var shellLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = EditorTheme.AppBackground
        };
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        shellLayout.Controls.Add(CommandBar, 0, 0);
        shellLayout.Controls.Add(mainLayout, 0, 1);
        shellLayout.Controls.Add(StatusLabel, 0, 2);

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

    private static Label BuildStatusLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Height = 30,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            BackColor = EditorTheme.StatusBar,
            ForeColor = EditorTheme.SecondaryText
        };
    }

    private void CenterCanvas(ScrollableControl canvasPanel)
    {
        var x = Math.Max(22, (canvasPanel.ClientSize.Width - Canvas.Width) / 2);
        var y = Math.Max(22, (canvasPanel.ClientSize.Height - Canvas.Height) / 4);
        Canvas.Location = new Point(x, y);
    }
}
