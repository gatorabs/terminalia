using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Assets;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.ValueObjects;
using Terminalia.TileMapGame;

namespace Terminalia.UnitTests.TileMapGame;

public sealed class SceneAssemblerTests
{
    [Fact]
    public void Assemble_Should_Create_Entities_For_Known_Tiles_And_Camera()
    {
        var scene = SceneAssembler.Assemble(CreateMap(), new FakeRenderer());

        var entities = scene.GetEntities().ToList();
        Assert.NotNull(scene.FindEntity("Player"));
        Assert.NotNull(scene.FindEntity("Camera"));
        Assert.Equal(9, entities.Count(e => e.Name == "Wall"));
        Assert.Single(entities, e => e.Name == "Item");
    }

    [Fact]
    public void Assemble_Should_Apply_Entity_Properties_To_Player()
    {
        var properties = new Dictionary<Position, IReadOnlyDictionary<string, string>>
        {
            [new Position(1, 1)] = new Dictionary<string, string> { [EntityPropertyKeys.Speed] = "3" }
        };
        var map = CreateMap(properties);

        var scene = SceneAssembler.Assemble(map, new FakeRenderer());

        var player = scene.FindEntity("Player");
        Assert.Equal(3, player!.GetComponent<SpeedComponent>()!.CellsPerStep);
    }

    [Fact]
    public void Assemble_Should_Wire_Zone_Transition_Into_Scene_Update()
    {
        var zones = new Dictionary<string, Zone> { ["exit"] = new Zone("exit", "next_map") };
        var cells = new string?[4 * 3];
        cells[1 * 4 + 2] = "exit";
        var map = CreateMap(zones: new ZoneGrid(4, 3, zones, cells));
        string? transitionTarget = null;

        var scene = SceneAssembler.Assemble(map, new FakeRenderer(), name => transitionTarget = name);
        var player = scene.FindEntity("Player")!;
        player.GetComponent<VelocityComponent>()!.Velocity = new Velocity(1, 0);
        scene.Update(new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Equal(new Position(2, 1), player.GetComponent<TransformComponent>()!.Position);
        Assert.Equal("next_map", transitionTarget);
    }

    private static TileMap CreateMap(
        IReadOnlyDictionary<Position, IReadOnlyDictionary<string, string>>? entityProperties = null,
        ZoneGrid? zones = null)
    {
        return new TileMap(
            "test_map",
            4,
            3,
            ["####", "#@.#", "##*#"],
            new Dictionary<char, string> { ['#'] = "wall", ['.'] = "floor", ['@'] = "player_spawn", ['*'] = "relic" },
            zones,
            TileMap.DefaultCameraZoom,
            entityProperties);
    }

    private sealed class FakeRenderer : IRenderer
    {
        public int Width => 80;

        public int Height => 25;

        public void Clear()
        {
        }

        public void Draw(int x, int y, char character)
        {
        }

        public void DrawText(int x, int y, string text)
        {
        }

        public void DrawTextCentered(int y, string text)
        {
        }

        public void Present()
        {
        }
    }
}
