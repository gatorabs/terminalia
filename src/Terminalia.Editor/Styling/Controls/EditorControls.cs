using System.Runtime.InteropServices;
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

    public static void ApplyDarkScrollBars(Control control)
    {
        if (control.IsHandleCreated)
            SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
        else
            control.HandleCreated += (_, _) => SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
    }

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd, string? pszSubAppName, string? pszSubIdList);
}
