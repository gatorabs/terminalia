using Terminalia.Core.Input;

namespace Terminalia.Input.Console.Mapping;

public sealed class KeyMapper
{
    private readonly Dictionary<ConsoleKey, InputCommand> _commands = [];

    public static KeyMapper CreateDefault()
    {
        var keyMapper = new KeyMapper();

        keyMapper.Map(ConsoleKey.Enter, InputCommand.Confirm);
        keyMapper.Map(ConsoleKey.Escape, InputCommand.Exit);

        return keyMapper;
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
