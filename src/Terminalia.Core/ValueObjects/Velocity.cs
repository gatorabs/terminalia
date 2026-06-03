namespace Terminalia.Core.ValueObjects;

public readonly record struct Velocity(int DeltaX, int DeltaY)
{
    public static Velocity Zero { get; } = new(0, 0);
}
