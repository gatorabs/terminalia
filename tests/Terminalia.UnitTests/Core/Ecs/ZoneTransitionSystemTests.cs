using Terminalia.Core.Assets;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.UnitTests.Core.Ecs;

public sealed class ZoneTransitionSystemTests
{
    [Fact]
    public void ZoneTransitionSystem_Should_Fire_Transition_When_Entering_Zone_With_Target()
    {
        var scene = new Scene("Zone Test");
        scene.AddEntity(CreatePlayer(new Position(2, 1)));
        string? transitionTarget = null;

        new ZoneTransitionSystem(CreateGrid(), map => transitionTarget = map).Update(scene, CreateGameTime());

        Assert.Equal("next_map", transitionTarget);
    }

    [Fact]
    public void ZoneTransitionSystem_Should_Update_Zone_Name_Without_Transition_When_Target_Is_Null()
    {
        var scene = new Scene("Zone Test");
        var player = CreatePlayer(new Position(0, 0));
        scene.AddEntity(player);
        var fired = false;

        new ZoneTransitionSystem(CreateGrid(), _ => fired = true).Update(scene, CreateGameTime());

        Assert.False(fired);
        Assert.Equal("decor", player.GetComponent<ZoneComponent>()!.ZoneName);
    }

    [Fact]
    public void ZoneTransitionSystem_Should_Not_Fire_Twice_For_Same_Zone()
    {
        var scene = new Scene("Zone Test");
        scene.AddEntity(CreatePlayer(new Position(2, 1)));
        var fireCount = 0;
        var system = new ZoneTransitionSystem(CreateGrid(), _ => fireCount++);

        system.Update(scene, CreateGameTime());
        system.Update(scene, CreateGameTime());

        Assert.Equal(1, fireCount);
    }

    [Fact]
    public void ZoneTransitionSystem_Should_Reset_Zone_Name_When_Leaving_Zone()
    {
        var scene = new Scene("Zone Test");
        var player = CreatePlayer(new Position(2, 1));
        scene.AddEntity(player);
        var system = new ZoneTransitionSystem(CreateGrid(), _ => { });

        system.Update(scene, CreateGameTime());
        player.GetComponent<TransformComponent>()!.Position = new Position(1, 1);
        system.Update(scene, CreateGameTime());

        Assert.Equal(string.Empty, player.GetComponent<ZoneComponent>()!.ZoneName);
        Assert.Null(player.GetComponent<ZoneComponent>()!.TargetMap);
    }

    [Fact]
    public void ZoneTransitionSystem_Should_Ignore_Entities_Without_Zone_Component()
    {
        var scene = new Scene("Zone Test");
        var entity = new Entity("Wall");
        entity.AddComponent(new TransformComponent(new Position(2, 1)));
        scene.AddEntity(entity);
        var fired = false;

        new ZoneTransitionSystem(CreateGrid(), _ => fired = true).Update(scene, CreateGameTime());

        Assert.False(fired);
    }

    private static ZoneGrid CreateGrid()
    {
        var zones = new Dictionary<string, Zone>
        {
            ["exit"] = new Zone("exit", "next_map"),
            ["decor"] = new Zone("decor", null)
        };
        var cells = new string?[3 * 3];
        cells[1 * 3 + 2] = "exit";
        cells[0 * 3 + 0] = "decor";
        return new ZoneGrid(3, 3, zones, cells);
    }

    private static Entity CreatePlayer(Position position)
    {
        var player = new Entity("Player");
        player.AddComponent(new TransformComponent(position));
        player.AddComponent(new ZoneComponent(string.Empty, null));
        return player;
    }

    private static GameTime CreateGameTime()
    {
        return new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1);
    }
}
