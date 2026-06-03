namespace Terminalia.Core.Runtime;

public sealed class GameLoop
{
    private readonly IGame _game;
    private readonly IClock _clock;
    private readonly FrameRateSettings _settings;

    public GameLoop(IGame game, IClock clock, FrameRateSettings settings)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void Run()
    {
        _game.Initialize();

        while (_game.IsRunning)
        {
            var time = _clock.GetGameTime();

            _game.HandleInput();
            _game.Update(time);
            _game.Render();

            _clock.WaitForNextFrame(_settings.TargetFps);
        }
    }
}
