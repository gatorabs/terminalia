# Terminalia - ECS-lite

## Purpose

Terminalia uses an ECS-lite model.

This means:

- Entity identifies a game object.
- Component stores data.
- System executes behavior.

This is not a full high-performance ECS framework.

The goal is simplicity, readability, and architecture clarity.

## Entity

An entity has:

- `EntityId`
- Name.
- Active state.
- List of components.

Suggested class:

```csharp
public sealed class Entity
{
    public EntityId Id { get; }
    public string Name { get; }
    public bool IsActive { get; private set; }

    public void AddComponent(IComponent component);
    public T? GetComponent<T>() where T : class, IComponent;
    public bool HasComponent<T>() where T : class, IComponent;
    public void RemoveComponent<T>() where T : class, IComponent;
}
```

## Component

A component should be data-oriented.

Suggested interface:

```csharp
public interface IComponent
{
}
```

## Initial Components

Required MVP components:

- `TransformComponent`
- `RenderComponent`
- `VelocityComponent`
- `ColliderComponent`
- `AnimatedSpriteComponent`

Optional after MVP:

- `HealthComponent`
- `InventoryComponent`
- `AiComponent`
- `DialogueComponent`
- `InteractableComponent`

## TransformComponent

Stores position.

```csharp
public sealed class TransformComponent : IComponent
{
    public Position Position { get; set; }
}
```

## RenderComponent

Stores render data.

```csharp
public sealed class RenderComponent : IComponent
{
    public char Character { get; set; }
    public int Layer { get; set; }
    public bool IsVisible { get; set; }
}
```

## ColliderComponent

Stores collision data.

```csharp
public sealed class ColliderComponent : IComponent
{
    public Size Size { get; set; }
    public bool IsSolid { get; set; }
}
```

## System

A system updates a scene.

Suggested interface:

```csharp
public interface IGameSystem
{
    void Update(Scene scene, GameTime time);
}
```

## Initial Systems

Required MVP systems:

- `MovementSystem`
- `CollisionSystem`
- `AnimationSystem`
- `RenderSystem`

## Rules

- Components should avoid business logic.
- Systems should contain behavior.
- Entity should not know specific systems.
- Systems should not depend on terminal rendering directly.
- Rendering must happen through `IRenderer`.

## Acceptance Criteria

The ECS-lite implementation is accepted when:

- Entities can receive components.
- Components can be retrieved by type.
- Systems can process entities with required components.
- Movement can be implemented as a system.
- Collision can be implemented as a system.
