using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;
using Terminalia.Editor.Styling.Layout;

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
        ForeColor = EditorColors.PrimaryText;
        Cursor = Cursors.Hand;
        Font = EditorFonts.UiFont;
        UseVisualStyleBackColor = false;
        TextAlign = ContentAlignment.MiddleCenter;

        _currentBackColor = EditorColors.ButtonBackground;
        _targetBackColor = _currentBackColor;

        _animationTimer.Interval = EditorLayout.AnimationInterval;
        _animationTimer.Tick += (_, _) => Animate();
    }

    public bool IsPrimary
    {
        get => _isPrimary;
        set
        {
            _isPrimary = value;
            ForeColor = _isPrimary ? Color.Black : EditorColors.PrimaryText;
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
        SetTarget(Enabled ? NormalColor : EditorColors.ButtonPressed);
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        base.OnEnabledChanged(e);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);

        using var fill = new SolidBrush(Enabled ? _currentBackColor : EditorColors.ButtonPressed);
        pevent.Graphics.FillRectangle(fill, bounds);

        if (_pulse > 0.01)
        {
            using var pulseBrush = new SolidBrush(Color.FromArgb((int)(38 * _pulse), Color.White));
            pevent.Graphics.FillRectangle(pulseBrush, bounds);
        }

        using var border = new Pen(IsPrimary ? EditorColors.PrimaryText : EditorColors.BorderSoft);
        pevent.Graphics.DrawRectangle(border, bounds);

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
            Enabled ? ForeColor : EditorColors.SecondaryText,
            GetTextFormatFlags());
    }

    private Color NormalColor => IsPrimary ? EditorColors.Accent : EditorColors.ButtonBackground;

    private Color HoverColor => IsPrimary ? EditorColors.AccentHover : EditorColors.ButtonHover;

    private Color PressedColor => IsPrimary ? EditorColors.AccentPressed : EditorColors.ButtonPressed;

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
        _currentBackColor = EditorColors.Lerp(_currentBackColor, _targetBackColor, 0.35);
        _pulse *= 0.72;

        if (EditorColors.ColorDistance(_currentBackColor, _targetBackColor) < 3 && _pulse < 0.01)
        {
            _currentBackColor = _targetBackColor;
            _pulse = 0;
            _animationTimer.Stop();
        }

        Invalidate();
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
