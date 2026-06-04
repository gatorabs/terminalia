using Terminalia.Core.Abstractions.Rendering;

namespace Terminalia.Sample.Rpg;

internal sealed class WinFormsRenderer : IRenderer
{
    private static readonly int CharWidth;
    private static readonly int LineHeight;

    static WinFormsRenderer()
    {
        var size = TextRenderer.MeasureText("M", SampleTheme.TerminalFont);
        CharWidth = size.Width - 3;
        LineHeight = size.Height + 4;
    }

    private Graphics? _graphics;
    private Point _origin;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public void SetContext(Graphics graphics, Point origin, int mapWidth, int mapHeight)
    {
        _graphics = graphics;
        _origin = origin;
        Width = mapWidth;
        Height = mapHeight;
    }

    public void Clear() { }

    public void Draw(int x, int y, char character)
    {
        if (_graphics is null) return;
        TextRenderer.DrawText(
            _graphics,
            character.ToString(),
            SampleTheme.TerminalFont,
            new Point(_origin.X + (x * CharWidth), _origin.Y + (y * LineHeight)),
            SampleTheme.GetTileColor(character));
    }

    public void DrawText(int x, int y, string text)
    {
        if (_graphics is null) return;
        TextRenderer.DrawText(
            _graphics,
            text,
            SampleTheme.TerminalFont,
            new Point(_origin.X + (x * CharWidth), _origin.Y + (y * LineHeight)),
            SampleTheme.PrimaryText);
    }

    public void Present() { }
}
