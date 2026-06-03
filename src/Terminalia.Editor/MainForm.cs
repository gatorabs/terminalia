using System.Runtime.InteropServices;

namespace Terminalia.Editor;

public sealed class MainForm : Form
{
    private readonly EditorFileService _fileService = new();
    private readonly EditorShell _shell = new();
    private readonly StartPage _startPage = new();
    private readonly LoadingOverlay _loadingOverlay = new();
    private readonly EditorState _state = new();

    public MainForm()
    {
        Text = "Terminalia Editor";
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = EditorTheme.AppBackground;
        Font = EditorTheme.UiFont;
        KeyPreview = true;
        TryUseDarkTitleBar();

        _shell.Visible = false;
        Controls.Add(_shell);
        Controls.Add(_startPage);
        Controls.Add(_loadingOverlay);
        _loadingOverlay.BringToFront();
        WireEvents();
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
        if (_shell.Runtime.IsRunning && !IsEditingTextInput())
        {
            var keyCode = keyData & Keys.KeyCode;
            if (_shell.Runtime.TryHandleInput(keyCode))
            {
                _shell.Inspector.SetRunState(_shell.Runtime.IsRunning);
                if (!_shell.Runtime.IsRunning)
                    _shell.ExitGameMode();
                UpdateStatus();
                return true;
            }
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void WireEvents()
    {
        _shell.CommandBar.NewRequested += (_, _) => NewMap();
        _shell.CommandBar.OpenRequested += (_, _) => OpenMap();
        _shell.CommandBar.SaveRequested += (_, _) => SaveMap();
        _shell.CommandBar.SaveAsRequested += (_, _) => SaveMapAs();
        _shell.CommandBar.OpenSampleRequested += async (_, _) => await OpenSampleMapWithLoadingAsync();
        _shell.CommandBar.SaveSampleRequested += async (_, _) => await SaveSampleMapWithLoadingAsync();
        _shell.CommandBar.RunExternalRequested += async (_, _) => await PlaytestSampleWithLoadingAsync();
        _shell.CommandBar.MapPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Map);
        _shell.CommandBar.ToolPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Tool);
        _shell.CommandBar.PalettePanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Palette);
        _shell.CommandBar.RunPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Run);

        _startPage.NewMapRequested += (_, _) => NewMap();
        _startPage.OpenMapRequested += (_, _) => OpenMap();
        _startPage.OpenSampleRequested += async (_, _) => await OpenSampleMapWithLoadingAsync();

        _shell.Inspector.ApplyMapPropertiesRequested += (_, args) => ApplyMapProperties(args);
        _shell.Inspector.ToolChanged += (_, tool) =>
        {
            _state.ToolMode = tool;
            _shell.Canvas.ToolMode = tool;
            UpdateStatus();
        };
        _shell.Inspector.TileSelected += (_, tile) =>
        {
            _state.SelectedTile = tile;
            _shell.Canvas.SelectedTile = tile;
            UpdateStatus();
        };
        _shell.Inspector.RunEmbeddedRequested += (_, _) => ToggleEmbeddedRun();
        _shell.Inspector.ResetEmbeddedRequested += (_, _) =>
        {
            _shell.Runtime.ResetGame();
            UpdateStatus();
        };

        _shell.Canvas.DocumentChanged += (_, _) => MarkDirty();
        _shell.Canvas.CursorCellChanged += (_, _) =>
        {
            _state.CursorCell = _shell.Canvas.CursorCell;
            UpdateStatus();
        };
        _shell.Canvas.TilePicked += (_, tile) =>
        {
            _state.SelectedTile = tile;
            _shell.Canvas.SelectedTile = tile;
            _shell.Inspector.SelectTile(tile);
            UpdateStatus();
        };
    }

    private void LoadDocument(Terminalia.Assets.Json.MapAssetDocument document, string? currentFile, bool markClean)
    {
        if (_shell.Runtime.IsRunning)
        {
            _shell.Runtime.StopGame();
            _shell.ExitGameMode();
            _shell.Inspector.SetRunState(false);
        }

        _state.Document = document;
        _state.CurrentFile = currentFile;
        _shell.Canvas.LoadDocument(document);
        _shell.Runtime.LoadDocument(document.ToTileMap());
        _shell.Inspector.LoadDocument(document.Name, document.Width, document.Height);
        ShowEditor();

        if (markClean)
        {
            MarkClean();
        }
        else
        {
            MarkDirty();
        }

        UpdateStatus();
    }

    private void NewMap()
    {
        if (!ConfirmCanDiscardChanges())
        {
            return;
        }

        LoadDocument(_fileService.CreateDefaultMap(), currentFile: null, markClean: true);
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

        LoadDocument(_fileService.LoadMap(dialog.FileName), dialog.FileName, markClean: true);
    }

    private bool SaveMap()
    {
        if (_state.CurrentFile is null)
        {
            return SaveMapAs();
        }

        _fileService.SaveMap(_state.Document, _state.CurrentFile);
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
            FileName = $"{_state.Document.Name}.json"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        _state.CurrentFile = dialog.FileName;
        _fileService.SaveMap(_state.Document, _state.CurrentFile);
        MarkClean();
        UpdateStatus();
        return true;
    }

    private async Task OpenSampleMapWithLoadingAsync()
    {
        if (!ConfirmCanDiscardChanges())
        {
            return;
        }

        await RunWithLoadingAsync(
            "Opening sample...",
            () =>
            {
                var sampleMapPath = _fileService.GetSampleMapPath();
                if (sampleMapPath is null)
                {
                    return false;
                }

                LoadDocument(_fileService.LoadMap(sampleMapPath), sampleMapPath, markClean: true);
                return true;
            },
            ShowRepositoryNotFound);
    }

    private bool SaveToSampleMap(bool showConfirmation)
    {
        return SaveToSampleMap(showConfirmation, showRepositoryMessage: true);
    }

    private bool SaveToSampleMap(bool showConfirmation, bool showRepositoryMessage)
    {
        var sampleMapPath = _fileService.GetSampleMapPath();
        if (sampleMapPath is null)
        {
            if (showRepositoryMessage)
            {
                ShowRepositoryNotFound();
            }

            return false;
        }

        _fileService.SaveMap(_state.Document, sampleMapPath);
        _state.CurrentFile = sampleMapPath;
        MarkClean();
        UpdateStatus();

        if (showConfirmation)
        {
            MessageBox.Show(this, "Sample map saved.", "Terminalia Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        return true;
    }

    private async Task SaveSampleMapWithLoadingAsync()
    {
        var saved = await RunWithLoadingAsync("Saving sample...", () => SaveToSampleMap(showConfirmation: false, showRepositoryMessage: false), ShowRepositoryNotFound);
        if (saved)
        {
            MessageBox.Show(this, "Sample map saved.", "Terminalia Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void PlaytestSample()
    {
        if (!SaveToSampleMap(showConfirmation: false))
        {
            return;
        }

        if (!_fileService.PlaytestSample())
        {
            ShowRepositoryNotFound();
        }
    }

    private async Task PlaytestSampleWithLoadingAsync()
    {
        await RunWithLoadingAsync("Launching game...", PlaytestSampleStarted, ShowRepositoryNotFound);
    }

    private bool PlaytestSampleStarted()
    {
        return SaveToSampleMap(showConfirmation: false, showRepositoryMessage: false) && _fileService.PlaytestSample();
    }

    private async Task<bool> RunWithLoadingAsync(string message, Func<bool> action, Action onFailure)
    {
        _loadingOverlay.ShowLoading(message);
        _shell.Enabled = false;
        _startPage.Enabled = false;
        var succeeded = false;
        Exception? failure = null;

        try
        {
            await Task.Delay(140);
            succeeded = action();
            await Task.Delay(120);
        }
        catch (Exception ex)
        {
            failure = ex;
        }
        finally
        {
            _shell.Enabled = true;
            _startPage.Enabled = true;
            _loadingOverlay.HideLoading();
            _loadingOverlay.SendToBack();
        }

        if (failure is not null)
        {
            MessageBox.Show(this, failure.Message, "Terminalia Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!succeeded)
        {
            onFailure();
        }

        return succeeded;
    }

    private void ShowEditor()
    {
        if (_shell.Visible)
        {
            return;
        }

        _shell.Visible = true;
        _startPage.Visible = false;
        _shell.BringToFront();
        _loadingOverlay.BringToFront();
    }

    private void ApplyMapProperties(MapPropertiesChangedEventArgs args)
    {
        _state.Document.Name = string.IsNullOrWhiteSpace(args.Name)
            ? "untitled_map"
            : args.Name.Trim();
        _state.Document.Resize(args.Width, args.Height);
        _shell.Canvas.RefreshDocument();
        _shell.Runtime.LoadDocument(_state.Document.ToTileMap());
        UpdateStatus();
    }

    private void ToggleEmbeddedRun()
    {
        if (_shell.Runtime.IsRunning)
        {
            _shell.Runtime.StopGame();
            _shell.ExitGameMode();
        }
        else
        {
            _shell.Runtime.StartGame();
            _shell.EnterGameMode();
            _shell.Runtime.Focus();
        }

        _shell.Inspector.SetRunState(_shell.Runtime.IsRunning);
        UpdateStatus();
    }

    private void MarkDirty()
    {
        _state.HasUnsavedChanges = true;
        _shell.Runtime.LoadDocument(_state.Document.ToTileMap(), preserveRunState: true);
        UpdateStatus();
    }

    private void MarkClean()
    {
        _state.HasUnsavedChanges = false;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var dirtyMarker = _state.HasUnsavedChanges ? "*" : "";
        var cellText = _state.CursorCell is { } cell
            ? $"Cell: {cell.X},{cell.Y}"
            : "Cell: -";

        Text = $"Terminalia Editor{dirtyMarker}";
        _shell.SetStatus(
            $"{_state.Document.Name}{dirtyMarker} | {_state.Document.Width}x{_state.Document.Height} | {cellText} | Tool: {_state.ToolMode} | Tile: {_state.SelectedTile} | {_state.CurrentFile ?? "Unsaved"}");
    }

    private bool ConfirmCanDiscardChanges()
    {
        if (!_state.HasUnsavedChanges)
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

    private void ShowRepositoryNotFound()
    {
        MessageBox.Show(
            this,
            "Could not find Terminalia.sln from the editor output folder.",
            "Terminalia Editor",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
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
