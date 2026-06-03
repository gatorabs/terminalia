using Terminalia.Core.Input;

namespace Terminalia.Input.Console;

public sealed class KeyMap
{
    private readonly Dictionary<ConsoleKey, InputCommand> _commands = [];

    public static KeyMap CreateDefault()
    {
        var keyMap = new KeyMap();

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
