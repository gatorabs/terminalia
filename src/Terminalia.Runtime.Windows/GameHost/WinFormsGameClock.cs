using Terminalia.Core.Abstractions.Runtime;
using Terminalia.Core.Runtime;

namespace Terminalia.Runtime.Windows.GameHost;

internal sealed class WinFormsGameClock : IClock
{
    private DateTime _lastTick = DateTime.UtcNow;
    private TimeSpan _totalTime = TimeSpan.Zero;
    private int _frame;

    public GameTime GetGameTime()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastTick;
        _lastTick = now;
        _totalTime += elapsed;
        return new GameTime(elapsed, _totalTime, _frame++);
    }

    public void WaitForNextFrame(int targetFps) { }
}
