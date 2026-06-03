using Terminalia.Core.Input;
using Terminalia.Input.Console.Abstractions;
using Terminalia.Input.Console.Mapping;
using Terminalia.Input.Console.Providers;

namespace Terminalia.Input.Console.Tests;

public sealed class ConsoleInputProviderTests
{
    [Fact]
    public void ConsoleInputProvider_Should_Return_No_Commands_When_No_Key_Is_Available()
    {
        var keyboard = new FakeConsoleKeyboard();
        var provider = new ConsoleInputProvider(
            keyboard,
            new InputCommandMapper(KeyMapper.CreateDefault()));

        var commands = provider.GetCommands();

        Assert.Empty(commands);
    }

    [Fact]
    public void ConsoleInputProvider_Should_Map_Available_Keys_To_Commands()
    {
        var keyboard = new FakeConsoleKeyboard(ConsoleKey.Enter, ConsoleKey.Escape);
        var provider = new ConsoleInputProvider(
            keyboard,
            new InputCommandMapper(KeyMapper.CreateDefault()));

        var commands = provider.GetCommands();

        Assert.Equal([InputCommand.Confirm, InputCommand.Exit], commands);
    }

    private sealed class FakeConsoleKeyboard : IConsoleKeyboard
    {
        private readonly Queue<ConsoleKeyInfo> _keys;

        public FakeConsoleKeyboard(params ConsoleKey[] keys)
        {
            _keys = new Queue<ConsoleKeyInfo>(keys.Select(key =>
                new ConsoleKeyInfo('\0', key, shift: false, alt: false, control: false)));
        }

        public bool KeyAvailable => _keys.Count > 0;

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            return _keys.Dequeue();
        }
    }
}
