namespace Terminalia.Rendering.Terminal;

// Not yet wired into RenderSystem — integrate when viewport culling is needed for maps larger than the terminal.
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
