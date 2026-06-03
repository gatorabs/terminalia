using Terminalia.Core.Input;

namespace Terminalia.Sample.Rpg;

internal sealed class WinFormsInputProvider : IInputProvider
{
    private readonly Queue<InputCommand> _generic = [];
    private readonly Queue<RpgCommand> _rpg = [];
    private readonly RpgKeyMap _keyMap = RpgKeyMap.CreateDefault();

    public void HandleKey(Keys key)
    {
        if (_keyMap.TryGetCommand(key, out var rpgCmd))
            _rpg.Enqueue(rpgCmd);
        else if (key == Keys.Escape)
            _generic.Enqueue(InputCommand.Exit);
    }

    public IReadOnlyCollection<InputCommand> GetCommands()
    {
        if (_generic.Count == 0) return [];
        var cmds = _generic.ToArray();
        _generic.Clear();
        return cmds;
    }

    public IReadOnlyCollection<RpgCommand> GetRpgCommands()
    {
        if (_rpg.Count == 0) return [];
        var cmds = _rpg.ToArray();
        _rpg.Clear();
        return cmds;
    }
}
