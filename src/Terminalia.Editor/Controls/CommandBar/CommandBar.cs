using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;
using Terminalia.Editor.Styling.Layout;

namespace Terminalia.Editor.Controls.CommandBar;

public sealed partial class CommandBar : UserControl
{
    private const int SepAdvance = 22;
    private const int MinGap = 2;

    private readonly List<CommandBarItem> _items = [];
    private readonly System.Windows.Forms.Timer _animTimer = new();
    private readonly System.Windows.Forms.Timer _entranceTimer = new();
    private readonly ToolTip _toolTip = new();
    private CommandBarItem? _hoverItem;
    private CommandBarItem? _pressItem;
    private CommandBarItem? _selectToolItem;
    private int _entranceOffset = -56;

    public event EventHandler? NewRequested;
    public event EventHandler? OpenRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? SaveAsRequested;
    public event EventHandler? RunExternalRequested;
    public event EventHandler? MapPanelRequested;
    public event EventHandler? ToolPanelRequested;
    public event EventHandler? PalettePanelRequested;
    public event EventHandler? RunPanelRequested;
    public event EventHandler? GamePanelRequested;
    public event EventHandler? ExportRequested;
    public event EventHandler? SelectToolRequested;

    public CommandBar()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.AppBackground;
        MinimumSize = new Size(EditorLayout.CommandBarWidth, 0);
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, false);

        _toolTip.BackColor = EditorColors.Surface;
        _toolTip.ForeColor = EditorColors.PrimaryText;
        _toolTip.OwnerDraw = true;
        _toolTip.Draw += DrawToolTip;
        _toolTip.Popup += (_, e) =>
        {
            var measured = TextRenderer.MeasureText(_hoverItem?.Caption ?? "", EditorFonts.UiFont);
            e.ToolTipSize = new Size(measured.Width + 16, measured.Height + 8);
        };

        AddCmd("[#]", "Map", () => MapPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[T]", "Tool", () => ToolPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[P]", "Palette", () => PalettePanelRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[R]", "Run", () => RunPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[G]", "Game", () => GamePanelRequested?.Invoke(this, EventArgs.Empty));
        AddSep();
        AddCmd("-+-", "Select", () => SelectToolRequested?.Invoke(this, EventArgs.Empty));
        _selectToolItem = _items[^1];
        AddSep();
        AddCmd("[+]", "New", () => NewRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[O]", "Open", () => OpenRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[S]", "Save", () => SaveRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[A]", "Save As", () => SaveAsRequested?.Invoke(this, EventArgs.Empty));
        AddSep();
        AddCmd("[>]", "Run in Console", () => RunExternalRequested?.Invoke(this, EventArgs.Empty), isPrimary: true);
        AddSep();
        AddCmd("[E]", "Export", () => ExportRequested?.Invoke(this, EventArgs.Empty));

        foreach (var item in _items)
        {
            item.BackColor = item.TargetColor = NormalColor(item);
        }

        _animTimer.Interval = EditorLayout.AnimationInterval;
        _animTimer.Tick += (_, _) => AnimTick();

        _entranceTimer.Interval = EditorLayout.AnimationInterval;
        _entranceTimer.Tick += (_, _) => EntranceTick();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
            _animTimer.Dispose();
            _entranceTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    public void SetSelectToolActive(bool active)
    {
        if (_selectToolItem is { } item && item.IsActive != active)
        {
            item.IsActive = active;
            Invalidate();
        }
    }

    private void AddCmd(string glyph, string caption, Action action, bool isPrimary = false)
    {
        _items.Add(new CommandBarItem
        {
            Glyph = glyph,
            Caption = caption,
            Click = action,
            IsPrimary = isPrimary
        });
    }

    private void AddSep() => _items.Add(new CommandBarItem { IsSeparator = true });

    private static Color NormalColor(CommandBarItem item) =>
        item.IsPrimary ? EditorColors.Accent : EditorColors.CommandBackground;
}
