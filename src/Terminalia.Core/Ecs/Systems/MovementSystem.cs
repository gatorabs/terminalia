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
        ArgumentNullException.ThrowIfNull(time);

        foreach (var entity in scene.Entities.Where(entity => entity.IsActive))
        {
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
