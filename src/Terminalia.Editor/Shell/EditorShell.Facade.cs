using Terminalia.Assets.Json;
using Terminalia.Editor.Controls.Inspector;
using Terminalia.Editor.Selection;
using Terminalia.Editor.State;

namespace Terminalia.Editor.Shell;

public sealed partial class EditorShell
{
    public void LoadMapDocument(MapAssetDocument document)
    {
        Canvas.LoadDocument(document);
        Canvas.SelectedZone = null;
        Runtime.LoadDocument(document.ToTileMap());
        Inspector.LoadDocument(document.Name, document.Width, document.Height, document.CameraZoom);
        Inspector.LoadZones(document.GetZoneNames());
    }

    public void StopRunningGame()
    {
        Runtime.StopGame();
        ExitGameMode();
        Inspector.SetRunState(false);
    }

    public void ApplyToolMode(EditorToolMode tool)
    {
        Canvas.ToolMode = tool;
        Canvas.SelectedZone = null;
        Inspector.ClearZoneSelection();
        Inspector.SetToolMode(tool);
        CommandBar.SetSelectToolActive(tool == EditorToolMode.Select);
        if (tool != EditorToolMode.Select)
        {
            Canvas.ClearSelection();
        }
    }

    public void SelectZone(string? zoneName, EditorToolMode toolMode)
    {
        Canvas.SelectedZone = zoneName;
        Canvas.ToolMode = toolMode;
        Inspector.SetToolMode(toolMode);
        CommandBar.SetSelectToolActive(false);
        Canvas.ClearSelection();
    }

    public void SetSelectedZone(string? zoneName)
    {
        Canvas.SelectedZone = zoneName;
    }

    public void SelectTile(char tile)
    {
        Canvas.SelectedTile = tile;
    }

    public void ShowObjectSelection(SelectedObjectInfo info)
    {
        Inspector.ShowObjectSelection(info, Canvas.KernelPreviewVisible);
        ShowInspectorSection(InspectorSection.Object);
    }

    public void RefreshCameraZoom(MapAssetDocument document)
    {
        Inspector.LoadDocument(document.Name, document.Width, document.Height, document.CameraZoom);
        Canvas.Invalidate();
    }
}
