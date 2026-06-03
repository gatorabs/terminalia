namespace Terminalia.Core.Ecs.Components;

public sealed class RenderComponent : IComponent
{
    public RenderComponent(char character, int layer = 0, bool isVisible = true)
    {
        Character = character;
        Layer = layer;
        IsVisible = isVisible;
    }

    public char Character { get; set; }

    public int Layer { get; set; }

    public bool IsVisible { get; set; }
}
