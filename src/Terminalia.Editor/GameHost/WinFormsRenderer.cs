using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Editor.Styling.Colors;

namespace Terminalia.Editor.GameHost;

internal sealed class WinFormsRenderer : IRenderer, IDisposable
{
    private readonly StringFormat _centerFormat = new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center,
        FormatFlags = StringFormatFlags.NoClip
    };

    private Bitmap? _backBuffer;
    private Graphics? _frameGraphics;
    private Font _font = new(FontFamily.GenericMonospace, 14, FontStyle.Bold);
    private int _cellSize = 32;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public Bitmap? BackBuffer => _backBuffer;

    public void Configure(int tileWidth, int tileHeight, int cellSize)
    {
        _cellSize = cellSize;
        Width = tileWidth;
        Height = tileHeight;

        _frameGraphics?.Dispose();
        _frameGraphics = null;
        _backBuffer?.Dispose();
        _backBuffer = new Bitmap(tileWidth * cellSize, tileHeight * cellSize);
        _font.Dispose();
        _font = new Font(FontFamily.GenericMonospace, Math.Max(7f, cellSize * 0.45f), FontStyle.Bold);
    }

    public void Clear()
    {
        if (_backBuffer is null) return;
        _frameGraphics?.Dispose();
        _frameGraphics = Graphics.FromImage(_backBuffer);
        _frameGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        _frameGraphics.Clear(EditorColors.TerminalBackground);
    }

    public void Draw(int x, int y, char character)
    {
        if (_frameGraphics is null) return;
        var bounds = new RectangleF(x * _cellSize, y * _cellSize, _cellSize, _cellSize);
        using var brush = new SolidBrush(GetCharColor(character));
        _frameGraphics.DrawString(character.ToString(), _font, brush, bounds, _centerFormat);
    }

    public void DrawText(int x, int y, string text)
    {
        if (_frameGraphics is null) return;
        using var brush = new SolidBrush(EditorColors.PrimaryText);
        _frameGraphics.DrawString(text, _font, brush, x * _cellSize, y * _cellSize);
    }

    public void Present()
    {
        _frameGraphics?.Dispose();
        _frameGraphics = null;
    }

    public void Dispose()
    {
        _frameGraphics?.Dispose();
        _backBuffer?.Dispose();
        _font.Dispose();
        _centerFormat.Dispose();
    }

    private static Color GetCharColor(char c) => c switch
    {
        '#' => Color.FromArgb(148, 163, 184),
        '.' => Color.FromArgb(71, 85, 105),
        '@' => Color.FromArgb(96, 165, 250),
        '*' => Color.FromArgb(250, 204, 21),
        _ => EditorColors.PrimaryText
    };
}
