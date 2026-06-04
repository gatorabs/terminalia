using System.Diagnostics;
using Terminalia.Assets.Json;
using Terminalia.Editor.Controls.Inspector;

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
        _shell.Canvas.LoadDocument(document);
        _shell.Runtime.LoadDocument(document.ToTileMap());
        _shell.Inspector.LoadDocument(document.Name, document.Width, document.Height);
        ShowEditor();

        if (markClean)
            MarkClean();
        else
            MarkDirty();

        UpdateStatus();
    }

    private void NewMap()
    {
        if (!ConfirmCanDiscardChanges())
            return;

        LoadDocument(_fileService.CreateDefaultMap(), currentFile: null, markClean: true);
    }

    private void OpenMap()
    {
        if (!ConfirmCanDiscardChanges())
            return;

        using var dialog = new OpenFileDialog
        {
            Filter = "Terminalia map (*.json)|*.json|All files (*.*)|*.*",
            Title = "Open Terminalia map"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        LoadDocument(_fileService.LoadMap(dialog.FileName), dialog.FileName, markClean: true);
    }

    private bool SaveMap()
    {
        if (_state.CurrentFile is null)
            return SaveMapAs();

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
            return false;

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
            return;

        var outputDir = Path.Combine(dialog.SelectedPath, _state.Document.Name);
        var progress = new Progress<string>(msg => BeginInvoke(() => _loadingOverlay.ShowLoading(msg)));

        _loadingOverlay.ShowLoading("Exporting game...");

        var result = await _exportService.ExportAsync(_state.Document, outputDir, progress);

        _loadingOverlay.HideLoading();

        if (result.IsSuccess)
        {
            var response = MessageBox.Show(this,
                $"Exported to:\n{result.OutputDirectory}\n\nOpen folder?",
                "Export Complete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (response == DialogResult.Yes)
                Process.Start("explorer.exe", result.OutputDirectory);
        }
        else
        {
            MessageBox.Show(this, result.ErrorMessage, "Export Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LaunchGameWindow()
    {
        var window = new Terminalia.Runtime.Windows.GameWindow(
            _state.Document.ToTileMap(),
            cellSize: _shell.Canvas.CellSize);
        window.Show(this);
    }

    private void ShowEditor()
    {
        if (_shell.Visible)
            return;

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
            _shell.EnterGameMode();
            _shell.Runtime.StartGame();
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
            return true;

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

}
