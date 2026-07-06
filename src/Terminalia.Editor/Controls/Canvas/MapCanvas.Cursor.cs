using Terminalia.Editor.State;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Cursors;

namespace Terminalia.Editor.Controls.Canvas;

public sealed partial class MapCanvas
{
    private void UpdateCursorCell(Point location)
    {
        Point? nextCell = TryGetCell(location, out var cell) ? cell : null;

        if (_cursorCell == nextCell)
        {
            return;
        }

        var prev = _cursorCell;
        _cursorCell = nextCell;
        CursorCellChanged?.Invoke(this, EventArgs.Empty);

        if (prev is { } p)
        {
            Invalidate(GetCellInvalidRect(p));
        }

        if (nextCell is { } n)
        {
            Invalidate(GetCellInvalidRect(n));
        }
    }

    private bool TryGetCell(Point location, out Point cell)
    {
        cell = default;

        if (location.X < CanvasPadding || location.Y < CanvasPadding)
        {
            return false;
        }

        var x = (location.X - CanvasPadding) / CellSize;
        var y = (location.Y - CanvasPadding) / CellSize;

        if (!_document.Contains(x, y))
        {
            return false;
        }

        cell = new Point(x, y);
        return true;
    }

    private void UpdateTileCursor()
    {
        if (_toolMode == EditorToolMode.Zone)
        {
            Cursor = EditorCursors.Blank;
            return;
        }

        if (_toolMode == EditorToolMode.Select)
        {
            Cursor = Cursors.Default;
            return;
        }

        Cursor = EditorCursors.ForTile(_selectedTile, EditorTileColors.TileText(_selectedTile));
    }
}
