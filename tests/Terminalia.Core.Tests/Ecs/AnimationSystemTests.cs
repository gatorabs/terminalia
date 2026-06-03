using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

namespace Terminalia.Core.Tests.Ecs;

public sealed class AnimationSystemTests
{
    [Fact]
    public void AnimationSystem_Should_Advance_Frame_After_Frame_Duration()
    {
        var scene = new Scene("Animation Test");
        var entity = new Entity("Torch");
        var animation = new AnimatedSpriteComponent(['-', '+'], TimeSpan.FromMilliseconds(100));
        var render = new RenderComponent('-');

        entity.AddComponent(animation);
        entity.AddComponent(render);
        scene.AddEntity(entity);

        new AnimationSystem().Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100), 1));

        Assert.Equal(1, animation.CurrentFrameIndex);
        Assert.Equal('+', render.Character);
    }
}
