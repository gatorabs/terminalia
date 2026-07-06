using Terminalia.Assets.Json;
using Terminalia.Editor.Screens.Dialogs;
using Terminalia.Editor.Services;

namespace Terminalia.Editor;

public sealed partial class MainForm
{
    private void LoadDocument(MapAssetDocument document, string? currentFile, bool markClean)
    {
        if (_shell.Runtime.IsRunning)
        {
            _shell.StopRunningGame();
        }

        _state.Document = document;
        _state.CurrentFile = currentFile;
        _state.SelectedZone = null;

        _shell.LoadMapDocument(document);

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
        var name = MapNameDialog.Prompt(this);
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

    private string GetAssetDirectory() =>
        _state.CurrentFile is not null
            ? Path.GetDirectoryName(_state.CurrentFile) ?? EditorFileService.DefaultMapsDirectory
            : EditorFileService.DefaultMapsDirectory;
}
