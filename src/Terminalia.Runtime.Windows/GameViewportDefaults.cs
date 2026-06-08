namespace Terminalia.Runtime.Windows;

public static class GameViewportDefaults
{
    public const int GamePadding = 16;
    public const int HintBarHeight = 22;
    public const int ReferenceCellSize = 72;

    public static readonly Size WindowClientSize = new(900, 600);

    public static readonly Size KernelSize = ComputeKernelSize();

    public static int ComputeCellSize(Size availableArea)
    {
        var cellWidth = availableArea.Width / KernelSize.Width;
        var cellHeight = availableArea.Height / KernelSize.Height;
        return Math.Max(1, Math.Min(cellWidth, cellHeight));
    }

    public static Size ComputeAvailableArea(Size clientSize)
    {
        return new Size(
            Math.Max(0, clientSize.Width - GamePadding * 2),
            Math.Max(0, clientSize.Height - GamePadding * 2 - HintBarHeight));
    }

    private static Size ComputeKernelSize()
    {
        var availableWidth = WindowClientSize.Width - GamePadding * 2;
        var availableHeight = WindowClientSize.Height - GamePadding * 2 - HintBarHeight;

        return new Size(
            Math.Max(1, availableWidth / ReferenceCellSize),
            Math.Max(1, availableHeight / ReferenceCellSize));
    }
}
