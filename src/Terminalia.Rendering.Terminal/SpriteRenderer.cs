namespace Terminalia.Rendering.Terminal;

public sealed class SpriteRenderer
{
    public void Draw(ConsoleFrameBuffer frameBuffer, int x, int y, IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(frameBuffer);
        ArgumentNullException.ThrowIfNull(lines);

        for (var row = 0; row < lines.Count; row++)
        {
            frameBuffer.DrawText(x, y + row, lines[row]);
        }
    }
}
