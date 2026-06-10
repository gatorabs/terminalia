namespace Terminalia.Assets.Json;

public sealed class GameMenuDto
{
    public string Heading { get; set; } = string.Empty;

    public List<GameMenuItemDto> Items { get; set; } = [];
}

public sealed class GameMenuItemDto
{
    public string Label { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
}
