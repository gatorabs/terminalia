using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Shared;

public sealed class MapTabBar : Control
{
    private sealed record TabEntry(string Text, Rectangle Bounds, Rectangle CloseBounds);

    private const int TabPadding = 10;
    private const int CloseWidth = 20;
    private const int MaxTabWidth = 150;
    private const int MinTabWidth = 60;

    private readonly List<TabEntry> _tabs = [];
    private int _activeIndex = -1;
    private int _hoverIndex = -1;
    private bool _hoverClose;
    private Rectangle _addBounds;

    public event EventHandler<int>? TabSelected;
    public event EventHandler<int>? TabCloseRequested;
    public event EventHandler? NewTabRequested;

    public MapTabBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Font = EditorFonts.UiFont;
    }

    public void SetTabs(IReadOnlyList<(string name, bool dirty)> tabs, int activeIndex)
    {
        _activeIndex = activeIndex;
        _tabs.Clear();
        var x = 0;
        foreach (var (name, dirty) in tabs)
        {
            var text = dirty ? name + " •" : name;
            var w = Math.Clamp(TextRenderer.MeasureText(text, Font).Width + TabPadding * 2 + CloseWidth, MinTabWidth, MaxTabWidth);
            _tabs.Add(new TabEntry(text, new Rectangle(x, 0, w, Height), new Rectangle(x + w - CloseWidth, (Height - 16) / 2, 16, 16)));
            x += w;
        }
        _addBounds = new Rectangle(x + 6, (Height - 20) / 2, 20, 20);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(EditorColors.AppBackground);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        for (var i = 0; i < _tabs.Count; i++)
        {
            var tab = _tabs[i];
            var isActive = i == _activeIndex;
            var isHover = i == _hoverIndex;
            var bgColor = isActive ? EditorColors.Surface : isHover ? EditorColors.CommandHover : EditorColors.AppBackground;

            using var bg = new SolidBrush(bgColor);
            g.FillRectangle(bg, tab.Bounds);

            if (isActive)
            {
                using var accent = new Pen(EditorColors.Accent, 2);
                g.DrawLine(accent, tab.Bounds.Left, 0, tab.Bounds.Right - 1, 0);
            }

            var textColor = isActive ? EditorColors.PrimaryText : EditorColors.SecondaryText;
            var textRect = new Rectangle(tab.Bounds.Left + TabPadding, 0, tab.Bounds.Width - CloseWidth - TabPadding, Height);
            TextRenderer.DrawText(g, tab.Text, Font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

            if (isActive || isHover)
            {
                var closeColor = (_hoverClose && isHover) ? EditorColors.PrimaryText : EditorColors.SecondaryText;
                TextRenderer.DrawText(g, "×", Font, tab.CloseBounds, closeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            using var sep = new Pen(EditorColors.BorderSoft);
            g.DrawLine(sep, tab.Bounds.Right - 1, 6, tab.Bounds.Right - 1, Height - 6);
        }

        var addHover = _hoverIndex == -2;
        using var addBg = new SolidBrush(addHover ? EditorColors.CommandHover : EditorColors.AppBackground);
        g.FillRectangle(addBg, _addBounds);
        TextRenderer.DrawText(g, "+", Font, _addBounds, addHover ? EditorColors.PrimaryText : EditorColors.SecondaryText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        using var border = new Pen(EditorColors.BorderSoft);
        g.DrawLine(border, 0, Height - 1, Width, Height - 1);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        if (_addBounds.Contains(e.Location))
        {
            NewTabRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        for (var i = 0; i < _tabs.Count; i++)
        {
            if (_tabs[i].CloseBounds.Contains(e.Location))
            {
                TabCloseRequested?.Invoke(this, i);
                return;
            }

            if (_tabs[i].Bounds.Contains(e.Location))
            {
                TabSelected?.Invoke(this, i);
                return;
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var prevHover = _hoverIndex;
        var prevClose = _hoverClose;
        _hoverIndex = _addBounds.Contains(e.Location) ? -2 : -1;
        _hoverClose = false;

        for (var i = 0; i < _tabs.Count; i++)
        {
            if (!_tabs[i].Bounds.Contains(e.Location))
            {
                continue;
            }
            _hoverIndex = i;
            _hoverClose = _tabs[i].CloseBounds.Contains(e.Location);
            break;
        }

        if (_hoverIndex != prevHover || _hoverClose != prevClose)
        {
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hoverIndex = -1;
        _hoverClose = false;
        Invalidate();
    }
}
