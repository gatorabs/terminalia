using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Assets;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Runtime;

public static class SceneAssembler
{
    public static Scene Assemble(TileMap map, IRenderer renderer, IEnumerable<EntityRule>? rules = null)
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

        scene.AddSystem(new MovementSystem());
        scene.AddSystem(new CollisionSystem());
        scene.AddSystem(new RenderSystem(renderer));

        return scene;
    }
}
