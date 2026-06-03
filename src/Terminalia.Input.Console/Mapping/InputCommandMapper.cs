using Terminalia.Core.Input;

namespace Terminalia.Input.Console.Mapping;

public sealed class InputCommandMapper
{
    private readonly KeyMapper _keyMapper;

    public InputCommandMapper(KeyMapper keyMapper)
    {
        _keyMapper = keyMapper ?? throw new ArgumentNullException(nameof(keyMapper));
    }

    public InputCommand Map(ConsoleKey key)
    {
        return _keyMapper.TryGetCommand(key, out var command)
            ? command
            : InputCommand.None;
    }
}
