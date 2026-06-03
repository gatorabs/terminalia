using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed class InspectorPanel : UserControl
{
    private readonly TextBox _nameTextBox = new();
    private readonly NumericUpDown _widthInput = new();
    private readonly NumericUpDown _heightInput = new();
    private readonly TrackBar _zoomInput = new();
    private readonly RadioButton _paintButton = new();
    private readonly RadioButton _fillButton = new();
    private readonly Button _runEmbeddedButton = EditorTheme.CreateButton("Run in editor", primary: true);
    private readonly Button _resetEmbeddedButton = EditorTheme.CreateButton("Reset embedded");

    public event EventHandler<MapPropertiesChangedEventArgs>? ApplyMapPropertiesRequested;
    public event EventHandler<int>? ZoomChanged;
    public event EventHandler<EditorToolMode>? ToolChanged;
    public event EventHandler<char>? TileSelected;
    public event EventHandler? RunEmbeddedRequested;
    public event EventHandler? ResetEmbeddedRequested;

    public InspectorPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.Navigation;
        Padding = new Padding(12);

        var scrollHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = EditorTheme.Navigation,
            Padding = new Padding(0)
        };

        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            BackColor = EditorTheme.Navigation,
            Padding = new Padding(0)
        };

        stack.Controls.Add(BuildMapSection());
        stack.Controls.Add(BuildViewSection());
        stack.Controls.Add(BuildToolSection());
        stack.Controls.Add(BuildPaletteSection());
        stack.Controls.Add(BuildRunSection());

        scrollHost.Controls.Add(stack);
        scrollHost.Resize += (_, _) => stack.Width = scrollHost.ClientSize.Width;
        Controls.Add(scrollHost);
    }

    public void LoadDocument(MapAssetDocument document)
    {
        _nameTextBox.Text = document.Name;
        _widthInput.Value = document.Width;
        _heightInput.Value = document.Height;
    }

    public void SetRunState(bool isRunning)
    {
        _runEmbeddedButton.Text = isRunning ? "Pause embedded" : "Run in editor";
    }

    public void SelectTile(char tile)
    {
        TileSelected?.Invoke(this, tile);
    }

    private Control BuildMapSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(0, 4, 0, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        EditorTheme.StyleTextBox(_nameTextBox);
        _widthInput.Minimum = 4;
        _widthInput.Maximum = 80;
        _heightInput.Minimum = 4;
        _heightInput.Maximum = 40;
        EditorTheme.StyleNumberInput(_widthInput);
        EditorTheme.StyleNumberInput(_heightInput);

        var applyButton = EditorTheme.CreateButton("Apply", primary: true);
        applyButton.Dock = DockStyle.Fill;
        applyButton.Click += (_, _) => ApplyMapPropertiesRequested?.Invoke(
            this,
            new MapPropertiesChangedEventArgs(
                _nameTextBox.Text,
                (int)_widthInput.Value,
                (int)_heightInput.Value));

        layout.Controls.Add(EditorTheme.CreateFieldLabel("Name"), 0, 0);
        layout.Controls.Add(_nameTextBox, 1, 0);
        layout.Controls.Add(EditorTheme.CreateFieldLabel("Width"), 0, 1);
        layout.Controls.Add(_widthInput, 1, 1);
        layout.Controls.Add(EditorTheme.CreateFieldLabel("Height"), 0, 2);
        layout.Controls.Add(_heightInput, 1, 2);
        layout.Controls.Add(applyButton, 0, 3);
        layout.SetColumnSpan(applyButton, 2);

        return CreateSection("Map", layout, 156);
    }

    private Control BuildViewSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 4, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _zoomInput.Minimum = 16;
        _zoomInput.Maximum = 48;
        _zoomInput.TickFrequency = 8;
        _zoomInput.SmallChange = 4;
        _zoomInput.LargeChange = 8;
        _zoomInput.Value = 32;
        _zoomInput.Dock = DockStyle.Fill;
        _zoomInput.BackColor = EditorTheme.Section;
        _zoomInput.ForeColor = EditorTheme.PrimaryText;
        _zoomInput.ValueChanged += (_, _) => ZoomChanged?.Invoke(this, _zoomInput.Value);

        layout.Controls.Add(EditorTheme.CreateFieldLabel("Zoom"), 0, 0);
        layout.Controls.Add(_zoomInput, 0, 1);
        return CreateSection("View", layout, 112);
    }

    private Control BuildToolSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 6, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

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
        return CreateSection("Tool", layout, 122);
    }

    private Control BuildPaletteSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(0, 6, 0, 0)
        };

        layout.Controls.Add(CreateTileButton("#  Wall", '#'), 0, 0);
        layout.Controls.Add(CreateTileButton(".  Floor", '.'), 0, 1);
        layout.Controls.Add(CreateTileButton("@  Player", '@'), 0, 2);
        layout.Controls.Add(CreateTileButton("*  Relic", '*'), 0, 3);
        return CreateSection("Palette", layout, 202);
    }

    private Control BuildRunSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 6, 0, 0)
        };

        _runEmbeddedButton.Dock = DockStyle.Top;
        _resetEmbeddedButton.Dock = DockStyle.Top;
        _runEmbeddedButton.Click += (_, _) => RunEmbeddedRequested?.Invoke(this, EventArgs.Empty);
        _resetEmbeddedButton.Click += (_, _) => ResetEmbeddedRequested?.Invoke(this, EventArgs.Empty);

        layout.Controls.Add(_runEmbeddedButton, 0, 0);
        layout.Controls.Add(_resetEmbeddedButton, 0, 1);
        return CreateSection("Run", layout, 132);
    }

    private static Panel CreateSection(string title, Control content, int height)
    {
        var section = new Panel
        {
            Dock = DockStyle.Top,
            Height = height,
            BackColor = EditorTheme.Section,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 10)
        };
        section.Paint += (_, e) =>
        {
            using var border = new Pen(EditorTheme.Border);
            e.Graphics.DrawRectangle(border, 0, 0, section.Width - 1, section.Height - 1);
        };

        var label = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Height = 24,
            Font = EditorTheme.SectionFont,
            ForeColor = EditorTheme.PrimaryText,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = EditorTheme.Section
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        content.Dock = DockStyle.Fill;
        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(content, 0, 1);
        section.Controls.Add(layout);
        return section;
    }

    private static void ConfigureToolButton(RadioButton radioButton, string text, bool isChecked)
    {
        radioButton.Text = text;
        radioButton.Checked = isChecked;
        radioButton.Dock = DockStyle.Fill;
        radioButton.BackColor = EditorTheme.Section;
        radioButton.ForeColor = EditorTheme.PrimaryText;
        radioButton.Margin = new Padding(0, 0, 0, 4);
    }

    private Button CreateTileButton(string text, char tile)
    {
        var button = EditorTheme.CreateButton(text);
        button.Dock = DockStyle.Fill;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Margin = new Padding(0, 0, 0, 6);
        button.Click += (_, _) => TileSelected?.Invoke(this, tile);
        return button;
    }
}

public sealed class MapPropertiesChangedEventArgs(string name, int width, int height) : EventArgs
{
    public string Name { get; } = name;

    public int Width { get; } = width;

    public int Height { get; } = height;
}
