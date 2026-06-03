using Terminalia.Core.Input;

namespace Terminalia.Input.Console;

public sealed class InputCommandMapper
{
    private readonly KeyMap _keyMap;

    public InputCommandMapper(KeyMap keyMap)
    {
        _keyMap = keyMap ?? throw new ArgumentNullException(nameof(keyMap));
    }

    public InputCommand Map(ConsoleKey key)
    {
        return _keyMap.TryGetCommand(key, out var command)
            ? command
            : InputCommand.None;
    }
}
