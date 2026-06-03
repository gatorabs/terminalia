namespace Terminalia.Editor;

public sealed class CommandBar : UserControl
{
    private const int SepAdvance = 22;
    private const int MinGap = 2;

    private readonly List<ToolbarCommand> _items = [];
    private readonly System.Windows.Forms.Timer _animTimer = new();
    private readonly System.Windows.Forms.Timer _entranceTimer = new();
    private readonly ToolTip _toolTip = new();
    private ToolbarCommand? _hoverItem;
    private ToolbarCommand? _pressItem;
    private int _entranceOffset = -56;

    public event EventHandler? NewRequested;
    public event EventHandler? OpenRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? SaveAsRequested;
    public event EventHandler? OpenSampleRequested;
    public event EventHandler? SaveSampleRequested;
    public event EventHandler? RunExternalRequested;
    public event EventHandler? MapPanelRequested;
    public event EventHandler? ToolPanelRequested;
    public event EventHandler? PalettePanelRequested;
    public event EventHandler? RunPanelRequested;

    public CommandBar()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;
        MinimumSize = new Size(EditorTheme.CommandBarWidth, 0);
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, false);

        _toolTip.BackColor = EditorTheme.Surface;
        _toolTip.ForeColor = EditorTheme.PrimaryText;
        _toolTip.OwnerDraw = true;
        _toolTip.Draw += DrawToolTip;

        AddCmd("[#]", "Map", () => MapPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[T]", "Tool", () => ToolPanelRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[P]", "Palette", () => PalettePanelRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[R]", "Run", () => RunPanelRequested?.Invoke(this, EventArgs.Empty));
        AddSep();
        AddCmd("[+]", "New", () => NewRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[O]", "Open", () => OpenRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[S]", "Save", () => SaveRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[A]", "Save As", () => SaveAsRequested?.Invoke(this, EventArgs.Empty));
        AddSep();
        AddCmd("[L]", "Open Sample", () => OpenSampleRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[W]", "Save Sample", () => SaveSampleRequested?.Invoke(this, EventArgs.Empty));
        AddCmd("[>]", "Run External", () => RunExternalRequested?.Invoke(this, EventArgs.Empty), isPrimary: true);

        foreach (var item in _items)
            item.BackColor = item.TargetColor = NormalColor(item);

        _animTimer.Interval = EditorTheme.AnimationInterval;
        _animTimer.Tick += (_, _) => AnimTick();

        _entranceTimer.Interval = EditorTheme.AnimationInterval;
        _entranceTimer.Tick += (_, _) =>
        {
            _entranceOffset = (int)Math.Round(_entranceOffset * 0.66);
            if (Math.Abs(_entranceOffset) <= 1)
            {
                _entranceOffset = 0;
                _entranceTimer.Stop();
            }
            CalculateBounds();
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
        CalculateBounds();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(EditorTheme.AppBackground);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rail = new Rectangle(
            EditorTheme.CommandPadding, EditorTheme.CommandPadding,
            Width - EditorTheme.CommandPadding * 2,
            Height - EditorTheme.CommandPadding * 2);

        using (var railPath = EditorTheme.CreateRoundedPath(rail, 18))
        using (var railFill = new SolidBrush(EditorTheme.Surface))
        using (var railBorder = new Pen(EditorTheme.BorderSoft))
        {
            g.FillPath(railFill, railPath);
            g.DrawPath(railBorder, railPath);
        }

        DrawBrand(g);

        foreach (var item in _items)
        {
            if (item.IsSeparator)
                PaintSeparator(g, item);
            else
                PaintButton(g, item);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var hit = HitTest(e.Location);
        if (hit == _hoverItem) return;

        _hoverItem = hit;
        Cursor = hit is not null ? Cursors.Hand : Cursors.Default;

        foreach (var item in _items)
        {
            if (item.IsSeparator) continue;
            item.TargetColor = item == hit
                ? (item.IsPrimary ? EditorTheme.AccentHover : EditorTheme.CommandHover)
                : NormalColor(item);
        }

        if (hit is not null)
            _toolTip.Show(hit.Caption, this, new Point(Width + 4, hit.Bounds.Top), 3000);
        else
            _toolTip.Hide(this);

        StartAnim();
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hoverItem = null;
        _pressItem = null;
        Cursor = Cursors.Default;
        _toolTip.Hide(this);

        foreach (var item in _items)
        {
            if (!item.IsSeparator)
                item.TargetColor = NormalColor(item);
        }

        StartAnim();
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        var hit = HitTest(e.Location);
        if (hit is null) return;

        _pressItem = hit;
        hit.Pulse = 1;
        hit.TargetColor = hit.IsPrimary ? EditorTheme.AccentPressed : EditorTheme.ButtonPressed;
        StartAnim();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_pressItem is null) return;
        var pressed = _pressItem;
        _pressItem = null;

        pressed.TargetColor = pressed == _hoverItem
            ? (pressed.IsPrimary ? EditorTheme.AccentHover : EditorTheme.CommandHover)
            : NormalColor(pressed);

        if (pressed == _hoverItem)
            pressed.Click?.Invoke();

        StartAnim();
        Invalidate();
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

    private void AddCmd(string glyph, string caption, Action action, bool isPrimary = false)
    {
        _items.Add(new ToolbarCommand
        {
            Glyph = glyph,
            Caption = caption,
            Click = action,
            IsPrimary = isPrimary
        });
    }

    private void AddSep() => _items.Add(new ToolbarCommand { IsSeparator = true });

    private void CalculateBounds()
    {
        var sepCount = _items.Count(c => c.IsSeparator);
        var buttonCount = _items.Count - sepCount;
        var available = Height - EditorTheme.BrandSectionHeight - EditorTheme.CommandPadding;
        var sizeOnlyNeeded = buttonCount * EditorTheme.CommandSize + sepCount * SepAdvance;

        int size, gap;
        if (sizeOnlyNeeded <= available)
        {
            size = EditorTheme.CommandSize;
            gap = buttonCount > 1
                ? Math.Min(EditorTheme.CommandGap, (available - sizeOnlyNeeded) / (buttonCount - 1))
                : EditorTheme.CommandGap;
        }
        else
        {
            size = Math.Max(24, (available - Math.Max(0, buttonCount - 1) * MinGap - sepCount * SepAdvance) / Math.Max(1, buttonCount));
            gap = buttonCount > 1
                ? Math.Max(MinGap, (available - buttonCount * size - sepCount * SepAdvance) / (buttonCount - 1))
                : MinGap;
        }

        var x = (Width - size) / 2 + _entranceOffset;
        var y = EditorTheme.BrandSectionHeight;

        foreach (var item in _items)
        {
            if (item.IsSeparator)
            {
                item.Bounds = new Rectangle(x, y + 3, size, 10);
                y += SepAdvance;
            }
            else
            {
                item.Bounds = new Rectangle(x, y, size, size);
                y += size + gap;
            }
        }
    }

    private void DrawBrand(Graphics g)
    {
        using var titleFont = new Font(FontFamily.GenericMonospace, 14, FontStyle.Bold);
        using var smallFont = new Font(FontFamily.GenericMonospace, 7, FontStyle.Regular);

        TextRenderer.DrawText(g, ">_", titleFont,
            new Rectangle(0, 28, Width, 24), EditorTheme.PrimaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "TERM", smallFont,
            new Rectangle(0, 52, Width, 16), EditorTheme.SecondaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static void PaintSeparator(Graphics g, ToolbarCommand item)
    {
        using var pen = new Pen(EditorTheme.BorderSoft);
        var midY = item.Bounds.Top + item.Bounds.Height / 2;
        g.DrawLine(pen, item.Bounds.Left + 10, midY, item.Bounds.Right - 10, midY);
    }

    private void PaintButton(Graphics g, ToolbarCommand item)
    {
        var b = item.Bounds;
        b.Inflate(-1, -1);
        if (item == _pressItem)
            b.Offset(0, 1);

        using var path = EditorTheme.CreateRoundedPath(b, 12);
        using var fill = new SolidBrush(item.BackColor);
        using var border = new Pen(item.IsPrimary ? EditorTheme.PrimaryText : EditorTheme.BorderSoft);
        g.FillPath(fill, path);

        if (item.Pulse > 0.01)
        {
            using var pulseBrush = new SolidBrush(Color.FromArgb((int)(42 * item.Pulse), Color.White));
            g.FillPath(pulseBrush, path);
        }

        g.DrawPath(border, path);

        using var iconFont = new Font(FontFamily.GenericMonospace, item.Glyph.Length > 2 ? 9 : 10, FontStyle.Bold);
        TextRenderer.DrawText(g, item.Glyph, iconFont, b,
            item.IsPrimary ? Color.Black : EditorTheme.PrimaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    private void DrawToolTip(object? sender, DrawToolTipEventArgs e)
    {
        using var fill = new SolidBrush(EditorTheme.Surface);
        using var border = new Pen(EditorTheme.Border);
        e.Graphics.FillRectangle(fill, e.Bounds);
        e.Graphics.DrawRectangle(border, new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1));
        TextRenderer.DrawText(e.Graphics, e.ToolTipText, EditorTheme.UiFont, e.Bounds,
            EditorTheme.PrimaryText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private ToolbarCommand? HitTest(Point p)
    {
        foreach (var item in _items)
        {
            if (!item.IsSeparator && item.Bounds.Contains(p))
                return item;
        }
        return null;
    }

    private void AnimTick()
    {
        var allDone = true;
        foreach (var item in _items)
        {
            if (item.IsSeparator) continue;
            item.BackColor = EditorTheme.Lerp(item.BackColor, item.TargetColor, 0.32);
            item.Pulse *= 0.72;
            if (ColorDist(item.BackColor, item.TargetColor) < 3 && item.Pulse < 0.01)
            {
                item.BackColor = item.TargetColor;
                item.Pulse = 0;
            }
            else
            {
                allDone = false;
            }
        }
        if (allDone) _animTimer.Stop();
        Invalidate();
    }

    private void StartAnim()
    {
        if (!_animTimer.Enabled)
            _animTimer.Start();
    }

    private static Color NormalColor(ToolbarCommand item) =>
        item.IsPrimary ? EditorTheme.Accent : EditorTheme.CommandBackground;

    private static int ColorDist(Color a, Color b) =>
        Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
}
