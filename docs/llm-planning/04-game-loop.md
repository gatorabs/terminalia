# Terminalia - Game Loop

## Purpose

The game loop controls the execution of the engine.

It repeatedly:

1. Reads input.
2. Updates the game state.
3. Renders the frame.
4. Waits for the next frame.

## MVP Game Loop

The first version should use a simple fixed target FPS.

Default target:

```text
30 FPS
```

## Suggested Flow

```text
Initialize game

while game is running:
    calculate game time
    handle input
    update current scene
    render current scene
    wait until next frame
```

## Suggested Class

```csharp
public sealed class GameLoop
{
    private readonly IGame _game;
    private readonly IClock _clock;
    private readonly FrameRateSettings _settings;

    public GameLoop(
        IGame game,
        IClock clock,
        FrameRateSettings settings)
    {
        _game = game;
        _clock = clock;
        _settings = settings;
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
```

## Required Types

- `GameLoop`
- `GameTime`
- `FrameRateSettings`
- `IClock`
- `SystemClock`

## Clock Responsibility

The clock is responsible for:

- Measuring elapsed time.
- Measuring total time.
- Counting frames.
- Waiting for the next frame.

## MVP Constraint

Do not implement advanced fixed timestep with interpolation in the first version.

That can be added later.

## Future Improvement

Later versions may support:

- Fixed update.
- Variable render update.
- Interpolation.
- Pause/resume.
- Slow motion.
- Frame limiter.
- Performance metrics.

## Acceptance Criteria

The game loop is accepted when:

- The game initializes once.
- The loop keeps running while `IsRunning` is true.
- Input, update, and render are called in order.
- The loop can be stopped.
- The frame rate can be configured.
