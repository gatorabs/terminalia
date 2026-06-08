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
    public static Scene Assemble(TileMap map, IRenderer renderer, Action<string>? onTransition = null, IEnumerable<EntityRule>? rules = null)
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

        var camera = new Entity("Camera");
        camera.AddComponent(new CameraComponent(new Size(renderer.Width, renderer.Height)));
        scene.AddEntity(camera);

        scene.AddSystem(new MovementSystem());
        scene.AddSystem(new CollisionSystem());
        scene.AddSystem(new CameraFollowSystem(new Size(map.Width, map.Height)));
        scene.AddSystem(new RenderSystem(renderer));

        if (map.Zones != ZoneGrid.Empty)
        {
            scene.AddSystem(new ZoneTransitionSystem(map.Zones, onTransition ?? (_ => { })));
        }

        return scene;
    }
}
