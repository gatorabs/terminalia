using Terminalia.Core.Assets;
using Terminalia.Runtime;
using Timer = System.Windows.Forms.Timer;

namespace Terminalia.Editor.GameHost;

internal sealed class EditorGameHost : IDisposable
{
    private readonly WinFormsRenderer _renderer = new();
    private readonly WinFormsInputProvider _inputProvider = new();
    private readonly WinFormsGameClock _clock = new();
    private readonly Timer _timer;
    private TileMapGame? _game;
    private Control? _surface;

    public event EventHandler? GameStopped;

    public bool IsRunning => _game?.IsRunning == true;

    public int CellSize { get; set; } = 32;

    public EditorGameHost()
    {
        _timer = new Timer { Interval = 1000 / 30 };
        _timer.Tick += OnTick;
    }

    public void Start(TileMap map, Control surface)
    {
        _surface = surface;
        _renderer.Configure(map.Width, map.Height, CellSize);
        var scene = SceneAssembler.Assemble(map, _renderer);
        _game = new TileMapGame(scene, _renderer, _inputProvider);
        _game.Initialize();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _game = null;
        GameStopped?.Invoke(this, EventArgs.Empty);
    }

    public void Reload(TileMap map)
    {
        var wasRunning = IsRunning;
        _timer.Stop();
        _game = null;
        if (wasRunning && _surface is not null)
            Start(map, _surface);
    }

    public void HandleKey(Keys key) => _inputProvider.HandleKey(key);

    public Bitmap? GetFrame() => _renderer.BackBuffer;

    public void Dispose()
    {
        _timer.Dispose();
        _renderer.Dispose();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_game is null || !_game.IsRunning)
        {
            Stop();
            return;
        }

        var time = _clock.GetGameTime();
        _game.HandleInput();
        _game.Update(time);
        _game.Render();
        _surface?.Invalidate();
    }
}
