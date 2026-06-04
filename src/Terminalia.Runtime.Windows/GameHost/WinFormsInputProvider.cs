using Terminalia.Core.Abstractions.Input;
using Terminalia.Core.Input;

namespace Terminalia.Runtime.Windows.GameHost;

internal sealed class WinFormsInputProvider : IInputProvider
{
    private readonly Queue<InputCommand> _pending = [];

    public void HandleKey(Keys key)
    {
        var command = key switch
        {
            Keys.Up or Keys.W    => InputCommand.MoveUp,
            Keys.Down or Keys.S  => InputCommand.MoveDown,
            Keys.Left or Keys.A  => InputCommand.MoveLeft,
            Keys.Right or Keys.D => InputCommand.MoveRight,
            Keys.Enter           => InputCommand.Confirm,
            Keys.Escape          => InputCommand.Exit,
            _                    => InputCommand.None
        };

        if (command != InputCommand.None)
        {
            _pending.Enqueue(command);
        }
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
