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
        └── Terminalia.TileMapGame
                    ↑
                    └── Terminalia.Runtime.Windows   (TileMapGame + Assets.Json)
                                ↑
                                └── Terminalia.Editor (Runtime.Windows + Assets.Json)
```

- `Terminalia.Core` nunca referencia `System.Console`, JSON, file system, WinForms, ou qualquer adapter.
- Adapters (`Rendering.Terminal`, `Input.Console`, `Assets.Json`) dependem de Core e implementam suas interfaces.
- `Terminalia.TileMapGame` contém as regras do jogo tile-based (`SceneAssembler`, `TileMapGameSession`, `EntityRule`); depende apenas de Core.
- `Terminalia.Runtime.Windows` hospeda o jogo em WinForms (`WinFormsGameHost`, `WinFormsRenderer`, `WinFormsInputProvider`, `GameWindow`).
- `Terminalia.Editor` referencia `Assets.Json` + `Runtime.Windows`; o Core chega transitivamente. Core jamais depende do Editor.
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

Interpretação dos limites:
- O limite é **por arquivo**: cada partial class conta individualmente.
- 150 LOC vale para tipos derivados de `Form`/`UserControl`. Controles custom derivados de `Control`/`Panel`/`Button` (canvases, seções, botões) seguem o limite de 300, com alvo recomendado de 200.

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
- Adapters pequenos (< ~500 LOC totais, ex.: `Rendering.Terminal`, `Assets.Json`) podem manter estrutura plana na raiz; adotar subdiretórios quando o projeto crescer.

---

## Organização do Editor (WinForms)

### Estrutura de diretórios alvo

```
Terminalia.Editor/
├── Program.cs
├── MainForm/                        (partials da MainForm — cada um ≤150 LOC)
│   ├── MainForm.cs                  (campos, construtor, overrides, DllImport)
│   ├── MainForm.Events.cs           (WireEvents: wiring de todos os eventos)
│   ├── MainForm.Actions.cs          (ações de arquivo: new/open/save)
│   ├── MainForm.Run.cs              (export, run externo/embutido, game definition)
│   ├── MainForm.Status.cs           (status bar, dirty/clean, consultas de mapas)
│   └── MainForm.Tabs.cs             (gerência de abas)
│
├── Shell/
│   ├── EditorShell.cs               (composição e API do shell)
│   ├── EditorShell.Layout.cs        (partial — init visual)
│   ├── EditorShell.Facade.cs        (partial — operações compostas sobre os filhos)
│   └── ClickOutsideFilter.cs        (IMessageFilter extraído)
│
├── Screens/
│   ├── StartPage/
│   │   ├── StartPage.cs             (UserControl host)
│   │   └── TerminalMenu.cs          (componente extraído)
│   └── Dialogs/
│       └── MapNameDialog.cs         (sufixo Dialog)
│
├── Controls/
│   ├── Canvas/                      (namespace ...Controls.Canvas; sufixo Canvas)
│   │   ├── MapCanvas.cs             (+ partials .Painting / .Interaction / .Cursor)
│   │   └── EngineCanvas.cs
│   ├── CommandBar/                  (namespace ...Controls.CommandBar; prefixo CommandBar)
│   │   ├── CommandBar.cs            (+ partials .Painting / .Interaction / .Layout)
│   │   └── CommandBarItem.cs
│   ├── Inspector/                   (namespace ...Controls.Inspector; prefixo Inspector)
│   │   ├── InspectorPanel.cs        (+ partials .Sections / .Palette)
│   │   ├── Inspector*Section.cs     (seções), InspectorSectionCard.cs, InspectorEventArgs.cs
│   │   └── InspectorSection.cs      (enum)
│   └── Shared/                      (namespace ...Controls.Shared)
│       ├── ModernButton.cs
│       ├── CardPanel.cs
│       ├── MapTabBar.cs
│       └── LoadingOverlay.cs
│
├── Selection/                       (descrição/edição de objetos selecionados)
│   ├── Descriptors/                 (*TileDescriptor)
│   └── SelectableRegistry.cs, SelectedObjectInfo.cs, EditableProperty*.cs
│
├── Services/
│   ├── EditorFileService.cs
│   └── ExportService.cs, ExportResult.cs
│
├── State/
│   ├── EditorState.cs
│   └── EditorToolMode.cs
│
└── Styling/
    ├── Colors/
    │   ├── EditorColors.cs          (Color tokens + Lerp/WithAlpha/WhiteOverlay)
    │   └── EditorTileColors.cs      (cores de tile e paleta de zonas)
    ├── Fonts/
    │   └── EditorFonts.cs           (Font tokens + MonospaceBold(size))
    ├── Layout/
    │   └── EditorLayout.cs          (const int — dimensões e espaçamentos)
    ├── Drawing/
    │   └── EditorDrawing.cs         (CreateRoundedPath)
    ├── Cursors/
    │   └── EditorCursors.cs         (cursores custom por tile)
    └── Controls/
        └── EditorControls.cs        (Create* + Style* + ApplyDarkScrollBars)
