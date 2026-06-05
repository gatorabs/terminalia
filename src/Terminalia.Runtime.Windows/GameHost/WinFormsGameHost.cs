using Terminalia.Assets.Json;
using Terminalia.Core.Assets;
using Terminalia.TileMapGame;
using Timer = System.Windows.Forms.Timer;

namespace Terminalia.Runtime.Windows.GameHost;

public sealed class WinFormsGameHost : IDisposable
{
    private readonly WinFormsRenderer _renderer = new();
    private readonly WinFormsInputProvider _inputProvider = new();
    private readonly WinFormsGameClock _clock = new();
    private readonly Timer _timer;
    private TileMapGameSession? _game;
    private Action? _onFrameReady;
    private string? _assetDirectory;
    private string? _pendingTransition;

    public event EventHandler? GameStopped;

    public bool IsRunning => _game?.IsRunning == true;

    public int CellSize { get; set; } = 32;

    public WinFormsGameHost()
    {
        _timer = new Timer { Interval = 1000 / 30 };
        _timer.Tick += OnTick;
    }

    public void Start(TileMap map, Action onFrameReady, string? assetDirectory = null)
    {
        _onFrameReady = onFrameReady;
        _assetDirectory = assetDirectory;
        _pendingTransition = null;
        _renderer.Configure(map.Width, map.Height, CellSize);
        var scene = SceneAssembler.Assemble(map, _renderer, t => _pendingTransition = t);
        _game = new TileMapGameSession(scene, _renderer, _inputProvider);
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
        if (wasRunning && _onFrameReady is not null)
        {
            Start(map, _onFrameReady, _assetDirectory);
        }
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

        if (_pendingTransition is not null)
        {
            var target = _pendingTransition;
            _pendingTransition = null;
            TransitionToMap(target);
            return;
        }

        _onFrameReady?.Invoke();
    }

    private void TransitionToMap(string targetMap)
    {
        if (_assetDirectory is null || _onFrameReady is null)
        {
            return;
        }

        var path = Path.Combine(_assetDirectory, targetMap + ".json");
        if (!File.Exists(path))
        {
            return;
        }

        var nextMap = MapAssetDocument.Load(path).ToTileMap();
        _timer.Stop();
        _game = null;
        Start(nextMap, _onFrameReady, _assetDirectory);
        _onFrameReady();
    }
}
