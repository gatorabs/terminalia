using Terminalia.Core.Abstractions.Input;
using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Abstractions.Runtime;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Input;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Runtime;

public sealed class TileMapGame : IGame
{
    private readonly Scene _scene;
    private readonly IRenderer _renderer;
    private readonly IInputProvider _inputProvider;
    private readonly Entity? _player;

    public bool IsRunning { get; private set; } = true;

    public TileMapGame(Scene scene, IRenderer renderer, IInputProvider inputProvider)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _inputProvider = inputProvider ?? throw new ArgumentNullException(nameof(inputProvider));
        _player = scene.FindEntity("Player");
    }

    public void Initialize() { }

    public void HandleInput()
    {
        var commands = _inputProvider.GetCommands();

        if (commands.Contains(InputCommand.Exit))
        {
            Stop();
            return;
        }

        if (_player is null)
            return;

        var velocity = commands.LastOrDefault(c => c is
            InputCommand.MoveUp or InputCommand.MoveDown or
            InputCommand.MoveLeft or InputCommand.MoveRight) switch
        {
            InputCommand.MoveUp    => new Velocity(0, -1),
            InputCommand.MoveDown  => new Velocity(0, 1),
            InputCommand.MoveLeft  => new Velocity(-1, 0),
            InputCommand.MoveRight => new Velocity(1, 0),
            _                      => Velocity.Zero
        };

        var velocityComponent = _player.GetComponent<VelocityComponent>();
        if (velocityComponent is not null)
            velocityComponent.Velocity = velocity;
    }

    public void Update(GameTime time)
    {
        _renderer.Clear();
        _scene.Update(time);
    }

    public void Render()
    {
        _renderer.Present();
    }

    public void Stop() => IsRunning = false;
}
