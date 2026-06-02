# Terminalia - LLM Execution Prompts

## General Instruction for the LLM

You are helping build Terminalia, a .NET 8 terminal-based ASCII game engine.

Follow the existing architecture documents.

Do not create features outside the current requested phase.

Do not introduce unnecessary packages.

Do not make `Terminalia.Core` depend on infrastructure, console, JSON, file system, or rendering implementation.

Prefer simple, readable, and testable code.

Whenever you generate code:

- Include namespaces.
- Include project path.
- Keep classes small.
- Avoid overengineering.
- Add tests when requested.
- Do not skip acceptance criteria.

---

## Prompt 1 - Create Solution Structure

Create the initial .NET solution structure for Terminalia.

Use the following projects:

- `Terminalia.Core`
- `Terminalia.Rendering.Terminal`
- `Terminalia.Input.Console`
- `Terminalia.Assets.Json`
- `Terminalia.Infrastructure`
- `Terminalia.Sample.Rpg`
- `Terminalia.Core.Tests`
- `Terminalia.Rendering.Terminal.Tests`
- `Terminalia.Assets.Json.Tests`

Apply project references according to `02-solution-structure.md`.

Do not implement engine logic yet.

Return:

- Commands used.
- Final folder structure.
- csproj reference summary.

Acceptance criteria:

- Solution builds.
- Project references respect dependency rule.
- Core has no external dependency on adapters.

---

## Prompt 2 - Implement Core Domain Skeleton

Implement the initial core domain in `Terminalia.Core`.

Create:

- `IGame`
- `GameLoop`
- `GameTime`
- `FrameRateSettings`
- `IClock`
- `Scene`
- `SceneId`
- `Entity`
- `EntityId`
- `IComponent`
- `IGameSystem`
- `IGameEvent`
- `EventBus`
- `IRenderer`
- `IInputProvider`
- `IAssetLoader`

Do not implement terminal rendering.
Do not implement JSON loading.
Do not use Console.

Also create basic unit tests in `Terminalia.Core.Tests`.

Acceptance criteria:

- Entities can add and retrieve components.
- Scene can add and return entities.
- Game loop can call game methods in order using fake clock.
- Core tests pass.

---

## Prompt 3 - Implement ECS-lite Components and Systems

Implement MVP components and systems.

Components:

- `TransformComponent`
- `RenderComponent`
- `VelocityComponent`
- `ColliderComponent`
- `AnimatedSpriteComponent`

Value objects:

- `Position`
- `Size`
- `Velocity`
- `Bounds`

Systems:

- `MovementSystem`
- `CollisionSystem`
- `AnimationSystem`

Do not implement terminal rendering.

Add tests for:

- Movement changes position.
- Collision blocks movement.
- Animation advances frames based on time.

Acceptance criteria:

- Systems process entities with required components.
- Tests pass.
- No infrastructure dependency is added to Core.

---

## Prompt 4 - Implement Terminal Renderer

Implement `Terminalia.Rendering.Terminal`.

Create:

- `TerminalRenderer`
- `ConsoleFrameBuffer`
- `Cell`
- `TerminalCamera`
- `SpriteRenderer`

The renderer must implement `IRenderer` from Core.

Use a back buffer and front buffer.

Do not call `Console.WriteLine` repeatedly to redraw the whole screen.

Add tests for frame buffer behavior.

Acceptance criteria:

- Renderer can draw characters by coordinate.
- Renderer can draw text.
- Frame buffer can clear.
- Frame buffer can calculate changed cells.
- Rendering project depends only on Core.

---

## Prompt 5 - Implement Console Input

Implement `Terminalia.Input.Console`.

Create:

- `ConsoleInputProvider`
- `KeyMap`
- `InputCommandMapper`
- `InputCommand` enum if not already present in Core.

Map:

- W or UpArrow to `MoveUp`.
- S or DownArrow to `MoveDown`.
- A or LeftArrow to `MoveLeft`.
- D or RightArrow to `MoveRight`.
- Enter to `Confirm`.
- Escape to `Exit`.

Input must not block the game loop.

Acceptance criteria:

- Input provider implements `IInputProvider`.
- Input commands are mapped correctly.
- Core does not reference Console.

---

