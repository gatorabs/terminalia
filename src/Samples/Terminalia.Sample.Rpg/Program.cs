using Terminalia.Assets.Json;
using Terminalia.Core.Assets;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Input;
using Terminalia.Core.Rendering;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;
using Terminalia.Input.Console;
using Terminalia.Rendering.Terminal;

var assetLoader = new JsonAssetLoader(Path.Combine(AppContext.BaseDirectory, "assets"));
var map = assetLoader.LoadMap("first_dungeon");
var scene = CreateSceneFromMap(map);
var player = scene.Entities.Single(entity => entity.Name == "Player");
var inputProvider = new ConsoleInputProvider();
IRenderer renderer = new TerminalRenderer(width: 62, height: 12);
var clock = new SystemClock();
var isRunning = true;

do
{
    var time = clock.GetGameTime();
    var commands = inputProvider.GetCommands();

    if (commands.Contains(InputCommand.Exit))
    {
        isRunning = false;
    }

    ApplyMovementCommands(player, commands);
    new CollisionSystem().Update(scene, time);
    RenderFrame(renderer, map, scene);

    if (Console.IsOutputRedirected)
    {
        break;
    }

    clock.WaitForNextFrame(30);
}
while (isRunning);

static Scene CreateSceneFromMap(TileMap map)
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
                    scene.AddEntity(CreateEntity("Stone Wall", position, '#', layer: 1, Velocity.Zero, solid: true));
                    break;
                case '@':
                    scene.AddEntity(CreateEntity("Player", position, '@', layer: 2, Velocity.Zero, solid: true));
                    break;
                case '*':
                    scene.AddEntity(CreateEntity("Relic", position, '*', layer: 1, Velocity.Zero, solid: false));
                    break;
            }
        }
    }

    return scene;
}

static Entity CreateEntity(
    string name,
    Position position,
    char character,
    int layer,
    Velocity velocity,
    bool solid)
{
    var entity = new Entity(name);
    entity.AddComponent(new TransformComponent(position));
    entity.AddComponent(new RenderComponent(character, layer));
    entity.AddComponent(new VelocityComponent(velocity));
    entity.AddComponent(new ColliderComponent(new Size(1, 1), solid));
    return entity;
}

static void ApplyMovementCommands(Entity player, IReadOnlyCollection<InputCommand> commands)
{
    var velocity = commands.LastOrDefault(command => command is
        InputCommand.MoveUp or
        InputCommand.MoveDown or
        InputCommand.MoveLeft or
        InputCommand.MoveRight) switch
    {
        InputCommand.MoveUp => new Velocity(0, -1),
        InputCommand.MoveDown => new Velocity(0, 1),
        InputCommand.MoveLeft => new Velocity(-1, 0),
        InputCommand.MoveRight => new Velocity(1, 0),
        _ => Velocity.Zero
    };

    player.GetComponent<VelocityComponent>()!.Velocity = velocity;
}

static void RenderFrame(IRenderer renderer, TileMap map, Scene scene)
{
    renderer.Clear();
    renderer.DrawText(0, 0, "Terminalia Sample RPG");
    renderer.DrawText(0, 1, "====================");
    DrawMap(renderer, map, x: 0, y: 3);
    DrawEntities(renderer, scene, offsetX: 0, offsetY: 3);
    renderer.DrawText(22, 3, $"Map: {map.Name}");
    renderer.DrawText(22, 5, "Loaded from JSON assets");
    renderer.DrawText(22, 6, "Move: WASD or arrows");
    renderer.DrawText(22, 7, "Exit: ESC");
    renderer.DrawText(22, 9, "Next: visual editor contract");
    renderer.Present();
}

static void DrawMap(IRenderer renderer, TileMap map, int x, int y)
{
    for (var row = 0; row < map.Height; row++)
    {
        for (var column = 0; column < map.Width; column++)
        {
            var tile = map.Tiles[row][column];
            var character = tile == '#' ? '#' : '.';
            renderer.Draw(x + column, y + row, character);
        }
    }
}

static void DrawEntities(IRenderer renderer, Scene scene, int offsetX, int offsetY)
{
    var translatedScene = new Scene($"{scene.Name} Render Snapshot");

    foreach (var entity in scene.Entities)
    {
        var transform = entity.GetComponent<TransformComponent>();
        var render = entity.GetComponent<RenderComponent>();

        if (transform is null || render is null)
        {
            continue;
        }

        var translatedEntity = new Entity(entity.Id, entity.Name);
        translatedEntity.AddComponent(new TransformComponent(new Position(
            offsetX + transform.Position.X,
            offsetY + transform.Position.Y)));
        translatedEntity.AddComponent(new RenderComponent(render.Character, render.Layer, render.IsVisible));
        translatedScene.AddEntity(translatedEntity);
    }

    new RenderSystem(renderer).Update(
        translatedScene,
        new GameTime(TimeSpan.Zero, TimeSpan.Zero, 0));
}
