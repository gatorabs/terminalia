using Terminalia.Editor.Selection;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;

namespace Terminalia.Editor.Controls.Inspector;

internal static class InspectorPropertyEditors
{
    public static Control Create(EditableProperty property, Action<string, string> onEdited)
    {
        if (property.IsReadOnly)
        {
            var label = EditorControls.CreateFieldLabel(property.Value);
            label.ForeColor = EditorColors.PrimaryText;
            return label;
        }

        return property.Kind switch
        {
            EditablePropertyKind.Number => CreateNumberEditor(property, onEdited),
            EditablePropertyKind.Toggle => CreateToggleEditor(property, onEdited),
            _ => CreateTextEditor(property, onEdited)
        };
    }

    private static Control CreateNumberEditor(EditableProperty property, Action<string, string> onEdited)
    {
        var input = new NumericUpDown
        {
            Minimum = property.Minimum,
            Maximum = property.Maximum,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 0, 2)
        };
        EditorControls.StyleNumberInput(input);

        var initial = int.TryParse(property.Value, out var value) ? value : property.Minimum;
        input.Value = Math.Clamp(initial, property.Minimum, property.Maximum);
        input.ValueChanged += (_, _) => onEdited(property.Key, ((int)input.Value).ToString());
        return input;
    }

    private static Control CreateToggleEditor(EditableProperty property, Action<string, string> onEdited)
    {
        var checkBox = new CheckBox
        {
            Dock = DockStyle.Fill,
            BackColor = EditorColors.Section,
            Checked = string.Equals(property.Value, "true", StringComparison.OrdinalIgnoreCase)
        };
        checkBox.CheckedChanged += (_, _) => onEdited(property.Key, checkBox.Checked ? "true" : "false");
        return checkBox;
    }

    private static Control CreateTextEditor(EditableProperty property, Action<string, string> onEdited)
    {
        var textBox = new TextBox { Dock = DockStyle.Fill, Text = property.Value, Margin = new Padding(0, 2, 0, 2) };
        EditorControls.StyleTextBox(textBox);
        textBox.Leave += (_, _) => onEdited(property.Key, textBox.Text);
        return textBox;
    }
}
