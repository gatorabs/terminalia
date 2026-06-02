# Terminalia - Solution Structure

## Target Solution Name

```text
Terminalia.sln
```

## Repository Structure

```text
Terminalia/
|-- src/
|   |-- Terminalia.Core/
|   |-- Terminalia.Rendering.Terminal/
|   |-- Terminalia.Input.Console/
|   |-- Terminalia.Assets.Json/
|   |-- Terminalia.Infrastructure/
|   `-- Samples/
|       `-- Terminalia.Sample.Rpg/
|
|-- tests/
|   |-- Terminalia.Core.Tests/
|   |-- Terminalia.Rendering.Terminal.Tests/
|   `-- Terminalia.Assets.Json.Tests/
|
|-- benchmarks/
|   `-- Terminalia.Benchmarks/
|
|-- docs/
|   |-- architecture.md
|   |-- game-loop.md
|   |-- rendering-pipeline.md
|   |-- ecs-lite.md
|   `-- adr/
|       |-- 0001-clean-architecture-adapted.md
|       |-- 0002-ecs-lite.md
|       |-- 0003-terminal-rendering-buffer.md
|       `-- 0004-json-assets.md
|
|-- tools/
|   `-- Terminalia.AssetCompiler/
|
|-- .github/
|   `-- workflows/
|       `-- ci.yml
|
|-- README.md
|-- LICENSE
`-- Terminalia.sln
```

## Project Responsibilities

### Terminalia.Core

Contains the engine core.

Must include:

- Game loop abstractions.
- Scene model.
- Entity model.
- Components.
- Systems.
- Events.
- Value objects.
- Interfaces for rendering, input, and asset loading.

Must not include:

- Console rendering implementation.
- JSON implementation.
- File system implementation.
- External UI library dependency.

### Terminalia.Rendering.Terminal

Contains the terminal renderer.

Must include:

- Terminal renderer.
- Frame buffer.
- Cell model.
- Camera.
- Sprite drawing.
- Terminal viewport.

### Terminalia.Input.Console

Contains console keyboard input.

Must include:

- Keyboard reader.
- Key mapping.
- Input commands.

### Terminalia.Assets.Json

Contains JSON asset loading.

Must include:

- Map loader.
- Sprite loader.
- Animation loader.
- DTOs for JSON definitions.

### Terminalia.Infrastructure

Contains infrastructure concerns.

Must include:

- File system adapter.
- Logging adapter.
- Configuration objects.

### Terminalia.Sample.Rpg

A sample game proving the engine works.

Must include:

- `Program.cs`.
- Sample `Game` class.
- Assets folder.
- Scenes.
- Maps.
- Sprites.
- Sample entities.

## Initial dotnet Commands

```powershell
dotnet new sln -n Terminalia

dotnet new classlib -n Terminalia.Core -o src/Terminalia.Core
dotnet new classlib -n Terminalia.Rendering.Terminal -o src/Terminalia.Rendering.Terminal
dotnet new classlib -n Terminalia.Input.Console -o src/Terminalia.Input.Console
dotnet new classlib -n Terminalia.Assets.Json -o src/Terminalia.Assets.Json
dotnet new classlib -n Terminalia.Infrastructure -o src/Terminalia.Infrastructure

dotnet new console -n Terminalia.Sample.Rpg -o src/Samples/Terminalia.Sample.Rpg

dotnet new xunit -n Terminalia.Core.Tests -o tests/Terminalia.Core.Tests
dotnet new xunit -n Terminalia.Rendering.Terminal.Tests -o tests/Terminalia.Rendering.Terminal.Tests
dotnet new xunit -n Terminalia.Assets.Json.Tests -o tests/Terminalia.Assets.Json.Tests
```

## Project References

```powershell
dotnet add src/Terminalia.Rendering.Terminal/Terminalia.Rendering.Terminal.csproj reference src/Terminalia.Core/Terminalia.Core.csproj
dotnet add src/Terminalia.Input.Console/Terminalia.Input.Console.csproj reference src/Terminalia.Core/Terminalia.Core.csproj
dotnet add src/Terminalia.Assets.Json/Terminalia.Assets.Json.csproj reference src/Terminalia.Core/Terminalia.Core.csproj
dotnet add src/Terminalia.Infrastructure/Terminalia.Infrastructure.csproj reference src/Terminalia.Core/Terminalia.Core.csproj

dotnet add src/Samples/Terminalia.Sample.Rpg/Terminalia.Sample.Rpg.csproj reference src/Terminalia.Core/Terminalia.Core.csproj
dotnet add src/Samples/Terminalia.Sample.Rpg/Terminalia.Sample.Rpg.csproj reference src/Terminalia.Rendering.Terminal/Terminalia.Rendering.Terminal.csproj
dotnet add src/Samples/Terminalia.Sample.Rpg/Terminalia.Sample.Rpg.csproj reference src/Terminalia.Input.Console/Terminalia.Input.Console.csproj
dotnet add src/Samples/Terminalia.Sample.Rpg/Terminalia.Sample.Rpg.csproj reference src/Terminalia.Assets.Json/Terminalia.Assets.Json.csproj
dotnet add src/Samples/Terminalia.Sample.Rpg/Terminalia.Sample.Rpg.csproj reference src/Terminalia.Infrastructure/Terminalia.Infrastructure.csproj
```

## Acceptance Criteria

The structure is accepted when:

- The solution builds successfully.
- Each project has clear responsibility.
- Core has no dependency on adapters.
- Sample game references engine modules.
- Test projects reference the modules they test.
