namespace Terminalia.Editor;

public sealed class ModernButton : Button
{
    private readonly System.Windows.Forms.Timer _animationTimer = new();
    private Color _currentBackColor;
    private Color _targetBackColor;
    private bool _isHovered;
    private bool _isPressed;
    private double _pulse;
    private bool _isPrimary;

    public ModernButton()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.Transparent;
        ForeColor = EditorTheme.PrimaryText;
        Cursor = Cursors.Hand;
        Font = EditorTheme.UiFont;
        UseVisualStyleBackColor = false;
        TextAlign = ContentAlignment.MiddleCenter;

        _currentBackColor = EditorTheme.ButtonBackground;
        _targetBackColor = _currentBackColor;

        _animationTimer.Interval = EditorTheme.AnimationInterval;
        _animationTimer.Tick += (_, _) => Animate();
    }

    public bool IsPrimary
    {
        get => _isPrimary;
        set
        {
            _isPrimary = value;
            ForeColor = _isPrimary ? Color.Black : EditorTheme.PrimaryText;
            _currentBackColor = NormalColor;
            _targetBackColor = NormalColor;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        SetTarget(HoverColor);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        SetTarget(NormalColor);
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        if (mevent.Button == MouseButtons.Left)
        {
            _isPressed = true;
            _pulse = 1;
            SetTarget(PressedColor);
        }

        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _isPressed = false;
        SetTarget(_isHovered ? HoverColor : NormalColor);
        base.OnMouseUp(mevent);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        SetTarget(Enabled ? NormalColor : EditorTheme.ButtonPressed);
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        base.OnEnabledChanged(e);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);

        using var path = EditorTheme.CreateRoundedPath(bounds, EditorTheme.ButtonRadius);
        using var fill = new SolidBrush(Enabled ? _currentBackColor : EditorTheme.ButtonPressed);
        pevent.Graphics.FillPath(fill, path);

        if (_pulse > 0.01)
        {
            using var pulseBrush = new SolidBrush(Color.FromArgb((int)(38 * _pulse), Color.White));
            pevent.Graphics.FillPath(pulseBrush, path);
        }

        using var border = new Pen(IsPrimary ? EditorTheme.PrimaryText : EditorTheme.BorderSoft);
        pevent.Graphics.DrawPath(border, path);

        var textOffset = _isPressed ? 1 : 0;
        var textBounds = new Rectangle(
            Padding.Left,
            Padding.Top + textOffset,
            Width - Padding.Horizontal,
            Height - Padding.Vertical);
        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            textBounds,
            Enabled ? ForeColor : EditorTheme.SecondaryText,
            GetTextFormatFlags());
    }

    private Color NormalColor => IsPrimary ? EditorTheme.Accent : EditorTheme.ButtonBackground;

    private Color HoverColor => IsPrimary ? EditorTheme.AccentHover : EditorTheme.ButtonHover;

    private Color PressedColor => IsPrimary ? EditorTheme.AccentPressed : EditorTheme.ButtonPressed;

    private void SetTarget(Color color)
    {
        _targetBackColor = color;
        if (!_animationTimer.Enabled)
        {
            _animationTimer.Start();
        }
    }

    private void Animate()
    {
        _currentBackColor = EditorTheme.Lerp(_currentBackColor, _targetBackColor, 0.35);
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

    private TextFormatFlags GetTextFormatFlags()
    {
        var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;

        flags |= TextAlign switch
        {
            ContentAlignment.MiddleLeft or ContentAlignment.TopLeft or ContentAlignment.BottomLeft => TextFormatFlags.Left,
            ContentAlignment.MiddleRight or ContentAlignment.TopRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
            _ => TextFormatFlags.HorizontalCenter
        };

        return flags;
    }
}
