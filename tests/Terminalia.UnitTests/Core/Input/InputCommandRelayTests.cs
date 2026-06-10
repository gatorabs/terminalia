using Terminalia.Core.Input;

namespace Terminalia.UnitTests.Core.Input;

public sealed class InputCommandRelayTests
{
    [Fact]
    public void Relay_Should_Return_Empty_By_Default()
    {
        var relay = new InputCommandRelay();

        Assert.Empty(relay.GetCommands());
    }

    [Fact]
    public void Relay_Should_Return_Published_Commands()
    {
        var relay = new InputCommandRelay();
        relay.Publish([InputCommand.MoveUp, InputCommand.Confirm]);

        Assert.Equal([InputCommand.MoveUp, InputCommand.Confirm], relay.GetCommands());
    }

    [Fact]
    public void Relay_Should_Drain_Commands_After_Read()
    {
        var relay = new InputCommandRelay();
        relay.Publish([InputCommand.MoveDown]);

        relay.GetCommands();

        Assert.Empty(relay.GetCommands());
    }
}
