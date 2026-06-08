using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed partial class MainForm
{
    private sealed class MapTab
    {
        public EditorState State { get; } = new();
        public string DisplayName => State.CurrentFile is not null
            ? Path.GetFileNameWithoutExtension(State.CurrentFile)
            : State.Document?.Name ?? "untitled";
    }

    private readonly List<MapTab> _tabs = [];
    private int _activeTabIndex = -1;

    private void AddTab(MapAssetDocument document, string? filePath, bool markClean)
    {
        var tab = new MapTab();
        _tabs.Add(tab);
        _activeTabIndex = _tabs.Count - 1;
        _state = tab.State;
        LoadDocument(document, filePath, markClean);
        ShowEditor();
    }

    private void SwitchToTab(int index)
    {
        if (index < 0 || index >= _tabs.Count || index == _activeTabIndex)
        {
            return;
        }

        if (_shell.Runtime.IsRunning)
        {
            _shell.Runtime.StopGame();
            _shell.ExitGameMode();
            _shell.Inspector.SetRunState(false);
        }

        _activeTabIndex = index;
        _state = _tabs[index].State;

        _shell.Canvas.LoadDocument(_state.Document);
        _shell.Canvas.SelectedTile = _state.SelectedTile;
        _shell.Canvas.SelectedZone = _state.SelectedZone;
        _shell.Canvas.ToolMode = _state.ToolMode;
        _shell.Runtime.LoadDocument(_state.Document.ToTileMap());
        _shell.Inspector.LoadDocument(_state.Document.Name, _state.Document.Width, _state.Document.Height, _state.Document.CameraZoom);
        _shell.Inspector.LoadZones(_state.Document.GetZoneNames());
        RefreshTabBar();
        UpdateStatus();
    }

    private void CloseTab(int index)
    {
        if (index < 0 || index >= _tabs.Count)
        {
            return;
        }

        var tab = _tabs[index];
        if (tab.State.HasUnsavedChanges)
        {
            SwitchToTab(index);

            var result = MessageBox.Show(this,
                $"Save '{tab.DisplayName}' before closing?",
                "Terminalia Editor",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                return;
            }

            if (result == DialogResult.Yes && !SaveMap())
            {
                return;
            }
        }

        _tabs.RemoveAt(index);

        if (_tabs.Count == 0)
        {
            AddTab(_fileService.CreateDefaultMap(), null, markClean: true);
            return;
        }

        SwitchToTab(Math.Min(index, _tabs.Count - 1));
    }

    private void RefreshTabBar()
    {
        var tabs = _tabs.Select(t => (t.DisplayName, t.State.HasUnsavedChanges)).ToList();
        _shell.TabBar.SetTabs(tabs, _activeTabIndex);
    }

    private bool ConfirmCanCloseAllTabs()
    {
        var dirty = _tabs.Where(t => t.State.HasUnsavedChanges).ToList();
        if (dirty.Count == 0)
        {
            return true;
        }

        var names = string.Join(", ", dirty.Select(t => $"'{t.DisplayName}'"));
        var result = MessageBox.Show(this,
            $"Save changes to {names}?",
            "Terminalia Editor",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (result == DialogResult.Cancel)
        {
            return false;
        }

        if (result == DialogResult.No)
        {
            return true;
        }

        foreach (var tab in dirty)
        {
            SwitchToTab(_tabs.IndexOf(tab));
            if (!SaveMap())
            {
                return false;
            }
        }

        return true;
    }
}
