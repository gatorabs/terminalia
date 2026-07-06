using Terminalia.Editor.Services;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Screens.Dialogs;

public sealed class MapNameDialog : Form
{
    private readonly TextBox _nameBox;

    public MapNameDialog()
    {
        Text = "New Map";
        Size = new Size(300, 175);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = EditorColors.AppBackground;
        ForeColor = EditorColors.PrimaryText;
        Font = EditorFonts.UiFont;

        _nameBox = new TextBox { Text = "new_map", Dock = DockStyle.Fill };
        EditorControls.StyleTextBox(_nameBox);

        var pathHint = new Label
        {
            Text = $"Salvo em: {EditorFileService.DefaultMapsDirectory}",
            Dock = DockStyle.Fill,
            AutoSize = false,
            AutoEllipsis = true,
            ForeColor = EditorColors.SecondaryText,
            Font = EditorFonts.HintFont
        };

        var ok = EditorControls.CreateButton("Create", primary: true);
        ok.DialogResult = DialogResult.OK;
        ok.Dock = DockStyle.Fill;

        var cancel = EditorControls.CreateButton("Cancel");
        cancel.DialogResult = DialogResult.Cancel;
        cancel.Dock = DockStyle.Fill;

        var buttonRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttonRow.Controls.Add(ok, 0, 0);
        buttonRow.Controls.Add(cancel, 1, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12),
            BackColor = EditorColors.AppBackground
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        layout.Controls.Add(EditorControls.CreateFieldLabel("Map name:"), 0, 0);
        layout.Controls.Add(_nameBox, 0, 1);
        layout.Controls.Add(pathHint, 0, 2);
        layout.Controls.Add(buttonRow, 0, 3);

        Controls.Add(layout);
        AcceptButton = ok;
        CancelButton = cancel;
        _nameBox.SelectAll();
    }

    public static string? Prompt(IWin32Window owner)
    {
        using var dialog = new MapNameDialog();
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog._nameBox.Text.Trim() : null;
    }
}
