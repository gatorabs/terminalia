using Terminalia.Assets.Json;
using Terminalia.Core.Assets;
using Terminalia.Editor.Controls.Shared;
using Terminalia.Editor.Selection;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Inspector;

public sealed class InspectorPanel : UserControl
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

    private Control BuildMapSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(0, 2, 0, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        EditorControls.StyleTextBox(_nameTextBox);
        _widthInput.Minimum = 4;
        _widthInput.Maximum = 80;
        _heightInput.Minimum = 4;
        _heightInput.Maximum = 40;
        _cameraZoomInput.Minimum = TileMap.MinCameraZoom;
        _cameraZoomInput.Maximum = TileMap.MaxCameraZoom;
        _cameraZoomInput.Increment = 4;
        EditorControls.StyleNumberInput(_widthInput);
        EditorControls.StyleNumberInput(_heightInput);
        EditorControls.StyleNumberInput(_cameraZoomInput);

        var applyButton = EditorControls.CreateButton("Apply", primary: true);
        applyButton.Dock = DockStyle.Fill;
        applyButton.Height = 30;
        applyButton.Click += (_, _) => ApplyMapPropertiesRequested?.Invoke(this, new MapPropertiesChangedEventArgs(_nameTextBox.Text, (int)_widthInput.Value, (int)_heightInput.Value, (int)_cameraZoomInput.Value));

        layout.Controls.Add(EditorControls.CreateFieldLabel("Name"), 0, 0);
        layout.Controls.Add(_nameTextBox, 1, 0);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Width"), 0, 1);
        layout.Controls.Add(_widthInput, 1, 1);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Height"), 0, 2);
        layout.Controls.Add(_heightInput, 1, 2);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Zoom"), 0, 3);
        layout.Controls.Add(_cameraZoomInput, 1, 3);
        layout.Controls.Add(applyButton, 0, 4);
        layout.SetColumnSpan(applyButton, 2);

        return CreateSection("Map", layout, 192);
    }

    private Control BuildToolSection()
    {
        _toolSection.ToolChanged += (_, tool) => ToolChanged?.Invoke(this, tool);
        return CreateSection("Tool", _toolSection, 128);
    }

    private Control BuildPaletteSection()
    {
        var tilesView = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = EditorColors.Section
        };
        tilesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tilesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tilesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tilesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tilesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tilesView.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tilesView.Controls.Add(CreateTileButton("#  Wall", '#'), 0, 0);
        tilesView.Controls.Add(CreateTileButton(".  Floor", '.'), 0, 1);
        tilesView.Controls.Add(CreateTileButton("@  Player", '@'), 0, 2);
        tilesView.Controls.Add(CreateTileButton("*  Relic", '*'), 0, 3);

        var zonesNavButton = EditorControls.CreateButton("Z  Zones");
        zonesNavButton.Dock = DockStyle.Fill;
        zonesNavButton.TextAlign = ContentAlignment.MiddleLeft;
        zonesNavButton.Margin = new Padding(0, 0, 0, 4);
        tilesView.Controls.Add(zonesNavButton, 0, 4);

        var zonesView = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            BackColor = EditorColors.Section,
            Padding = new Padding(0, 2, 0, 0),
            Visible = false
        };
        zonesView.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        zonesView.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        zonesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        zonesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        zonesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        zonesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        zonesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        zonesView.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var backButton = EditorControls.CreateButton("‹  Tiles");
        backButton.Dock = DockStyle.Fill;
        zonesView.Controls.Add(backButton, 0, 0);
        zonesView.SetColumnSpan(backButton, 2);

        EditorControls.StyleTextBox(_zoneNameTextBox);

        EditorControls.StyleComboBox(_zoneTargetCombo);
        _zoneTargetCombo.DropDownStyle = ComboBoxStyle.DropDown;

        EditorControls.StyleListBox(_zoneListBox);
        _zoneListBox.SelectedIndexChanged += (_, _) =>
        {
            if (_suppressZoneEvents)
            {
                return;
            }
            var selected = _zoneListBox.SelectedItem as string;
            ZoneSelected?.Invoke(this, new ZoneSelectedEventArgs(selected));
        };

        var addButton = EditorControls.CreateButton("Add / Update", primary: true);
        addButton.Dock = DockStyle.Fill;
        addButton.Click += (_, _) =>
        {
            var name = _zoneNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            ZoneDefinitionApplied?.Invoke(this, new ZoneDefinedEventArgs(name, _zoneTargetCombo.Text.Trim()));
        };

        var removeButton = EditorControls.CreateButton("Remove");
        removeButton.Dock = DockStyle.Fill;
        removeButton.Click += (_, _) =>
        {
            if (_zoneListBox.SelectedItem is string name)
            {
                ZoneRemoveRequested?.Invoke(this, name);
            }
        };

        zonesView.Controls.Add(EditorControls.CreateFieldLabel("Name"), 0, 1);
        zonesView.Controls.Add(_zoneNameTextBox, 1, 1);
        zonesView.Controls.Add(EditorControls.CreateFieldLabel("Target"), 0, 2);
        zonesView.Controls.Add(_zoneTargetCombo, 1, 2);
        zonesView.Controls.Add(addButton, 0, 3);
        zonesView.SetColumnSpan(addButton, 2);
        zonesView.Controls.Add(_zoneListBox, 0, 4);
        zonesView.SetColumnSpan(_zoneListBox, 2);
        zonesView.Controls.Add(removeButton, 0, 5);
        zonesView.SetColumnSpan(removeButton, 2);

        zonesNavButton.Click += (_, _) =>
        {
            tilesView.Visible = false;
            zonesView.Visible = true;
            _sectionHeights[InspectorSection.Palette] = 315;
            _sections[InspectorSection.Palette].Height = 315;
            Height = ActiveSectionHeight;
            SectionResized?.Invoke(this, EventArgs.Empty);
        };

        backButton.Click += (_, _) =>
        {
            zonesView.Visible = false;
            tilesView.Visible = true;
            _sectionHeights[InspectorSection.Palette] = 255;
            _sections[InspectorSection.Palette].Height = 255;
            Height = ActiveSectionHeight;
            SectionResized?.Invoke(this, EventArgs.Empty);
            ZoneSelected?.Invoke(this, new ZoneSelectedEventArgs(null));
        };

        var container = new Panel { Dock = DockStyle.Fill, BackColor = EditorColors.Section };
        container.Controls.Add(zonesView);
        container.Controls.Add(tilesView);

        return CreateSection("Palette", container, 255);
    }

    private Control BuildObjectSection()
    {
        _objectSection.PropertyEdited += (_, args) => ObjectPropertyEdited?.Invoke(this, args);
        _objectSection.KernelPreviewToggled += (_, show) => KernelPreviewToggled?.Invoke(this, show);
        return CreateSection("Object", _objectSection, 128);
    }

    private Control BuildGameSection()
    {
        _gameSection.GameDefinitionApplied += (_, args) => GameDefinitionApplied?.Invoke(this, args);
        _gameSection.PreferredHeightChanged += (_, height) => ResizeSection(InspectorSection.Game, height);
        return CreateSection("Game", _gameSection, 250);
    }

    public void LoadGameDefinition(GameAssetDocument document)
    {
        _gameSection.LoadGame(document);
    }

    public void LoadAvailableMapsForGame(IEnumerable<string> mapNames)
    {
        _gameSection.LoadAvailableMaps(mapNames);
    }

    private void ResizeSection(InspectorSection section, int height)
    {
        _sectionHeights[section] = height;
        _sections[section].Height = height;
        Height = ActiveSectionHeight;
        SectionResized?.Invoke(this, EventArgs.Empty);
    }

    private void ResizeObjectSection()
    {
        ResizeSection(InspectorSection.Object, _objectSection.PreferredSectionHeight);
    }

    private Control BuildRunSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Padding = new Padding(0, 4, 0, 0),
            BackColor = EditorColors.Section
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        _runEmbeddedButton.Dock = DockStyle.Fill;
        _runEmbeddedButton.Click += (_, _) => RunEmbeddedRequested?.Invoke(this, EventArgs.Empty);

        layout.Controls.Add(_runEmbeddedButton, 0, 0);
        return CreateSection("Run", layout, 100);
    }

    private static Panel CreateSection(string title, Control content, int height)
    {
        var section = new CardPanel
        {
            Dock = DockStyle.Top,
            Height = height,
            FillColor = EditorColors.Section,
            BorderColor = EditorColors.BorderSoft,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 10)
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 24,
            Font = EditorFonts.SectionFont,
            ForeColor = EditorColors.SecondaryText,
            BackColor = EditorColors.Section,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var separator = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = EditorColors.BorderSoft
        };

        var contentArea = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = EditorColors.Section,
            Padding = new Padding(0, 6, 0, 0)
        };
        EditorControls.ApplyDarkScrollBars(contentArea);

        content.Dock = DockStyle.Fill;
        contentArea.Controls.Add(content);

        section.Controls.Add(contentArea);
        section.Controls.Add(separator);
        section.Controls.Add(titleLabel);

        return section;
    }

    private Button CreateTileButton(string text, char tile)
    {
        var button = EditorControls.CreateButton(text);
        button.Dock = DockStyle.Fill;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Margin = new Padding(0, 0, 0, 4);
        button.Click += (_, _) => TileSelected?.Invoke(this, tile);
        return button;
    }

    private void AddSection(InspectorSection section, Control control, int height)
    {
        _sections[section] = control;
        _sectionHeights[section] = height;
        _stack.Controls.Add(control);
    }
}

public enum InspectorSection
{
    Map,
    Tool,
    Palette,
    Object,
    Game,
    Run
}

public sealed class MapPropertiesChangedEventArgs(string name, int width, int height, int cameraZoom) : EventArgs
{
    public string Name { get; } = name;
    public int Width { get; } = width;
    public int Height { get; } = height;
    public int CameraZoom { get; } = cameraZoom;
}

public sealed class ZoneDefinedEventArgs(string name, string? targetMap) : EventArgs
{
    public string Name { get; } = name;
    public string? TargetMap { get; } = string.IsNullOrWhiteSpace(targetMap) ? null : targetMap;
}

public sealed class ZoneSelectedEventArgs(string? zoneName) : EventArgs
{
    public string? ZoneName { get; } = zoneName;
}
