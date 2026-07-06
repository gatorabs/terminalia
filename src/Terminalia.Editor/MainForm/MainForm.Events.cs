using Terminalia.Editor.Controls.Inspector;
using Terminalia.Editor.Selection.Descriptors;
using Terminalia.Editor.State;

namespace Terminalia.Editor;

public sealed partial class MainForm
{
    private void WireEvents()
    {
        _shell.CommandBar.NewRequested += (_, _) => NewMap();
        _shell.CommandBar.OpenRequested += (_, _) => OpenMap();
        _shell.CommandBar.SaveRequested += (_, _) => SaveMap();
        _shell.CommandBar.SaveAsRequested += (_, _) => SaveMapAs();
        _shell.CommandBar.RunExternalRequested += (_, _) => LaunchGameWindow();
        _shell.CommandBar.ExportRequested += (_, _) => ExportGame();
        _shell.CommandBar.MapPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Map);
        _shell.CommandBar.ToolPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Tool);
        _shell.CommandBar.PalettePanelRequested += (_, _) =>
        {
            _shell.ToggleInspectorSection(InspectorSection.Palette);
            RefreshAvailableMaps();
        };
        _shell.CommandBar.RunPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Run);
        _shell.CommandBar.GamePanelRequested += (_, _) =>
        {
            _shell.Inspector.LoadGameDefinition(_gameDocument);
            _shell.Inspector.LoadAvailableMapsForGame(GetAvailableMapNames());
            _shell.ToggleInspectorSection(InspectorSection.Game);
        };

        _startPage.NewMapRequested += (_, _) => NewMap();
        _startPage.OpenMapRequested += (_, _) => OpenMap();

        _shell.Inspector.ApplyMapPropertiesRequested += (_, args) => ApplyMapProperties(args);
        _shell.Inspector.GameDefinitionApplied += (_, args) => ApplyGameDefinition(args.Document);
        _shell.Inspector.ToolChanged += (_, tool) => ApplyToolMode(tool);
        _shell.CommandBar.SelectToolRequested += (_, _) =>
            ApplyToolMode(_state.ToolMode == EditorToolMode.Select ? EditorToolMode.Paint : EditorToolMode.Select);
        _shell.Inspector.TileSelected += (_, tile) =>
        {
            _state.SelectedTile = tile;
            _shell.SelectTile(tile);
            UpdateStatus();
        };
        _shell.Inspector.RunEmbeddedRequested += (_, _) => ToggleEmbeddedRun();

        _shell.Inspector.ZoneDefinitionApplied += (_, args) =>
        {
            _state.Document.DefineZone(args.Name, args.TargetMap);
            _shell.Inspector.LoadZones(_state.Document.GetZoneNames());
            MarkDirty();
        };

        _shell.Inspector.ZoneRemoveRequested += (_, name) =>
        {
            _state.Document.RemoveZoneDefinition(name);
            _shell.Inspector.LoadZones(_state.Document.GetZoneNames());
            if (_state.SelectedZone == name)
            {
                ApplyToolMode(EditorToolMode.Paint);
            }
            MarkDirty();
        };

        _shell.Inspector.ZoneSelected += (_, args) =>
        {
            _state.SelectedZone = args.ZoneName;
            _state.ToolMode = args.ZoneName is not null ? EditorToolMode.Zone : EditorToolMode.Paint;
            _shell.SelectZone(args.ZoneName, _state.ToolMode);
            UpdateStatus();
        };

        _shell.Canvas.ZonePicked += (_, zoneName) =>
        {
            _state.SelectedZone = zoneName;
            _shell.SetSelectedZone(zoneName);
            UpdateStatus();
        };

        _shell.Runtime.GameStopped += (_, _) =>
        {
            _shell.ExitGameMode();
            _shell.Inspector.SetRunState(false);
            UpdateStatus();
        };

        _shell.Runtime.TransitionFailed += (_, msg) =>
            MessageBox.Show(this, msg, "Zone Transition", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        _shell.TabBar.TabSelected += (_, i) => SwitchToTab(i);
        _shell.TabBar.TabCloseRequested += (_, i) => CloseTab(i);
        _shell.TabBar.NewTabRequested += (_, _) => NewMap();

        _shell.Canvas.DocumentChanged += (_, _) => MarkDirty();
        _shell.Canvas.CursorCellChanged += (_, _) =>
        {
            _state.CursorCell = _shell.Canvas.CursorCell;
            UpdateStatus();
        };
        _shell.Canvas.TilePicked += (_, tile) =>
        {
            _state.SelectedTile = tile;
            _shell.SelectTile(tile);
            _shell.Inspector.SelectTile(tile);
            UpdateStatus();
        };

        _shell.Canvas.SelectionChanged += (_, cell) =>
        {
            if (cell is { } selected)
            {
                _shell.ShowObjectSelection(_selectableRegistry.Describe(_state.Document, selected));
            }
            else
            {
                _shell.Inspector.ClearObjectSelection();
            }
        };

        _shell.Inspector.ObjectPropertyEdited += (_, args) =>
        {
            if (_shell.Canvas.SelectedCell is not { } cell)
            {
                return;
            }

            _selectableRegistry.ApplyProperty(_state.Document, cell, args.Key, args.Value);
            if (args.Key == PlayerTileDescriptor.CameraZoomKey)
            {
                _shell.RefreshCameraZoom(_state.Document);
            }
            MarkDirty();
        };

        _shell.Inspector.KernelPreviewToggled += (_, show) => _shell.Canvas.KernelPreviewVisible = show;
    }

    private void ApplyToolMode(EditorToolMode tool)
    {
        _state.ToolMode = tool;
        _state.SelectedZone = null;
        _shell.ApplyToolMode(tool);
        UpdateStatus();
    }
}
