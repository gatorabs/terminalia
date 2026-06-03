namespace Terminalia.Core.ValueObjects;

public readonly record struct Position(int X, int Y)
{
    public Position Translate(Velocity velocity)
    {
        return new Position(X + velocity.DeltaX, Y + velocity.DeltaY);
    }
}
