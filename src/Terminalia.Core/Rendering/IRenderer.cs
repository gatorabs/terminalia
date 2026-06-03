namespace Terminalia.Core.Rendering;

public interface IRenderer
{
    int Width { get; }

    int Height { get; }

    void Clear();

    void Draw(int x, int y, char character);

    void DrawText(int x, int y, string text);

    void Present();
}
