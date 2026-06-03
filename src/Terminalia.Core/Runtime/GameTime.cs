namespace Terminalia.Core.Runtime;

public sealed class GameTime
{
    public GameTime(TimeSpan elapsed, TimeSpan total, int frame)
    {
        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), "Elapsed time cannot be negative.");
        }

        if (total < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(total), "Total time cannot be negative.");
        }
        ArgumentOutOfRangeException.ThrowIfNegative(frame);

        Elapsed = elapsed;
        Total = total;
        Frame = frame;
    }

    public TimeSpan Elapsed { get; }

    public TimeSpan Total { get; }

    public int Frame { get; }
}
