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

    public event EventHandler<MapPropertiesChangedEventArgs>? ApplyMapPropertiesRequested;
    public event EventHandler<EditorToolMode>? ToolChanged;
    public event EventHandler<char>? TileSelected;
    public event EventHandler? RunEmbeddedRequested;

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
        AddSection(InspectorSection.Palette, BuildPaletteSection(), 210);
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
        applyButton.Click += (_, _) => ApplyMapPropertiesRequested?.Invoke(
            this,
            new MapPropertiesChangedEventArgs(
                _nameTextBox.Text,
                (int)_widthInput.Value,
                (int)_heightInput.Value));

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
                ToolChanged?.Invoke(this, EditorToolMode.Paint);
        };
        _fillButton.CheckedChanged += (_, _) =>
        {
            if (_fillButton.Checked)
                ToolChanged?.Invoke(this, EditorToolMode.Fill);
        };

        layout.Controls.Add(_paintButton, 0, 0);
        layout.Controls.Add(_fillButton, 0, 1);
        return CreateSection("Tool", layout, 128);
    }

    private Control BuildPaletteSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(0, 4, 0, 0),
            BackColor = EditorColors.Section
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        layout.Controls.Add(CreateTileButton("#  Wall", '#'), 0, 0);
        layout.Controls.Add(CreateTileButton(".  Floor", '.'), 0, 1);
        layout.Controls.Add(CreateTileButton("@  Player", '@'), 0, 2);
        layout.Controls.Add(CreateTileButton("*  Relic", '*'), 0, 3);
        return CreateSection("Palette", layout, 210);
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

        // Controls added last render first in Dock.Top stack (highest z-order = top)
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
