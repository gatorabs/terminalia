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

        keyMapper.Map(ConsoleKey.UpArrow, InputCommand.MoveUp);
        keyMapper.Map(ConsoleKey.DownArrow, InputCommand.MoveDown);
        keyMapper.Map(ConsoleKey.LeftArrow, InputCommand.MoveLeft);
        keyMapper.Map(ConsoleKey.RightArrow, InputCommand.MoveRight);
        keyMapper.Map(ConsoleKey.W, InputCommand.MoveUp);
        keyMapper.Map(ConsoleKey.S, InputCommand.MoveDown);
        keyMapper.Map(ConsoleKey.A, InputCommand.MoveLeft);
        keyMapper.Map(ConsoleKey.D, InputCommand.MoveRight);

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
