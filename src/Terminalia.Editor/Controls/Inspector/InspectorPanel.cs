using Terminalia.Editor.Controls.Shared;
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
    private readonly RadioButton _paintButton = new();
    private readonly RadioButton _fillButton = new();
    private readonly Button _runEmbeddedButton = EditorControls.CreateButton("Run", primary: true);
    private readonly TextBox _zoneNameTextBox = new();
    private readonly ComboBox _zoneTargetCombo = new();
    private readonly ListBox _zoneListBox = new();

    public event EventHandler<MapPropertiesChangedEventArgs>? ApplyMapPropertiesRequested;
    public event EventHandler<EditorToolMode>? ToolChanged;
    public event EventHandler<char>? TileSelected;
    public event EventHandler? RunEmbeddedRequested;
    public event EventHandler<ZoneDefinedEventArgs>? ZoneDefinitionApplied;
    public event EventHandler<ZoneSelectedEventArgs>? ZoneSelected;
    public event EventHandler<string>? ZoneRemoveRequested;

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

        AddSection(InspectorSection.Map, BuildMapSection(), 160);
        AddSection(InspectorSection.Tool, BuildToolSection(), 128);
        AddSection(InspectorSection.Palette, BuildPaletteSection(), 440);
        AddSection(InspectorSection.Run, BuildRunSection(), 138);

        Controls.Add(_stack);
        SetActiveSection(InspectorSection.Map);
    }

    public void LoadDocument(string name, int width, int height)
    {
        _nameTextBox.Text = name;
        _widthInput.Value = width;
        _heightInput.Value = height;
    }

    public void LoadZones(IEnumerable<string> zoneNames)
    {
        _zoneListBox.Items.Clear();
        foreach (var name in zoneNames)
        {
            _zoneListBox.Items.Add(name);
        }
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
            RowCount = 4,
            Padding = new Padding(0, 2, 0, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        EditorControls.StyleTextBox(_nameTextBox);
        _widthInput.Minimum = 4;
        _widthInput.Maximum = 80;
        _heightInput.Minimum = 4;
        _heightInput.Maximum = 40;
        EditorControls.StyleNumberInput(_widthInput);
        EditorControls.StyleNumberInput(_heightInput);

        var applyButton = EditorControls.CreateButton("Apply", primary: true);
        applyButton.Dock = DockStyle.Fill;
        applyButton.Height = 30;
        applyButton.Click += (_, _) => ApplyMapPropertiesRequested?.Invoke(this, new MapPropertiesChangedEventArgs(_nameTextBox.Text, (int)_widthInput.Value, (int)_heightInput.Value));

        layout.Controls.Add(EditorControls.CreateFieldLabel("Name"), 0, 0);
        layout.Controls.Add(_nameTextBox, 1, 0);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Width"), 0, 1);
        layout.Controls.Add(_widthInput, 1, 1);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Height"), 0, 2);
        layout.Controls.Add(_heightInput, 1, 2);
        layout.Controls.Add(applyButton, 0, 3);
        layout.SetColumnSpan(applyButton, 2);

        return CreateSection("Map", layout, 160);
    }

    private Control BuildToolSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 4, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        ConfigureToolButton(_paintButton, "Paint", isChecked: true);
        ConfigureToolButton(_fillButton, "Fill", isChecked: false);

        _paintButton.CheckedChanged += (_, _) =>
        {
            if (_paintButton.Checked)
            {
                ToolChanged?.Invoke(this, EditorToolMode.Paint);
            }
        };
        _fillButton.CheckedChanged += (_, _) =>
        {
            if (_fillButton.Checked)
            {
                ToolChanged?.Invoke(this, EditorToolMode.Fill);
            }
        };

        layout.Controls.Add(_paintButton, 0, 0);
        layout.Controls.Add(_fillButton, 0, 1);
        return CreateSection("Tool", layout, 128);
    }

    private Control BuildPaletteSection()
    {
        var tileTable = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 4,
            Height = 148,
            BackColor = EditorColors.Section
        };
        tileTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tileTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tileTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tileTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tileTable.Controls.Add(CreateTileButton("#  Wall", '#'), 0, 0);
        tileTable.Controls.Add(CreateTileButton(".  Floor", '.'), 0, 1);
        tileTable.Controls.Add(CreateTileButton("@  Player", '@'), 0, 2);
        tileTable.Controls.Add(CreateTileButton("*  Relic", '*'), 0, 3);

        var divider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = EditorColors.BorderSoft };

        var zoneLabel = new Label
        {
            Text = "Zones",
            Dock = DockStyle.Top,
            Height = 26,
            Font = EditorFonts.SectionFont,
            ForeColor = EditorColors.SecondaryText,
            BackColor = EditorColors.Section,
            TextAlign = ContentAlignment.BottomLeft
        };

        var zoneTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(0, 2, 0, 0)
        };
        zoneTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        zoneTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        zoneTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        zoneTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        zoneTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        zoneTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        zoneTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        EditorControls.StyleTextBox(_zoneNameTextBox);

        _zoneTargetCombo.Dock = DockStyle.Fill;
        _zoneTargetCombo.Font = EditorFonts.UiFont;
        _zoneTargetCombo.BackColor = EditorColors.Surface;
        _zoneTargetCombo.ForeColor = EditorColors.PrimaryText;
        _zoneTargetCombo.DropDownStyle = ComboBoxStyle.DropDown;
        _zoneTargetCombo.FlatStyle = FlatStyle.Flat;

        _zoneListBox.Dock = DockStyle.Fill;
        _zoneListBox.Font = EditorFonts.UiFont;
        _zoneListBox.BackColor = EditorColors.Surface;
        _zoneListBox.ForeColor = EditorColors.PrimaryText;
        _zoneListBox.BorderStyle = BorderStyle.None;
        _zoneListBox.SelectedIndexChanged += (_, _) =>
        {
            var selected = _zoneListBox.SelectedItem as string;
            ZoneSelected?.Invoke(this, new ZoneSelectedEventArgs(selected));
        };

        var addButton = EditorControls.CreateButton("Add / Update", primary: true);
        addButton.Dock = DockStyle.Fill;
        addButton.Height = 30;
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
        removeButton.Height = 30;
        removeButton.Click += (_, _) =>
        {
            if (_zoneListBox.SelectedItem is string name)
            {
                ZoneRemoveRequested?.Invoke(this, name);
            }
        };

        zoneTable.Controls.Add(EditorControls.CreateFieldLabel("Name"), 0, 0);
        zoneTable.Controls.Add(_zoneNameTextBox, 1, 0);
        zoneTable.Controls.Add(EditorControls.CreateFieldLabel("Target"), 0, 1);
        zoneTable.Controls.Add(_zoneTargetCombo, 1, 1);
        zoneTable.Controls.Add(addButton, 0, 2);
        zoneTable.SetColumnSpan(addButton, 2);
        zoneTable.Controls.Add(_zoneListBox, 0, 3);
        zoneTable.SetColumnSpan(_zoneListBox, 2);
        zoneTable.Controls.Add(removeButton, 0, 4);
        zoneTable.SetColumnSpan(removeButton, 2);

        var container = new Panel { Dock = DockStyle.Fill, BackColor = EditorColors.Section };
        container.Controls.Add(zoneTable);
        container.Controls.Add(zoneLabel);
        container.Controls.Add(divider);
        container.Controls.Add(tileTable);

        return CreateSection("Palette", container, 440);
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

    private static void ConfigureToolButton(RadioButton radioButton, string text, bool isChecked)
    {
        radioButton.Text = text;
        radioButton.Checked = isChecked;
        radioButton.Dock = DockStyle.Fill;
        radioButton.Font = EditorFonts.UiFont;
        radioButton.BackColor = EditorColors.Section;
        radioButton.ForeColor = EditorColors.PrimaryText;
        radioButton.Margin = new Padding(0, 0, 0, 4);
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
    Run
}

public sealed class MapPropertiesChangedEventArgs(string name, int width, int height) : EventArgs
{
    public string Name { get; } = name;
    public int Width { get; } = width;
    public int Height { get; } = height;
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
