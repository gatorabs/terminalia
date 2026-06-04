using Terminalia.Core.Runtime;
using Terminalia.Core.Abstractions.Runtime;

namespace Terminalia.Sample.Rpg;

internal sealed class WindowsFormsClock : IClock
{
    private DateTime _lastTick = DateTime.UtcNow;
    private TimeSpan _totalTime = TimeSpan.Zero;
    private long _frame;

    public GameTime GetGameTime()
    {
        var now = DateTime.UtcNow;
        var delta = now - _lastTick;
        _lastTick = now;
        _totalTime += delta;
        _frame++;
        return new GameTime(delta, _totalTime, (int)(_frame % int.MaxValue));
    }

    public void WaitForNextFrame(int targetFps) { }
}
