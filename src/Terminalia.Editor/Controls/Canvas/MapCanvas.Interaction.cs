using Terminalia.Editor.State;

namespace Terminalia.Editor.Controls.Canvas;

public sealed partial class MapCanvas
{
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
        {
            if (e.Button == MouseButtons.Right)
            {
                PickTileAt(e.Location);
            }
            return;
        }

        UpdateCursorCell(e.Location);

        if (ToolMode == EditorToolMode.Select)
        {
            SelectAt(e.Location);
            return;
        }

        if (ToolMode == EditorToolMode.Fill)
        {
            FillAt(e.Location);
            return;
        }

        _isPainting = true;
        PaintAt(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        UpdateCursorCell(e.Location);

        if (_isPainting && e.Button == MouseButtons.Left &&
            (ToolMode == EditorToolMode.Paint || ToolMode == EditorToolMode.Zone))
        {
            PaintAt(e.Location);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _isPainting = false;
        _lastPaintedCell = null;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (_cursorCell is { } prev)
        {
            _cursorCell = null;
            CursorCellChanged?.Invoke(this, EventArgs.Empty);
            Invalidate(GetCellInvalidRect(prev));
        }
    }

    private void PaintAt(Point location)
    {
        if (!TryGetCell(location, out var cell) || _lastPaintedCell == cell)
        {
            return;
        }

        var selectedTileBefore = GetSelectedTile();

        if (ToolMode == EditorToolMode.Zone)
        {
            _document.SetZone(cell.X, cell.Y, _selectedZone);
        }
        else
        {
            Point? prevUnique = null;
            if (_selectedTile == '@' && _document.TryFindTile('@', out var old))
            {
                prevUnique = new Point(old.X, old.Y);
            }
            _document.SetTile(cell.X, cell.Y, _selectedTile);
            if (prevUnique is { } prev && prev != cell)
            {
                Invalidate(GetCellInvalidRect(prev));
            }
        }

        _lastPaintedCell = cell;
        RefreshSelectionAfterEdit(selectedTileBefore);
        UpdateKernelPreview();
        Invalidate(GetCellInvalidRect(cell));
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FillAt(Point location)
    {
        if (!TryGetCell(location, out var cell))
        {
            return;
        }

        var selectedTileBefore = GetSelectedTile();
        _document.FloodFill(cell.X, cell.Y, _selectedTile);
        RefreshSelectionAfterEdit(selectedTileBefore);
        UpdateKernelPreview();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SelectAt(Point location)
    {
        if (!TryGetCell(location, out var cell))
        {
            ClearSelection();
            return;
        }

        SetSelection(_selectedCell == cell ? null : cell);
    }

    private void PickTileAt(Point location)
    {
        if (!TryGetCell(location, out var cell))
        {
            return;
        }

        if (ToolMode == EditorToolMode.Zone)
        {
            ZonePicked?.Invoke(this, _document.GetZoneAt(cell.X, cell.Y));
        }
        else
        {
            TilePicked?.Invoke(this, _document.GetTile(cell.X, cell.Y));
        }
    }

    private char? GetSelectedTile()
    {
        return _selectedCell is { } cell ? _document.GetTile(cell.X, cell.Y) : null;
    }

    private void RefreshSelectionAfterEdit(char? selectedTileBefore)
    {
        if (_selectedCell is not null && GetSelectedTile() != selectedTileBefore)
        {
            ClearSelection();
        }
    }

}
