namespace Terminalia.Core.Input;

public interface IInputProvider
{
    IReadOnlyCollection<InputCommand> GetCommands();
}
