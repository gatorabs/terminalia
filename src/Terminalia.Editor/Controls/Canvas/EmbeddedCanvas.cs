using Terminalia.Core.Assets;
using Terminalia.Editor.Styling.Colors;

namespace Terminalia.Editor.Controls.Canvas;

public sealed class EmbeddedCanvas : Control
{
    private const int CanvasPadding = 24;

    private TileMap _document = TileMap.Empty;
    private Point _playerPosition;
    private bool _hasPlayer;
    private int _cellSize = 32;

    public EmbeddedCanvas()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.TerminalBackground;
        DoubleBuffered = true;
        TabStop = true;
    }

    public bool IsRunning { get; private set; }

    public int CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = Math.Clamp(value, 16, 48);
            Invalidate();
        }
    }

    public void LoadDocument(TileMap document, bool preserveRunState = false)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        if (!preserveRunState ||
            !_hasPlayer ||
            !_document.Contains(_playerPosition.X, _playerPosition.Y) ||
            _document.GetTile(_playerPosition.X, _playerPosition.Y) == '#')
        {
            ResetGame();
            return;
        }

        Invalidate();
    }

    public void StartGame()
    {
        IsRunning = true;
        if (!_hasPlayer)
            ResetGame();

        Focus();
        Invalidate();
    }

    public void StopGame()
    {
        IsRunning = false;
        Invalidate();
    }

    public void ResetGame()
    {
        _hasPlayer = TryFindPlayerSpawn(out _playerPosition);
        Invalidate();
    }

    public bool TryHandleInput(Keys key)
    {
        if (!IsRunning || !_hasPlayer)
            return false;

        var delta = key switch
        {
            Keys.W or Keys.Up => new Point(0, -1),
            Keys.S or Keys.Down => new Point(0, 1),
            Keys.A or Keys.Left => new Point(-1, 0),
            Keys.D or Keys.Right => new Point(1, 0),
            Keys.Escape => StopAndHandle(),
            _ => Point.Empty
        };

        if (delta == Point.Empty)
            return key == Keys.Escape;

        var next = new Point(_playerPosition.X + delta.X, _playerPosition.Y + delta.Y);
        if (_document.Contains(next.X, next.Y) && _document.GetTile(next.X, next.Y) != '#')
        {
            _playerPosition = next;
            Invalidate();
        }

        return true;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible) Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(EditorColors.TerminalBackground);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var mapPixelWidth = (_document.Width * _cellSize) + (CanvasPadding * 2);
        var mapPixelHeight = (_document.Height * _cellSize) + (CanvasPadding * 2);
        var ox = Math.Max(CanvasPadding, (Width - mapPixelWidth) / 2) + CanvasPadding;
        var oy = Math.Max(CanvasPadding, (Height - mapPixelHeight) / 4) + CanvasPadding;

        e.Graphics.TranslateTransform(ox, oy);

        for (var y = 0; y < _document.Height; y++)
        {
            for (var x = 0; x < _document.Width; x++)
            {
                DrawCell(e.Graphics, x, y, GetVisibleTile(x, y));
            }
        }
    }

    private char GetVisibleTile(int x, int y)
    {
        if (_hasPlayer && _playerPosition.X == x && _playerPosition.Y == y)
            return '@';
        var tile = _document.GetTile(x, y);
        return tile == '@' ? '.' : tile;
    }

    private void DrawCell(Graphics graphics, int x, int y, char tile)
    {
        var bounds = new Rectangle(x * _cellSize, y * _cellSize, _cellSize, _cellSize);

        using var tileFont = GetTileFont();
        using var textBrush = new SolidBrush(GetTileColor(tile));
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoClip
        };

        graphics.DrawString(tile.ToString(), tileFont, textBrush, bounds, format);
    }

    private static Color GetTileColor(char tile) => tile switch
    {
        '#' => Color.FromArgb(148, 163, 184),
        '.' => Color.FromArgb(71, 85, 105),
        '@' => Color.FromArgb(96, 165, 250),
        '*' => Color.FromArgb(250, 204, 21),
        _ => EditorColors.PrimaryText
    };

    private Font GetTileFont()
    {
        var size = Math.Max(7, _cellSize * 0.45f);
        return new Font(FontFamily.GenericMonospace, size, FontStyle.Bold);
    }

    private bool TryFindPlayerSpawn(out Point playerPosition)
    {
        for (var y = 0; y < _document.Height; y++)
        {
            for (var x = 0; x < _document.Width; x++)
            {
                if (_document.GetTile(x, y) == '@')
                {
                    playerPosition = new Point(x, y);
                    return true;
                }
            }
        }

        playerPosition = default;
        return false;
    }

    private Point StopAndHandle()
    {
        StopGame();
        return Point.Empty;
    }
}
