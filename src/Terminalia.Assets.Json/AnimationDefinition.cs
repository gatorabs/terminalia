namespace Terminalia.Assets.Json;

internal sealed class AnimationDefinition
{
    public string? Name { get; set; }

    public int FrameDurationMs { get; set; }

    public List<List<string>>? Frames { get; set; }
}
