using System.Diagnostics;
using System.Runtime.InteropServices;
using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class MainForm : Form
{
    private readonly MapCanvas _canvas = new();
    private readonly EmbeddedGameSurface _gameSurface = new();
    private readonly Label _statusLabel = new();
    private readonly TextBox _nameTextBox = new();
    private readonly NumericUpDown _widthInput = new();
    private readonly NumericUpDown _heightInput = new();
    private readonly TrackBar _zoomInput = new();
    private readonly Button _runEmbeddedButton = new();
    private readonly Button _resetEmbeddedButton = new();
    private readonly List<Control> _sidebarStretchControls = [];
    private readonly List<Control> _sidebarSections = [];
    private string? _currentFile;
    private bool _hasUnsavedChanges;

    public MainForm()
    {
        Text = "Terminalia Editor";
        MinimumSize = new Size(1180, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = EditorTheme.AppBackground;
        Font = EditorTheme.UiFont;
        KeyPreview = true;
        TryUseDarkTitleBar();

        var commandBar = BuildCommandBar();
        var sidePanel = BuildSidePanel();
        var canvasPanel = BuildCanvasPanel();
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = EditorTheme.AppBackground,
            Padding = new Padding(0)
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 248));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
        mainLayout.Controls.Add(sidePanel, 0, 0);
        mainLayout.Controls.Add(canvasPanel, 1, 0);
        mainLayout.Controls.Add(_gameSurface, 2, 0);

        _statusLabel.Dock = DockStyle.Bottom;
        _statusLabel.Height = 30;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.Padding = new Padding(12, 0, 0, 0);
        _statusLabel.BackColor = EditorTheme.StatusBar;
        _statusLabel.ForeColor = EditorTheme.SecondaryText;

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = EditorTheme.AppBackground,
            Padding = new Padding(0)
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        shell.Controls.Add(commandBar, 0, 0);
        shell.Controls.Add(mainLayout, 0, 1);
        shell.Controls.Add(_statusLabel, 0, 2);

        Controls.Add(shell);

        _canvas.DocumentChanged += (_, _) => MarkDirty();
        _canvas.CursorCellChanged += (_, _) => UpdateStatus();
        _canvas.TilePicked += (_, tile) =>
        {
            _canvas.SelectedTile = tile;
            UpdateStatus();
        };
        _canvas.LoadDocument(MapAssetDocument.CreateDefault());
        _gameSurface.LoadDocument(_canvas.Document);
        SyncPropertiesFromDocument();
        MarkClean();
        UpdateStatus();
    }

    private Control BuildCommandBar()
    {
        var bar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = EditorTheme.Surface,
            Padding = new Padding(14, 7, 14, 7)
        };

        var title = new Label
        {
            Text = "Terminalia Editor",
            Dock = DockStyle.Left,
            Width = 170,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
            ForeColor = EditorTheme.PrimaryText
        };

        var commands = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = EditorTheme.Surface,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        commands.Controls.Add(CreateCommandButton("New", NewMap));
        commands.Controls.Add(CreateCommandButton("Open", OpenMap));
        commands.Controls.Add(CreateCommandButton("Save", () => SaveMap()));
        commands.Controls.Add(CreateCommandButton("Save as", () => SaveMapAs()));
        commands.Controls.Add(CreateCommandButton("Open sample", OpenSampleMap));
        commands.Controls.Add(CreateCommandButton("Save sample", () => SaveToSampleMap()));
        commands.Controls.Add(CreateCommandButton("Run external", PlaytestSample));

        bar.Controls.Add(commands);
        bar.Controls.Add(title);
        return bar;
    }

    private Control BuildCanvasPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = false,
            BackColor = EditorTheme.Workspace,
            Padding = new Padding(22)
        };

        panel.Controls.Add(_canvas);
        return panel;
    }

    private Control BuildSidePanel()
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = false,
            BackColor = EditorTheme.Navigation,
            Padding = new Padding(12)
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10),
            BackColor = EditorTheme.Navigation,
            Margin = new Padding(0)
        };

        panel.Controls.Add(BuildPropertiesPanel());
        panel.Controls.Add(BuildZoomPanel());
        panel.Controls.Add(BuildToolPanel());
        panel.Controls.Add(BuildPalettePanel());
        panel.Controls.Add(BuildSamplePanel());
        host.Controls.Add(panel);
        host.Resize += (_, _) => ResizeSidebar(host, panel);
        panel.HandleCreated += (_, _) => ResizeSidebar(host, panel);

        return host;
    }

    private void ResizeSidebar(Control host, Control panel)
    {
        var width = Math.Max(120, host.ClientSize.Width - host.Padding.Horizontal);
        panel.Width = width;
        var sectionWidth = Math.Max(120, width - panel.Padding.Horizontal);
        var contentWidth = Math.Max(80, sectionWidth - 24);
        foreach (var section in _sidebarSections)
        {
            section.Width = sectionWidth;
        }

        foreach (var control in _sidebarStretchControls)
        {
            control.Width = contentWidth;
        }
    }

    private Control BuildZoomPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 2, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var label = new Label
        {
            Text = "Zoom",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = EditorTheme.SecondaryText
        };

        _zoomInput.Minimum = 16;
        _zoomInput.Maximum = 48;
        _zoomInput.TickFrequency = 8;
        _zoomInput.SmallChange = 4;
        _zoomInput.LargeChange = 8;
        _zoomInput.Value = _canvas.CellSize;
        _zoomInput.Dock = DockStyle.Fill;
        _zoomInput.Height = 34;
        _zoomInput.BackColor = EditorTheme.Section;
        _zoomInput.ForeColor = EditorTheme.PrimaryText;
        _zoomInput.ValueChanged += (_, _) =>
        {
            _canvas.CellSize = _zoomInput.Value;
            UpdateStatus();
        };

        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(_zoomInput, 0, 1);

        return CreateSection("View", 112, layout);
    }

    private Control BuildToolPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(0, 4, 0, 0),
            WrapContents = false
        };

        var paintButton = new RadioButton
        {
            Text = "Paint",
            Checked = true,
            Width = 176,
            Height = 28,
            BackColor = EditorTheme.Section,
            ForeColor = EditorTheme.PrimaryText,
            Margin = new Padding(0, 0, 0, 4)
        };
        paintButton.CheckedChanged += (_, _) =>
        {
            if (paintButton.Checked)
            {
                _canvas.ToolMode = EditorToolMode.Paint;
                UpdateStatus();
            }
        };

        var fillButton = new RadioButton
        {
            Text = "Fill",
            Width = 176,
            Height = 28,
            BackColor = EditorTheme.Section,
            ForeColor = EditorTheme.PrimaryText,
            Margin = new Padding(0)
        };
        fillButton.CheckedChanged += (_, _) =>
        {
            if (fillButton.Checked)
            {
                _canvas.ToolMode = EditorToolMode.Fill;
                UpdateStatus();
            }
        };

        panel.Controls.Add(paintButton);
        panel.Controls.Add(fillButton);

        return CreateSection("Tool", 118, panel);
    }

    private Control BuildPropertiesPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(0, 2, 0, 0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        StyleTextBox(_nameTextBox);
        _nameTextBox.Width = 114;
        _widthInput.Minimum = 4;
        _widthInput.Maximum = 80;
        _widthInput.Width = 114;
        _heightInput.Minimum = 4;
        _heightInput.Maximum = 40;
        _heightInput.Width = 114;
        StyleNumberInput(_widthInput);
        StyleNumberInput(_heightInput);

        var applyButton = new Button
        {
            Text = "Apply",
            Dock = DockStyle.Fill
        };
        StyleButton(applyButton, true);
        applyButton.Click += (_, _) => ApplyMapProperties();

        layout.Controls.Add(CreateFieldLabel("Name"), 0, 0);
        layout.Controls.Add(_nameTextBox, 1, 0);
        layout.Controls.Add(CreateFieldLabel("Width"), 0, 1);
        layout.Controls.Add(_widthInput, 1, 1);
        layout.Controls.Add(CreateFieldLabel("Height"), 0, 2);
        layout.Controls.Add(_heightInput, 1, 2);
        layout.Controls.Add(applyButton, 0, 3);
        layout.SetColumnSpan(applyButton, 2);

        return CreateSection("Map", 154, layout);
    }

    private Control BuildPalettePanel()
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            WrapContents = false
        };

        panel.Controls.Add(CreateTileButton("#", "Wall", '#'));
        panel.Controls.Add(CreateTileButton(".", "Floor", '.'));
        panel.Controls.Add(CreateTileButton("@", "Player", '@'));
        panel.Controls.Add(CreateTileButton("*", "Relic", '*'));

        return CreateSection("Palette", 218, panel);
    }

    private Control BuildSamplePanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(0, 2, 0, 0),
            WrapContents = false
        };

        panel.Controls.Add(CreateEmbeddedRunButton());
        panel.Controls.Add(CreateEmbeddedResetButton());
        panel.Controls.Add(CreateSampleButton("Run external", PlaytestSample));

        return CreateSection("Run", 158, panel);
    }

    private Button CreateEmbeddedRunButton()
    {
        _runEmbeddedButton.Text = "Run in editor";
        _runEmbeddedButton.Width = 176;
        _runEmbeddedButton.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        _runEmbeddedButton.Height = 29;
        _runEmbeddedButton.Margin = new Padding(0, 0, 0, 6);
        StyleButton(_runEmbeddedButton, true);
        _sidebarStretchControls.Add(_runEmbeddedButton);
        _runEmbeddedButton.Click += (_, _) => ToggleEmbeddedRun();
        return _runEmbeddedButton;
    }

    private Button CreateEmbeddedResetButton()
    {
        _resetEmbeddedButton.Text = "Reset embedded";
        _resetEmbeddedButton.Width = 176;
        _resetEmbeddedButton.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        _resetEmbeddedButton.Height = 29;
        _resetEmbeddedButton.Margin = new Padding(0, 0, 0, 6);
        StyleButton(_resetEmbeddedButton);
        _sidebarStretchControls.Add(_resetEmbeddedButton);
        _resetEmbeddedButton.Click += (_, _) =>
        {
            _gameSurface.ResetGame();
            UpdateStatus();
        };
        return _resetEmbeddedButton;
    }

    private Button CreateSampleButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 176,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            Height = 29,
            Margin = new Padding(0, 0, 0, 6)
        };

        StyleButton(button);
        _sidebarStretchControls.Add(button);
        button.Click += (_, _) => action();
        return button;
    }

    private Button CreateTileButton(string symbol, string label, char tile)
    {
        var button = new Button
        {
            Text = $"{symbol}  {label}",
            Tag = tile,
            Width = 176,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            Height = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 5)
        };

        StyleButton(button);
        _sidebarStretchControls.Add(button);
        button.Click += (_, _) =>
        {
            _canvas.SelectedTile = tile;
            UpdateStatus();
        };
        return button;
    }

    private Control CreateSection(string title, int height, Control content)
    {
        var section = new Panel
        {
            Width = 216,
            Height = height,
            AutoSize = false,
            BackColor = EditorTheme.Section,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 8)
        };
        _sidebarSections.Add(section);
        section.Paint += (_, e) =>
        {
            using var border = new Pen(EditorTheme.Border);
            e.Graphics.DrawRectangle(border, 0, 0, section.Width - 1, section.Height - 1);
        };

        var label = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Height = 24,
            Font = EditorTheme.SectionFont,
            ForeColor = EditorTheme.PrimaryText,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = EditorTheme.Section,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        content.Dock = DockStyle.Fill;
        content.Margin = new Padding(0);
        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(content, 0, 1);
        section.Controls.Add(layout);
        return section;
    }

    private Button CreateCommandButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = text.Length > 10 ? 108 : 86,
            Height = 30,
            Margin = new Padding(0, 0, 8, 0)
        };

        StyleButton(button);
        button.Click += (_, _) => action();
        return button;
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = EditorTheme.SecondaryText
        };
    }

    private static void StyleButton(Button button, bool primary = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = primary ? EditorTheme.Accent : EditorTheme.Border;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = primary ? EditorTheme.AccentHover : EditorTheme.ButtonHover;
        button.FlatAppearance.MouseDownBackColor = primary ? EditorTheme.AccentPressed : EditorTheme.ButtonPressed;
        button.BackColor = primary ? EditorTheme.Accent : EditorTheme.ButtonBackground;
        button.ForeColor = primary ? Color.White : EditorTheme.PrimaryText;
        button.Font = EditorTheme.UiFont;
        button.Padding = new Padding(8, 0, 8, 0);
    }

    private static void StyleTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = EditorTheme.InputBackground;
        textBox.ForeColor = EditorTheme.PrimaryText;
    }

    private static void StyleNumberInput(NumericUpDown input)
    {
        input.BorderStyle = BorderStyle.FixedSingle;
        input.BackColor = EditorTheme.InputBackground;
        input.ForeColor = EditorTheme.PrimaryText;
    }

    private static void StyleMenuItems(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            item.BackColor = EditorTheme.Surface;
            item.ForeColor = EditorTheme.PrimaryText;
            item.Font = EditorTheme.UiFont;

            if (item is ToolStripMenuItem menuItem)
            {
                StyleMenuItems(menuItem.DropDownItems);
            }
        }
    }

    private void NewMap()
    {
        if (!ConfirmCanDiscardChanges())
        {
            return;
        }

        _currentFile = null;
        _canvas.LoadDocument(MapAssetDocument.CreateDefault());
        _gameSurface.LoadDocument(_canvas.Document);
        SyncPropertiesFromDocument();
        MarkClean();
        UpdateStatus();
    }

    private void OpenMap()
    {
        if (!ConfirmCanDiscardChanges())
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Terminalia map (*.json)|*.json|All files (*.*)|*.*",
            Title = "Open Terminalia map"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var document = MapAssetDocument.Load(dialog.FileName);
        _currentFile = dialog.FileName;
        _canvas.LoadDocument(document);
        _gameSurface.LoadDocument(_canvas.Document);
        SyncPropertiesFromDocument();
        MarkClean();
        UpdateStatus();
    }

    private bool SaveMap()
    {
        if (_currentFile is null)
        {
            return SaveMapAs();
        }

        _canvas.Document.Save(_currentFile);
        MarkClean();
        UpdateStatus();
        return true;
    }

    private bool SaveMapAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Terminalia map (*.json)|*.json|All files (*.*)|*.*",
            Title = "Save Terminalia map",
            FileName = $"{_canvas.Document.Name}.json"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        _currentFile = dialog.FileName;
        _canvas.Document.Save(_currentFile);
        MarkClean();
        UpdateStatus();
        return true;
    }

    private void UpdateStatus()
    {
        var dirtyMarker = _hasUnsavedChanges ? "*" : "";
        var cellText = _canvas.CursorCell is { } cell
            ? $"Cell: {cell.X},{cell.Y}"
            : "Cell: -";
        Text = $"Terminalia Editor{dirtyMarker}";
        _statusLabel.Text = $"{_canvas.Document.Name}{dirtyMarker} | {_canvas.Document.Width}x{_canvas.Document.Height} | {cellText} | Tool: {_canvas.ToolMode} | Tile: {_canvas.SelectedTile} | {_currentFile ?? "Unsaved"}";
    }

    private void SyncPropertiesFromDocument()
    {
        _nameTextBox.Text = _canvas.Document.Name;
        _widthInput.Value = _canvas.Document.Width;
        _heightInput.Value = _canvas.Document.Height;
    }

    private void ApplyMapProperties()
    {
        var document = _canvas.Document;
        document.Name = string.IsNullOrWhiteSpace(_nameTextBox.Text)
            ? "untitled_map"
            : _nameTextBox.Text.Trim();
        document.Resize((int)_widthInput.Value, (int)_heightInput.Value);
        _canvas.RefreshDocument();
        _gameSurface.LoadDocument(_canvas.Document);
        UpdateStatus();
    }

    private void OpenSampleMap()
    {
        if (!ConfirmCanDiscardChanges())
        {
            return;
        }

        var sampleMapPath = GetSampleMapPath();
        if (sampleMapPath is null)
        {
            ShowRepositoryNotFound();
            return;
        }

        var document = MapAssetDocument.Load(sampleMapPath);
        _currentFile = sampleMapPath;
        _canvas.LoadDocument(document);
        _gameSurface.LoadDocument(_canvas.Document);
        SyncPropertiesFromDocument();
        MarkClean();
        UpdateStatus();
    }

    private bool SaveToSampleMap()
    {
        var sampleMapPath = GetSampleMapPath();
        if (sampleMapPath is null)
        {
            ShowRepositoryNotFound();
            return false;
        }

        _canvas.Document.Save(sampleMapPath);
        _currentFile = sampleMapPath;
        MarkClean();
        UpdateStatus();
        MessageBox.Show(this, "Sample map saved.", "Terminalia Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return true;
    }

    private void PlaytestSample()
    {
        var repositoryRoot = FindRepositoryRoot();
        if (repositoryRoot is null)
        {
            ShowRepositoryNotFound();
            return;
        }

        if (!SaveToSampleMap())
        {
            return;
        }

        var projectPath = Path.Combine(repositoryRoot, "src", "Samples", "Terminalia.Sample.Rpg", "Terminalia.Sample.Rpg.csproj");
        var command = $"/c start \"Terminalia Sample\" dotnet run --project \"{projectPath}\" --no-restore";

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = command,
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false
        });
    }

    private string? GetSampleMapPath()
    {
        var repositoryRoot = FindRepositoryRoot();
        return repositoryRoot is null
            ? null
            : Path.Combine(repositoryRoot, "src", "Samples", "Terminalia.Sample.Rpg", "assets", "maps", "first_dungeon.json");
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Terminalia.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private void ShowRepositoryNotFound()
    {
        MessageBox.Show(
            this,
            "Could not find Terminalia.sln from the editor output folder.",
            "Terminalia Editor",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!ConfirmCanDiscardChanges())
        {
            e.Cancel = true;
            return;
        }

        base.OnFormClosing(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_gameSurface.IsRunning && !IsEditingTextInput())
        {
            var keyCode = keyData & Keys.KeyCode;
            if (_gameSurface.TryHandleInput(keyCode))
            {
                UpdateEmbeddedRunButton();
                UpdateStatus();
                return true;
            }
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void MarkDirty()
    {
        _hasUnsavedChanges = true;
        _gameSurface.LoadDocument(_canvas.Document, preserveRunState: true);
        UpdateStatus();
    }

    private void MarkClean()
    {
        _hasUnsavedChanges = false;
        UpdateStatus();
    }

    private bool ConfirmCanDiscardChanges()
    {
        if (!_hasUnsavedChanges)
        {
            return true;
        }

        var result = MessageBox.Show(
            this,
            "Save changes before continuing?",
            "Terminalia Editor",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        return result switch
        {
            DialogResult.Yes => SaveMap(),
            DialogResult.No => true,
            _ => false
        };
    }

    private void ToggleEmbeddedRun()
    {
        if (_gameSurface.IsRunning)
        {
            _gameSurface.StopGame();
        }
        else
        {
            _gameSurface.StartGame();
            _gameSurface.Focus();
        }

        UpdateEmbeddedRunButton();
        UpdateStatus();
    }

    private void UpdateEmbeddedRunButton()
    {
        _runEmbeddedButton.Text = _gameSurface.IsRunning ? "Pause embedded" : "Run in editor";
    }

    private bool IsEditingTextInput()
    {
        return ActiveControl is TextBoxBase or NumericUpDown or TrackBar;
    }

    private void TryUseDarkTitleBar()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        var value = 1;
        _ = DwmSetWindowAttribute(Handle, 20, ref value, sizeof(int));
        _ = DwmSetWindowAttribute(Handle, 19, ref value, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
}

public enum EditorToolMode
{
    Paint,
    Fill
}

public sealed class MapCanvas : Control
{
    private const int CanvasPadding = 24;
    private int _cellSize = 32;
    private MapAssetDocument _document = MapAssetDocument.CreateDefault();
    private char _selectedTile = '#';
    private bool _isPainting;
    private Point? _lastPaintedCell;
    private Point? _cursorCell;

    public event EventHandler? DocumentChanged;
    public event EventHandler? CursorCellChanged;
    public event EventHandler<char>? TilePicked;

    public MapCanvas()
    {
        Location = new Point(0, 0);
        BackColor = EditorTheme.Workspace;
        DoubleBuffered = true;
        Font = new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold);
        UpdateCanvasSize();
    }

    public MapAssetDocument Document => _document;

    public Point? CursorCell => _cursorCell;

    public EditorToolMode ToolMode { get; set; } = EditorToolMode.Paint;

    public int CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = Math.Clamp(value, 16, 48);
            UpdateCanvasSize();
            Invalidate();
        }
    }

    public char SelectedTile
    {
        get => _selectedTile;
        set
        {
            _selectedTile = value;
        }
    }

    public void LoadDocument(MapAssetDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        UpdateCanvasSize();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshDocument()
    {
        UpdateCanvasSize();
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(EditorTheme.Workspace);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        e.Graphics.TranslateTransform(CanvasPadding, CanvasPadding);

        for (var y = 0; y < _document.Height; y++)
        {
            for (var x = 0; x < _document.Width; x++)
            {
                DrawCell(e.Graphics, x, y, _document.GetTile(x, y));
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
        {
            if (e.Button == MouseButtons.Right)
            {
                PickTileAt(e.Location);
            }

            return;
        }

        UpdateCursorCell(e.Location);

        if (ToolMode == EditorToolMode.Fill)
        {
            FillAt(e.Location);
            return;
        }

        _isPainting = true;
        PaintAt(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        UpdateCursorCell(e.Location);

        if (ToolMode == EditorToolMode.Paint && _isPainting && e.Button == MouseButtons.Left)
        {
            PaintAt(e.Location);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        _isPainting = false;
        _lastPaintedCell = null;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (_cursorCell is not null)
        {
            _cursorCell = null;
            CursorCellChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void PaintAt(Point location)
    {
        if (location.X < CanvasPadding || location.Y < CanvasPadding)
        {
            return;
        }

        var x = (location.X - CanvasPadding) / CellSize;
        var y = (location.Y - CanvasPadding) / CellSize;
        var cell = new Point(x, y);

        if (!_document.Contains(x, y) || _lastPaintedCell == cell)
        {
            return;
        }

        _document.SetTile(x, y, _selectedTile);
        _lastPaintedCell = cell;
        Invalidate(new Rectangle(
            CanvasPadding + (x * CellSize),
            CanvasPadding + (y * CellSize),
            CellSize + 1,
            CellSize + 1));
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FillAt(Point location)
    {
        if (!TryGetCell(location, out var cell))
        {
            return;
        }

        _document.FloodFill(cell.X, cell.Y, _selectedTile);
        Invalidate();
        DocumentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PickTileAt(Point location)
    {
        if (TryGetCell(location, out var cell))
        {
            TilePicked?.Invoke(this, _document.GetTile(cell.X, cell.Y));
        }
    }

    private void UpdateCursorCell(Point location)
    {
        Point? nextCell = TryGetCell(location, out var cell) ? cell : null;

        if (_cursorCell == nextCell)
        {
            return;
        }

        _cursorCell = nextCell;
        CursorCellChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool TryGetCell(Point location, out Point cell)
    {
        cell = default;

        if (location.X < CanvasPadding || location.Y < CanvasPadding)
        {
            return false;
        }

        var x = (location.X - CanvasPadding) / CellSize;
        var y = (location.Y - CanvasPadding) / CellSize;

        if (!_document.Contains(x, y))
        {
            return false;
        }

        cell = new Point(x, y);
        return true;
    }

    private void DrawCell(Graphics graphics, int x, int y, char tile)
    {
        var bounds = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);

        using var background = new SolidBrush(GetTileColor(tile));
        using var border = new Pen(EditorTheme.GridLine);
        using var tileFont = GetTileFont();
        using var textBrush = new SolidBrush(GetTileTextColor(tile));
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoClip
        };
        graphics.FillRectangle(background, bounds);
        graphics.DrawRectangle(border, bounds);
        graphics.DrawString(tile.ToString(), tileFont, textBrush, bounds, format);
    }

    private static Color GetTileColor(char tile)
    {
        return tile switch
        {
            '#' => Color.FromArgb(30, 41, 54),
            '.' => Color.FromArgb(15, 23, 32),
            '@' => Color.FromArgb(16, 42, 66),
            '*' => Color.FromArgb(58, 45, 15),
            _ => EditorTheme.Section
        };
    }

    private static Color GetTileTextColor(char tile)
    {
        return tile switch
        {
            '#' => Color.FromArgb(148, 163, 184),
            '.' => Color.FromArgb(71, 85, 105),
            '@' => Color.FromArgb(96, 165, 250),
            '*' => Color.FromArgb(250, 204, 21),
            _ => EditorTheme.PrimaryText
        };
    }

    private void UpdateCanvasSize()
    {
        Size = new Size(
            (_document.Width * CellSize) + (CanvasPadding * 2),
            (_document.Height * CellSize) + (CanvasPadding * 2));
    }

    private Font GetTileFont()
    {
        var size = Math.Max(7, CellSize * 0.45f);
        return new Font(FontFamily.GenericMonospace, size, FontStyle.Bold);
    }
}

public sealed class EmbeddedGameSurface : Control
{
    private readonly Font _terminalFont = new(FontFamily.GenericMonospace, 14, FontStyle.Bold);
    private readonly Font _hudFont = new("Segoe UI", 9, FontStyle.Regular);
    private readonly Font _titleFont = new("Segoe UI Semibold", 11, FontStyle.Bold);
    private MapAssetDocument _document = MapAssetDocument.CreateDefault();
    private Point _playerPosition;
    private bool _hasPlayer;

    public EmbeddedGameSurface()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.AppBackground;
        DoubleBuffered = true;
        Padding = new Padding(20);
        Margin = new Padding(0);
        TabStop = true;
    }

    public bool IsRunning { get; private set; }

    public void LoadDocument(MapAssetDocument document, bool preserveRunState = false)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        if (!preserveRunState ||
            !_hasPlayer ||
            !_document.Contains(_playerPosition.X, _playerPosition.Y) ||
            _document.GetTile(_playerPosition.X, _playerPosition.Y) == '#')
        {
            ResetGame();
            return;
        }

        RefreshPreview();
    }

    public void StartGame()
    {
        IsRunning = true;
        if (!_hasPlayer)
        {
            ResetGame();
        }

        Focus();
        RefreshPreview();
    }

    public void StopGame()
    {
        IsRunning = false;
        RefreshPreview();
    }

    public void ResetGame()
    {
        _hasPlayer = TryFindPlayerSpawn(out _playerPosition);
        RefreshPreview();
    }

    public bool TryHandleInput(Keys key)
    {
        if (!IsRunning || !_hasPlayer)
        {
            return false;
        }

        var delta = key switch
        {
            Keys.W or Keys.Up => new Point(0, -1),
            Keys.S or Keys.Down => new Point(0, 1),
            Keys.A or Keys.Left => new Point(-1, 0),
            Keys.D or Keys.Right => new Point(1, 0),
            Keys.Escape => StopAndHandle(),
            _ => Point.Empty
        };

        if (delta == Point.Empty)
        {
            return key == Keys.Escape;
        }

        var next = new Point(_playerPosition.X + delta.X, _playerPosition.Y + delta.Y);
        if (_document.Contains(next.X, next.Y) && _document.GetTile(next.X, next.Y) != '#')
        {
            _playerPosition = next;
            RefreshPreview();
        }

        return true;
    }

    public void RefreshPreview()
    {
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(EditorTheme.AppBackground);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var chrome = new Rectangle(18, 18, Width - 36, Height - 36);
        if (chrome.Width <= 0 || chrome.Height <= 0)
        {
            return;
        }

        using var panelBrush = new SolidBrush(EditorTheme.TerminalBackground);
        using var border = new Pen(EditorTheme.Border);
        e.Graphics.FillRectangle(panelBrush, chrome);
        e.Graphics.DrawRectangle(border, chrome);

        DrawHeader(e.Graphics, chrome);
        DrawMap(e.Graphics, chrome);
        DrawHud(e.Graphics, chrome);
    }

    private void DrawHeader(Graphics graphics, Rectangle chrome)
    {
        var header = new Rectangle(chrome.X, chrome.Y, chrome.Width, 42);
        using var headerBrush = new SolidBrush(EditorTheme.TerminalHeader);
        graphics.FillRectangle(headerBrush, header);

        using var red = new SolidBrush(Color.FromArgb(255, 95, 86));
        using var yellow = new SolidBrush(Color.FromArgb(255, 189, 46));
        using var green = new SolidBrush(Color.FromArgb(39, 201, 63));
        graphics.FillEllipse(red, header.X + 14, header.Y + 16, 10, 10);
        graphics.FillEllipse(yellow, header.X + 32, header.Y + 16, 10, 10);
        graphics.FillEllipse(green, header.X + 50, header.Y + 16, 10, 10);

        TextRenderer.DrawText(
            graphics,
            "Terminalia Runtime",
            _titleFont,
            new Rectangle(header.X + 72, header.Y, header.Width - 88, header.Height),
            Color.FromArgb(226, 232, 240),
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    private void DrawMap(Graphics graphics, Rectangle chrome)
    {
        var origin = new Point(chrome.X + 22, chrome.Y + 66);
        var lineHeight = TextRenderer.MeasureText("M", _terminalFont).Height + 2;
        var visibleRows = Math.Max(1, (chrome.Height - 170) / lineHeight);
        var rows = Math.Min(_document.Height, visibleRows);

        for (var y = 0; y < rows; y++)
        {
            var xOffset = origin.X;
            for (var x = 0; x < _document.Width; x++)
            {
                var tile = _document.GetTile(x, y);
                var visibleTile = _hasPlayer && _playerPosition.X == x && _playerPosition.Y == y
                    ? '@'
                    : tile == '@' ? '.' : tile;
                var textSize = TextRenderer.MeasureText(tile.ToString(), _terminalFont);
                TextRenderer.DrawText(
                    graphics,
                    visibleTile.ToString(),
                    _terminalFont,
                    new Point(xOffset, origin.Y + (y * lineHeight)),
                    GetTerminalTileColor(visibleTile));
                xOffset += textSize.Width - 3;

                if (xOffset > chrome.Right - 24)
                {
                    break;
                }
            }
        }
    }

    private void DrawHud(Graphics graphics, Rectangle chrome)
    {
        var hudTop = Math.Max(chrome.Bottom - 104, chrome.Y + 120);
        var hud = new Rectangle(chrome.X + 18, hudTop, chrome.Width - 36, chrome.Bottom - hudTop - 16);
        using var hudBrush = new SolidBrush(Color.FromArgb(24, 33, 42));
        using var border = new Pen(Color.FromArgb(53, 65, 80));
        graphics.FillRectangle(hudBrush, hud);
        graphics.DrawRectangle(border, hud);

        var lines = new[]
        {
            $"Map: {_document.Name}",
            $"Size: {_document.Width}x{_document.Height}",
            IsRunning ? "Running in editor" : "Paused",
            _hasPlayer ? "Move: WASD or arrows" : "Add a player spawn (@)"
        };

        for (var i = 0; i < lines.Length; i++)
        {
            TextRenderer.DrawText(
                graphics,
                lines[i],
                _hudFont,
                new Rectangle(hud.X + 12, hud.Y + 10 + (i * 20), hud.Width - 24, 20),
                i == 0 ? Color.FromArgb(147, 197, 253) : Color.FromArgb(203, 213, 225),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    private static Color GetTerminalTileColor(char tile)
    {
        return tile switch
        {
            '#' => Color.FromArgb(148, 163, 184),
            '.' => Color.FromArgb(71, 85, 105),
            '@' => Color.FromArgb(96, 165, 250),
            '*' => Color.FromArgb(250, 204, 21),
            _ => Color.FromArgb(226, 232, 240)
        };
    }

    private bool TryFindPlayerSpawn(out Point playerPosition)
    {
        for (var y = 0; y < _document.Height; y++)
        {
            for (var x = 0; x < _document.Width; x++)
            {
                if (_document.GetTile(x, y) == '@')
                {
                    playerPosition = new Point(x, y);
                    return true;
                }
            }
        }

        playerPosition = default;
        return false;
    }

    private Point StopAndHandle()
    {
        StopGame();
        return Point.Empty;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _terminalFont.Dispose();
            _hudFont.Dispose();
            _titleFont.Dispose();
        }

        base.Dispose(disposing);
    }
}

internal static class EditorTheme
{
    public static readonly Color AppBackground = Color.FromArgb(9, 13, 20);
    public static readonly Color Workspace = Color.FromArgb(11, 16, 24);
    public static readonly Color Navigation = Color.FromArgb(14, 20, 29);
    public static readonly Color Surface = Color.FromArgb(16, 23, 33);
    public static readonly Color Section = Color.FromArgb(18, 26, 38);
    public static readonly Color StatusBar = Color.FromArgb(12, 18, 27);
    public static readonly Color InputBackground = Color.FromArgb(10, 15, 23);
    public static readonly Color ButtonBackground = Color.FromArgb(23, 33, 47);
    public static readonly Color ButtonHover = Color.FromArgb(30, 43, 61);
    public static readonly Color ButtonPressed = Color.FromArgb(20, 29, 41);
    public static readonly Color Border = Color.FromArgb(45, 58, 74);
    public static readonly Color GridLine = Color.FromArgb(47, 62, 78);
    public static readonly Color PrimaryText = Color.FromArgb(226, 232, 240);
    public static readonly Color SecondaryText = Color.FromArgb(148, 163, 184);
    public static readonly Color Accent = Color.FromArgb(59, 130, 246);
    public static readonly Color AccentHover = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentPressed = Color.FromArgb(29, 78, 216);
    public static readonly Color TerminalBackground = Color.FromArgb(13, 17, 23);
    public static readonly Color TerminalHeader = Color.FromArgb(21, 30, 42);
    public static readonly Font UiFont = new("Segoe UI", 9, FontStyle.Regular);
    public static readonly Font SectionFont = new("Segoe UI Semibold", 9, FontStyle.Bold);
}

internal sealed class ModernMenuColorTable : ProfessionalColorTable
{
    public override Color MenuStripGradientBegin => EditorTheme.Surface;
    public override Color MenuStripGradientEnd => EditorTheme.Surface;
    public override Color ToolStripDropDownBackground => EditorTheme.Surface;
    public override Color ImageMarginGradientBegin => EditorTheme.Surface;
    public override Color ImageMarginGradientMiddle => EditorTheme.Surface;
    public override Color ImageMarginGradientEnd => EditorTheme.Surface;
    public override Color MenuItemSelected => EditorTheme.ButtonHover;
    public override Color MenuItemBorder => EditorTheme.Border;
    public override Color MenuItemSelectedGradientBegin => EditorTheme.ButtonHover;
    public override Color MenuItemSelectedGradientEnd => EditorTheme.ButtonHover;
    public override Color MenuItemPressedGradientBegin => EditorTheme.Surface;
    public override Color MenuItemPressedGradientMiddle => EditorTheme.Surface;
    public override Color MenuItemPressedGradientEnd => EditorTheme.Surface;
}
