namespace Terminalia.Editor;

public sealed class RoundedPanel : Panel
{
    public RoundedPanel()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);

        BackColor = Color.Transparent;
        FillColor = EditorTheme.Section;
        BorderColor = EditorTheme.BorderSoft;
        CornerRadius = EditorTheme.CornerRadius;
        BorderThickness = 1;
    }

    public Color FillColor { get; set; }

    public Color BorderColor { get; set; }

    public int CornerRadius { get; set; }

    public int BorderThickness { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;

        using var path = EditorTheme.CreateRoundedPath(bounds, CornerRadius);
        using var fill = new SolidBrush(FillColor);
        e.Graphics.FillPath(fill, path);

        if (BorderThickness > 0)
        {
            using var border = new Pen(BorderColor, BorderThickness);
            e.Graphics.DrawPath(border, path);
        }

        base.OnPaint(e);
    }
}
