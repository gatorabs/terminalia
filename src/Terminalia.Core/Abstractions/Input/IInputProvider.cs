using Terminalia.Core.Input;

namespace Terminalia.Core.Abstractions.Input;

public interface IInputProvider
{
    IReadOnlyCollection<InputCommand> GetCommands();
}
