using Terminalia.Editor.State;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;
using Terminalia.Runtime.Windows;

namespace Terminalia.Editor.Controls.Canvas;

public sealed partial class MapCanvas
{
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(EditorColors.Workspace);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        e.Graphics.TranslateTransform(CanvasPadding, CanvasPadding);

        for (var y = 0; y < _document.Height; y++)
        {
            for (var x = 0; x < _document.Width; x++)
            {
                DrawCell(e.Graphics, x, y, _document.GetTile(x, y));
            }
        }

        if (_document.HasZones)
        {
            DrawZoneOverlay(e.Graphics);
        }

        if (_kernelPreviewFocus is { } focus)
        {
            DrawKernelPreview(e.Graphics, ComputeKernelBounds(focus));
        }

        if (_selectedCell is { } selected)
        {
            DrawSelection(e.Graphics, selected);
        }

        if (_cursorCell is { } cursor)
        {
            DrawCursor(e.Graphics, cursor.X, cursor.Y);
        }
    }

    private void DrawCell(Graphics graphics, int x, int y, char tile)
    {
        var bounds = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);

        using var background = new SolidBrush(EditorTileColors.TileBackground(tile));
        using var border = new Pen(EditorColors.GridLine);
        using var tileFont = GetTileFont();
        using var textBrush = new SolidBrush(EditorTileColors.TileText(tile));
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoClip
        };

        graphics.FillRectangle(background, bounds);
        graphics.DrawRectangle(border, bounds);
        graphics.DrawString(tile.ToString(), tileFont, textBrush, bounds, format);
    }

    private void DrawZoneOverlay(Graphics graphics)
    {
        for (var y = 0; y < _document.Height; y++)
        {
            for (var x = 0; x < _document.Width; x++)
            {
                var zoneName = _document.GetZoneAt(x, y);
                if (zoneName is null)
                {
                    continue;
                }

                var bounds = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
                using var brush = new SolidBrush(EditorTileColors.ZoneOverlay(zoneName));
                graphics.FillRectangle(brush, bounds);
            }
        }
    }

    private void DrawCursor(Graphics g, int cx, int cy)
    {
        var b = new Rectangle(cx * CellSize + 1, cy * CellSize + 1, CellSize - 2, CellSize - 2);
        var color = GetCursorColor();

        using var pen = new Pen(color, 2f);
        g.DrawRectangle(pen, b);

        var mx = b.X + b.Width / 2;
        var my = b.Y + b.Height / 2;
        const int arm = 4;
        g.DrawLine(pen, mx - arm, my, mx + arm, my);
        g.DrawLine(pen, mx, my - arm, mx, my + arm);
    }

    private Color GetCursorColor()
    {
        if (ToolMode == EditorToolMode.Fill)
        {
            return EditorColors.FillToolAccent;
        }

        if (ToolMode == EditorToolMode.Select)
        {
            return EditorColors.SelectAccent;
        }

        if (ToolMode == EditorToolMode.Zone && _selectedZone is not null)
        {
            return EditorTileColors.ZoneCursor(_selectedZone);
        }

        return EditorColors.AccentHover;
    }

    private void DrawSelection(Graphics g, Point cell)
    {
        var bounds = new Rectangle(cell.X * CellSize + 1, cell.Y * CellSize + 1, CellSize - 2, CellSize - 2);
        using var pen = new Pen(EditorColors.SelectAccent, 2.5f);
        g.DrawRectangle(pen, bounds);
    }

    private void DrawKernelPreview(Graphics g, Rectangle kernel)
    {
        var bounds = new Rectangle(
            kernel.X * CellSize,
            kernel.Y * CellSize,
            kernel.Width * CellSize,
            kernel.Height * CellSize);

        using var fill = new SolidBrush(EditorColors.SelectKernelFill);
        using var border = new Pen(EditorColors.SelectKernelBorder, 2f);
        g.FillRectangle(fill, bounds);
        g.DrawRectangle(border, bounds);
    }

    private Rectangle ComputeKernelBounds(Point focus)
    {
        var kernelSize = GameViewportDefaults.ComputeKernelSize(_document.CameraZoom);
        var originX = ClampOrigin(focus.X - kernelSize.Width / 2, _document.Width - kernelSize.Width);
        var originY = ClampOrigin(focus.Y - kernelSize.Height / 2, _document.Height - kernelSize.Height);
        return new Rectangle(originX, originY, kernelSize.Width, kernelSize.Height);
    }

    private static int ClampOrigin(int value, int max) => Math.Clamp(value, 0, Math.Max(0, max));

    private Rectangle GetCellInvalidRect(Point cell) =>
        new Rectangle(CanvasPadding + cell.X * CellSize - 1, CanvasPadding + cell.Y * CellSize - 1, CellSize + 2, CellSize + 2);

    private void UpdateCanvasSize()
    {
        Size = new Size((_document.Width * CellSize) + (CanvasPadding * 2), (_document.Height * CellSize) + (CanvasPadding * 2));
    }

    private Font GetTileFont()
    {
        var size = Math.Max(7, CellSize * 0.45f);
        return EditorFonts.MonospaceBold(size);
    }
}
