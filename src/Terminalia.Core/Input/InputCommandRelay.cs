using Terminalia.Core.Abstractions.Input;

namespace Terminalia.Core.Input;

public sealed class InputCommandRelay : IInputProvider
{
    private readonly List<InputCommand> _pending = [];

    public void Publish(IReadOnlyCollection<InputCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        _pending.AddRange(commands);
    }

    public IReadOnlyCollection<InputCommand> GetCommands()
    {
        if (_pending.Count == 0)
        {
            return [];
        }

        var commands = _pending.ToArray();
        _pending.Clear();
        return commands;
    }
}
