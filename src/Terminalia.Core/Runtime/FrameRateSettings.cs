namespace Terminalia.Core.Runtime;

public sealed class FrameRateSettings
{
    public FrameRateSettings(int targetFps = 30)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetFps);
        TargetFps = targetFps;
    }

    public int TargetFps { get; }
}
