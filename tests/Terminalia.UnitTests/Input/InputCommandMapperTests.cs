using Terminalia.Core.Input;
using Terminalia.Input.Console.Mapping;

namespace Terminalia.UnitTests.Input;

public sealed class InputCommandMapperTests
{
    [Theory]
    [InlineData(ConsoleKey.Enter, InputCommand.Confirm)]
    [InlineData(ConsoleKey.Escape, InputCommand.Exit)]
    public void InputCommandMapper_Should_Map_Default_Keys(ConsoleKey key, InputCommand expected)
    {
        var mapper = new InputCommandMapper(KeyMapper.CreateDefault());

        var command = mapper.Map(key);

        Assert.Equal(expected, command);
    }

    [Fact]
    public void InputCommandMapper_Should_Return_None_For_Unmapped_Key()
    {
        var mapper = new InputCommandMapper(KeyMapper.CreateDefault());

        var command = mapper.Map(ConsoleKey.Spacebar);

        Assert.Equal(InputCommand.None, command);
    }
}
