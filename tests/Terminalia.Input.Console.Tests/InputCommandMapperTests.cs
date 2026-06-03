using Terminalia.Core.Input;

namespace Terminalia.Input.Console.Tests;

public sealed class InputCommandMapperTests
{
    [Theory]
    [InlineData(ConsoleKey.W, InputCommand.MoveUp)]
    [InlineData(ConsoleKey.UpArrow, InputCommand.MoveUp)]
    [InlineData(ConsoleKey.S, InputCommand.MoveDown)]
    [InlineData(ConsoleKey.DownArrow, InputCommand.MoveDown)]
    [InlineData(ConsoleKey.A, InputCommand.MoveLeft)]
    [InlineData(ConsoleKey.LeftArrow, InputCommand.MoveLeft)]
    [InlineData(ConsoleKey.D, InputCommand.MoveRight)]
    [InlineData(ConsoleKey.RightArrow, InputCommand.MoveRight)]
    [InlineData(ConsoleKey.Enter, InputCommand.Confirm)]
    [InlineData(ConsoleKey.Escape, InputCommand.Exit)]
    public void InputCommandMapper_Should_Map_Default_Keys(ConsoleKey key, InputCommand expected)
    {
        var mapper = new InputCommandMapper(KeyMap.CreateDefault());

        var command = mapper.Map(key);

        Assert.Equal(expected, command);
    }

    [Fact]
    public void InputCommandMapper_Should_Return_None_For_Unmapped_Key()
    {
        var mapper = new InputCommandMapper(KeyMap.CreateDefault());

        var command = mapper.Map(ConsoleKey.Spacebar);

        Assert.Equal(InputCommand.None, command);
    }
}
