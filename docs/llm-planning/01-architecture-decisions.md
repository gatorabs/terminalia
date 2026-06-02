# Terminalia - Architecture Decisions

## Chosen Architecture

Terminalia uses:

- Clean Architecture adapted for game engines.
- DDD-lite for domain modeling.
- ECS-lite for entities and components.
- Ports and Adapters for external concerns.

## Why Not Pure Clean Architecture?

Pure Clean Architecture is excellent for business applications, but can become artificial in a game engine.

Game engines are driven by:

- Game loop.
- Rendering.
- Update cycles.
- Input.
- Scenes.
- Entities.
- Systems.

Therefore, Terminalia uses Clean Architecture principles without forcing corporate patterns such as repositories and use cases everywhere.

## Dependency Rule

The dependency rule is:

```text
Engine.Core must not depend on infrastructure.
Infrastructure depends on Core.
Rendering depends on Core.
Input depends on Core.
Assets depends on Core.
Sample games depend on the engine modules.
```

## Main Layers

- `Terminalia.Core`
- `Terminalia.Rendering.Terminal`
- `Terminalia.Input.Console`
- `Terminalia.Assets.Json`
- `Terminalia.Infrastructure`
- `Samples`
- `Tests`
- `Benchmarks`

## Core Responsibilities

`Terminalia.Core` owns:

- Game loop abstractions.
- Scene model.
- Entity model.
- Component model.
- System model.
- Game events.
- Value objects.
- Engine abstractions.

## Adapter Responsibilities

External modules implement Core contracts:

- Terminal rendering.
- Console input.
- JSON asset loading.
- File system.
- Logging.

## Architectural Rule

No class inside `Terminalia.Core` may reference:

- `System.Console` directly.
- JSON serialization directly.
- File system APIs directly.
- Serilog directly.
- Spectre.Console directly.
- Terminal.Gui directly.

## Accepted Patterns

Terminalia may use:

- Adapter.
- Strategy.
- Command.
- Observer/Event Bus.
- Factory.
- State.
- Value Object.
- ECS-lite.

## Avoided Patterns in MVP

Avoid these unless strictly necessary:

- Repository pattern.
- Mediator pipeline.
- Heavy dependency injection container.
- Complex event sourcing.
- Reflection-heavy plugin loading.
- Dynamic scripting runtime.

## Confidence Level

Confidence: High.

This architecture is appropriate for a small but serious terminal game engine in .NET.
