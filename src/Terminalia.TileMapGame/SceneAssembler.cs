using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Assets;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.TileMapGame;

public static class SceneAssembler
{
    public static Scene Assemble(
        TileMap map,
        IRenderer renderer,
        Action<string>? onTransition = null,
        IEnumerable<EntityRule>? rules = null,
        int viewportWidth = 0,
        int viewportHeight = 0)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(renderer);

        var scene = new Scene(map.Name);
        var ruleMap = (rules ?? DefaultEntityRules.All).ToDictionary(r => r.Tile);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.GetTile(x, y);
                if (ruleMap.TryGetValue(tile, out var rule))
                {
                    scene.AddEntity(rule.CreateEntity(new Position(x, y)));
                }
            }
        }

        var vpW = viewportWidth > 0 ? viewportWidth : renderer.Width;
        var vpH = viewportHeight > 0 ? viewportHeight : renderer.Height;

        var cameraEntity = new Entity("Camera");
        cameraEntity.AddComponent(new CameraComponent());
        scene.AddEntity(cameraEntity);

        scene.AddSystem(new MovementSystem());
        scene.AddSystem(new CollisionSystem());
        scene.AddSystem(new CameraSystem(map.Width, map.Height, vpW, vpH));
        scene.AddSystem(new RenderSystem(renderer));

        if (map.Zones != ZoneGrid.Empty)
        {
            scene.AddSystem(new ZoneTransitionSystem(map.Zones, onTransition ?? (_ => { })));
        }

        return scene;
    }
}
