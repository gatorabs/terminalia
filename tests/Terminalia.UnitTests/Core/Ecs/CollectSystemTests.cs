using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.UnitTests.Core.Ecs;

public sealed class CollectSystemTests
{
    [Fact]
    public void CollectSystem_Should_Collect_When_Player_Overlaps_Collectible()
    {
        var scene = new Scene("Collect Test");
        scene.AddEntity(CreatePlayer(new Position(2, 2)));
        var relic = CreateRelic(new Position(2, 2), disappearOnCollect: true);
        scene.AddEntity(relic);

        new CollectSystem().Update(scene, CreateGameTime());

        Assert.True(relic.GetComponent<CollectibleComponent>()!.IsCollected);
        Assert.False(relic.GetComponent<RenderComponent>()!.IsVisible);
    }

    [Fact]
    public void CollectSystem_Should_Keep_Visible_When_Disappear_Is_Disabled()
    {
        var scene = new Scene("Collect Test");
        scene.AddEntity(CreatePlayer(new Position(2, 2)));
        var relic = CreateRelic(new Position(2, 2), disappearOnCollect: false);
        scene.AddEntity(relic);

        new CollectSystem().Update(scene, CreateGameTime());

        Assert.True(relic.GetComponent<CollectibleComponent>()!.IsCollected);
        Assert.True(relic.GetComponent<RenderComponent>()!.IsVisible);
    }

    [Fact]
    public void CollectSystem_Should_Not_Collect_When_Positions_Differ()
    {
        var scene = new Scene("Collect Test");
        scene.AddEntity(CreatePlayer(new Position(1, 1)));
        var relic = CreateRelic(new Position(2, 2), disappearOnCollect: true);
        scene.AddEntity(relic);

        new CollectSystem().Update(scene, CreateGameTime());

        Assert.False(relic.GetComponent<CollectibleComponent>()!.IsCollected);
    }

    [Fact]
    public void CollectSystem_Should_Do_Nothing_Without_Player()
    {
        var scene = new Scene("Collect Test");
        var relic = CreateRelic(new Position(2, 2), disappearOnCollect: true);
        scene.AddEntity(relic);

        new CollectSystem().Update(scene, CreateGameTime());

        Assert.False(relic.GetComponent<CollectibleComponent>()!.IsCollected);
    }

    private static Entity CreatePlayer(Position position)
    {
        var player = new Entity("Player");
        player.AddComponent(new TransformComponent(position));
        return player;
    }

    private static Entity CreateRelic(Position position, bool disappearOnCollect)
    {
        var relic = new Entity("Item");
        relic.AddComponent(new TransformComponent(position));
        relic.AddComponent(new RenderComponent('*', layer: 1));
        relic.AddComponent(new CollectibleComponent(disappearOnCollect));
        return relic;
    }

    private static GameTime CreateGameTime()
    {
        return new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1);
    }
}
