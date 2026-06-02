# Terminalia - Sample RPG Game

## Purpose

The sample game proves the engine works.

A game engine without a sample game is incomplete.

The sample should be small but polished.

## Project

```text
src/Samples/Terminalia.Sample.Rpg
```

## Sample Concept

A small ASCII dungeon where the player can move around, collide with walls, and see simple entities.

## MVP Features

The sample must include:

- Player character.
- Dungeon map loaded from JSON.
- Walls.
- Floor.
- Enemy placeholder.
- Movement.
- Collision.
- Exit command.
- Simple HUD text.

## Controls

- WASD or arrows = move.
- ESC = exit.

## Example Visual

```text
########################################
#......................................#
#..............#.......................#
#......@.......#..........E............#
#..............#.......................#
########################################

HP: 10    Terminalia Sample RPG    ESC: Exit
```

## Required Files

```text
Terminalia.Sample.Rpg/
|-- Program.cs
|-- SampleRpgGame.cs
|-- GameBootstrapper.cs
|-- scenes/
|   `-- DungeonSceneFactory.cs
|-- assets/
|   |-- maps/
|   |   `-- first_dungeon.json
|   |-- sprites/
|   |   `-- player_idle.txt
|   `-- animations/
|       `-- player_walk.json
`-- README.md
```

## Rules

- Sample game can compose dependencies.
- Sample game can reference all engine modules.
- Sample game should not contain engine core logic.
- Sample game should demonstrate how a developer would use Terminalia.

## Acceptance Criteria

The sample is accepted when:

- Running the sample starts the game.
- The map appears in the terminal.
- The player can move.
- The player cannot cross walls.
- Pressing ESC exits the game.
