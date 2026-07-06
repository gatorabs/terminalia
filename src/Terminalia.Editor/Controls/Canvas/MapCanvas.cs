using Terminalia.Assets.Json;
using Terminalia.Editor.State;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Canvas;

public sealed partial class MapCanvas : Control
{
    private const int CanvasPadding = 24;
    private int _cellSize = 32;
    private MapAssetDocument _document = MapAssetDocument.CreateDefault();
    private char _selectedTile = '#';
    private EditorToolMode _toolMode = EditorToolMode.Paint;
    private string? _selectedZone;
    private bool _isPainting;
    private Point? _lastPaintedCell;
    private Point? _cursorCell;
    private Point? _selectedCell;
    private Point? _kernelPreviewFocus;
    private bool _kernelPreviewVisible;

    public event EventHandler? DocumentChanged;
    public event EventHandler? CursorCellChanged;
    public event EventHandler<char>? TilePicked;
    public event EventHandler<Point?>? SelectionChanged;
    public event EventHandler<string?>? ZonePicked;

    public MapCanvas()
    {
        Location = new Point(0, 0);
        BackColor = EditorColors.Workspace;
        DoubleBuffered = true;
        Font = EditorFonts.CanvasFont;
        UpdateCanvasSize();
        UpdateTileCursor();
    }

    public MapAssetDocument Document => _document;

    public Point? CursorCell => _cursorCell;

    public EditorToolMode ToolMode
    {
        get => _toolMode;
        set
        {
            _toolMode = value;
            UpdateTileCursor();
        }
    }

    public int CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = Math.Clamp(value, 16, 48);
            UpdateCanvasSize();
            Invalidate();
        }
    }

    public char SelectedTile
    {
        get => _selectedTile;
        set
        {
            _selectedTile = value;
            UpdateTileCursor();
        }
    }

    public string? SelectedZone
    {
        get => _selectedZone;
        set => _selectedZone = value;
    }

    public Point? SelectedCell => _selectedCell;

    public bool KernelPreviewVisible
    {
        get => _kernelPreviewVisible;
        set
        {
            _kernelPreviewVisible = value;
            UpdateKernelPreview();
        }
    }

    public void LoadDocument(MapAssetDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        ClearSelection();
        UpdateCanvasSize();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshDocument()
    {
        ClearSelection();
        UpdateCanvasSize();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearSelection()
    {
        SetSelection(null);
    }

    private void SetSelection(Point? cell)
    {
        if (_selectedCell == cell)
        {
            return;
        }

        _selectedCell = cell;
        UpdateKernelPreview();
        SelectionChanged?.Invoke(this, _selectedCell);
        Invalidate();
    }

    private void UpdateKernelPreview()
    {
        var nextFocus = _kernelPreviewVisible
            && _selectedCell is { } cell
            && _document.GetTile(cell.X, cell.Y) == '@'
                ? cell
                : (Point?)null;

        if (_kernelPreviewFocus == nextFocus)
        {
            return;
        }

        _kernelPreviewFocus = nextFocus;
        Invalidate();
    }
}
