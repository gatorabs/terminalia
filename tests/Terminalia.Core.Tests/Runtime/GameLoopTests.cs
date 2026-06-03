using Terminalia.Core.Runtime;

namespace Terminalia.Core.Tests.Runtime;

public sealed class GameLoopTests
{
    [Fact]
    public void GameLoop_Should_Call_Game_Methods_In_Order()
    {
        var game = new FakeGame(stopAfterUpdates: 1);
        var clock = new FakeClock();
        var loop = new GameLoop(game, clock, new FrameRateSettings(30));

        loop.Run();

        Assert.Equal(
            ["Initialize", "HandleInput", "Update", "Render"],
            game.Calls);
    }

    [Fact]
    public void GameLoop_Should_Stop_When_Game_Is_No_Longer_Running()
    {
        var game = new FakeGame(stopAfterUpdates: 2);
        var clock = new FakeClock();
        var loop = new GameLoop(game, clock, new FrameRateSettings(30));

        loop.Run();

        Assert.Equal(2, game.UpdateCount);
        Assert.Equal(2, clock.WaitCount);
    }

    private sealed class FakeGame : IGame
    {
        private readonly int _stopAfterUpdates;

        public FakeGame(int stopAfterUpdates)
        {
            _stopAfterUpdates = stopAfterUpdates;
        }

        public bool IsRunning { get; private set; }

        public int UpdateCount { get; private set; }

        public List<string> Calls { get; } = [];

        public void Initialize()
        {
            Calls.Add(nameof(Initialize));
            IsRunning = true;
        }

        public void HandleInput()
        {
            Calls.Add(nameof(HandleInput));
        }

        public void Update(GameTime time)
        {
            Calls.Add(nameof(Update));
            UpdateCount++;

            if (UpdateCount >= _stopAfterUpdates)
            {
                Stop();
            }
        }

        public void Render()
        {
            Calls.Add(nameof(Render));
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }

    private sealed class FakeClock : IClock
    {
        private int _frame;

        public int WaitCount { get; private set; }

        public GameTime GetGameTime()
        {
            return new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16 * _frame), _frame++);
        }

        public void WaitForNextFrame(int targetFps)
        {
            WaitCount++;
        }
    }
}
