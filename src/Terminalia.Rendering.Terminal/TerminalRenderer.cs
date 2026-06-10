using Terminalia.Core.Abstractions.Rendering;

namespace Terminalia.Rendering.Terminal;

public sealed class TerminalRenderer : IRenderer
{
    private readonly ConsoleFrameBuffer _frameBuffer;
    private readonly TextWriter _output;

    public TerminalRenderer(int width, int height)
        : this(width, height, Console.Out)
    {
    }

    internal TerminalRenderer(int width, int height, TextWriter output)
    {
        _frameBuffer = new ConsoleFrameBuffer(width, height);
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public int Width => _frameBuffer.Width;

    public int Height => _frameBuffer.Height;

    public void Clear()
    {
        _frameBuffer.Clear();
    }

    public void Draw(int x, int y, char character)
    {
        _frameBuffer.Draw(x, y, character);
    }

    public void DrawText(int x, int y, string text)
    {
        _frameBuffer.DrawText(x, y, text);
    }

    public void DrawTextCentered(int y, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var x = Math.Max(0, (Width - text.Length) / 2);
        _frameBuffer.DrawText(x, y, text);
    }

    public void Present()
    {
        if (Console.IsOutputRedirected)
        {
            PresentSnapshot();
            return;
        }

        foreach (var change in _frameBuffer.GetChangedCells())
        {
            Console.SetCursorPosition(change.X, change.Y);
            Console.ForegroundColor = change.Cell.Foreground;
            Console.BackgroundColor = change.Cell.Background;
            Console.Write(change.Cell.Character);
        }

        Console.ResetColor();
        _frameBuffer.SwapBuffers();
    }

    private void PresentSnapshot()
    {
        foreach (var line in _frameBuffer.GetBackBufferLines())
        {
            _output.WriteLine(line.TrimEnd());
        }

        _frameBuffer.SwapBuffers();
    }
}
