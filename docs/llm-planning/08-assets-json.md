# Terminalia - JSON Assets

## Purpose

Terminalia uses JSON files to load maps, sprites, and animations.

This keeps the sample game data-driven.

## Module

Implementation belongs to:

```text
src/Terminalia.Assets.Json
```

## Core Interface

`Terminalia.Core` should define:

```csharp
public interface IAssetLoader
{
    TileMap LoadMap(string assetName);
    Sprite LoadSprite(string assetName);
    Animation LoadAnimation(string assetName);
}
```

## Asset Folder Structure

Sample game assets should use:

```text
assets/
|-- maps/
|   `-- first_dungeon.json
|-- sprites/
|   |-- player_idle.txt
|   `-- enemy_idle.txt
|-- animations/
|   `-- player_walk.json
`-- dialogs/
    `-- intro.json
```

## Map Example

```json
{
  "name": "first_dungeon",
  "width": 40,
  "height": 12,
  "tiles": [
    "########################################",
    "#......................................#",
    "#..............#.......................#",
    "#......@.......#..........E............#",
    "#..............#.......................#",
    "########################################"
  ],
  "legend": {
    "#": "wall",
    ".": "floor",
    "@": "player_spawn",
    "E": "enemy_spawn"
  }
}
```

## Sprite Example

```text
 O
/|\
/ \
```

## Animation Example

```json
{
  "name": "player_walk",
  "frameDurationMs": 150,
  "frames": [
    [
      " O ",
      "/|\\",
      "/ \\"
    ],
    [
      " O ",
      "\\|/",
      "/ \\"
    ]
  ]
}
```

## Rules

- JSON DTOs belong to `Terminalia.Assets.Json`.
- Core domain models belong to `Terminalia.Core`.
- Asset loader maps DTOs to Core models.
- Do not expose JSON-specific types to Core.

## Acceptance Criteria

Assets are accepted when:

- A map can be loaded from JSON.
- A sprite can be loaded from text.
- An animation can be loaded from JSON.
- The sample game can create entities from a map file.
