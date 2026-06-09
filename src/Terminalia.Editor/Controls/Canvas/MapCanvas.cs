using Terminalia.Assets.Json;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Cursors;
using Terminalia.Runtime.Windows;

namespace Terminalia.Editor.Controls.Canvas;

public sealed class MapCanvas : Control
{
    private static readonly Color[] ZonePalette =
    [
        Color.FromArgb(60, 255, 100, 100),
        Color.FromArgb(60, 100, 255, 100),
        Color.FromArgb(60, 100, 100, 255),
        Color.FromArgb(60, 255, 255, 100),
        Color.FromArgb(60, 255, 100, 255),
        Color.FromArgb(60, 100, 255, 255)
    ];

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

    public MapCanvas()
    {
        Location = new Point(0, 0);
        BackColor = EditorColors.Workspace;
        DoubleBuffered = true;
        Font = new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold);
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

                var colorIndex = Math.Abs(zoneName.GetHashCode()) % ZonePalette.Length;
                var bounds = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
                using var brush = new SolidBrush(ZonePalette[colorIndex]);
                graphics.FillRectangle(brush, bounds);
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

    public event EventHandler<string?>? ZonePicked;

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

        _document.FloodFill(cell.X, cell.Y, _selectedTile);
        UpdateKernelPreview();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
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

        Cursor = EditorCursors.ForTile(_selectedTile, GetTileTextColor(_selectedTile));
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
            return Color.FromArgb(255, 80, 140, 255);
        }

        if (ToolMode == EditorToolMode.Select)
        {
            return Color.FromArgb(255, 255, 196, 64);
        }

        if (ToolMode == EditorToolMode.Zone && _selectedZone is not null)
        {
            var idx = Math.Abs(_selectedZone.GetHashCode()) % ZonePalette.Length;
            var zc = ZonePalette[idx];
            return Color.FromArgb(255, zc.R, zc.G, zc.B);
        }

        return Color.White;
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

    private void ClearSelection()
    {
        SetSelection(null);
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

    private Rectangle ComputeKernelBounds(Point focus)
    {
        var kernelSize = GameViewportDefaults.ComputeKernelSize(_document.CameraZoom);
        var originX = ClampOrigin(focus.X - kernelSize.Width / 2, _document.Width - kernelSize.Width);
        var originY = ClampOrigin(focus.Y - kernelSize.Height / 2, _document.Height - kernelSize.Height);
        return new Rectangle(originX, originY, kernelSize.Width, kernelSize.Height);
    }

    private static int ClampOrigin(int value, int max) => Math.Clamp(value, 0, Math.Max(0, max));

    private void DrawSelection(Graphics g, Point cell)
    {
        var bounds = new Rectangle(cell.X * CellSize + 1, cell.Y * CellSize + 1, CellSize - 2, CellSize - 2);
        using var pen = new Pen(Color.FromArgb(255, 255, 196, 64), 2.5f);
        g.DrawRectangle(pen, bounds);
    }

    private void DrawKernelPreview(Graphics g, Rectangle kernel)
    {
        var bounds = new Rectangle(
            kernel.X * CellSize,
            kernel.Y * CellSize,
            kernel.Width * CellSize,
            kernel.Height * CellSize);

        using var fill = new SolidBrush(Color.FromArgb(26, 255, 196, 64));
        using var border = new Pen(Color.FromArgb(220, 255, 196, 64), 2f);
        g.FillRectangle(fill, bounds);
        g.DrawRectangle(border, bounds);
    }

    private Rectangle GetCellInvalidRect(Point cell) =>
        new Rectangle(CanvasPadding + cell.X * CellSize - 1, CanvasPadding + cell.Y * CellSize - 1, CellSize + 2, CellSize + 2);

    private void DrawCell(Graphics graphics, int x, int y, char tile)
    {
        var bounds = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);

        using var background = new SolidBrush(GetTileColor(tile));
        using var border = new Pen(EditorColors.GridLine);
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

    private static Color GetTileColor(char tile) => tile switch
    {
        '#' => Color.FromArgb(30, 30, 30),
        '.' => Color.FromArgb(8, 8, 8),
        '@' => Color.FromArgb(18, 18, 18),
        '*' => Color.FromArgb(22, 22, 22),
        _ => EditorColors.Section
    };

    private static Color GetTileTextColor(char tile) => tile switch
    {
        '#' => Color.FromArgb(230, 230, 230),
        '.' => Color.FromArgb(86, 86, 86),
        '@' => Color.FromArgb(255, 255, 255),
        '*' => Color.FromArgb(214, 214, 214),
        _ => EditorColors.PrimaryText
    };

    private void UpdateCanvasSize()
    {
        Size = new Size((_document.Width * CellSize) + (CanvasPadding * 2), (_document.Height * CellSize) + (CanvasPadding * 2));
    }

    private Font GetTileFont()
    {
        var size = Math.Max(7, CellSize * 0.45f);
        return new Font(FontFamily.GenericMonospace, size, FontStyle.Bold);
    }
}
