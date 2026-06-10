using Terminalia.Core.Abstractions.Input;
using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Abstractions.Runtime;
using Terminalia.Core.Assets;
using Terminalia.Core.Input;
using Terminalia.Core.Menus;

namespace Terminalia.Core.Runtime;

public sealed class GameFlowSession : IGame
{
    private readonly GameDefinition _definition;
    private readonly IRenderer _renderer;
    private readonly IInputProvider _input;
    private readonly Func<IInputProvider, IGame> _gameplayFactory;
    private readonly InputCommandRelay _relay = new();
    private MenuController _menu;
    private IGame? _gameplay;

    public GameFlowSession(GameDefinition definition, IRenderer renderer, IInputProvider input, Func<IInputProvider, IGame> gameplayFactory)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _gameplayFactory = gameplayFactory ?? throw new ArgumentNullException(nameof(gameplayFactory));
        _menu = new MenuController(definition.TitleMenu);
    }

    public bool IsRunning { get; private set; } = true;

    public GameFlowState State { get; private set; } = GameFlowState.Title;

    public IInputProvider GameplayInput => _relay;

    public void Initialize()
    {
    }

    public void ReplaceGameplay(IGame gameplay)
    {
        ArgumentNullException.ThrowIfNull(gameplay);
        _gameplay = gameplay;
        gameplay.Initialize();
    }

    public void HandleInput()
    {
        var commands = _input.GetCommands();

        if (State == GameFlowState.Playing)
        {
            HandlePlayingInput(commands);
        }
        else
        {
            HandleMenuInput(commands);
        }
    }

    public void Update(GameTime time)
    {
        if (!IsRunning)
        {
            return;
        }

        if (State == GameFlowState.Playing && _gameplay is not null)
        {
            _gameplay.Update(time);
            return;
        }

        _renderer.Clear();
        MenuView.Draw(_renderer, _menu.Definition, _menu.SelectedIndex);
    }

    public void Render()
    {
        if (!IsRunning)
        {
            return;
        }

        if (State == GameFlowState.Playing && _gameplay is not null)
        {
            _gameplay.Render();
            return;
        }

        _renderer.Present();
    }

    public void Stop() => IsRunning = false;

    private void HandlePlayingInput(IReadOnlyCollection<InputCommand> commands)
    {
        if (_gameplay is null || !_gameplay.IsRunning)
        {
            EnterTitle();
            return;
        }

        if (commands.Contains(InputCommand.Exit))
        {
            _menu = new MenuController(_definition.PauseMenu);
            State = GameFlowState.Paused;
            return;
        }

        _relay.Publish(commands);
        _gameplay.HandleInput();

        if (!_gameplay.IsRunning)
        {
            EnterTitle();
        }
    }

    private void HandleMenuInput(IReadOnlyCollection<InputCommand> commands)
    {
        foreach (var command in commands)
        {
            if (command == InputCommand.Exit)
            {
                HandleMenuEscape();
                return;
            }

            if (_menu.Handle(command) is { } action)
            {
                Execute(action);
                return;
            }
        }
    }

    private void HandleMenuEscape()
    {
        if (State == GameFlowState.Paused)
        {
            ResumeGameplay();
        }
        else
        {
            Stop();
        }
    }

    private void Execute(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.StartGame:
                StartGameplay();
                break;
            case MenuAction.ResumeGame:
                ResumeGameplay();
                break;
            case MenuAction.ReturnToTitle:
                EnterTitle();
                break;
            case MenuAction.QuitGame:
                Stop();
                break;
        }
    }

    private void StartGameplay()
    {
        _gameplay = _gameplayFactory(_relay);
        _gameplay.Initialize();
        State = GameFlowState.Playing;
    }

    private void ResumeGameplay()
    {
        if (_gameplay is { IsRunning: true })
        {
            State = GameFlowState.Playing;
        }
        else
        {
            EnterTitle();
        }
    }

    private void EnterTitle()
    {
        _gameplay = null;
        _menu = new MenuController(_definition.TitleMenu);
        State = GameFlowState.Title;
    }
}
