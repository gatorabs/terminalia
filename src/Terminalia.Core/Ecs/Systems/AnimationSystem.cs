using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

namespace Terminalia.Core.Ecs.Systems;

public sealed class AnimationSystem : IGameSystem
{
    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(time);

        foreach (var entity in scene.Entities.Where(entity => entity.IsActive))
        {
            var animation = entity.GetComponent<AnimatedSpriteComponent>();
            if (animation is null)
            {
                continue;
            }

            animation.Elapsed += time.Elapsed;
            while (animation.Elapsed >= animation.FrameDuration)
            {
                animation.Elapsed -= animation.FrameDuration;
                animation.AdvanceFrame();
            }

            var render = entity.GetComponent<RenderComponent>();
            if (render is not null)
            {
                render.Character = animation.CurrentFrame;
            }
        }
    }
}
