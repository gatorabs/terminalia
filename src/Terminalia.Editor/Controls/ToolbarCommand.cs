namespace Terminalia.Editor;

public sealed class ToolbarCommand : Control
{
    private readonly System.Windows.Forms.Timer _animationTimer = new();
    private Color _currentBackColor;
    private Color _targetBackColor;
    private bool _isHovered;
    private bool _isPressed;
    private double _pulse;
    private bool _isPrimary;

    public ToolbarCommand()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor |
            ControlStyles.Selectable,
            true);

        BackColor = Color.Transparent;
        ForeColor = EditorTheme.PrimaryText;
        Cursor = Cursors.Hand;
        Font = EditorTheme.UiFont;
        Size = new Size(96, 34);
        MinimumSize = new Size(72, 34);

        _currentBackColor = EditorTheme.CommandBackground;
        _targetBackColor = _currentBackColor;

        _animationTimer.Interval = EditorTheme.AnimationInterval;
        _animationTimer.Tick += (_, _) => Animate();
    }

    public string Caption { get; set; } = string.Empty;

    public string Shortcut { get; set; } = string.Empty;

    public bool IsPrimary
    {
        get => _isPrimary;
        set
        {
            _isPrimary = value;
            _currentBackColor = NormalColor;
            _targetBackColor = NormalColor;
            Invalidate();
        }
    }

    public int PreferredCommandWidth { get; set; } = 96;

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        SetTarget(IsPrimary ? EditorTheme.AccentHover : EditorTheme.CommandHover);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        SetTarget(NormalColor);
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            _pulse = 1;
            SetTarget(IsPrimary ? EditorTheme.AccentPressed : EditorTheme.ButtonPressed);
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isPressed = false;
        SetTarget(_isHovered
            ? IsPrimary ? EditorTheme.AccentHover : EditorTheme.CommandHover
            : NormalColor);
        base.OnMouseUp(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        SetTarget(Enabled
            ? NormalColor
            : EditorTheme.ButtonPressed);
        base.OnEnabledChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (_isPressed)
        {
            bounds.Offset(0, 1);
        }

        using var path = EditorTheme.CreateRoundedPath(bounds, 12);
        using var fill = new SolidBrush(Enabled ? _currentBackColor : EditorTheme.ButtonPressed);
        using var border = new Pen(IsPrimary ? EditorTheme.PrimaryText : EditorTheme.BorderSoft);
        e.Graphics.FillPath(fill, path);

        if (_pulse > 0.01)
        {
            using var pulseBrush = new SolidBrush(Color.FromArgb((int)(42 * _pulse), Color.White));
            e.Graphics.FillPath(pulseBrush, path);
        }

        e.Graphics.DrawPath(border, path);

        var dotBounds = new Rectangle(bounds.Left + 12, bounds.Top + 13, 7, 7);
        using var dotBrush = new SolidBrush(IsPrimary ? Color.Black : EditorTheme.PrimaryText);
        e.Graphics.FillEllipse(dotBrush, dotBounds);

        var textBounds = new Rectangle(bounds.Left + 28, bounds.Top + 2, bounds.Width - 38, bounds.Height - 4);
        TextRenderer.DrawText(
            e.Graphics,
            Caption,
            Font,
            textBounds,
            Enabled ? (IsPrimary ? Color.Black : EditorTheme.PrimaryText) : EditorTheme.SecondaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        if (!string.IsNullOrWhiteSpace(Shortcut) && Width > 118)
        {
            using var shortcutFont = new Font(Font.FontFamily, 7.5f, FontStyle.Regular);
            TextRenderer.DrawText(
                e.Graphics,
                Shortcut,
                shortcutFont,
                new Rectangle(bounds.Left + 28, bounds.Top + 18, bounds.Width - 38, bounds.Height - 18),
                EditorTheme.SecondaryText,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        }
    }

    private void SetTarget(Color color)
    {
        _targetBackColor = color;
        if (!_animationTimer.Enabled)
        {
            _animationTimer.Start();
        }
    }

    private Color NormalColor => IsPrimary ? EditorTheme.Accent : EditorTheme.CommandBackground;

    private void Animate()
    {
        _currentBackColor = EditorTheme.Lerp(_currentBackColor, _targetBackColor, 0.32);
        _pulse *= 0.72;

        if (ColorDistance(_currentBackColor, _targetBackColor) < 3 && _pulse < 0.01)
        {
            _currentBackColor = _targetBackColor;
            _pulse = 0;
            _animationTimer.Stop();
        }

        Invalidate();
    }

    private static int ColorDistance(Color left, Color right)
    {
        return Math.Abs(left.R - right.R) + Math.Abs(left.G - right.G) + Math.Abs(left.B - right.B);
    }
}
