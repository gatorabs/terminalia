using Terminalia.Assets.Json;

namespace Terminalia.Sample.Rpg;

public sealed class SampleGameSurface : Control
{
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly WinFormsInputProvider _inputProvider = new();
    private readonly WinFormsRenderer _renderer = new();
    private readonly WindowsFormsClock _clock = new();
    private readonly RpgGame _game;
    private readonly string _mapName;
    private readonly int _mapWidth;
    private readonly int _mapHeight;

    public SampleGameSurface()
    {
        BackColor = SampleTheme.AppBackground;
        DoubleBuffered = true;
        TabStop = true;

        var assetLoader = new JsonAssetLoader(Path.Combine(AppContext.BaseDirectory, "assets"));
        var tilemap = assetLoader.LoadMap("first_dungeon");
        _mapName = tilemap.Name;
        _mapWidth = tilemap.Width;
        _mapHeight = tilemap.Height;
        _game = new RpgGame(_inputProvider, _renderer, tilemap);
        _game.Initialize();

        _timer.Interval = 33;
        _timer.Tick += (_, _) => TickGame();
        _timer.Start();
    }

    public void HandleKey(Keys key) => _inputProvider.HandleKey(key);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(SampleTheme.AppBackground);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var terminal = new Rectangle(28, 28, Width - 56, Height - 56);
        if (terminal.Width <= 0 || terminal.Height <= 0)
            return;

        using var background = new SolidBrush(SampleTheme.TerminalBackground);
        using var border = new Pen(SampleTheme.Border);
        e.Graphics.FillRectangle(background, terminal);
        e.Graphics.DrawRectangle(border, terminal);

        DrawHeader(e.Graphics, terminal);
        _renderer.SetContext(e.Graphics, new Point(terminal.X + 30, terminal.Y + 76), _mapWidth, _mapHeight);
        _game.Render();
        DrawHud(e.Graphics, terminal);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _timer.Dispose();
        base.Dispose(disposing);
    }

    private void TickGame()
    {
        var time = _clock.GetGameTime();
        _game.HandleInput();
        _game.Update(time);
        if (!_game.IsRunning)
        {
            FindForm()?.Close();
            return;
        }
        Invalidate();
    }

    private static void DrawHeader(Graphics graphics, Rectangle terminal)
    {
        var header = new Rectangle(terminal.X, terminal.Y, terminal.Width, 46);
        using var headerBrush = new SolidBrush(SampleTheme.TerminalHeader);
        graphics.FillRectangle(headerBrush, header);
        TextRenderer.DrawText(
            graphics,
            "Terminalia Sample RPG",
            SampleTheme.TitleFont,
            new Rectangle(header.X + 18, header.Y, header.Width - 36, header.Height),
            SampleTheme.PrimaryText,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    private void DrawHud(Graphics graphics, Rectangle terminal)
    {
        var hud = new Rectangle(terminal.Right - 260, terminal.Bottom - 112, 230, 84);
        using var hudBrush = new SolidBrush(Color.FromArgb(24, 33, 42));
        using var hudBorder = new Pen(Color.FromArgb(53, 65, 80));
        graphics.FillRectangle(hudBrush, hud);
        graphics.DrawRectangle(hudBorder, hud);

        var lines = new[]
        {
            $"Map: {_mapName}",
            $"Size: {_mapWidth}x{_mapHeight}",
            "Move: WASD or arrows",
            "Exit: ESC"
        };

        for (var i = 0; i < lines.Length; i++)
        {
            TextRenderer.DrawText(
                graphics,
                lines[i],
                SampleTheme.HudFont,
                new Rectangle(hud.X + 12, hud.Y + 10 + (i * 18), hud.Width - 24, 18),
                i == 0 ? Color.FromArgb(147, 197, 253) : Color.FromArgb(203, 213, 225),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }
}
