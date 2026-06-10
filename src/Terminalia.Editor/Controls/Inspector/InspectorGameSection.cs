using Terminalia.Assets.Json;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;

namespace Terminalia.Editor.Controls.Inspector;

public sealed class InspectorGameSection : Panel
{
    private const int MainViewHeight = 250;
    private const int MenuViewHeight = 380;

    private readonly TextBox _titleTextBox = new();
    private readonly ComboBox _startMapCombo = new();
    private readonly InspectorMenuItemsEditor _menuEditor = new();
    private readonly Panel _mainView = new();
    private readonly Panel _menuView = new();
    private GameMenuDto _titleMenu = new();
    private GameMenuDto _pauseMenu = new();
    private bool _editingPauseMenu;

    public event EventHandler<GameDefinitionAppliedEventArgs>? GameDefinitionApplied;
    public event EventHandler<int>? PreferredHeightChanged;

    public InspectorGameSection()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.Section;

        EditorControls.StyleTextBox(_titleTextBox);
        EditorControls.StyleComboBox(_startMapCombo);

        BuildMainView();
        BuildMenuView();

        Controls.Add(_menuView);
        Controls.Add(_mainView);
        ShowMainView();
    }

    public void LoadGame(GameAssetDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _titleTextBox.Text = document.Title;
        _startMapCombo.Text = document.StartMap;
        _titleMenu = document.TitleMenu;
        _pauseMenu = document.PauseMenu;
        ShowMainView();
    }

    public void LoadAvailableMaps(IEnumerable<string> mapNames)
    {
        var current = _startMapCombo.Text;
        _startMapCombo.Items.Clear();
        foreach (var name in mapNames)
        {
            _startMapCombo.Items.Add(name);
        }
        _startMapCombo.Text = current;
    }

    private void BuildMainView()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = EditorColors.Section,
            Padding = new Padding(0, 2, 0, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var titleMenuButton = EditorControls.CreateButton("Title menu  ›");
        titleMenuButton.Dock = DockStyle.Fill;
        titleMenuButton.TextAlign = ContentAlignment.MiddleLeft;
        titleMenuButton.Click += (_, _) => ShowMenuView(editPauseMenu: false);

        var pauseMenuButton = EditorControls.CreateButton("Pause menu  ›");
        pauseMenuButton.Dock = DockStyle.Fill;
        pauseMenuButton.TextAlign = ContentAlignment.MiddleLeft;
        pauseMenuButton.Click += (_, _) => ShowMenuView(editPauseMenu: true);

        var applyButton = EditorControls.CreateButton("Apply", primary: true);
        applyButton.Dock = DockStyle.Fill;
        applyButton.Click += (_, _) => ApplyDefinition();

        layout.Controls.Add(EditorControls.CreateFieldLabel("Title"), 0, 0);
        layout.Controls.Add(_titleTextBox, 1, 0);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Start"), 0, 1);
        layout.Controls.Add(_startMapCombo, 1, 1);
        layout.Controls.Add(titleMenuButton, 0, 2);
        layout.SetColumnSpan(titleMenuButton, 2);
        layout.Controls.Add(pauseMenuButton, 0, 3);
        layout.SetColumnSpan(pauseMenuButton, 2);
        layout.Controls.Add(applyButton, 0, 4);
        layout.SetColumnSpan(applyButton, 2);

        _mainView.Dock = DockStyle.Fill;
        _mainView.BackColor = EditorColors.Section;
        _mainView.Controls.Add(layout);
    }

    private void BuildMenuView()
    {
        var backButton = EditorControls.CreateButton("‹  Game");
        backButton.Dock = DockStyle.Top;
        backButton.Height = 30;
        backButton.Click += (_, _) => CommitMenuAndShowMain();

        _menuView.Dock = DockStyle.Fill;
        _menuView.BackColor = EditorColors.Section;
        _menuView.Visible = false;
        _menuView.Controls.Add(_menuEditor);
        _menuView.Controls.Add(backButton);
    }

    private void ShowMenuView(bool editPauseMenu)
    {
        _editingPauseMenu = editPauseMenu;
        _menuEditor.LoadMenu(editPauseMenu ? _pauseMenu : _titleMenu);
        _mainView.Visible = false;
        _menuView.Visible = true;
        PreferredHeightChanged?.Invoke(this, MenuViewHeight);
    }

    private void CommitMenuAndShowMain()
    {
        if (_editingPauseMenu)
        {
            _pauseMenu = _menuEditor.ToMenu();
        }
        else
        {
            _titleMenu = _menuEditor.ToMenu();
        }
        ShowMainView();
    }

    private void ShowMainView()
    {
        _menuView.Visible = false;
        _mainView.Visible = true;
        PreferredHeightChanged?.Invoke(this, MainViewHeight);
    }

    private void ApplyDefinition()
    {
        var document = new GameAssetDocument
        {
            Title = _titleTextBox.Text.Trim(),
            StartMap = _startMapCombo.Text.Trim(),
            TitleMenu = _titleMenu,
            PauseMenu = _pauseMenu
        };
        GameDefinitionApplied?.Invoke(this, new GameDefinitionAppliedEventArgs(document));
    }
}

public sealed class GameDefinitionAppliedEventArgs(GameAssetDocument document) : EventArgs
{
    public GameAssetDocument Document { get; } = document;
}
