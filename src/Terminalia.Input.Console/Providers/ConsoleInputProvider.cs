using Terminalia.Core.Input;
using Terminalia.Input.Console.Abstractions;
using Terminalia.Input.Console.Mapping;

namespace Terminalia.Input.Console.Providers;

public sealed class ConsoleInputProvider : IInputProvider
{
    private readonly IConsoleKeyboard _keyboard;
    private readonly InputCommandMapper _mapper;

    public ConsoleInputProvider()
        : this(new ConsoleKeyboardProvider(), new InputCommandMapper(KeyMapper.CreateDefault()))
    {
    }

    internal ConsoleInputProvider(IConsoleKeyboard keyboard, InputCommandMapper mapper)
    {
        _keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public IReadOnlyCollection<InputCommand> GetCommands()
    {
        var commands = new List<InputCommand>();

        while (_keyboard.KeyAvailable)
        {
            var key = _keyboard.ReadKey(intercept: true);
            var command = _mapper.Map(key.Key);

            if (command != InputCommand.None)
            {
                commands.Add(command);
            }
        }

        return commands;
    }
}
