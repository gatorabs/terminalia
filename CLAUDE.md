# Terminalia — Guia de Desenvolvimento para Claude

Engine de jogo ASCII modular em .NET, com editor WinForms integrado. Segue Clean Architecture + DDD-lite + ECS-lite + Ports & Adapters.

---

## Regras de arquitetura (não negociáveis)

### Dependency Rule

```
Terminalia.Core          (zero dependências externas)
        ↑
        ├── Terminalia.Rendering.Terminal
        ├── Terminalia.Input.Console
        ├── Terminalia.Assets.Json
        └── Terminalia.Editor
                ↑
                └── Samples/*
```

- `Terminalia.Core` nunca referencia `System.Console`, JSON, file system, WinForms, ou qualquer adapter.
- Adapters (`Rendering.Terminal`, `Input.Console`, `Assets.Json`) dependem de Core e implementam suas interfaces.
- `Terminalia.Editor` depende de Core. Core jamais depende do Editor.
- Samples dependem de Core e dos adapters necessários. Nunca do Editor.

### Camadas e responsabilidades

| Camada | Projeto | Responsabilidade |
|--------|---------|-----------------|
| Domain | `Core/Ecs/`, `Core/Scenes/`, `Core/Assets/` | Entidades, componentes, sistemas, regras de jogo |
| Application | `Core/Runtime/` | GameLoop, orquestração de cena, ciclo de vida |
| Port | `Core/Rendering/`, `Core/Input/` | Interfaces abstratas (`IRenderer`, `IInputProvider`) |
| Adapter | `Rendering.Terminal`, `Input.Console`, `Assets.Json` | Implementações concretas das portas |
| UI | `Editor/` | WinForms; não contém lógica de domínio |

---

## DDD para jogos

### Tipos canônicos

| Tipo DDD | Mapeamento no projeto | Regra |
|----------|-----------------------|-------|
| Aggregate Root | `Scene` | Ponto de entrada para adicionar/remover entidades e sistemas |
| Entity | `Entity` | Identificada por `EntityId`; contém componentes |
| Value Object | `Position`, `Size`, `Velocity`, `Bounds` | `readonly record struct`; imutável; sem identidade |
| Domain Event | `IGameEvent` (a criar) | Publicado via `Scene`; consumido por sistemas |
| Repository | `IAssetLoader` | Único ponto de acesso a ativos externos |
| Service | `GameLoop` | Orquestra ciclo sem conter estado de domínio |

### Regras DDD

- Value Objects nunca têm ID. Se precisar de identidade, é Entity.
- Componentes são **dados puros** (`IComponent`): sem métodos de negócio, sem referência a outros componentes.
- Sistemas contêm **toda a lógica**: leem/escrevem componentes, publicam eventos.
- `Scene` não expõe coleções internas. Acesso via métodos específicos (`GetEntitiesWithComponent<T>`).
- Domain Events transitam pelo `Scene`, não por singletons globais.

---

## ECS-lite

```
Entity        → container de componentes, identificado por EntityId
IComponent    → dados puros, sem lógica
IGameSystem   → lógica por frame; recebe IEnumerable<Entity> + GameTime
Scene         → Aggregate Root; gerencia entidades e sistemas
GameLoop      → chama Scene.Update + Scene.Render a cada frame
```

### Ciclo por frame

```
Input → Update (cada IGameSystem.Update) → Render (IRenderer via RenderSystem)
```

### Criando novos componentes

- Arquivo: `Core/Ecs/Components/NomeComponent.cs`
- Namespace: `Terminalia.Core.Ecs.Components`
- Tipo: `sealed class` implementando `IComponent`
- Apenas campos/propriedades de dados. Zero métodos de negócio.

### Criando novos sistemas

- Arquivo: `Core/Ecs/Systems/NomeSystem.cs`
- Namespace: `Terminalia.Core.Ecs.Systems`
- Tipo: `sealed class` implementando `IGameSystem`
- Recebe dependências via construtor (ex: `IRenderer`, `IInputProvider`)

---

## Regras anti-monolítico

### Limites por arquivo

| Tipo de arquivo | Limite recomendado |
|-----------------|-------------------|
| `Form` / `UserControl` | 150 LOC |
| Serviço / Sistema | 200 LOC |
| Qualquer outro | 300 LOC |

