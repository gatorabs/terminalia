using Terminalia.Rendering.Terminal;

namespace Terminalia.Rendering.Terminal.Tests;

public sealed class ConsoleFrameBufferTests
{
    [Fact]
    public void FrameBuffer_Should_Update_Cell()
    {
        var buffer = new ConsoleFrameBuffer(4, 3);

        buffer.Draw(2, 1, '@');

        Assert.Equal('@', buffer.GetBackCell(2, 1).Character);
    }

    [Fact]
    public void FrameBuffer_Should_Draw_Text()
    {
        var buffer = new ConsoleFrameBuffer(8, 3);

        buffer.DrawText(1, 1, "RPG");

        Assert.Equal('R', buffer.GetBackCell(1, 1).Character);
        Assert.Equal('P', buffer.GetBackCell(2, 1).Character);
        Assert.Equal('G', buffer.GetBackCell(3, 1).Character);
    }

    [Fact]
    public void FrameBuffer_Should_Clear_Back_Buffer()
    {
        var buffer = new ConsoleFrameBuffer(4, 3);
        buffer.Draw(1, 1, '#');

        buffer.Clear();

        Assert.Equal(' ', buffer.GetBackCell(1, 1).Character);
    }

    [Fact]
    public void FrameBuffer_Should_Calculate_Changed_Cells()
    {
        var buffer = new ConsoleFrameBuffer(4, 3);

        buffer.Draw(1, 1, '@');
        var changes = buffer.GetChangedCells();

        Assert.Contains(changes, change =>
            change.X == 1 &&
            change.Y == 1 &&
            change.Cell.Character == '@');
    }

    [Fact]
    public void FrameBuffer_Should_Clear_Changes_After_Swap()
    {
        var buffer = new ConsoleFrameBuffer(4, 3);
        buffer.Draw(1, 1, '@');

        buffer.SwapBuffers();

        Assert.Empty(buffer.GetChangedCells());
    }

    [Theory]
    [InlineData('\x1B')]
    [InlineData('\n')]
    [InlineData('\r')]
    [InlineData('\x00')]
    [InlineData('\x07')]
    public void FrameBuffer_Should_Sanitize_Control_Characters(char controlChar)
    {
        var buffer = new ConsoleFrameBuffer(4, 3);

        buffer.Draw(1, 1, controlChar);

        Assert.Equal('?', buffer.GetBackCell(1, 1).Character);
    }

    [Fact]
    public void FrameBuffer_DrawText_Should_Sanitize_Control_Characters()
    {
        var buffer = new ConsoleFrameBuffer(8, 3);
        var esc = (char)0x1B;
        var text = string.Concat("A", esc, "Bc");

        buffer.DrawText(0, 0, text);

        Assert.Equal('A', buffer.GetBackCell(0, 0).Character);
        Assert.Equal('?', buffer.GetBackCell(1, 0).Character);
        Assert.Equal('B', buffer.GetBackCell(2, 0).Character);
        Assert.Equal('c', buffer.GetBackCell(3, 0).Character);
    }
}
