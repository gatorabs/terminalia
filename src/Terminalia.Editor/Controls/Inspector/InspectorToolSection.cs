using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Inspector;

public sealed class InspectorToolSection : Panel
{
    private readonly CheckBox _selectButton = new();
    private readonly RadioButton _paintButton = new();
    private readonly RadioButton _fillButton = new();
    private bool _suppressEvents;

    public event EventHandler<EditorToolMode>? ToolChanged;

    public InspectorToolSection()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.Section;

        ConfigureSelectButton();
        ConfigureRadioButton(_paintButton, "Paint", isChecked: true);
        ConfigureRadioButton(_fillButton, "Fill", isChecked: false);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = EditorColors.Section,
            Padding = new Padding(0, 4, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 9));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        var separator = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = EditorColors.BorderSoft,
            Margin = new Padding(0)
        };
        var separatorHost = new Panel { Dock = DockStyle.Fill, BackColor = EditorColors.Section, Margin = new Padding(0, 0, 0, 4) };
        separatorHost.Controls.Add(separator);

        layout.Controls.Add(_selectButton, 0, 0);
        layout.Controls.Add(separatorHost, 0, 1);
        layout.Controls.Add(_paintButton, 0, 2);
        layout.Controls.Add(_fillButton, 0, 3);
        Controls.Add(layout);
    }

    public void SetToolMode(EditorToolMode mode)
    {
        _suppressEvents = true;
        _selectButton.Checked = mode == EditorToolMode.Select;
        _paintButton.Checked = mode == EditorToolMode.Paint;
        _fillButton.Checked = mode == EditorToolMode.Fill;
        ApplySelectVisual();
        _suppressEvents = false;
    }

    private void ConfigureSelectButton()
    {
        _selectButton.Text = "⌖  Select";
        _selectButton.Appearance = Appearance.Button;
        _selectButton.FlatStyle = FlatStyle.Flat;
        _selectButton.TextAlign = ContentAlignment.MiddleCenter;
        _selectButton.Dock = DockStyle.Fill;
        _selectButton.Font = EditorFonts.UiFont;
        _selectButton.Margin = new Padding(0, 0, 0, 4);
        _selectButton.FlatAppearance.BorderColor = EditorColors.SelectAccent;
        _selectButton.FlatAppearance.BorderSize = 1;
        _selectButton.FlatAppearance.CheckedBackColor = EditorColors.SelectAccent;
        _selectButton.FlatAppearance.MouseOverBackColor = EditorColors.ButtonHover;
        ApplySelectVisual();

        _selectButton.CheckedChanged += (_, _) =>
        {
            ApplySelectVisual();
            if (_suppressEvents)
            {
                return;
            }

            if (_selectButton.Checked)
            {
                _suppressEvents = true;
                _paintButton.Checked = false;
                _fillButton.Checked = false;
                _suppressEvents = false;
                ToolChanged?.Invoke(this, EditorToolMode.Select);
            }
            else
            {
                _paintButton.Checked = true;
            }
        };
    }

    private void ApplySelectVisual()
    {
        _selectButton.BackColor = _selectButton.Checked ? EditorColors.SelectAccent : EditorColors.ButtonBackground;
        _selectButton.ForeColor = _selectButton.Checked ? EditorColors.SelectAccentText : EditorColors.SelectAccent;
    }

    private void ConfigureRadioButton(RadioButton radioButton, string text, bool isChecked)
    {
        radioButton.Text = text;
        radioButton.Checked = isChecked;
        radioButton.Dock = DockStyle.Fill;
        radioButton.Font = EditorFonts.UiFont;
        radioButton.BackColor = EditorColors.Section;
        radioButton.ForeColor = EditorColors.PrimaryText;
        radioButton.Margin = new Padding(0, 0, 0, 4);

        radioButton.CheckedChanged += (_, _) =>
        {
            if (_suppressEvents || !radioButton.Checked)
            {
                return;
            }

            _suppressEvents = true;
            _selectButton.Checked = false;
            ApplySelectVisual();
            _suppressEvents = false;
            ToolChanged?.Invoke(this, radioButton == _paintButton ? EditorToolMode.Paint : EditorToolMode.Fill);
        };
    }
}
