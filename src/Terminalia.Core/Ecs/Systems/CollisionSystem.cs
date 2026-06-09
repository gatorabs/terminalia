using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Systems;

public sealed class CollisionSystem : IGameSystem
{
    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var solidColliders = scene.GetEntities()
            .Select(entity => new
            {
                Entity = entity,
                Transform = entity.GetComponent<TransformComponent>(),
                Collider = entity.GetComponent<ColliderComponent>()
            })
            .Where(item => item.Transform is not null && item.Collider is { IsSolid: true })
            .ToList();

        foreach (var item in solidColliders)
        {
            var velocity = item.Entity.GetComponent<VelocityComponent>();
            if (velocity is null || velocity.Velocity == Velocity.Zero)
            {
                continue;
            }

            var steps = Math.Max(Math.Abs(velocity.Velocity.DeltaX), Math.Abs(velocity.Velocity.DeltaY));
            var step = new Velocity(Math.Sign(velocity.Velocity.DeltaX), Math.Sign(velocity.Velocity.DeltaY));

            for (var i = 0; i < steps; i++)
            {
                var targetPosition = item.Transform!.Position.Translate(step);
                var targetBounds = new Bounds(targetPosition, item.Collider!.Size);
                var collides = solidColliders.Any(other =>
                    !ReferenceEquals(other.Entity, item.Entity) &&
                    targetBounds.Intersects(new Bounds(other.Transform!.Position, other.Collider!.Size)));

                if (collides)
                {
                    break;
                }

                item.Transform.Position = targetPosition;
            }
        }
    }
}
