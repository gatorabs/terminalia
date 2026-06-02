# Terminalia - Input System

## Purpose

The input system reads player input and converts it into engine commands.

Input must be abstracted so the engine core does not depend on `Console.ReadKey`.

## Core Interface

`Terminalia.Core` should define:

```csharp
public interface IInputProvider
{
    IReadOnlyCollection<InputCommand> GetCommands();
}
```

## Input Commands

Suggested commands:

- `MoveUp`
- `MoveDown`
- `MoveLeft`
- `MoveRight`
- `Confirm`
- `Cancel`
- `Exit`
- `None`

Suggested type:

```csharp
public enum InputCommand
{
    None,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Confirm,
    Cancel,
    Exit
}
```

## Console Implementation

Implementation belongs to:

```text
src/Terminalia.Input.Console
```

## Required Classes

- `ConsoleInputProvider`
- `KeyMap`
- `InputCommandMapper`

## Default Key Mapping

- W or UpArrow = `MoveUp`
- S or DownArrow = `MoveDown`
- A or LeftArrow = `MoveLeft`
- D or RightArrow = `MoveRight`
- Enter = `Confirm`
- Escape = `Exit`

## Rules

- Core must not call `Console.ReadKey`.
- Console input implementation may use `Console.KeyAvailable`.
- Input must not block the game loop.
- Input provider should return zero or more commands per frame.

## Acceptance Criteria

Input is accepted when:

- Pressing movement keys generates movement commands.
- Pressing Escape generates `Exit`.
- Game loop does not freeze waiting for input.
- Core has no direct dependency on Console.
