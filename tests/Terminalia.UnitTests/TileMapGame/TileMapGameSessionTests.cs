using Terminalia.Core.Abstractions.Input;
using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Input;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;
using Terminalia.TileMapGame;

namespace Terminalia.UnitTests.TileMapGame;

public sealed class TileMapGameSessionTests
{
    [Fact]
    public void HandleInput_Should_Stop_Session_On_Exit_Command()
    {
        var session = CreateSession(out _, out var input, out _);
        input.Commands = [InputCommand.Exit];

        session.HandleInput();

        Assert.False(session.IsRunning);
    }

    [Fact]
    public void HandleInput_Should_Apply_Movement_Scaled_By_Speed()
    {
        var session = CreateSession(out var scene, out var input, out _, speed: 2);
        input.Commands = [InputCommand.MoveRight];

        session.HandleInput();

        var velocity = scene.FindEntity("Player")!.GetComponent<VelocityComponent>()!.Velocity;
        Assert.Equal(new Velocity(2, 0), velocity);
    }

    [Fact]
    public void HandleInput_Should_Use_Last_Movement_Command()
    {
        var session = CreateSession(out var scene, out var input, out _);
        input.Commands = [InputCommand.MoveRight, InputCommand.MoveUp];

        session.HandleInput();

        var velocity = scene.FindEntity("Player")!.GetComponent<VelocityComponent>()!.Velocity;
        Assert.Equal(new Velocity(0, -1), velocity);
    }

    [Fact]
    public void HandleInput_Should_Not_Throw_Without_Player()
    {
        var scene = new Scene("Session Test");
        var session = new TileMapGameSession(scene, new FakeRenderer(), new FakeInputProvider());

        session.HandleInput();

        Assert.True(session.IsRunning);
    }

    [Fact]
    public void Update_Should_Clear_Renderer_And_Render_Should_Present()
    {
        var session = CreateSession(out _, out _, out var renderer);

        session.Update(new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));
        session.Render();

        Assert.Equal(1, renderer.ClearCount);
        Assert.Equal(1, renderer.PresentCount);
    }

    private static TileMapGameSession CreateSession(out Scene scene, out FakeInputProvider input, out FakeRenderer renderer, int speed = 1)
    {
        scene = new Scene("Session Test");
        var player = new Entity("Player");
        player.AddComponent(new TransformComponent(new Position(1, 1)));
        player.AddComponent(new VelocityComponent(Velocity.Zero));
        player.AddComponent(new SpeedComponent(speed));
        scene.AddEntity(player);

        input = new FakeInputProvider();
        renderer = new FakeRenderer();
        return new TileMapGameSession(scene, renderer, input);
    }

    private sealed class FakeInputProvider : IInputProvider
    {
        public IReadOnlyCollection<InputCommand> Commands { get; set; } = [];

        public IReadOnlyCollection<InputCommand> GetCommands() => Commands;
    }

    private sealed class FakeRenderer : IRenderer
    {
        public int Width => 80;

        public int Height => 25;

        public int ClearCount { get; private set; }

        public int PresentCount { get; private set; }

        public void Clear()
        {
            ClearCount++;
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
            PresentCount++;
        }
    }
}
