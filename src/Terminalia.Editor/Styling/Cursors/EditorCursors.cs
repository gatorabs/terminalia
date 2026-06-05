namespace Terminalia.Editor.Styling.Cursors;

internal static class EditorCursors
{
    public static readonly Cursor Blank = CreateBlankCursor();
    public static readonly Cursor Terminal = CreateTerminalCursor();

    private static Cursor CreateBlankCursor()
    {
        using var bmp = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        return new Cursor(bmp.GetHicon());
    }

    private static Cursor CreateTerminalCursor()
    {
        const int size = 20;
        using var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

        Point[] shadow = [
            new(1, 1), new(13, 13), new(9, 13),
            new(12, 19), new(9, 19), new(6, 13), new(1, 19)
        ];
        Point[] arrow = [
            new(0, 0), new(12, 12), new(8, 12),
            new(11, 18), new(8, 18), new(5, 12), new(0, 18)
        ];

        using var shadowBrush = new SolidBrush(Color.FromArgb(25, 25, 25));
        using var fillBrush = new SolidBrush(Color.FromArgb(210, 210, 210));

        g.FillPolygon(shadowBrush, shadow);
        g.FillPolygon(fillBrush, arrow);

        return new Cursor(bmp.GetHicon());
    }
}
