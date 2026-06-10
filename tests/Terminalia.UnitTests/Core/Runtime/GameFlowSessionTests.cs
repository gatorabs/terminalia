using Terminalia.Core.Abstractions.Input;
using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Abstractions.Runtime;
using Terminalia.Core.Assets;
using Terminalia.Core.Input;
using Terminalia.Core.Runtime;

namespace Terminalia.UnitTests.Core.Runtime;

public sealed class GameFlowSessionTests
{
    [Fact]
    public void Flow_Should_Start_On_Title_Menu()
    {
        var (flow, _, _, _) = CreateFlow();

        Assert.Equal(GameFlowState.Title, flow.State);
        Assert.True(flow.IsRunning);
    }

    [Fact]
    public void Flow_Should_Start_Gameplay_When_Start_Is_Confirmed()
    {
        var (flow, input, _, factory) = CreateFlow();

        input.Push(InputCommand.Confirm);
        flow.HandleInput();

        Assert.Equal(GameFlowState.Playing, flow.State);
        Assert.Equal(1, factory.Invocations);
        Assert.Same(flow.GameplayInput, factory.LastInput);
    }

    [Fact]
    public void Flow_Should_Relay_Movement_To_Gameplay_While_Playing()
    {
        var (flow, input, _, factory) = CreateFlow();
        StartGameplay(flow, input);

        input.Push(InputCommand.MoveRight);
        flow.HandleInput();

        Assert.Equal([InputCommand.MoveRight], factory.LastGameplay!.ReceivedCommands);
    }

    [Fact]
    public void Flow_Should_Pause_On_Exit_Without_Leaking_The_Command()
    {
        var (flow, input, _, factory) = CreateFlow();
        StartGameplay(flow, input);

        input.Push(InputCommand.Exit, InputCommand.MoveRight);
        flow.HandleInput();

        Assert.Equal(GameFlowState.Paused, flow.State);
        Assert.Empty(factory.LastGameplay!.ReceivedCommands);
    }

    [Fact]
    public void Flow_Should_Resume_From_Pause_On_Exit()
    {
        var (flow, input, _, _) = CreateFlow();
        StartGameplay(flow, input);
        Pause(flow, input);

        input.Push(InputCommand.Exit);
        flow.HandleInput();

        Assert.Equal(GameFlowState.Playing, flow.State);
    }

    [Fact]
    public void Flow_Should_Resume_From_Pause_Menu_Item()
    {
        var (flow, input, _, _) = CreateFlow();
        StartGameplay(flow, input);
        Pause(flow, input);

        input.Push(InputCommand.Confirm);
        flow.HandleInput();

        Assert.Equal(GameFlowState.Playing, flow.State);
    }

    [Fact]
    public void Flow_Should_Return_To_Title_And_Recreate_Gameplay_On_Next_Start()
    {
        var (flow, input, _, factory) = CreateFlow();
        StartGameplay(flow, input);
        Pause(flow, input);

        input.Push(InputCommand.MoveDown);
        flow.HandleInput();
        input.Push(InputCommand.Confirm);
        flow.HandleInput();
        Assert.Equal(GameFlowState.Title, flow.State);

        StartGameplay(flow, input);

        Assert.Equal(2, factory.Invocations);
    }

    [Fact]
    public void Flow_Should_Stop_On_Quit_From_Pause_Menu()
    {
        var (flow, input, _, _) = CreateFlow();
        StartGameplay(flow, input);
        Pause(flow, input);

        input.Push(InputCommand.MoveDown);
        flow.HandleInput();
        input.Push(InputCommand.MoveDown);
        flow.HandleInput();
        input.Push(InputCommand.Confirm);
        flow.HandleInput();

        Assert.False(flow.IsRunning);
    }

    [Fact]
    public void Flow_Should_Stop_On_Exit_From_Title()
    {
        var (flow, input, _, _) = CreateFlow();

        input.Push(InputCommand.Exit);
        flow.HandleInput();

        Assert.False(flow.IsRunning);
    }

    [Fact]
    public void Flow_Should_Render_Menu_While_Not_Playing()
    {
        var (flow, _, renderer, _) = CreateFlow();

        flow.Update(CreateGameTime());
        flow.Render();

        Assert.Equal(1, renderer.ClearCount);
        Assert.Equal(1, renderer.PresentCount);
        Assert.NotEmpty(renderer.Texts);
    }

