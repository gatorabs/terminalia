namespace Terminalia.Editor.Controls.Inspector;

public sealed class MapPropertiesChangedEventArgs(string name, int width, int height, int cameraZoom) : EventArgs
{
    public string Name { get; } = name;
    public int Width { get; } = width;
    public int Height { get; } = height;
    public int CameraZoom { get; } = cameraZoom;
}

public sealed class ZoneDefinedEventArgs(string name, string? targetMap) : EventArgs
{
    public string Name { get; } = name;
    public string? TargetMap { get; } = string.IsNullOrWhiteSpace(targetMap) ? null : targetMap;
}

public sealed class ZoneSelectedEventArgs(string? zoneName) : EventArgs
{
    public string? ZoneName { get; } = zoneName;
}
