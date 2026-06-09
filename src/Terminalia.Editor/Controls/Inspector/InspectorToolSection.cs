using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Inspector;

public sealed class InspectorToolSection : Panel
{
    private readonly RadioButton _paintButton = new();
    private readonly RadioButton _fillButton = new();
    private bool _suppressEvents;

    public event EventHandler<EditorToolMode>? ToolChanged;

    public InspectorToolSection()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.Section;

        ConfigureRadioButton(_paintButton, "Paint", EditorToolMode.Paint, isChecked: true);
        ConfigureRadioButton(_fillButton, "Fill", EditorToolMode.Fill, isChecked: false);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = EditorColors.Section,
            Padding = new Padding(0, 4, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        layout.Controls.Add(_paintButton, 0, 0);
        layout.Controls.Add(_fillButton, 0, 1);
        Controls.Add(layout);
    }

    public void SetToolMode(EditorToolMode mode)
    {
        _suppressEvents = true;
        _paintButton.Checked = mode == EditorToolMode.Paint;
        _fillButton.Checked = mode == EditorToolMode.Fill;
        _suppressEvents = false;
    }

    private void ConfigureRadioButton(RadioButton radioButton, string text, EditorToolMode mode, bool isChecked)
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
            if (!_suppressEvents && radioButton.Checked)
            {
                ToolChanged?.Invoke(this, mode);
            }
        };
    }
}
