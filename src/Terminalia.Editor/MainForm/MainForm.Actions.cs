using System.Diagnostics;
using Terminalia.Assets.Json;
using Terminalia.Editor.Controls.Inspector;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor;

public sealed partial class MainForm
{
    private void LoadDocument(MapAssetDocument document, string? currentFile, bool markClean)
    {
        if (_shell.Runtime.IsRunning)
        {
            _shell.Runtime.StopGame();
            _shell.ExitGameMode();
            _shell.Inspector.SetRunState(false);
        }

        _state.Document = document;
        _state.CurrentFile = currentFile;
        _state.SelectedZone = null;

        _shell.Canvas.LoadDocument(document);
        _shell.Canvas.SelectedZone = null;
        _shell.Runtime.LoadDocument(document.ToTileMap());
        _shell.Inspector.LoadDocument(document.Name, document.Width, document.Height, document.CameraZoom);
        _shell.Inspector.LoadZones(document.GetZoneNames());

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
        var name = PromptMapName();
        if (name is null)
        {
            return;
        }

        if (IsMapNameTaken(name))
        {
            MessageBox.Show(this, $"A map named '{name}' already exists. Choose a different name.",
                "New Map", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var doc = _fileService.CreateDefaultMap();
        if (!string.IsNullOrWhiteSpace(name))
        {
            doc.Name = name;
        }
        AddTab(doc, filePath: null, markClean: true);
    }

    private bool IsMapNameTaken(string name)
    {
        var takenByTab = _tabs.Any(t =>
        {
            var tabName = t.State.CurrentFile is not null
                ? Path.GetFileNameWithoutExtension(t.State.CurrentFile)
                : t.State.Document.Name;
            return string.Equals(tabName, name, StringComparison.OrdinalIgnoreCase);
        });

        if (takenByTab)
        {
            return true;
        }

        return File.Exists(EditorFileService.AutoSavePath(name));
    }

    private string? PromptMapName()
    {
        using var form = new Form
        {
            Text = "New Map",
            Size = new Size(300, 175),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = EditorColors.AppBackground,
            ForeColor = EditorColors.PrimaryText,
            Font = EditorFonts.UiFont
        };

        var textBox = new TextBox { Text = "new_map", Dock = DockStyle.Fill };
        EditorControls.StyleTextBox(textBox);

        var mapsDir = EditorFileService.DefaultMapsDirectory;
        var pathHint = new Label
        {
            Text = $"Salvo em: {mapsDir}",
            Dock = DockStyle.Fill,
            AutoSize = false,
            AutoEllipsis = true,
            ForeColor = EditorColors.SecondaryText,
            Font = new Font(EditorFonts.UiFont.FontFamily, 7.5f)
        };

        var ok = EditorControls.CreateButton("Create", primary: true);
        ok.DialogResult = DialogResult.OK;
        ok.Dock = DockStyle.Fill;

        var cancel = EditorControls.CreateButton("Cancel");
        cancel.DialogResult = DialogResult.Cancel;
        cancel.Dock = DockStyle.Fill;

        var buttonRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttonRow.Controls.Add(ok, 0, 0);
        buttonRow.Controls.Add(cancel, 1, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12),
            BackColor = EditorColors.AppBackground
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        layout.Controls.Add(EditorControls.CreateFieldLabel("Map name:"), 0, 0);
        layout.Controls.Add(textBox, 0, 1);
        layout.Controls.Add(pathHint, 0, 2);
        layout.Controls.Add(buttonRow, 0, 3);

        form.Controls.Add(layout);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        textBox.SelectAll();

        return form.ShowDialog(this) == DialogResult.OK ? textBox.Text.Trim() : null;
    }

    private void OpenMap()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Terminalia map (*.json)|*.json|All files (*.*)|*.*",
            Title = "Open Terminalia map",
            InitialDirectory = EditorFileService.DefaultMapsDirectory
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var existingIndex = _tabs.FindIndex(t =>
            string.Equals(t.State.CurrentFile, dialog.FileName, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            SwitchToTab(existingIndex);
            return;
        }

        AddTab(_fileService.LoadMap(dialog.FileName), dialog.FileName, markClean: true);
    }

    private bool SaveMap()
    {
        if (_state.CurrentFile is null)
        {
            _state.CurrentFile = EditorFileService.AutoSavePath(_state.Document.Name);
        }

        _fileService.SaveMap(_state.Document, _state.CurrentFile);
        MarkClean();
        UpdateStatus();
        return true;
    }

    private void SaveAllMaps()
    {
        foreach (var tab in _tabs)
        {
            tab.State.CurrentFile ??= EditorFileService.AutoSavePath(tab.State.Document.Name);
            _fileService.SaveMap(tab.State.Document, tab.State.CurrentFile);
            tab.State.HasUnsavedChanges = false;
        }
        RefreshTabBar();
        UpdateStatus();
    }

    private string GetAssetDirectory() =>
        _state.CurrentFile is not null
            ? Path.GetDirectoryName(_state.CurrentFile) ?? EditorFileService.DefaultMapsDirectory
            : EditorFileService.DefaultMapsDirectory;

    private bool SaveMapAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Terminalia map (*.json)|*.json|All files (*.*)|*.*",
            Title = "Save Terminalia map",
            FileName = $"{_state.Document.Name}.json",
            InitialDirectory = _state.CurrentFile is not null
                ? Path.GetDirectoryName(_state.CurrentFile)
                : EditorFileService.DefaultMapsDirectory
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

    private async void ExportGame()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose export destination",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        SaveAllMaps();
        var outputDir = Path.Combine(dialog.SelectedPath, _state.Document.Name);
        var progress = new Progress<string>(msg => BeginInvoke(() => _loadingOverlay.ShowLoading(msg)));

        _loadingOverlay.ShowLoading("Exporting game...");

        var result = await _exportService.ExportAsync(_state.Document, _gameDocument, outputDir, progress);

        _loadingOverlay.HideLoading();

        if (result.IsSuccess)
        {
            var response = MessageBox.Show(this,
                $"Exported to:\n{result.OutputDirectory}\n\nOpen folder?",
                "Export Complete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (response == DialogResult.Yes)
            {
                Process.Start("explorer.exe", result.OutputDirectory);
            }
        }
        else
        {
            MessageBox.Show(this, result.ErrorMessage, "Export Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LaunchGameWindow()
    {
        SaveAllMaps();
        var window = new Terminalia.Runtime.Windows.GameWindow(
            BuildRunDefinition(), _state.Document.ToTileMap(), assetDirectory: GetAssetDirectory());
        window.Show(this);
    }

    private Terminalia.Core.Assets.GameDefinition BuildRunDefinition()
    {
        var document = new GameAssetDocument
        {
            Title = _gameDocument.Title,
            StartMap = _state.Document.Name,
            TitleMenu = _gameDocument.TitleMenu,
            PauseMenu = _gameDocument.PauseMenu
        };
        return document.ToGameDefinition();
    }

    private void ApplyGameDefinition(GameAssetDocument document)
    {
        try
        {
            _fileService.SaveGame(document);
        }
        catch (Exception ex) when (ex is InvalidDataException or ArgumentException)
        {
            MessageBox.Show(this, ex.Message, "Game Definition", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _gameDocument = document;
        UpdateStatus();
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
        _state.Document.CameraZoom = args.CameraZoom;
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
            SaveAllMaps();
            _shell.EnterGameMode();
            _shell.Runtime.StartGame(BuildRunDefinition(), GetAssetDirectory());
            _shell.Runtime.Focus();
        }

        _shell.Inspector.SetRunState(_shell.Runtime.IsRunning);
        UpdateStatus();
    }

    private void MarkDirty()
    {
        _state.HasUnsavedChanges = true;
        _shell.Runtime.LoadDocument(_state.Document.ToTileMap(), preserveRunState: true);
        RefreshTabBar();
        UpdateStatus();
    }

    private void MarkClean()
    {
        _state.HasUnsavedChanges = false;
        RefreshTabBar();
        UpdateStatus();
    }

    private void RefreshAvailableMaps()
    {
        _shell.Inspector.LoadAvailableMaps(GetAvailableMapNames());
    }

    private IEnumerable<string> GetAvailableMapNames()
    {
        var dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { EditorFileService.DefaultMapsDirectory };

        foreach (var tab in _tabs)
        {
            var dir = tab.State.CurrentFile is not null
                ? Path.GetDirectoryName(tab.State.CurrentFile)
                : null;
            if (dir is not null)
            {
                dirs.Add(dir);
            }
        }

        var fromDisk = dirs
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "*.json"))
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>();

        var fromTabs = _tabs
            .Select(t => t.State.CurrentFile is not null
                ? Path.GetFileNameWithoutExtension(t.State.CurrentFile)
                : t.State.Document.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .OfType<string>();

        return fromDisk
            .Concat(fromTabs)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n);
    }

    private void UpdateStatus()
    {
        if (_shell.Runtime.IsRunning)
        {
            Text = $"Terminalia Editor — {_gameDocument.Title}";
            _shell.SetStatus($"{_gameDocument.Title} | running | [ESC] menu | [Shift+ESC] stop");
            return;
        }

        var dirtyMarker = _state.HasUnsavedChanges ? "*" : "";
        var cellText = _state.CursorCell is { } cell
            ? $"Cell: {cell.X},{cell.Y}"
            : "Cell: -";

        Text = $"Terminalia Editor{dirtyMarker}";
        _shell.SetStatus(
            $"{_state.Document.Name}{dirtyMarker} | {_state.Document.Width}x{_state.Document.Height} | {cellText} | Tool: {_state.ToolMode} | Tile: {_state.SelectedTile} | {_state.CurrentFile ?? "Unsaved"}");
    }

}
