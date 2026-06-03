namespace Terminalia.Core.ValueObjects;

public readonly record struct Bounds(Position Position, Size Size)
{
    public int Left => Position.X;

    public int Top => Position.Y;

    public int Right => Position.X + Size.Width;

    public int Bottom => Position.Y + Size.Height;

    public bool Intersects(Bounds other)
    {
        return Left < other.Right &&
            Right > other.Left &&
            Top < other.Bottom &&
            Bottom > other.Top;
    }
}
