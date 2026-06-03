using Terminalia.Core.Events;

namespace Terminalia.Core.Tests.Events;

public sealed class EventBusTests
{
    [Fact]
    public void EventBus_Should_Publish_Subscribed_Event()
    {
        var bus = new EventBus();
        TestEvent? received = null;

        bus.Subscribe<TestEvent>(gameEvent => received = gameEvent);
        var published = new TestEvent("started");

        bus.Publish(published);

        Assert.Same(published, received);
    }

    private sealed record TestEvent(string Name) : IGameEvent;
}
