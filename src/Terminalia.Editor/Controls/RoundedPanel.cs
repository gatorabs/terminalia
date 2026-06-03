namespace Terminalia.Editor;

public sealed class RoundedPanel : Panel
{
    public RoundedPanel()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        FillColor = EditorTheme.Section;
        BorderColor = EditorTheme.BorderSoft;
        BorderThickness = 1;
    }

    public Color FillColor { get; set; }

    public Color BorderColor { get; set; }

    public int BorderThickness { get; set; }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(FillColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;

        if (BorderThickness > 0)
        {
            using var border = new Pen(BorderColor, BorderThickness);
            e.Graphics.DrawRectangle(border, bounds);
        }

        base.OnPaint(e);
    }
}
