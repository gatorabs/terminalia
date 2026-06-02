# Terminalia - Testing Strategy

## Purpose

Terminalia must include tests to demonstrate engineering quality.

The project should not rely only on manual testing through the terminal.

## Test Projects

```text
tests/
|-- Terminalia.Core.Tests/
|-- Terminalia.Rendering.Terminal.Tests/
`-- Terminalia.Assets.Json.Tests/
```

## Core Tests

Test:

- Entity creation.
- Component add/get/remove.
- Scene entity management.
- Movement system.
- Collision system.
- Event bus.
- Game loop stop behavior.

## Rendering Tests

Test:

- Frame buffer creation.
- Cell update.
- Buffer clear.
- Diff calculation.

Avoid testing actual terminal output in MVP.

## Asset Tests

Test:

- Map JSON parsing.
- Invalid map validation.
- Sprite loading.
- Animation loading.

## Suggested Test Names

- `Entity_Should_Add_And_Return_Component`
- `Scene_Should_Add_Entity`
- `MovementSystem_Should_Update_Position`
- `CollisionSystem_Should_Block_Solid_Collider`
- `FrameBuffer_Should_Update_Cell`
- `JsonAssetLoader_Should_Load_Map`
- `JsonAssetLoader_Should_Reject_Invalid_Map`

## Testing Rules

- Core tests must not require terminal.
- Core tests must not require real files unless necessary.
- Prefer fake implementations for renderer and input.
- Do not write tests that depend on frame timing precision.

## Acceptance Criteria

Testing is accepted when:

- `dotnet test` runs successfully.
- Core behavior has meaningful tests.
- Asset loading has validation tests.
- Rendering buffer has tests.
