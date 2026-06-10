using Terminalia.Assets.Json;
using Terminalia.Core.Abstractions.Input;
using Terminalia.Core.Abstractions.Runtime;
using Terminalia.Core.Assets;
using Terminalia.Core.Runtime;
using Terminalia.Runtime.Windows;
using Terminalia.TileMapGame;
using Timer = System.Windows.Forms.Timer;

namespace Terminalia.Runtime.Windows.GameHost;

public sealed class WinFormsGameHost : IDisposable
{
    private readonly WinFormsRenderer _renderer = new();
    private readonly WinFormsInputProvider _inputProvider = new();
    private readonly WinFormsGameClock _clock = new();
    private readonly Timer _timer;
    private IGame? _game;
    private TileMap? _startMap;
    private Action? _onFrameReady;
    private string? _assetDirectory;
    private string? _pendingTransition;
    private Size _kernelSize = GameViewportDefaults.ComputeKernelSize(TileMap.DefaultCameraZoom);

    public event EventHandler? GameStopped;
    public event EventHandler<string>? TransitionFailed;

    public bool IsRunning => _game?.IsRunning == true;

    public int CellSize { get; set; } = 32;

    public Size KernelSize => _kernelSize;

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
        _game = CreateGameplay(map, _inputProvider);
        _game.Initialize();
        _timer.Start();
    }

    public void Start(GameDefinition definition, TileMap startMap, Action onFrameReady, string? assetDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(startMap);

        _onFrameReady = onFrameReady;
        _assetDirectory = assetDirectory;
        _pendingTransition = null;
        _startMap = startMap;
        ConfigureRendererFor(startMap);
        var flow = new GameFlowSession(definition, _renderer, _inputProvider, input => CreateGameplay(_startMap!, input));
        flow.Initialize();
        _game = flow;
        _timer.Start();
    }

    public void ApplyZoom(int cellSize)
    {
        if (cellSize <= 0 || cellSize == CellSize || _game is null)
        {
            return;
        }

        CellSize = cellSize;
        _renderer.Configure(_kernelSize.Width, _kernelSize.Height, cellSize);
        _onFrameReady?.Invoke();
    }

    public void Stop()
    {
        _timer.Stop();
        _game = null;
        GameStopped?.Invoke(this, EventArgs.Empty);
    }

    public void Reload(TileMap map)
    {
        if (_game is GameFlowSession flow && flow.IsRunning)
        {
            _startMap = map;
            if (flow.State == GameFlowState.Playing)
            {
                flow.ReplaceGameplay(CreateGameplay(map, flow.GameplayInput));
            }
            return;
        }

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

    private TileMapGameSession CreateGameplay(TileMap map, IInputProvider input)
    {
        ConfigureRendererFor(map);
        var scene = SceneAssembler.Assemble(map, _renderer, t => _pendingTransition = t);
        return new TileMapGameSession(scene, _renderer, input);
    }

    private void ConfigureRendererFor(TileMap map)
    {
        _kernelSize = GameViewportDefaults.ComputeKernelSize(map.CameraZoom);
        _renderer.Configure(_kernelSize.Width, _kernelSize.Height, CellSize);
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

        if (_game is GameFlowSession flow)
        {
            flow.ReplaceGameplay(CreateGameplay(nextMap, flow.GameplayInput));
            _onFrameReady();
            return;
        }

        _timer.Stop();
        _game = null;
        Start(nextMap, _onFrameReady, _assetDirectory);
        _onFrameReady();
    }
}
