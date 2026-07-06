using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Styling.Cursors;

internal static class EditorCursors
{
    private const int GlyphAlpha = 170;

    private static readonly Dictionary<char, Cursor> _tileCache = [];

    public static readonly Cursor Blank = CreateBlankCursor();

    public static Cursor ForTile(char tile, Color color)
    {
        if (!_tileCache.TryGetValue(tile, out var cursor))
        {
            cursor = BuildTileCursor(tile, color);
            _tileCache[tile] = cursor;
        }
        return cursor;
    }

    private static Cursor CreateBlankCursor()
    {
        using var bmp = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        return new Cursor(bmp.GetHicon());
    }

    private static Cursor BuildTileCursor(char tile, Color color)
    {
        const int size = 32;
        using var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

        using var brush = new SolidBrush(EditorColors.WithAlpha(GlyphAlpha, color));
        using var font = EditorFonts.MonospaceBold(18);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString(tile.ToString(), font, brush, new RectangleF(0, 0, size, size), format);

        return new Cursor(bmp.GetHicon());
    }
}