```

### Regras do Editor

- Cada tela tem subdiretório próprio em `Screens/`.
- Nenhum controle define o layout de outro controle (sem `SetBounds` cruzado).
- `Styling/` é a **única** fonte de cores, fontes, espaçamentos e utilitários de desenho. Zero magic strings de cor.
- `MainForm` apenas orquestra: não desenha, não lê arquivo diretamente, não conhece detalhes de controles filhos.
- Comunicação entre controles via **eventos C#** (não chamadas diretas entre siblings).
- Mutações que atravessam vários filhos do shell passam pelos métodos de fachada de `EditorShell.Facade.cs` (`ApplyToolMode`, `LoadMapDocument`, `SelectZone`...); `MainForm` não faz cascatas de chamadas em `Canvas`/`Inspector`/`CommandBar`.
- Arquivos dentro de um subdiretório de `Styling/` seguem o sufixo do diretório: `Colors/` → `Editor*Colors`, `Fonts/` → `Editor*Fonts`, etc.
- **Exceção de namespace documentada**: os partials de `MainForm/` usam `namespace Terminalia.Editor` (o namespace `Terminalia.Editor.MainForm` colidiria com o nome da classe).

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

### O que testar e por quê faz sentido numa engine de jogo

A lógica da engine é **pura e determinística**: dado um estado de entrada (componentes + `GameTime`), o sistema produz o mesmo estado de saída toda vez. Isso torna cada sistema ECS, o frame buffer e os mappers de input candidatos ideais a unit tests — sem precisar de terminal, janela ou frame rate real.

| Módulo | O que testar | Por que vale a pena |
|--------|-------------|---------------------|
| `Core/Ecs/Systems` | Comportamento por frame (movimento, colisão, animação) | Lógica determinística; bugs aparecem cedo |
| `Core/Scenes` | Invariantes do Aggregate Root (`Scene`) | Garante que entidades duplicadas são rejeitadas etc. |
| `Core/Runtime` | Ordem e contagem de chamadas do `GameLoop` | Protege o contrato do ciclo Update/Render |
| `Rendering.Terminal` | Frame buffer: escrita, clear, delta de células | Erros de pixel são silenciosos sem testes |
| `Input.Console` | Mapeamento tecla → `InputCommand` | Simples e crítico para gameplay |
| `Assets.Json` | Serialização/deserialização e validações de segurança | Path traversal e dados inválidos devem falhar ruidosamente |
| `TileMapGame` | `SceneAssembler` (montagem de entidades/sistemas) e `TileMapGameSession` (input → velocity, stop) | Lógica determinística fora do Core |
| `Editor/Services` e `Editor/State` | Lógica de arquivo e estado do editor | Pendente: o projeto de testes é `net8.0` e o Editor é `net8.0-windows`; exigiria mudar o TFM dos testes. Sem testes de UI WinForms — custo alto, benefício baixo |

### Fakes vs Mocks

Preferir implementações fake concretas a mocks de framework:

```csharp
// Preferido
class FakeRenderer : IRenderer { public List<string> Drawn = new(); ... }

// Evitar
var mock = new Mock<IRenderer>();
```

### Localização dos testes

Um único projeto; subdiretórios espelham o módulo testado:

```
tests/
└── Terminalia.UnitTests/
    ├── Core/
    │   ├── Ecs/
    │   ├── Scenes/
    │   ├── Menus/
    │   ├── Input/
    │   └── Runtime/
    ├── Rendering/
    ├── Input/
    ├── Assets/
    └── TileMapGame/
```

O namespace raiz é `Terminalia.UnitTests.*` espelhando o caminho de diretório.

---

## Convenções de código (não negociáveis)

### Chaves obrigatórias

Nunca omitir `{}` em `if`, `else`, `for`, `foreach`, `while`, `do` — mesmo para corpos de uma linha:

```csharp
// Correto
if (condition)
{
    DoSomething();
}

// Proibido
if (condition)
    DoSomething();
```

### Argumentos inline (até 4)

Chamadas de método, construtor ou definições de tipo com **até 4 parâmetros** ficam em uma única linha. Com 5 ou mais, pode quebrar:

```csharp
// Correto — 3 args, inline
return new Position(x, y, layer);

// Proibido — 3 args, multi-linha
return new Position(
    x,
    y,
    layer);
```

### Construtores padrão sem throw

Nunca lançar exceção em construtores sem parâmetros (ou com todos opcionais). `new Foo()` jamais deve falhar.

### Sem comentários

Nenhum comentário `//` ou `/* */` no código. O código deve se auto-documentar pelos nomes. Exceções: `[Atributos]` e pragma directives.

---

## Checklist de entrega

Antes de considerar uma tarefa concluída, verificar cada item:

- [ ] Nenhum arquivo novo excede o limite de LOC do seu tipo
- [ ] Nenhuma inner class pública ou com mais de 20 LOC
- [ ] Dependency Rule preservada (Core sem deps externas; Editor não é referenciado por Core)
- [ ] Namespace espelha o caminho de diretório exato
- [ ] Cores e fontes usadas exclusivamente via `Styling/` (`EditorColors`, `EditorTileColors`, `EditorFonts`)
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