## Prompt 6 - Implement JSON Assets

Implement `Terminalia.Assets.Json`.

Create loaders for:

- Maps.
- Sprites.
- Animations.

Create DTOs:

- `MapDefinition`
- `SpriteDefinition` if necessary.
- `AnimationDefinition`

Map DTOs into Core models.

Do not expose JSON-specific DTOs to Core.

Add tests for valid and invalid map loading.

Acceptance criteria:

- Valid map loads successfully.
- Invalid map fails with clear error.
- Sprite text file loads.
- Animation JSON loads.
- Core does not depend on JSON package.

---

## Prompt 7 - Implement Sample RPG

Implement `Terminalia.Sample.Rpg`.

The sample must:

- Configure dependencies manually.
- Start the game loop.
- Load a dungeon map from JSON.
- Create player entity.
- Create wall entities.
- Render map and entities.
- Allow movement with WASD/arrows.
- Prevent player from crossing walls.
- Exit with ESC.

Do not add combat yet.

Acceptance criteria:

- `dotnet run --project src/Samples/Terminalia.Sample.Rpg` starts the game.
- Player moves.
- Player cannot cross walls.
- ESC exits.
- No engine logic is duplicated inside the sample.

---

## Prompt 8 - Add Documentation and README

Create repository documentation.

Files:

- `README.md`
- `docs/architecture.md`
- `docs/game-loop.md`
- `docs/rendering-pipeline.md`
- `docs/ecs-lite.md`
- `docs/adr/0001-clean-architecture-adapted.md`
- `docs/adr/0002-ecs-lite.md`
- `docs/adr/0003-terminal-rendering-buffer.md`
- `docs/adr/0004-json-assets.md`

README must include:

- Project description.
- Architecture summary.
- Features.
- How to run sample.
- Project structure.
- Roadmap.
- Contribution notes.

Acceptance criteria:

- Documentation matches implemented architecture.
- No claims about features that do not exist yet.
- README is suitable for GitHub portfolio.

---

## Master Prompt

Use this prompt before any code generation:

```text
You are helping build Terminalia, a .NET 8 game engine for ASCII/terminal games.

Before generating code, read and follow these principles:

1. The architecture is Clean Architecture adapted + DDD-lite + ECS-lite + Ports and Adapters.
2. Terminalia.Core cannot depend on Console, JSON, FileSystem, Serilog, Spectre.Console, Terminal.Gui, or any external implementation.
3. External implementations must live in adapter projects:
   - Terminalia.Rendering.Terminal
   - Terminalia.Input.Console
   - Terminalia.Assets.Json
   - Terminalia.Infrastructure
4. Do not add features outside the requested scope.
5. Do not create a visual editor, multiplayer, scripting, audio, or advanced physics in the MVP.
6. Prefer simple, testable, incremental code.
7. Always report the path of each file created or changed.
8. Always respect the acceptance criteria of the current phase document.
9. If there is ambiguity, choose the simplest option and document the decision.
10. Do not invent unnecessary dependencies.

Now execute only the requested phase.
```

## Recommended Execution Order

Read the planning documents in this order:

1. `00-project-vision.md`
2. `01-architecture-decisions.md`
3. `02-solution-structure.md`
4. `03-core-domain.md`
5. `04-game-loop.md`
6. `05-entity-component-system.md`
7. `06-rendering-terminal.md`
8. `07-input-system.md`
9. `08-assets-json.md`
10. `09-sample-game.md`
11. `10-testing-strategy.md`
12. `11-roadmap.md`
13. `12-llm-execution-prompts.md`

Then execute one prompt at a time:

1. Prompt 1: create solution.
2. Prompt 2: create Core.
3. Prompt 3: create ECS-lite.
4. Prompt 4: create Renderer.
5. Prompt 5: create Input.
6. Prompt 6: create Assets.
7. Prompt 7: create Sample RPG.
8. Prompt 8: create documentation.

## Critical Recommendation

Confidence: High.

For this project to work well as a portfolio project, the differentiator will not be having many features. The differentiator will be showing:

- Good architecture.
- Clear separation of responsibilities.
- Working engine.
- Playable sample.
- Tests.
- Strong README.
- Documented technical decisions.
- Realistic roadmap.
