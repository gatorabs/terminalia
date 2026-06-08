using System.Runtime.InteropServices;
using Terminalia.Core.Assets;
using Terminalia.Runtime.Windows.GameHost;

namespace Terminalia.Runtime.Windows;

public sealed class GameWindow : Form
{
    private const int GamePadding = GameViewportDefaults.GamePadding;
    private const int HintBarHeight = GameViewportDefaults.HintBarHeight;

    private readonly WinFormsGameHost _host;
    private readonly TileMap _map;
    private readonly Size _kernelSize;
    private readonly Font _hintFont = new(FontFamily.GenericMonospace, 8, FontStyle.Regular);

    private readonly string? _assetDirectory;

    public GameWindow(TileMap map, string? assetDirectory = null)
    {
        _map = map;
        _assetDirectory = assetDirectory;
        _kernelSize = GameViewportDefaults.ComputeKernelSize(map.CameraZoom);

        var defaultCellSize = GameViewportDefaults.ComputeCellSize(
            GameViewportDefaults.ComputeAvailableArea(GameViewportDefaults.WindowClientSize), _kernelSize);
        _host = new WinFormsGameHost { CellSize = defaultCellSize };

        Text = map.Name;
        BackColor = Color.Black;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        MinimumSize = ComputeMinimumSize();
        ClientSize = GameViewportDefaults.WindowClientSize;

        _host.GameStopped += (_, _) =>
        {
            if (!IsDisposed)
            {
                BeginInvoke(Close);
            }
        };

        _host.TransitionFailed += (_, msg) =>
        {
            if (!IsDisposed)
            {
                BeginInvoke(() => MessageBox.Show(this, msg, "Zone Transition", MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
        };

        TryUseDarkTitleBar();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _host.Start(_map, () =>
        {
            if (!IsDisposed)
            {
                Invalidate();
            }
        }, _assetDirectory);
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);
        _host.ApplyZoom(ComputeFittingCellSize());
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.Black);

        var frame = _host.GetFrame();
        if (frame is not null)
        {
            var available = GameViewportDefaults.ComputeAvailableArea(ClientSize);
            var ox = GamePadding + Math.Max(0, (available.Width - frame.Width) / 2);
            var oy = GamePadding + Math.Max(0, (available.Height - frame.Height) / 2);
            e.Graphics.DrawImage(frame, ox, oy);
        }

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
        if (e.KeyCode == Keys.Escape)
        {
            Close();
            return;
        }
        if (_host.IsRunning)
        {
            _host.HandleKey(e.KeyCode);
        }
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
        if (disposing)
        {
            _host.Dispose();
            _hintFont.Dispose();
        }
        base.Dispose(disposing);
    }

    private int ComputeFittingCellSize()
    {
        return GameViewportDefaults.ComputeCellSize(GameViewportDefaults.ComputeAvailableArea(ClientSize), _kernelSize);
    }

    private Size ComputeMinimumSize()
    {
        const int minCellSize = 4;
        return new Size(
            _kernelSize.Width * minCellSize + GamePadding * 2,
            _kernelSize.Height * minCellSize + GamePadding * 2 + HintBarHeight);
    }

    private void TryUseDarkTitleBar()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }
        var value = 1;
        _ = DwmSetWindowAttribute(Handle, 20, ref value, sizeof(int));
        _ = DwmSetWindowAttribute(Handle, 19, ref value, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int size);
}
