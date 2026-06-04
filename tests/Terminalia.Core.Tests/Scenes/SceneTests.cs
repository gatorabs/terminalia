using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Ecs;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

namespace Terminalia.Core.Tests.Scenes;

public sealed class SceneTests
{
    [Fact]
    public void Scene_Should_Add_Entity()
    {
        var scene = new Scene("Dungeon");
        var entity = new Entity("Player");

        scene.AddEntity(entity);

        Assert.Same(entity, scene.GetActiveEntities().Single());
    }

    [Fact]
    public void Scene_Should_Update_Registered_Systems()
    {
        var scene = new Scene("Dungeon");
        var system = new TestSystem();
        var time = new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1);

        scene.AddSystem(system);
        scene.Update(time);

        Assert.True(system.WasUpdated);
        Assert.Same(scene, system.Scene);
        Assert.Equal(time, system.Time);
    }

    [Fact]
    public void Scene_Should_Reject_Duplicate_Entity_Id()
    {
        var scene = new Scene("Dungeon");
        var id = EntityId.New();
        var first = new Entity(id, "FirstPlayer");
        var duplicate = new Entity(id, "SecondPlayer");

        scene.AddEntity(first);

        Assert.Throws<InvalidOperationException>(() => scene.AddEntity(duplicate));
        Assert.Single(scene.GetActiveEntities());
    }

    private sealed class TestSystem : IGameSystem
    {
        public bool WasUpdated { get; private set; }

        public Scene? Scene { get; private set; }

        public GameTime Time { get; private set; }

        public void Update(Scene scene, GameTime time)
        {
            WasUpdated = true;
            Scene = scene;
            Time = time;
        }
    }
}
