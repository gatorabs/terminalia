using Terminalia.Editor.Controls.Inspector;

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
        _shell.CommandBar.MapPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Map);
        _shell.CommandBar.ToolPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Tool);
        _shell.CommandBar.PalettePanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Palette);
        _shell.CommandBar.RunPanelRequested += (_, _) => _shell.ToggleInspectorSection(InspectorSection.Run);

        _startPage.NewMapRequested += (_, _) => NewMap();
        _startPage.OpenMapRequested += (_, _) => OpenMap();

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

        _shell.Runtime.GameStopped += (_, _) =>
        {
            _shell.ExitGameMode();
            _shell.Inspector.SetRunState(false);
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
}