    [Fact]
    public void Flow_Should_Delegate_Update_And_Render_While_Playing()
    {
        var (flow, input, renderer, factory) = CreateFlow();
        StartGameplay(flow, input);
        renderer.ClearCount = 0;

        flow.Update(CreateGameTime());
        flow.Render();

        Assert.Equal(1, factory.LastGameplay!.UpdateCount);
        Assert.Equal(1, factory.LastGameplay.RenderCount);
        Assert.Equal(0, renderer.ClearCount);
    }

    [Fact]
    public void Flow_Should_Keep_Playing_After_Gameplay_Replacement()
    {
        var (flow, input, _, _) = CreateFlow();
        StartGameplay(flow, input);

        var replacement = new FakeGameplay(flow.GameplayInput);
        flow.ReplaceGameplay(replacement);
        flow.Update(CreateGameTime());

        Assert.Equal(GameFlowState.Playing, flow.State);
        Assert.Equal(1, replacement.UpdateCount);
    }

    [Fact]
    public void Flow_Should_Return_To_Title_When_Gameplay_Stops_Itself()
    {
        var (flow, input, _, factory) = CreateFlow();
        StartGameplay(flow, input);
        factory.LastGameplay!.Stop();

        flow.HandleInput();

        Assert.Equal(GameFlowState.Title, flow.State);
        Assert.True(flow.IsRunning);
    }

    private static (GameFlowSession Flow, FakeInputProvider Input, FakeRenderer Renderer, FakeGameplayFactory Factory) CreateFlow()
    {
        var input = new FakeInputProvider();
        var renderer = new FakeRenderer();
        var factory = new FakeGameplayFactory();
        var definition = GameDefinition.CreateDefault("first_dungeon");
        var flow = new GameFlowSession(definition, renderer, input, factory.Create);
        flow.Initialize();
        return (flow, input, renderer, factory);
    }

    private static void StartGameplay(GameFlowSession flow, FakeInputProvider input)
    {
        input.Push(InputCommand.Confirm);
        flow.HandleInput();
    }

    private static void Pause(GameFlowSession flow, FakeInputProvider input)
    {
        input.Push(InputCommand.Exit);
        flow.HandleInput();
    }

    private static GameTime CreateGameTime()
    {
        return new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1);
    }

    private sealed class FakeGameplayFactory
    {
        public int Invocations { get; private set; }

        public IInputProvider? LastInput { get; private set; }

        public FakeGameplay? LastGameplay { get; private set; }

        public IGame Create(IInputProvider input)
        {
            Invocations++;
            LastInput = input;
            LastGameplay = new FakeGameplay(input);
            return LastGameplay;
        }
    }

    private sealed class FakeGameplay(IInputProvider input) : IGame
    {
        public bool IsRunning { get; private set; } = true;

        public List<InputCommand> ReceivedCommands { get; } = [];

        public int UpdateCount { get; private set; }

        public int RenderCount { get; private set; }

        public void Initialize()
        {
        }

        public void HandleInput()
        {
            ReceivedCommands.AddRange(input.GetCommands());
        }

        public void Update(GameTime time)
        {
            UpdateCount++;
        }

        public void Render()
        {
            RenderCount++;
        }

        public void Stop() => IsRunning = false;
    }

    private sealed class FakeInputProvider : IInputProvider
    {
        private readonly Queue<InputCommand[]> _batches = new();

        public void Push(params InputCommand[] commands) => _batches.Enqueue(commands);

        public IReadOnlyCollection<InputCommand> GetCommands() =>
            _batches.Count > 0 ? _batches.Dequeue() : [];
    }

    private sealed class FakeRenderer : IRenderer
    {
        public int Width => 40;

        public int Height => 20;

        public int ClearCount { get; set; }

        public int PresentCount { get; private set; }

        public List<string> Texts { get; } = [];

        public void Clear() => ClearCount++;

        public void Draw(int x, int y, char character)
        {
        }

        public void DrawText(int x, int y, string text) => Texts.Add(text);

        public void DrawTextCentered(int y, string text) => Texts.Add(text);

        public void Present() => PresentCount++;
    }
}
