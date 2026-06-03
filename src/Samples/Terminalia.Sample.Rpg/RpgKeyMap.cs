namespace Terminalia.Sample.Rpg;

internal sealed class RpgKeyMap
{
    private readonly Dictionary<Keys, RpgCommand> _commands = [];

    public static RpgKeyMap CreateDefault()
    {
        var map = new RpgKeyMap();
        map.Map(Keys.W, RpgCommand.MoveUp);
        map.Map(Keys.Up, RpgCommand.MoveUp);
        map.Map(Keys.S, RpgCommand.MoveDown);
        map.Map(Keys.Down, RpgCommand.MoveDown);
        map.Map(Keys.A, RpgCommand.MoveLeft);
        map.Map(Keys.Left, RpgCommand.MoveLeft);
        map.Map(Keys.D, RpgCommand.MoveRight);
        map.Map(Keys.Right, RpgCommand.MoveRight);
        return map;
    }

    public void Map(Keys key, RpgCommand command) => _commands[key] = command;

    public bool TryGetCommand(Keys key, out RpgCommand command) =>
        _commands.TryGetValue(key, out command);
}
