using Terminalia.Core.Assets;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Layout;
using Terminalia.Runtime.Windows.GameHost;

namespace Terminalia.Editor.Controls.Canvas;

public sealed class EngineCanvas : Control
{
    private readonly WinFormsGameHost _host = new();
    private TileMap _document = TileMap.Empty;

    public event EventHandler? GameStopped;

    public EngineCanvas()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.TerminalBackground;
        DoubleBuffered = true;
        TabStop = true;
        _host.GameStopped += OnHostGameStopped;
    }

    public bool IsRunning => _host.IsRunning;

    public int CellSize
    {
        get => _host.CellSize;
        set => _host.CellSize = value;
    }

    public void LoadDocument(TileMap document, bool preserveRunState = false)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        if (preserveRunState && IsRunning)
            _host.Reload(document);
        else if (!preserveRunState && IsRunning)
            StopGame();
    }

    public void StartGame()
    {
        _host.Start(_document, Invalidate);
        Focus();
        Invalidate();
    }

    public void StopGame()
    {
        _host.Stop();
        Invalidate();
    }

    public bool TryHandleInput(Keys key)
    {
        if (!IsRunning) return false;
        if (key == Keys.Escape) { StopGame(); return true; }
        _host.HandleKey(key);
        return true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(EditorColors.TerminalBackground);

        var frame = _host.GetFrame();
        if (frame is null) return;

        // Mirror EditorShell.CenterCanvas + MapCanvas.CanvasPadding (24) so tiles
        // land on the exact same pixels as the editor canvas when Play is pressed.
        const int p = EditorLayout.CanvasPadding;   // min margin = 16
        const int tp = 24;                           // MapCanvas.CanvasPadding
        var canvasW = frame.Width + 2 * tp;
        var canvasH = frame.Height + 2 * tp;
        var cx = Math.Max(p, (Width - canvasW) / 2);
        var cy = Math.Max(p, (Height - canvasH) / 4);
        e.Graphics.DrawImage(frame, cx + tp, cy + tp);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override void Dispose(bool disposing)
    {
        if (disposing) _host.Dispose();
        base.Dispose(disposing);
    }

    private void OnHostGameStopped(object? sender, EventArgs e)
    {
        Invalidate();
        GameStopped?.Invoke(this, EventArgs.Empty);
    }
}
