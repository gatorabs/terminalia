using Terminalia.Editor.Controls.Inspector;
using Terminalia.Editor.Services;

namespace Terminalia.Editor;

public sealed partial class MainForm
{
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
        _state.Document.SetCameraZoom(args.CameraZoom);
        _shell.Canvas.RefreshDocument();
        _shell.Runtime.LoadDocument(_state.Document.ToTileMap());
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