Ao atingir o limite: extrair responsabilidade para novo arquivo antes de continuar.

### Inner classes

- **Proibidas** se públicas ou com mais de 20 LOC.
- Toda inner class acima desse tamanho ganha arquivo próprio no mesmo diretório.
- Exemplo: `TerminalMenu` era inner class de `StartPage` → deve ser `Screens/StartPage/TerminalMenu.cs`.

### Partial classes

Usar para separar responsabilidades em `Form`s e `UserControl`s grandes:

```
MainForm.cs           → declaração de campos e construtor
MainForm.Events.cs    → todos os event handlers
MainForm.Layout.cs    → InitializeComponent e configuração visual (se necessário)
```

---

## Organização dos Adapters

Projetos adapter (`Rendering.Terminal`, `Input.Console`, `Assets.Json`) seguem esta estrutura interna:

```
Terminalia.Input.Console/
├── Terminalia.Input.Console.csproj   (InternalsVisibleTo definido aqui, não em AssemblyInfo.cs)
├── Abstractions/
│   └── IConsoleKeyboard.cs           (prefixo I — convenção de interface)
├── Mapping/
│   ├── InputCommandMapper.cs         (sufixo Mapper)
│   └── KeyMapper.cs                  (sufixo Mapper)
└── Providers/
    ├── ConsoleInputProvider.cs       (sufixo Provider — implementa a porta do Core)
    └── ConsoleKeyboardProvider.cs    (sufixo Provider)
```

### Regras dos Adapters

- `InternalsVisibleTo` vai no `.csproj` via `<InternalsVisibleTo Include="..." />`, nunca em `AssemblyInfo.cs`.
- Interfaces internas (abstrações para testabilidade) ficam em `Abstractions/`; o prefixo `I` já identifica o diretório.
- Cada subdiretório impõe um sufixo uniforme a todos os seus arquivos (`Mapping/` → `*Mapper`, `Providers/` → `*Provider`).
- Namespace espelha o diretório: `Terminalia.Input.Console.Providers`, `Terminalia.Input.Console.Mapping`, etc.

---

## Organização do Editor (WinForms)

### Estrutura de diretórios alvo

```
Terminalia.Editor/
├── Program.cs
├── MainForm.cs                      (<150 LOC — orquestração)
├── MainForm.Events.cs               (partial — wiring de eventos)
│
├── Shell/
│   ├── EditorShell.cs               (composição de layout)
│   └── EditorShell.Layout.cs        (partial — init visual)
│
├── Screens/
│   ├── StartPage/
│   │   ├── StartPage.cs             (UserControl host, <100 LOC)
│   │   └── TerminalMenu.cs          (componente extraído)
│   │
│   └── Workspace/
│       ├── MapCanvas.cs             (<200 LOC)
│       └── Tools/
│           ├── PaintTool.cs
│           ├── FillTool.cs
│           └── PickerTool.cs
│
├── Controls/
│   ├── Canvas/                      (namespace ...Controls.Canvas; sufixo Canvas)
│   │   ├── MapCanvas.cs
│   │   └── EmbeddedCanvas.cs
│   │
│   ├── CommandBar/                  (namespace ...Controls.CommandBar; prefixo CommandBar)
│   │   ├── CommandBar.cs
│   │   └── CommandBarItem.cs
│   │
│   ├── Inspector/                   (namespace ...Controls.Inspector; prefixo Inspector)
│   │   └── InspectorPanel.cs
│   │
│   └── Shared/                      (namespace ...Controls.Shared; sufixo semântico CLAUDE.md)
│       ├── ModernButton.cs
│       ├── CardPanel.cs
│       └── LoadingOverlay.cs
│
├── Services/
│   └── EditorFileService.cs
│
├── State/
│   ├── EditorState.cs
│   └── EditorToolMode.cs
│
└── Styling/
    ├── Colors/
    │   └── EditorColors.cs          (Color tokens + ColorDistance + Lerp)
    ├── Fonts/
    │   └── EditorFonts.cs           (Font tokens)
    ├── Layout/
    │   └── EditorLayout.cs          (const int — dimensões e espaçamentos)
    ├── Drawing/
    │   └── EditorDrawing.cs         (CreateRoundedPath)
    └── Controls/
        └── EditorControls.cs        (Create* + Style* + ApplyDarkScrollBars)
```

