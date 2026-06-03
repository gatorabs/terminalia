using System.Diagnostics;

namespace Terminalia.Core.Runtime;

public sealed class SystemClock : IClock
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private TimeSpan _lastFrameTime = TimeSpan.Zero;
    private int _frame;

    public GameTime GetGameTime()
    {
        var total = _stopwatch.Elapsed;
        var elapsed = total - _lastFrameTime;
        _lastFrameTime = total;

        return new GameTime(elapsed, total, _frame++);
    }

    public void WaitForNextFrame(int targetFps)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetFps);

        var frameDuration = TimeSpan.FromSeconds(1.0 / targetFps);
        var elapsedInFrame = _stopwatch.Elapsed - _lastFrameTime;
        var remaining = frameDuration - elapsedInFrame;

        if (remaining > TimeSpan.Zero)
        {
            Thread.Sleep(remaining);
        }
    }
}
