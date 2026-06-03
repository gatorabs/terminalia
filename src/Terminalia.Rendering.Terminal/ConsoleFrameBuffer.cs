namespace Terminalia.Rendering.Terminal;

public sealed class ConsoleFrameBuffer
{
    private readonly Cell[,] _backBuffer;
    private readonly Cell[,] _frontBuffer;
    private readonly Cell _clearCell;

    public ConsoleFrameBuffer(int width, int height, Cell? clearCell = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;
        _clearCell = clearCell ?? Cell.Empty;
        _backBuffer = new Cell[height, width];
        _frontBuffer = new Cell[height, width];

        Clear();
        Fill(_frontBuffer, _clearCell);
    }

    public int Width { get; }

    public int Height { get; }

    public void Clear()
    {
        Fill(_backBuffer, _clearCell);
    }

    public void Draw(int x, int y, char character)
    {
        Draw(x, y, new Cell(character, _clearCell.Foreground, _clearCell.Background));
    }

    public void Draw(int x, int y, Cell cell)
    {
        if (!IsInside(x, y))
        {
            return;
        }

        _backBuffer[y, x] = cell;
    }

    public void DrawText(int x, int y, string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        for (var index = 0; index < text.Length; index++)
        {
            Draw(x + index, y, text[index]);
        }
    }

    public Cell GetBackCell(int x, int y)
    {
        ThrowIfOutside(x, y);
        return _backBuffer[y, x];
    }

    public IReadOnlyList<ChangedCell> GetChangedCells()
    {
        var changes = new List<ChangedCell>();

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var backCell = _backBuffer[y, x];
                if (backCell != _frontBuffer[y, x])
                {
                    changes.Add(new ChangedCell(x, y, backCell));
                }
            }
        }

        return changes;
    }

    public string[] GetBackBufferLines()
    {
        var lines = new string[Height];

        for (var y = 0; y < Height; y++)
        {
            var line = new char[Width];
            for (var x = 0; x < Width; x++)
            {
                line[x] = _backBuffer[y, x].Character;
            }

            lines[y] = new string(line);
        }

        return lines;
    }

    public void SwapBuffers()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _frontBuffer[y, x] = _backBuffer[y, x];
            }
        }
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    private void ThrowIfOutside(int x, int y)
    {
        if (!IsInside(x, y))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinates must be inside the frame buffer.");
        }
    }

    private static void Fill(Cell[,] buffer, Cell cell)
    {
        for (var y = 0; y < buffer.GetLength(0); y++)
        {
            for (var x = 0; x < buffer.GetLength(1); x++)
            {
                buffer[y, x] = cell;
            }
        }
    }
}
