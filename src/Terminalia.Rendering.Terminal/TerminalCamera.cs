namespace Terminalia.Rendering.Terminal;

public sealed class TerminalCamera
{
    public TerminalCamera(int viewportWidth, int viewportHeight, int offsetX = 0, int offsetY = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(viewportWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(viewportHeight);

        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public int ViewportWidth { get; }

    public int ViewportHeight { get; }

    public int OffsetX { get; private set; }

    public int OffsetY { get; private set; }

    public (int X, int Y) WorldToScreen(int worldX, int worldY)
    {
        return (worldX - OffsetX, worldY - OffsetY);
    }

    public void MoveTo(int offsetX, int offsetY)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
    }
}
