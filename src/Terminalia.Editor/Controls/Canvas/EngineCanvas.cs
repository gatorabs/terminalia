using Terminalia.Core.Assets;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Cursors;
using Terminalia.Editor.Styling.Layout;
using Terminalia.Runtime.Windows;
using Terminalia.Runtime.Windows.GameHost;

namespace Terminalia.Editor.Controls.Canvas;

public sealed class EngineCanvas : Control
{
    private const int FramePadding = 24;

    private readonly WinFormsGameHost _host = new();
    private TileMap _document = TileMap.Empty;

    public event EventHandler? GameStopped;
    public event EventHandler<string>? TransitionFailed;

    public EngineCanvas()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.TerminalBackground;
        DoubleBuffered = true;
        TabStop = true;
        _host.GameStopped += OnHostGameStopped;
        _host.TransitionFailed += (_, msg) => TransitionFailed?.Invoke(this, msg);
    }

    public bool IsRunning => _host.IsRunning;

    public void LoadDocument(TileMap document, bool preserveRunState = false)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        if (preserveRunState && IsRunning)
        {
            _host.Reload(document);
        }
        else if (!preserveRunState && IsRunning)
        {
            StopGame();
        }
    }

    public void StartGame(string? assetDirectory = null)
    {
        var kernelSize = GameViewportDefaults.ComputeKernelSize(_document.CameraZoom);
        _host.CellSize = ComputeFittingCellSize(kernelSize);
        _host.Start(_document, Invalidate, assetDirectory);
        Cursor = EditorCursors.Blank;
        Focus();
        Invalidate();
    }

    public void StopGame()
    {
        _host.Stop();
        Cursor = Cursors.Default;
        Invalidate();
    }

    public bool TryHandleInput(Keys key)
    {
        if (!IsRunning)
        {
            return false;
        }
        if (key == Keys.Escape)
        {
            StopGame();
            return true;
        }
        _host.HandleKey(key);
        return true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(EditorColors.TerminalBackground);

        var frame = _host.GetFrame();
        if (frame is null)
        {
            return;
        }

        const int p = EditorLayout.CanvasPadding;
        var cx = Math.Max(p, (Width - frame.Width) / 2);
        var cy = Math.Max(p, (Height - frame.Height) / 2);
        e.Graphics.DrawImage(frame, cx, cy);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (IsRunning)
        {
            _host.ApplyZoom(ComputeFittingCellSize(_host.KernelSize));
        }
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);

    private int ComputeFittingCellSize(Size kernelSize)
    {
        var available = new Size(
            Math.Max(0, Width - 2 * FramePadding),
            Math.Max(0, Height - 2 * FramePadding));

        return GameViewportDefaults.ComputeCellSize(available, kernelSize);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _host.Dispose();
        }
        base.Dispose(disposing);
    }

    private void OnHostGameStopped(object? sender, EventArgs e)
    {
        Cursor = Cursors.Default;
        Invalidate();
        GameStopped?.Invoke(this, EventArgs.Empty);
    }
}
