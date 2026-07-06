using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;

namespace Terminalia.Editor.Controls.Inspector;

public sealed partial class InspectorPanel
{
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

        var zonesView = BuildZonesView(out var backButton);

        zonesNavButton.Click += (_, _) =>
        {
            tilesView.Visible = false;
            zonesView.Visible = true;
            ResizeSection(InspectorSection.Palette, 315);
        };

        backButton.Click += (_, _) =>
        {
            zonesView.Visible = false;
            tilesView.Visible = true;
            ResizeSection(InspectorSection.Palette, 255);
            ZoneSelected?.Invoke(this, new ZoneSelectedEventArgs(null));
        };

        var container = new Panel { Dock = DockStyle.Fill, BackColor = EditorColors.Section };
        container.Controls.Add(zonesView);
        container.Controls.Add(tilesView);

        return InspectorSectionCard.Create("Palette", container, 255);
    }

    private TableLayoutPanel BuildZonesView(out Button backButton)
    {
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

        backButton = EditorControls.CreateButton("‹  Tiles");
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

        return zonesView;
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
}
