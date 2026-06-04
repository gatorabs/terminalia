using Terminalia.Core.Assets;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Input;
using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Abstractions.Runtime;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;
using TileSize = Terminalia.Core.ValueObjects.Size;

namespace Terminalia.Sample.Rpg;

internal sealed class RpgGame : IGame
{
    private readonly WinFormsInputProvider _inputProvider;
    private readonly IRenderer _renderer;
    private readonly CollisionSystem _collisionSystem = new();
    private readonly TileMap _map;
    private readonly Scene _scene;
    private readonly Entity _player;

    public bool IsRunning { get; private set; } = true;

    public RpgGame(WinFormsInputProvider inputProvider, IRenderer renderer, TileMap map)
    {
        _inputProvider = inputProvider ?? throw new ArgumentNullException(nameof(inputProvider));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _scene = CreateSceneFromMap(map);
        _player = _scene.FindEntity("Player")!;
    }

    public void Initialize() { }

    public void HandleInput()
    {
        var generic = _inputProvider.GetCommands();
        if (generic.Contains(InputCommand.Exit))
        {
            Stop();
            return;
        }

        ApplyMovementCommands(_inputProvider.GetRpgCommands());
    }

    public void Update(GameTime time)
    {
        _collisionSystem.Update(_scene, time);
    }

    public void Render()
    {
        for (var y = 0; y < _map.Height; y++)
        {
            for (var x = 0; x < _map.Width; x++)
            {
                _renderer.Draw(x, y, GetVisibleTile(x, y));
            }
        }
    }

    public void Stop()
    {
        IsRunning = false;
    }

    private void ApplyMovementCommands(IReadOnlyCollection<RpgCommand> commands)
    {
        var velocity = commands.LastOrDefault(c => c is
            RpgCommand.MoveUp or RpgCommand.MoveDown or
            RpgCommand.MoveLeft or RpgCommand.MoveRight) switch
        {
            RpgCommand.MoveUp => new Velocity(0, -1),
            RpgCommand.MoveDown => new Velocity(0, 1),
            RpgCommand.MoveLeft => new Velocity(-1, 0),
            RpgCommand.MoveRight => new Velocity(1, 0),
            _ => Velocity.Zero
        };

        _player.GetComponent<VelocityComponent>()!.Velocity = velocity;
    }

    private char GetVisibleTile(int x, int y)
    {
        var playerTransform = _player.GetComponent<TransformComponent>()!;
        if (playerTransform.Position.X == x && playerTransform.Position.Y == y)
            return '@';

        var entity = _scene.GetActiveEntities()
            .Where(item => item.Name != "Player")
            .Select(item => new
            {
                Transform = item.GetComponent<TransformComponent>(),
                Render = item.GetComponent<RenderComponent>()
            })
            .Where(item => item.Transform?.Position.X == x && item.Transform.Position.Y == y && item.Render is not null)
            .OrderBy(item => item.Render!.Layer)
            .LastOrDefault();

        return entity?.Render?.Character ?? (_map.Tiles[y][x] == '#' ? '#' : '.');
    }

    private static Scene CreateSceneFromMap(TileMap map)
    {
        var scene = new Scene(map.Name);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.Tiles[y][x];
                var position = new Position(x, y);

                switch (tile)
                {
                    case '#':
                        scene.AddEntity(CreateStaticEntity("Stone Wall", position, '#', layer: 1, solid: true));
                        break;
                    case '@':
                        scene.AddEntity(CreateEntity("Player", position, '@', layer: 2, Velocity.Zero, solid: true));
                        break;
                    case '*':
                        scene.AddEntity(CreateStaticEntity("Relic", position, '*', layer: 1, solid: false));
                        break;
                }
            }
        }

        return scene;
    }

    private static Entity CreateStaticEntity(string name, Position position, char character, int layer, bool solid)
    {
        var entity = new Entity(name);
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent(character, layer));
        entity.AddComponent(new ColliderComponent(new TileSize(1, 1), solid));
        return entity;
    }

    private static Entity CreateEntity(string name, Position position, char character, int layer, Velocity velocity, bool solid)
    {
        var entity = new Entity(name);
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent(character, layer));
        entity.AddComponent(new VelocityComponent(velocity));
        entity.AddComponent(new ColliderComponent(new TileSize(1, 1), solid));
        return entity;
    }
}
