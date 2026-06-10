using System.Runtime.InteropServices;
using Terminalia.Editor.Controls.Shared;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Styling.Controls;

internal static class EditorControls
{
    public static Button CreateButton(string text, bool primary = false)
    {
        return new ModernButton
        {
            Text = text,
            Font = EditorFonts.UiFont,
            Padding = new Padding(8, 0, 8, 0),
            IsPrimary = primary
        };
    }

    public static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = EditorFonts.UiFont,
            ForeColor = EditorColors.SecondaryText
        };
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = EditorColors.InputBackground;
        textBox.ForeColor = EditorColors.PrimaryText;
        textBox.Font = EditorFonts.UiFont;
    }

    public static void StyleNumberInput(NumericUpDown input)
    {
        input.BorderStyle = BorderStyle.FixedSingle;
        input.BackColor = EditorColors.InputBackground;
        input.ForeColor = EditorColors.PrimaryText;
        input.Font = EditorFonts.UiFont;
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.Dock = DockStyle.Fill;
        comboBox.Font = EditorFonts.UiFont;
        comboBox.BackColor = EditorColors.Surface;
        comboBox.ForeColor = EditorColors.PrimaryText;
        comboBox.FlatStyle = FlatStyle.Flat;
    }

    public static void StyleListBox(ListBox listBox)
    {
        listBox.Dock = DockStyle.Fill;
        listBox.Font = EditorFonts.UiFont;
        listBox.BackColor = EditorColors.Surface;
        listBox.ForeColor = EditorColors.PrimaryText;
        listBox.BorderStyle = BorderStyle.None;
    }

    public static Control CreateButtonPair(string firstText, Action firstAction, string secondText, Action secondAction)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var first = CreateButton(firstText, primary: true);
        first.Dock = DockStyle.Fill;
        first.Click += (_, _) => firstAction();

        var second = CreateButton(secondText);
        second.Dock = DockStyle.Fill;
        second.Click += (_, _) => secondAction();

        row.Controls.Add(first, 0, 0);
        row.Controls.Add(second, 1, 0);
        return row;
    }

    public static void ApplyDarkScrollBars(Control control)
    {
        if (control.IsHandleCreated)
        {
            SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
        }
        else
        {
            control.HandleCreated += (_, _) => SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
        }
    }

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd, string? pszSubAppName, string? pszSubIdList);
}
