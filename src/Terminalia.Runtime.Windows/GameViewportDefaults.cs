using Terminalia.Core.Assets;

namespace Terminalia.Runtime.Windows;

public static class GameViewportDefaults
{
    public const int GamePadding = 16;
    public const int HintBarHeight = 22;

    public static readonly Size WindowClientSize = new(900, 600);

    public static Size ComputeKernelSize(int cameraZoom)
    {
        var referenceCellSize = Math.Clamp(cameraZoom, TileMap.MinCameraZoom, TileMap.MaxCameraZoom);
        var availableWidth = WindowClientSize.Width - GamePadding * 2;
        var availableHeight = WindowClientSize.Height - GamePadding * 2 - HintBarHeight;

        return new Size(
            Math.Max(1, availableWidth / referenceCellSize),
            Math.Max(1, availableHeight / referenceCellSize));
    }

    public static int ComputeCellSize(Size availableArea, Size kernelSize)
    {
        var cellWidth = availableArea.Width / kernelSize.Width;
        var cellHeight = availableArea.Height / kernelSize.Height;
        return Math.Max(1, Math.Min(cellWidth, cellHeight));
    }

    public static Size ComputeAvailableArea(Size clientSize)
    {
        return new Size(
            Math.Max(0, clientSize.Width - GamePadding * 2),
            Math.Max(0, clientSize.Height - GamePadding * 2 - HintBarHeight));
    }
}
