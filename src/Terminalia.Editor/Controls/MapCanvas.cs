using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class MapCanvas : Control
{
    private const int CanvasPadding = 24;
    private int _cellSize = 32;
    private MapAssetDocument _document = MapAssetDocument.CreateDefault();
    private char _selectedTile = '#';
    private bool _isPainting;
    private Point? _lastPaintedCell;
    private Point? _cursorCell;

    public event EventHandler? DocumentChanged;
    public event EventHandler? CursorCellChanged;
    public event EventHandler<char>? TilePicked;

    public MapCanvas()
    {
        Location = new Point(0, 0);
        BackColor = EditorTheme.Workspace;
        DoubleBuffered = true;
        Font = new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold);
        UpdateCanvasSize();
    }

    public MapAssetDocument Document => _document;

    public Point? CursorCell => _cursorCell;

    public EditorToolMode ToolMode { get; set; } = EditorToolMode.Paint;

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
        set => _selectedTile = value;
    }

    public void LoadDocument(MapAssetDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        UpdateCanvasSize();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshDocument()
    {
        UpdateCanvasSize();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(EditorTheme.Workspace);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        e.Graphics.TranslateTransform(CanvasPadding, CanvasPadding);

        for (var y = 0; y < _document.Height; y++)
        {
            for (var x = 0; x < _document.Width; x++)
            {
                DrawCell(e.Graphics, x, y, _document.GetTile(x, y));
            }
        }
    }

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

        if (ToolMode == EditorToolMode.Paint && _isPainting && e.Button == MouseButtons.Left)
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

        if (_cursorCell is not null)
        {
            _cursorCell = null;
            CursorCellChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void PaintAt(Point location)
    {
        if (!TryGetCell(location, out var cell) || _lastPaintedCell == cell)
        {
            return;
        }

        _document.SetTile(cell.X, cell.Y, _selectedTile);
        _lastPaintedCell = cell;
        Invalidate(new Rectangle(
            CanvasPadding + (cell.X * CellSize),
            CanvasPadding + (cell.Y * CellSize),
            CellSize + 1,
            CellSize + 1));
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FillAt(Point location)
    {
        if (!TryGetCell(location, out var cell))
        {
            return;
        }

        _document.FloodFill(cell.X, cell.Y, _selectedTile);
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PickTileAt(Point location)
    {
        if (TryGetCell(location, out var cell))
        {
            TilePicked?.Invoke(this, _document.GetTile(cell.X, cell.Y));
        }
    }

    private void UpdateCursorCell(Point location)
    {
        Point? nextCell = TryGetCell(location, out var cell) ? cell : null;

        if (_cursorCell == nextCell)
        {
            return;
        }

        _cursorCell = nextCell;
        CursorCellChanged?.Invoke(this, EventArgs.Empty);
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

    private void DrawCell(Graphics graphics, int x, int y, char tile)
    {
        var bounds = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);

        using var background = new SolidBrush(GetTileColor(tile));
        using var border = new Pen(EditorTheme.GridLine);
        using var tileFont = GetTileFont();
        using var textBrush = new SolidBrush(GetTileTextColor(tile));
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

    private static Color GetTileColor(char tile)
    {
        return tile switch
        {
            '#' => Color.FromArgb(30, 30, 30),
            '.' => Color.FromArgb(8, 8, 8),
            '@' => Color.FromArgb(18, 18, 18),
            '*' => Color.FromArgb(22, 22, 22),
            _ => EditorTheme.Section
        };
    }

    private static Color GetTileTextColor(char tile)
    {
        return tile switch
        {
            '#' => Color.FromArgb(230, 230, 230),
            '.' => Color.FromArgb(86, 86, 86),
            '@' => Color.FromArgb(255, 255, 255),
            '*' => Color.FromArgb(214, 214, 214),
            _ => EditorTheme.PrimaryText
        };
    }

    private void UpdateCanvasSize()
    {
        Size = new Size(
            (_document.Width * CellSize) + (CanvasPadding * 2),
            (_document.Height * CellSize) + (CanvasPadding * 2));
    }

    private Font GetTileFont()
    {
        var size = Math.Max(7, CellSize * 0.45f);
        return new Font(FontFamily.GenericMonospace, size, FontStyle.Bold);
    }
}
