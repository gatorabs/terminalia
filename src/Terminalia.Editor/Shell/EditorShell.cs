using Terminalia.Editor.Controls.Canvas;
using Terminalia.Editor.Controls.CommandBar;
using Terminalia.Editor.Controls.Inspector;
using Terminalia.Editor.Controls.Shared;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;
using Terminalia.Editor.Styling.Layout;

namespace Terminalia.Editor;

public sealed class EditorShell : UserControl
{
    private readonly Panel _mainHost = new();
    private Panel _canvasScroll = new();
    private ClickOutsideFilter? _clickOutsideFilter;

    public EditorShell()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.AppBackground;
        Font = EditorFonts.UiFont;

        CommandBar = new CommandBar();
        Inspector = new InspectorPanel();
        Inspector.Visible = false;
        Inspector.Width = EditorLayout.InspectorWidth;
        Inspector.Dock = DockStyle.None;
        Canvas = new MapCanvas();
        Runtime = new EngineCanvas();
        Runtime.Visible = false;
        StatusLabel = BuildStatusLabel();

        _canvasScroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = EditorColors.Workspace,
            Padding = new Padding(EditorLayout.CanvasPadding)
        };
        EditorControls.ApplyDarkScrollBars(_canvasScroll);
        _canvasScroll.Controls.Add(Canvas);
        _canvasScroll.Resize += (_, _) => CenterCanvas(_canvasScroll);
        _canvasScroll.MouseWheel += (_, e) => Canvas.CellSize += e.Delta > 0 ? 4 : -4;
        Canvas.SizeChanged += (_, _) => CenterCanvas(_canvasScroll);

        var contentHost = new Panel { Dock = DockStyle.Fill };
        contentHost.Controls.Add(Runtime);
        contentHost.Controls.Add(_canvasScroll);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = EditorColors.AppBackground
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, EditorLayout.CommandBarWidth));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.Controls.Add(CommandBar, 0, 0);
        mainLayout.Controls.Add(contentHost, 1, 0);

        _mainHost.Dock = DockStyle.Fill;
        _mainHost.BackColor = EditorColors.AppBackground;
        _mainHost.Controls.Add(mainLayout);
        _mainHost.Controls.Add(Inspector);
        _mainHost.Resize += (_, _) => PlaceInspector();

        var shellLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = EditorColors.AppBackground
        };
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, EditorLayout.StatusBarHeight));
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
        {
            Application.RemoveMessageFilter(_clickOutsideFilter);
        }
        base.Dispose(disposing);
    }

    public CommandBar CommandBar { get; }

    public InspectorPanel Inspector { get; }

    public MapCanvas Canvas { get; }

    public EngineCanvas Runtime { get; }

    public Label StatusLabel { get; }

    public void SetStatus(string text)
    {
        StatusLabel.Text = text;
    }

    public void EnterGameMode()
    {
        Inspector.Visible = false;
        Runtime.CellSize = Canvas.CellSize;
        _canvasScroll.Visible = false;
        Runtime.Visible = true;
        Runtime.BringToFront();
    }

    public void ExitGameMode()
    {
        Runtime.Visible = false;
        _canvasScroll.Visible = true;
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
            Height = EditorLayout.StatusBarHeight,
            Font = EditorFonts.SmallFont,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            BackColor = EditorColors.StatusBar,
            ForeColor = EditorColors.SecondaryText
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
                {
                    _inspector.Visible = false;
                }
            }
            return false;
        }
    }

    private void CenterCanvas(ScrollableControl canvasPanel)
    {
        var p = EditorLayout.CanvasPadding;
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

        const int flyoutGap = 6;
        var x = EditorLayout.CommandBarWidth + flyoutGap;

        var itemBounds = CommandBar.GetPanelItemBounds(Inspector.ActiveSection);
        var anchorY = itemBounds.Top;
        var maxTop = _mainHost.Height - Inspector.ActiveSectionHeight - flyoutGap;
        var top = Math.Max(flyoutGap, Math.Min(maxTop, anchorY));
        Inspector.Bounds = new Rectangle(x, top, EditorLayout.InspectorWidth, Inspector.ActiveSectionHeight);
    }
}
