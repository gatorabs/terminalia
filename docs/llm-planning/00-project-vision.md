# Terminalia - Project Vision

## Objective

Terminalia is a modular .NET game engine focused on creating terminal-based ASCII games.

The goal is not to compete with Unity, Godot, or MonoGame. The goal is to build a small, clean, extensible, and educational engine that demonstrates software engineering concepts through game development.

## Main Inspiration

Terminalia is inspired by terminal-looking games and ASCII-art games, such as Stone Story RPG, but it must not copy its mechanics, assets, visuals, or proprietary behavior.

## Main Goals

- Build a game engine in .NET 8 and C#.
- Support terminal rendering using characters.
- Provide a small game loop.
- Support scenes.
- Support entities and components.
- Support basic game systems.
- Support keyboard input.
- Support ASCII sprites.
- Support JSON-based maps and animations.
- Provide at least one playable sample game.

## Non-Goals for MVP

The first version must not include:

- Visual editor.
- Multiplayer.
- Networking.
- Advanced physics.
- Advanced scripting language.
- Plugin marketplace.
- 3D rendering.
- Real-time online features.
- Complex UI framework.
- Save system with database.
- Audio system.

## MVP Definition

The MVP is complete when:

- A player can move inside a terminal map.
- The map is loaded from JSON.
- The player cannot cross walls.
- The game loop runs at a controlled frame rate.
- Rendering uses a terminal buffer.
- There is a sample game demonstrating the engine.
- Core logic has unit tests.

## Technical Identity

Terminalia should look like a serious engineering project, not just a toy.

The repository must include:

- Clear architecture.
- Strong project structure.
- Tests.
- Documentation.
- Sample game.
- Roadmap.
- Architecture Decision Records.
