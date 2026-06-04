using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Systems;

public sealed class MovementSystem : IGameSystem
{
    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);

        foreach (var entity in scene.GetActiveEntities())
        {
            if (entity.HasComponent<ColliderComponent>())
            {
                continue;
            }

            var transform = entity.GetComponent<TransformComponent>();
            var velocity = entity.GetComponent<VelocityComponent>();

            if (transform is null || velocity is null || velocity.Velocity == Velocity.Zero)
            {
                continue;
            }

            transform.Position = transform.Position.Translate(velocity.Velocity);
        }
    }
}
