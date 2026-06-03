using Terminalia.Core.Ecs;

namespace Terminalia.Core.Tests.Ecs;

public sealed class EntityTests
{
    [Fact]
    public void Entity_Should_Add_And_Return_Component()
    {
        var entity = new Entity("Player");
        var component = new TestComponent(10);

        entity.AddComponent(component);

        Assert.True(entity.HasComponent<TestComponent>());
        Assert.Same(component, entity.GetComponent<TestComponent>());
    }

    [Fact]
    public void Entity_Should_Remove_Component()
    {
        var entity = new Entity("Player");
        entity.AddComponent(new TestComponent(10));

        entity.RemoveComponent<TestComponent>();

        Assert.False(entity.HasComponent<TestComponent>());
        Assert.Null(entity.GetComponent<TestComponent>());
    }

    private sealed record TestComponent(int Value) : IComponent;
}
