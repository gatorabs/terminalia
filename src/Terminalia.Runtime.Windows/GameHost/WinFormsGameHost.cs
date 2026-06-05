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
    private int _viewportWidth;
    private int _viewportHeight;

    public event EventHandler? GameStopped;
    public event EventHandler<string>? TransitionFailed;

    public bool IsRunning => _game?.IsRunning == true;

    public int CellSize { get; set; } = 32;

    public WinFormsGameHost()
    {
        _timer = new Timer { Interval = 1000 / 30 };
        _timer.Tick += OnTick;
    }

    public void Start(TileMap map, Action onFrameReady, string? assetDirectory = null, int viewportWidth = 0, int viewportHeight = 0)
    {
        _onFrameReady = onFrameReady;
        _assetDirectory = assetDirectory;
        _pendingTransition = null;
        _viewportWidth = viewportWidth > 0 ? viewportWidth : map.Width;
        _viewportHeight = viewportHeight > 0 ? viewportHeight : map.Height;
        _renderer.Configure(_viewportWidth, _viewportHeight, CellSize);
        var scene = SceneAssembler.Assemble(map, _renderer, t => _pendingTransition = t, null, _viewportWidth, _viewportHeight);
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
            Start(map, _onFrameReady, _assetDirectory, _viewportWidth, _viewportHeight);
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
            TransitionFailed?.Invoke(this, $"Cannot transition to '{targetMap}': map not saved. Save before running with zone transitions.");
            return;
        }

        var path = Path.Combine(_assetDirectory, targetMap + ".json");
        if (!File.Exists(path))
        {
            TransitionFailed?.Invoke(this, $"Target map file not found: {path}");
            return;
        }

        var nextMap = MapAssetDocument.Load(path).ToTileMap();
        _timer.Stop();
        _game = null;
        Start(nextMap, _onFrameReady, _assetDirectory, _viewportWidth, _viewportHeight);
        _onFrameReady();
    }
}
