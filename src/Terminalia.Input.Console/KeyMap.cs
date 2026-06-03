using Terminalia.Core.Input;

namespace Terminalia.Input.Console;

public sealed class KeyMap
{
    private readonly Dictionary<ConsoleKey, InputCommand> _commands = [];

    public static KeyMap CreateDefault()
    {
        var keyMap = new KeyMap();

        keyMap.Map(ConsoleKey.W, InputCommand.MoveUp);
        keyMap.Map(ConsoleKey.UpArrow, InputCommand.MoveUp);
        keyMap.Map(ConsoleKey.S, InputCommand.MoveDown);
        keyMap.Map(ConsoleKey.DownArrow, InputCommand.MoveDown);
        keyMap.Map(ConsoleKey.A, InputCommand.MoveLeft);
        keyMap.Map(ConsoleKey.LeftArrow, InputCommand.MoveLeft);
        keyMap.Map(ConsoleKey.D, InputCommand.MoveRight);
        keyMap.Map(ConsoleKey.RightArrow, InputCommand.MoveRight);
        keyMap.Map(ConsoleKey.Enter, InputCommand.Confirm);
        keyMap.Map(ConsoleKey.Escape, InputCommand.Exit);

        return keyMap;
    }

    public void Map(ConsoleKey key, InputCommand command)
    {
        _commands[key] = command;
    }

    public bool TryGetCommand(ConsoleKey key, out InputCommand command)
    {
        return _commands.TryGetValue(key, out command);
    }
}