### Regras do Editor

- Cada tela tem subdiretório próprio em `Screens/`.
- Nenhum controle define o layout de outro controle (sem `SetBounds` cruzado).
- `Styling/` é a **única** fonte de cores, fontes, espaçamentos e utilitários de desenho. Zero magic strings de cor.
- `MainForm` apenas orquestra: não desenha, não lê arquivo diretamente, não conhece detalhes de controles filhos.
- Comunicação entre controles via **eventos C#** (não chamadas diretas entre siblings).
- Arquivos dentro de um subdiretório de `Styling/` seguem o sufixo do diretório: `Colors/` → `Editor*Colors`, `Fonts/` → `Editor*Fonts`, etc.

---

## Convenções de nomenclatura

### Namespaces

O namespace espelha o caminho de diretório **sempre**:

```
src/Terminalia.Editor/Screens/StartPage/TerminalMenu.cs
→ namespace Terminalia.Editor.Screens.StartPage
```

### Sufixos por responsabilidade

| Sufixo | Quando usar |
|--------|-------------|
| `*Form` | Janela principal (`MainForm`) |
| `*Panel` | Container/painel WinForms sem lógica própria |
| `*Section` | Subdivisão de um painel (Inspector sections) |
| `*Control` | UserControl autossuficiente |
| `*Canvas` | Surface de desenho customizado |
| `*Component` | Dado puro no ECS |
| `*System` | Sistema ECS com lógica por frame |
| `*Service` | Lógica de aplicação sem estado de domínio |
| `*State` | Holder de estado mutable (editor state) |
| `*Provider` | Adapter de porta (Input, Render) |
| `*Loader` | Adapter de repositório (Assets) |
| `*Animator` | Lógica isolada de animação UI |

### Tipos de domínio

- Value Objects: `readonly record struct`
- Entidades e sistemas: `sealed class`
- Interfaces de porta: prefixo `I` + nome semântico (`IRenderer`, não `IRenderingService`)

---

## Testes

### O que testar

| Projeto | Estratégia |
|---------|-----------|
| `Core` | Unit tests; usar fakes inline (sem mocks de framework) |
| `Rendering.Terminal` | Unit tests no frame buffer e delta de células |
| `Input.Console` | Unit tests no mapeamento de teclas |
| `Assets.Json` | Unit tests de serialização/deserialização |
| `Editor` | **Sem testes de UI**; testar apenas `Services/` e `State/` |

### Fakes vs Mocks

Preferir implementações fake concretas a mocks de framework:

```csharp
// Preferido
class FakeRenderer : IRenderer { public List<string> Drawn = new(); ... }

// Evitar
var mock = new Mock<IRenderer>();
```

### Localização dos testes

```
tests/
├── Terminalia.Core.Tests/
│   ├── Ecs/
│   └── Runtime/
├── Terminalia.Rendering.Terminal.Tests/
├── Terminalia.Input.Console.Tests/
└── Terminalia.Assets.Json.Tests/
```

---

## Checklist de entrega

Antes de considerar uma tarefa concluída, verificar cada item:

- [ ] Nenhum arquivo novo excede o limite de LOC do seu tipo
- [ ] Nenhuma inner class pública ou com mais de 20 LOC
- [ ] Dependency Rule preservada (Core sem deps externas; Editor não é referenciado por Core)
- [ ] Namespace espelha o caminho de diretório exato
- [ ] Cores e fontes usadas exclusivamente via `EditorTheme`
- [ ] Comunicação entre controles irmãos feita via eventos, não chamadas diretas
- [ ] Novos componentes ECS são dados puros (sem métodos de negócio)
- [ ] Novos sistemas ECS recebem dependências via construtor
- [ ] Value Objects são `readonly record struct`

---

## Referências internas

- Documentação técnica detalhada: `docs/llm-planning/`
- Decisões de arquitetura: `docs/llm-planning/01-architecture-decisions.md`
- Estrutura da solução: `docs/llm-planning/02-solution-structure.md`
- Especificação ECS: `docs/llm-planning/05-entity-component-system.md`
