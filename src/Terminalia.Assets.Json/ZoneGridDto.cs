namespace Terminalia.Assets.Json;

public sealed class ZoneGridDto
{
    public Dictionary<string, ZoneDefDto> Defs { get; set; } = [];
    public Dictionary<string, string> Map { get; set; } = [];
}

public sealed class ZoneDefDto
{
    public string? TargetMap { get; set; }
}
