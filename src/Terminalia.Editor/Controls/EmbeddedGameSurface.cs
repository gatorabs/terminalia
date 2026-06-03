using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class EmbeddedGameSurface : Control
{
    private const int ChromeMargin = 18;
    private const int HeaderHeight = 42;
    private const int MapTopOffset = 66;   // HeaderHeight + 24px gap below header
    private const int HudHeight = 104;
    private const int HudBottomMargin = 16;
    private const int HudInset = 18;
    private const int MapReservedVert = MapTopOffset + HudHeight; // vertical space reserved for header+hud
    private const int KerningAdjust = -3;  // monospace glyph spacing correction

    private readonly Font _terminalFont = new(FontFamily.GenericMonospace, 14, FontStyle.Bold);
    private readonly Font _hudFont = new(FontFamily.GenericMonospace, 9, FontStyle.Regular);
    private readonly Font _titleFont = new(FontFamily.GenericMonospace, 11, FontStyle.Bold);
    private MapAssetDocument _document = MapAssetDocument.CreateDefault();
    private Point _playerPosition;
    private bool _hasPlayer;

    public EmbeddedGameSurface()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;
        DoubleBuffered = true;
        Padding = new Padding(20);
        TabStop = true;
    }

    public bool IsRunning { get; private set; }

    public void LoadDocument(MapAssetDocument document, bool preserveRunState = false)
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
        {
            ResetGame();
        }

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
        {
            return false;
        }

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
        {
            return key == Keys.Escape;
        }

        var next = new Point(_playerPosition.X + delta.X, _playerPosition.Y + delta.Y);
        if (_document.Contains(next.X, next.Y) && _document.GetTile(next.X, next.Y) != '#')
        {
            _playerPosition = next;
            Invalidate();
        }

        return true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(EditorTheme.AppBackground);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var chrome = new Rectangle(ChromeMargin, ChromeMargin, Width - ChromeMargin * 2, Height - ChromeMargin * 2);
        if (chrome.Width <= 0 || chrome.Height <= 0)
        {
            return;
        }

        using var panelBrush = new SolidBrush(EditorTheme.TerminalBackground);
        using var border = new Pen(EditorTheme.Border);
        using var chromePath = EditorTheme.CreateRoundedPath(chrome, 10);
        e.Graphics.FillPath(panelBrush, chromePath);
        e.Graphics.DrawPath(border, chromePath);

        DrawHeader(e.Graphics, chrome);
        DrawMap(e.Graphics, chrome);
        DrawHud(e.Graphics, chrome);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _terminalFont.Dispose();
            _hudFont.Dispose();
            _titleFont.Dispose();
        }

        base.Dispose(disposing);
    }

    private void DrawHeader(Graphics graphics, Rectangle chrome)
    {
        var header = new Rectangle(chrome.X, chrome.Y, chrome.Width, HeaderHeight);
        using var headerBrush = new SolidBrush(EditorTheme.TerminalHeader);
        graphics.FillRectangle(headerBrush, header);

        TextRenderer.DrawText(
            graphics,
            "Terminalia Runtime",
            _titleFont,
            new Rectangle(header.X + 16, header.Y, header.Width - 32, header.Height),
            EditorTheme.PrimaryText,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    private void DrawMap(Graphics graphics, Rectangle chrome)
    {
        var origin = new Point(chrome.X + 22, chrome.Y + MapTopOffset);
        var lineHeight = TextRenderer.MeasureText("M", _terminalFont).Height + 2;
        var visibleRows = Math.Max(1, (chrome.Height - MapReservedVert) / lineHeight);
        var rows = Math.Min(_document.Height, visibleRows);

        for (var y = 0; y < rows; y++)
        {
            var xOffset = origin.X;
            for (var x = 0; x < _document.Width; x++)
            {
                var tile = _document.GetTile(x, y);
                var visibleTile = _hasPlayer && _playerPosition.X == x && _playerPosition.Y == y
                    ? '@'
                    : tile == '@' ? '.' : tile;
                var textSize = TextRenderer.MeasureText(visibleTile.ToString(), _terminalFont);
                TextRenderer.DrawText(
                    graphics,
                    visibleTile.ToString(),
                    _terminalFont,
                    new Point(xOffset, origin.Y + (y * lineHeight)),
                    GetTerminalTileColor(visibleTile));
                xOffset += textSize.Width + KerningAdjust;

                if (xOffset > chrome.Right - HudInset - 6)
                {
                    break;
                }
            }
        }
    }

    private void DrawHud(Graphics graphics, Rectangle chrome)
    {
        var hudTop = Math.Max(chrome.Bottom - HudHeight, chrome.Y + MapTopOffset + HudHeight / 4);
        var hud = new Rectangle(chrome.X + HudInset, hudTop, chrome.Width - HudInset * 2, chrome.Bottom - hudTop - HudBottomMargin);
        using var hudBrush = new SolidBrush(EditorTheme.Section);
        using var border = new Pen(EditorTheme.Border);
        using var hudPath = EditorTheme.CreateRoundedPath(hud, 8);
        graphics.FillPath(hudBrush, hudPath);
        graphics.DrawPath(border, hudPath);

        var lines = new[]
        {
            $"Map: {_document.Name}",
            $"Size: {_document.Width}x{_document.Height}",
            IsRunning ? "Running in editor" : "Paused",
            _hasPlayer ? "Move: WASD or arrows" : "Add a player spawn (@)"
        };

        for (var i = 0; i < lines.Length; i++)
        {
            TextRenderer.DrawText(
                graphics,
                lines[i],
                _hudFont,
                new Rectangle(hud.X + 12, hud.Y + 10 + (i * 20), hud.Width - 24, 20),
                i == 0 ? EditorTheme.PrimaryText : EditorTheme.SecondaryText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
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

    private static Color GetTerminalTileColor(char tile)
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
}
