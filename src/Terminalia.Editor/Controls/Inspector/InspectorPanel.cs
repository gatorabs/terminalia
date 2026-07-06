using Terminalia.Editor.Selection;
using Terminalia.Editor.State;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Inspector;

public sealed partial class InspectorPanel : UserControl
{
    private readonly Dictionary<InspectorSection, Control> _sections = [];
    private readonly Dictionary<InspectorSection, int> _sectionHeights = [];
    private readonly Panel _stack = new();
    private readonly TextBox _nameTextBox = new();
    private readonly NumericUpDown _widthInput = new();
    private readonly NumericUpDown _heightInput = new();
    private readonly NumericUpDown _cameraZoomInput = new();
    private readonly InspectorToolSection _toolSection = new();
    private readonly InspectorObjectSection _objectSection = new();
    private readonly InspectorGameSection _gameSection = new();
    private readonly Button _runEmbeddedButton = EditorControls.CreateButton("Run", primary: true);
    private readonly TextBox _zoneNameTextBox = new();
    private readonly ComboBox _zoneTargetCombo = new();
    private readonly ListBox _zoneListBox = new();
    private bool _suppressZoneEvents;

    public event EventHandler<MapPropertiesChangedEventArgs>? ApplyMapPropertiesRequested;
    public event EventHandler<EditorToolMode>? ToolChanged;
    public event EventHandler<char>? TileSelected;
    public event EventHandler? RunEmbeddedRequested;
    public event EventHandler<ZoneDefinedEventArgs>? ZoneDefinitionApplied;
    public event EventHandler<ZoneSelectedEventArgs>? ZoneSelected;
    public event EventHandler<string>? ZoneRemoveRequested;
    public event EventHandler<bool>? KernelPreviewToggled;
    public event EventHandler<ObjectPropertyEditedEventArgs>? ObjectPropertyEdited;
    public event EventHandler<GameDefinitionAppliedEventArgs>? GameDefinitionApplied;
    public event EventHandler? SectionResized;

    public InspectorPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.Transparent;
        Font = EditorFonts.UiFont;

        _stack.Dock = DockStyle.Top;
        _stack.AutoSize = true;
        _stack.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _stack.BackColor = EditorColors.Section;
        _stack.Padding = new Padding(0);

        AddSection(InspectorSection.Map, BuildMapSection(), 192);
        AddSection(InspectorSection.Tool, BuildToolSection(), 128);
        AddSection(InspectorSection.Palette, BuildPaletteSection(), 255);
        AddSection(InspectorSection.Object, BuildObjectSection(), 128);
        AddSection(InspectorSection.Game, BuildGameSection(), 250);
        AddSection(InspectorSection.Run, BuildRunSection(), 138);

        Controls.Add(_stack);
        SetActiveSection(InspectorSection.Map);
    }

    public InspectorSection ActiveSection { get; private set; } = InspectorSection.Map;

    public int ActiveSectionHeight => _sectionHeights.TryGetValue(ActiveSection, out var height)
        ? height
        : 220;

    public void SetActiveSection(InspectorSection section)
    {
        ActiveSection = section;
        foreach (var (key, control) in _sections)
        {
            control.Visible = key == section;
        }

        Height = ActiveSectionHeight;
        Invalidate();
    }

    public void LoadDocument(string name, int width, int height, int cameraZoom)
    {
        _nameTextBox.Text = name;
        _widthInput.Value = width;
        _heightInput.Value = height;
        _cameraZoomInput.Value = Math.Clamp(cameraZoom, (int)_cameraZoomInput.Minimum, (int)_cameraZoomInput.Maximum);
    }

    public void LoadZones(IEnumerable<string> zoneNames)
    {
        _suppressZoneEvents = true;
        _zoneListBox.Items.Clear();
        foreach (var name in zoneNames)
        {
            _zoneListBox.Items.Add(name);
        }
        _suppressZoneEvents = false;
    }

    public void LoadAvailableMaps(IEnumerable<string> mapNames)
    {
        var current = _zoneTargetCombo.Text;
        _zoneTargetCombo.Items.Clear();
        foreach (var name in mapNames)
        {
            _zoneTargetCombo.Items.Add(name);
        }
        _zoneTargetCombo.Text = current;
    }

    public void SetRunState(bool isRunning)
    {
        _runEmbeddedButton.Text = isRunning ? "Stop" : "Run";
    }

    public void SelectTile(char tile)
    {
        TileSelected?.Invoke(this, tile);
    }

    public void SetToolMode(EditorToolMode mode)
    {
        _toolSection.SetToolMode(mode);
    }

    public void ShowObjectSelection(SelectedObjectInfo info, bool kernelPreviewActive)
    {
        _objectSection.ShowObject(info, kernelPreviewActive);
        ResizeObjectSection();
        SetActiveSection(InspectorSection.Object);
    }

    public void ClearObjectSelection()
    {
        _objectSection.ClearObject();
        ResizeObjectSection();
    }

    public void ClearZoneSelection()
    {
        _suppressZoneEvents = true;
        _zoneListBox.ClearSelected();
        _suppressZoneEvents = false;
    }
}
