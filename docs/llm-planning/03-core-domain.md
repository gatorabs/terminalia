# Terminalia - Core Domain

## Purpose

The core domain represents the engine runtime model.

It contains the concepts that exist regardless of how the game is rendered, how input is read, or where assets are stored.

## Main Concepts

### Game

Represents the running application.

Responsibilities:

- Initialize the game.
- Update the game.
- Render the game.
- Stop the game.

Suggested interface:

```csharp
public interface IGame
{
    bool IsRunning { get; }

    void Initialize();
    void HandleInput();
    void Update(GameTime time);
    void Render();
    void Stop();
}
```

### GameTime

Represents frame timing.

Suggested properties:

```csharp
public sealed class GameTime
{
    public TimeSpan Elapsed { get; }
    public TimeSpan Total { get; }
    public int Frame { get; }
}
```

### Scene

Represents a game scene, screen, or level.

Examples:

- `MainMenuScene`
- `DungeonScene`
- `CombatScene`
- `GameOverScene`

Responsibilities:

- Hold entities.
- Hold systems.
- Update systems.
- Publish events.

### Entity

An entity is an object in the game world.

Examples:

- Player.
- Enemy.
- Wall.
- Door.
- Item.

An entity should be lightweight.

### Component

A component stores data.

Examples:

- `TransformComponent`
- `RenderComponent`
- `ColliderComponent`
- `VelocityComponent`
- `HealthComponent`

Components should not contain complex behavior.

### System

A system contains behavior.

Examples:

- `MovementSystem`
- `CollisionSystem`
- `AnimationSystem`
- `RenderSystem`
- `AiSystem`

### Event

An event represents something that happened in the game.

Examples:

- `EntityMovedEvent`
- `CollisionDetectedEvent`
- `SceneChangedEvent`
- `EntityDestroyedEvent`

### Value Objects

Suggested value objects:

- `Position`
- `Size`
- `Velocity`
- `Bounds`
- `EntityId`
- `SceneId`
- `Layer`
- `FrameRate`

## Core Abstractions

The following interfaces must exist in Core:

- `IRenderer`
- `IInputProvider`
- `IAssetLoader`
- `IClock`
- `IGameSystem`
- `IGameEvent`

## Rules

- Core must be independent.
- Core must not use Console directly.
- Core must not use file system directly.
- Core must not use JSON directly.
- Core must not depend on any specific rendering library.

## Acceptance Criteria

This domain is accepted when:

- A scene can contain entities.
- An entity can contain components.
- A system can update entities inside a scene.
- Core can be tested without terminal rendering.
