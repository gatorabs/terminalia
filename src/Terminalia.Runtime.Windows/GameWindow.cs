using System.Runtime.InteropServices;
using Terminalia.Core.Assets;
using Terminalia.Runtime.Windows.GameHost;

namespace Terminalia.Runtime.Windows;

public sealed class GameWindow : Form
{
    private const int GamePadding = 16;
    private const int HintBarHeight = 22;

    private readonly WinFormsGameHost _host;
    private readonly TileMap _map;
    private readonly Font _hintFont = new(FontFamily.GenericMonospace, 8, FontStyle.Regular);

    public GameWindow(TileMap map, int cellSize = 24)
    {
        _map = map;
        _host = new WinFormsGameHost { CellSize = cellSize };

        Text = map.Name;
        BackColor = Color.Black;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        ClientSize = new Size(
            map.Width * cellSize + GamePadding * 2,
            map.Height * cellSize + GamePadding * 2 + HintBarHeight);

        _host.GameStopped += (_, _) => { if (!IsDisposed) BeginInvoke(Close); };

        TryUseDarkTitleBar();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _host.Start(_map, () => { if (!IsDisposed) Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.Black);

        var frame = _host.GetFrame();
        if (frame is not null)
            e.Graphics.DrawImage(frame, GamePadding, GamePadding);

        TextRenderer.DrawText(
            e.Graphics,
            "[ESC] exit",
            _hintFont,
            new Point(GamePadding, ClientSize.Height - HintBarHeight + 4),
            Color.FromArgb(50, 255, 255, 255));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape) { Close(); return; }
        if (_host.IsRunning) _host.HandleKey(e.KeyCode);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _host.Stop();
        base.OnFormClosed(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _host.Dispose(); _hintFont.Dispose(); }
        base.Dispose(disposing);
    }

    private void TryUseDarkTitleBar()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
        var value = 1;
        _ = DwmSetWindowAttribute(Handle, 20, ref value, sizeof(int));
        _ = DwmSetWindowAttribute(Handle, 19, ref value, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int size);
}
