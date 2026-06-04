using Terminalia.Core.Assets;
using Terminalia.Editor.GameHost;
using Terminalia.Editor.Styling.Colors;

namespace Terminalia.Editor.Controls.Canvas;

public sealed class EngineCanvas : Control
{
    private readonly EditorGameHost _host = new();
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
        _host.Start(_document, this);
        Focus();
        Invalidate();
    }

    public void StopGame()
    {
        _host.Stop();
        Invalidate();
    }

    public void ResetGame()
    {
        _host.Stop();
        _host.Start(_document, this);
        Focus();
        Invalidate();
    }

    public bool TryHandleInput(Keys key)
    {
        if (!IsRunning) return false;

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
        if (frame is null) return;

        var x = Math.Max(0, (Width - frame.Width) / 2);
        var y = Math.Max(0, (Height - frame.Height) / 4);
        e.Graphics.DrawImage(frame, x, y);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _host.Dispose();
        base.Dispose(disposing);
    }

    private void OnHostGameStopped(object? sender, EventArgs e)
    {
        Invalidate();
        GameStopped?.Invoke(this, EventArgs.Empty);
    }
}
