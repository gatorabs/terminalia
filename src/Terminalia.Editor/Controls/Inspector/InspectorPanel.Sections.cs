using Terminalia.Assets.Json;
using Terminalia.Core.Assets;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;

namespace Terminalia.Editor.Controls.Inspector;

public sealed partial class InspectorPanel
{
    private void AddSection(InspectorSection section, Control control, int height)
    {
        _sections[section] = control;
        _sectionHeights[section] = height;
        _stack.Controls.Add(control);
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

        return InspectorSectionCard.Create("Map", layout, 192);
    }

    private Control BuildToolSection()
    {
        _toolSection.ToolChanged += (_, tool) => ToolChanged?.Invoke(this, tool);
        return InspectorSectionCard.Create("Tool", _toolSection, 128);
    }

    private Control BuildObjectSection()
    {
        _objectSection.PropertyEdited += (_, args) => ObjectPropertyEdited?.Invoke(this, args);
        _objectSection.KernelPreviewToggled += (_, show) => KernelPreviewToggled?.Invoke(this, show);
        return InspectorSectionCard.Create("Object", _objectSection, 128);
    }

    public void LoadGameDefinition(GameAssetDocument document)
    {
        _gameSection.LoadGame(document);
    }

    public void LoadAvailableMapsForGame(IEnumerable<string> mapNames)
    {
        _gameSection.LoadAvailableMaps(mapNames);
    }

    private Control BuildGameSection()
    {
        _gameSection.GameDefinitionApplied += (_, args) => GameDefinitionApplied?.Invoke(this, args);
        _gameSection.PreferredHeightChanged += (_, height) => ResizeSection(InspectorSection.Game, height);
        return InspectorSectionCard.Create("Game", _gameSection, 250);
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
        return InspectorSectionCard.Create("Run", layout, 100);
    }
}
