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

    [Fact]
    public void EventBus_Should_Deliver_To_All_Handlers_Even_When_One_Throws()
    {
        var bus = new EventBus();
        var received = new List<string>();

        bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("handler 1 failed"));
        bus.Subscribe<TestEvent>(e => received.Add(e.Name));

        var aggregate = Assert.Throws<AggregateException>(() => bus.Publish(new TestEvent("test")));

        Assert.Single(aggregate.InnerExceptions);
        Assert.Contains("test", received);
    }

    [Fact]
    public void EventBus_Should_Collect_All_Handler_Exceptions()
    {
        var bus = new EventBus();

        bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("first"));
        bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("second"));

        var aggregate = Assert.Throws<AggregateException>(() => bus.Publish(new TestEvent("test")));

        Assert.Equal(2, aggregate.InnerExceptions.Count);
    }

    private sealed record TestEvent(string Name) : IGameEvent;
}
