namespace Terminalia.Core.ValueObjects;

public readonly record struct Size
{
    public Size(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;
    }
    poggers
    public int Width { get; }

    public int Height { get; }
}
