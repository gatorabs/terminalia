# Terminalia - Terminal Rendering

## Purpose

Terminalia renders games using terminal characters.

The renderer must be abstracted behind `IRenderer`.

The MVP renderer will use the system console.

## Core Interface

`Terminalia.Core` should define:

```csharp
public interface IRenderer
{
    int Width { get; }
    int Height { get; }

    void Clear();
    void Draw(int x, int y, char character);
    void DrawText(int x, int y, string text);
    void Present();
}
```

## Rendering Module

Implementation belongs to:

```text
src/Terminalia.Rendering.Terminal
```

## Frame Buffer

Do not draw directly to the terminal on every call.

Use a frame buffer.

The renderer should use:

- Back buffer.
- Front buffer.
- Diff rendering.

## Cell Model

Suggested type:

```csharp
public readonly record struct Cell(
    char Character,
    ConsoleColor Foreground,
    ConsoleColor Background
);
```

## Rendering Flow

1. Clear back buffer.
2. Draw map.
3. Draw entities.
4. Draw UI.
5. Compare back buffer with front buffer.
6. Write only changed cells to terminal.
7. Swap buffers.

## Initial Classes

- `TerminalRenderer`
- `ConsoleFrameBuffer`
- `Cell`
- `TerminalCamera`
- `SpriteRenderer`

## Layers

Entities should support layer order.

Example:

- Layer 0 = background/map.
- Layer 1 = items.
- Layer 2 = characters.
- Layer 3 = UI.

## Camera

MVP camera may be simple.

It should support:

- Viewport width.
- Viewport height.
- World offset x.
- World offset y.

## Avoid

Do not use:

- `Console.WriteLine` for the whole screen every frame.
- Direct rendering from entities.
- Rendering logic inside Core components.
- Rendering logic inside sample game classes.

## Acceptance Criteria

Rendering is accepted when:

- The renderer draws characters at coordinates.
- The renderer clears the screen.
- The renderer presents a frame.
- The renderer uses buffer-based rendering.
- The sample game does not call `Console.WriteLine` directly for gameplay rendering.
